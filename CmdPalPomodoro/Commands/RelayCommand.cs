using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalPomodoro.Commands;

internal sealed partial class RelayCommand : InvokableCommand
{
    private readonly Action _action;

    public RelayCommand(string id, string name, Action action)
    {
        Id = id;
        Name = name;
        _action = action;
    }

    public override CommandResult Invoke()
    {
        _action();
        return CommandResult.KeepOpen();
    }
}
