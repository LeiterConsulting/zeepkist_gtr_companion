using System;
using System.Windows.Forms;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Hub;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new HubApplicationContext());
    }
}
