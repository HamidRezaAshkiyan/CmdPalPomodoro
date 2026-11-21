using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;

namespace CmdPalPomodoro;

public class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
        {
            ComServer server = new();
            ManualResetEvent extensionDisposedEvent = new(false);

            CmdPalPomodoro extensionInstance = new(extensionDisposedEvent);
            server.RegisterClass<CmdPalPomodoro, IExtension>(() => extensionInstance);
            server.Start();

            extensionDisposedEvent.WaitOne();
            server.Stop();
            server.UnsafeDispose();
        }
        else
        {
            Console.WriteLine("Not being launched as an extension... exiting.");
        }
    }
}
