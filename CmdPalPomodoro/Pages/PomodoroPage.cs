using CmdPalPomodoro.Commands;
using CmdPalPomodoro.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalPomodoro.Pages;

internal sealed partial class PomodoroPage : ListPage
{
    private readonly PomodoroTimerService _timer = PomodoroTimerService.Instance;
    private readonly PomodoroSettingsPage _settingsPage = new();

    public PomodoroPage()
    {
        Id = "com.github.cmdpal.pomodoro.page";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Name = "Open";
        Title = "Pomodoro";
        _timer.Changed += (_, _) =>
        {
            Title = _timer.PageTitle;
            RaiseItemsChanged();
        };
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>
        {
            new ListItem(new RelayCommand("com.github.cmdpal.pomodoro.status", "Status", static () => { }))
            {
                Title = _timer.DisplayTime,
                Subtitle = _timer.StatusLine,
                Icon = new IconInfo("\uE823"),
            },
        };

        switch (_timer.Phase)
        {
            case PomodoroPhase.Idle:
            case PomodoroPhase.WaitingForWork:
                items.Add(Action("com.github.cmdpal.pomodoro.start", "Start focus", "\uE768", _timer.StartWork, "Start the next focus session"));
                break;
            case PomodoroPhase.WaitingForBreak:
                items.Add(Action(
                    "com.github.cmdpal.pomodoro.start-break",
                    $"Start {_timer.PendingBreakLabel.ToLowerInvariant()}",
                    "\uE768",
                    _timer.StartBreak,
                    "The timer stays stopped until you start the break"));
                items.Add(Action("com.github.cmdpal.pomodoro.skip", "Skip break", "\uE893", _timer.Skip, "Skip the rest and wait to start focus"));
                break;
            default:
                if (_timer.IsPaused)
                {
                    items.Add(Action("com.github.cmdpal.pomodoro.resume", "Resume", "\uE768", _timer.Resume, "Continue the current session"));
                }
                else
                {
                    items.Add(Action("com.github.cmdpal.pomodoro.pause", "Pause", "\uE769", _timer.Pause, "Pause the current session"));
                }

                items.Add(Action("com.github.cmdpal.pomodoro.skip", "Skip", "\uE893", _timer.Skip, "End this session and wait for the next step"));
                break;
        }

        items.Add(Action("com.github.cmdpal.pomodoro.reset", "Reset", "\uE72C", _timer.Reset, "Stop and return to ready. Keeps the finished-session count."));
        items.Add(Action("com.github.cmdpal.pomodoro.reset-cycle", "Reset cycle", "\uE777", _timer.ResetCycle, "Stop and clear the finished-session count"));
        items.Add(new ListItem(_settingsPage)
        {
            Title = "Settings",
            Subtitle = "Focus length, breaks, and long-break interval",
            Icon = new IconInfo("\uE713"),
        });

        return items.ToArray();
    }

    private static ListItem Action(string id, string title, string icon, Action action, string subtitle)
    {
        return new ListItem(new RelayCommand(id, title, action))
        {
            Title = title,
            Subtitle = subtitle,
            Icon = new IconInfo(icon),
        };
    }
}
