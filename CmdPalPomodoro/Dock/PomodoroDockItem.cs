using CmdPalPomodoro.Pages;
using CmdPalPomodoro.Services;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalPomodoro.Dock;

internal sealed partial class PomodoroDockItem : ListItem
{
    private readonly PomodoroTimerService _timer = PomodoroTimerService.Instance;

    public PomodoroDockItem(PomodoroPage page)
        : base(page)
    {
        Icon = new IconInfo("\uE823");
        Apply();
        _timer.Changed += (_, _) => Apply();
    }

    private void Apply()
    {
        Title = _timer.DockTitle;
        Subtitle = _timer.DockSubtitle;
    }
}
