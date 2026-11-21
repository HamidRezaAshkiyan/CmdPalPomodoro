using System.Runtime.InteropServices;
using Microsoft.CommandPalette.Extensions;

namespace CmdPalPomodoro;

[Guid("38533641-1ffe-4633-8ef1-298aa24ff630")]
public sealed partial class CmdPalPomodoro : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;
    private readonly CmdPalPomodoroCommandsProvider _provider = new();

    public CmdPalPomodoro(ManualResetEvent extensionDisposedEvent)
    {
        _extensionDisposedEvent = extensionDisposedEvent;
    }

    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    public void Dispose() => _extensionDisposedEvent.Set();
}
