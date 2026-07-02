using System.Windows.Forms;

namespace CrosshairMarker;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var appContext = new CrosshairApplicationContext();
        Application.Run(appContext);
    }
}
