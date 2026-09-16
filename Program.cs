using CladTracker.Services;

namespace CladTracker;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(new EntryStore()));
    }
}
