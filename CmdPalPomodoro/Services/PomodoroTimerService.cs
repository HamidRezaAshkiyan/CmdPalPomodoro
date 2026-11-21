using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalPomodoro.Services;

internal sealed class PomodoroStateChangedEventArgs : EventArgs
{
    public bool PhaseChanged { get; init; }
}

internal sealed partial class PomodoroTimerService : IDisposable
{
    public static PomodoroTimerService Instance { get; } = new();

    private readonly object _gate = new();
    private readonly System.Timers.Timer _timer;
    private PomodoroSettings _settings;
    private PomodoroPhase _phase = PomodoroPhase.Idle;
    private TimeSpan _remaining;
    private DateTime _endsAtUtc;
    private bool _running;
    private bool _paused;
    private int _completedWorkCount;
    private bool _pendingLongBreak;

    public event EventHandler<PomodoroStateChangedEventArgs>? Changed;

    private PomodoroTimerService()
    {
        _settings = PomodoroSettingsStore.Load();
        _remaining = _settings.WorkDuration;
        _timer = new System.Timers.Timer(1000) { AutoReset = true };
        _timer.Elapsed += (_, _) => Tick();
    }

    public PomodoroPhase Phase
    {
        get { lock (_gate) return _phase; }
    }

    public bool IsPaused
    {
        get { lock (_gate) return _paused; }
    }

    public int CompletedWorkCount
    {
        get { lock (_gate) return _completedWorkCount; }
    }

    public PomodoroSettings Settings
    {
        get { lock (_gate) return Clone(_settings); }
    }

    public string DisplayTime
    {
        get { lock (_gate) return Format(_remaining); }
    }

    public string PageTitle => Phase switch
    {
        PomodoroPhase.Work => $"Focus {DisplayTime}",
        PomodoroPhase.ShortBreak => $"Short break {DisplayTime}",
        PomodoroPhase.LongBreak => $"Long break {DisplayTime}",
        PomodoroPhase.WaitingForBreak => PendingBreakLabel,
        PomodoroPhase.WaitingForWork => "Start next focus",
        _ => "Pomodoro",
    };

    public string DockTitle
    {
        get
        {
            lock (_gate)
            {
                var time = Format(_remaining);
                if (_paused)
                {
                    return $"⏸ {time}";
                }

                return _phase switch
                {
                    PomodoroPhase.WaitingForBreak => "Break?",
                    PomodoroPhase.WaitingForWork => "Focus?",
                    _ => time,
                };
            }
        }
    }

    public string DockSubtitle
    {
        get
        {
            lock (_gate)
            {
                return _phase switch
                {
                    PomodoroPhase.Work => _paused ? "Paused · focus" : $"Focus · {Progress()}",
                    PomodoroPhase.ShortBreak => _paused ? "Paused · short break" : "Short break",
                    PomodoroPhase.LongBreak => _paused ? "Paused · long break" : "Long break",
                    PomodoroPhase.WaitingForBreak => _pendingLongBreak ? "Long break is ready" : "Short break is ready",
                    PomodoroPhase.WaitingForWork => "Break over",
                    _ => $"Ready · {Progress()}",
                };
            }
        }
    }

    public string StatusLine
    {
        get
        {
            lock (_gate)
            {
                return _phase switch
                {
                    PomodoroPhase.Work => _paused ? $"Paused · {Progress()}" : $"Focus · {Progress()}",
                    PomodoroPhase.ShortBreak => _paused ? "Short break paused" : "Short break",
                    PomodoroPhase.LongBreak => _paused ? "Long break paused" : "Long break",
                    PomodoroPhase.WaitingForBreak => _pendingLongBreak
                        ? "Work session finished. Start the long break when you are ready."
                        : "Work session finished. Start the short break when you are ready.",
                    PomodoroPhase.WaitingForWork => "Break finished. Start the next focus when you are ready.",
                    _ => $"Ready · {Progress()}",
                };
            }
        }
    }

    public string PendingBreakLabel
    {
        get { lock (_gate) return _pendingLongBreak ? "Long break" : "Short break"; }
    }

    public void StartWork()
    {
        lock (_gate)
        {
            if (_running && !_paused)
            {
                return;
            }

            if (_phase is PomodoroPhase.ShortBreak or PomodoroPhase.LongBreak or PomodoroPhase.WaitingForBreak)
            {
                return;
            }

            Begin(_settings.WorkDuration, PomodoroPhase.Work);
        }

        Publish(phaseChanged: true);
    }

    public void StartBreak()
    {
        PomodoroPhase started;
        lock (_gate)
        {
            if (_phase != PomodoroPhase.WaitingForBreak)
            {
                return;
            }

            started = _pendingLongBreak ? PomodoroPhase.LongBreak : PomodoroPhase.ShortBreak;
            var duration = started == PomodoroPhase.LongBreak
                ? _settings.LongBreakDuration
                : _settings.ShortBreakDuration;
            Begin(duration, started);
        }

        Publish(phaseChanged: true);
    }

    public void Pause()
    {
        lock (_gate)
        {
            if (!_running || _paused)
            {
                return;
            }

            _remaining = RemainingLocked();
            _paused = true;
            _timer.Stop();
        }

        Publish(phaseChanged: true);
    }

