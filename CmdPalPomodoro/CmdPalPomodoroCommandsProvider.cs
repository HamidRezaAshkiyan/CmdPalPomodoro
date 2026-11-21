using CmdPalPomodoro.Dock;
using CmdPalPomodoro.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalPomodoro;

public partial class CmdPalPomodoroCommandsProvider : CommandProvider
{
    private readonly PomodoroPage _page = new();
    private readonly ICommandItem[] _commands;
    private readonly ICommandItem _dockBand;

    public CmdPalPomodoroCommandsProvider()
    {
        DisplayName = "Pomodoro";
        Id = "com.github.cmdpal.pomodoro";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        var dockItem = new PomodoroDockItem(_page);
        _dockBand = new WrappedDockItem(
            [dockItem],
            "com.github.cmdpal.pomodoro.dock",
            "Pomodoro");

        _commands =
        [
            new CommandItem(_page)
            {
                Title = DisplayName,
                Subtitle = "Focus timer",
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands() => _commands;

    public override ICommandItem[]? GetDockBands() => [_dockBand];

    public override ICommandItem? GetCommandItem(string id)
    {
        if (string.Equals(id, _page.Id, StringComparison.Ordinal))
        {
            return _commands[0];
        }

        if (string.Equals(id, "com.github.cmdpal.pomodoro.dock", StringComparison.Ordinal))
        {
            return _dockBand;
        }

        return null;
    }
}
