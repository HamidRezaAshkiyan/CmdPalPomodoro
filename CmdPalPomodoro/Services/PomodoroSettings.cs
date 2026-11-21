namespace CmdPalPomodoro.Services;

internal sealed class PomodoroSettings
{
    public int WorkMinutes { get; set; } = 25;

    public int ShortBreakMinutes { get; set; } = 5;

    public int LongBreakMinutes { get; set; } = 15;

    public int SessionsUntilLongBreak { get; set; } = 4;

    public TimeSpan WorkDuration => TimeSpan.FromMinutes(WorkMinutes);

    public TimeSpan ShortBreakDuration => TimeSpan.FromMinutes(ShortBreakMinutes);

    public TimeSpan LongBreakDuration => TimeSpan.FromMinutes(LongBreakMinutes);

    public static bool TryNormalize(
        int work,
        int shortBreak,
        int longBreak,
        int sessions,
        out PomodoroSettings settings,
        out string error)
    {
        if (work is < 1 or > 180)
        {
            settings = new PomodoroSettings();
            error = "Focus length must be between 1 and 180 minutes.";
            return false;
        }

        if (shortBreak is < 1 or > 60)
        {
            settings = new PomodoroSettings();
            error = "Short break must be between 1 and 60 minutes.";
            return false;
        }

        if (longBreak is < 1 or > 120)
        {
            settings = new PomodoroSettings();
            error = "Long break must be between 1 and 120 minutes.";
            return false;
        }

        if (sessions is < 1 or > 12)
        {
            settings = new PomodoroSettings();
            error = "Sessions until a long break must be between 1 and 12.";
            return false;
        }

        settings = new PomodoroSettings
        {
            WorkMinutes = work,
            ShortBreakMinutes = shortBreak,
            LongBreakMinutes = longBreak,
            SessionsUntilLongBreak = sessions,
        };
        error = string.Empty;
        return true;
    }
}