    public void Resume()
    {
        lock (_gate)
        {
            if (!_paused)
            {
                return;
            }

            _paused = false;
            _endsAtUtc = DateTime.UtcNow + _remaining;
            _timer.Start();
        }

        Publish(phaseChanged: true);
    }

    public void Skip()
    {
        string? toast;
        lock (_gate)
        {
            toast = _phase switch
            {
                PomodoroPhase.Work => CompleteWorkLocked(),
                PomodoroPhase.ShortBreak or PomodoroPhase.LongBreak => CompleteBreakLocked(),
                PomodoroPhase.WaitingForBreak => SkipWaitingBreakLocked(),
                _ => null,
            };
        }

        if (toast is null)
        {
            return;
        }

        Notify(toast);
        Publish(phaseChanged: true);
    }

    public void Reset()
    {
        lock (_gate)
        {
            _timer.Stop();
            _running = false;
            _paused = false;
            _phase = PomodoroPhase.Idle;
            _remaining = _settings.WorkDuration;
            _pendingLongBreak = false;
        }

        Publish(phaseChanged: true);
    }

    public void ResetCycle()
    {
        lock (_gate)
        {
            _completedWorkCount = 0;
            _timer.Stop();
            _running = false;
            _paused = false;
            _phase = PomodoroPhase.Idle;
            _remaining = _settings.WorkDuration;
            _pendingLongBreak = false;
        }

        Publish(phaseChanged: true);
    }

    public bool TryUpdateSettings(PomodoroSettings settings, out string error)
    {
        if (!PomodoroSettings.TryNormalize(
                settings.WorkMinutes,
                settings.ShortBreakMinutes,
                settings.LongBreakMinutes,
                settings.SessionsUntilLongBreak,
                out var normalized,
                out error))
        {
            return false;
        }

        lock (_gate)
        {
            _settings = normalized;
            PomodoroSettingsStore.Save(_settings);
            if (_phase == PomodoroPhase.Idle && !_running)
            {
                _remaining = _settings.WorkDuration;
            }
        }

        Publish(phaseChanged: false);
        return true;
    }

    private void Tick()
    {
        string? toast = null;
        var phaseChanged = false;
        lock (_gate)
        {
            if (!_running || _paused)
            {
                return;
            }

            _remaining = RemainingLocked();
            if (_remaining <= TimeSpan.Zero)
            {
                toast = _phase switch
                {
                    PomodoroPhase.Work => CompleteWorkLocked(),
                    PomodoroPhase.ShortBreak or PomodoroPhase.LongBreak => CompleteBreakLocked(),
                    _ => null,
                };
                phaseChanged = true;
            }
        }

        if (toast is not null)
        {
            Notify(toast);
        }

        Publish(phaseChanged);
    }

    private void Begin(TimeSpan duration, PomodoroPhase phase)
    {
        _phase = phase;
        _remaining = duration;
        _endsAtUtc = DateTime.UtcNow + duration;
        _running = true;
        _paused = false;
        _timer.Start();
    }

    private string CompleteWorkLocked()
    {
        _timer.Stop();
        _running = false;
        _paused = false;
        _completedWorkCount++;
        _pendingLongBreak = _completedWorkCount % _settings.SessionsUntilLongBreak == 0;
        _phase = PomodoroPhase.WaitingForBreak;
        _remaining = _pendingLongBreak ? _settings.LongBreakDuration : _settings.ShortBreakDuration;
        return _pendingLongBreak
            ? "Pomodoro done — time for a long break"
            : "Pomodoro done — time for a short break";
    }

    private string CompleteBreakLocked()
    {
        _timer.Stop();
        _running = false;
        _paused = false;
        _phase = PomodoroPhase.WaitingForWork;
        _remaining = _settings.WorkDuration;
        return "Break over — start next focus";
    }

    private string SkipWaitingBreakLocked()
    {
        _phase = PomodoroPhase.WaitingForWork;
        _remaining = _settings.WorkDuration;
        _pendingLongBreak = false;
        return "Break skipped — start next focus";
    }

    private TimeSpan RemainingLocked()
    {
        var remaining = _endsAtUtc - DateTime.UtcNow;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    private string Progress() => $"{_completedWorkCount}/{_settings.SessionsUntilLongBreak}";

    private static PomodoroSettings Clone(PomodoroSettings settings) => new()
    {
        WorkMinutes = settings.WorkMinutes,
        ShortBreakMinutes = settings.ShortBreakMinutes,
        LongBreakMinutes = settings.LongBreakMinutes,
        SessionsUntilLongBreak = settings.SessionsUntilLongBreak,
    };

    internal static string Format(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        if (value.TotalHours >= 1)
        {
            return $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}";
        }

        return $"{(int)value.TotalMinutes:00}:{value.Seconds:00}";
    }

    private void Publish(bool phaseChanged)
    {
        Changed?.Invoke(this, new PomodoroStateChangedEventArgs { PhaseChanged = phaseChanged });
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }

    private static void Notify(string message)
    {
        try
        {
            new ToastStatusMessage(message).Show();
        }
        catch (Exception)
        {
            // The host may be unavailable; the Dock subtitle still shows the reminder.
        }
    }
}
