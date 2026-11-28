using System.Windows.Forms;

namespace EmissorLicencas;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new EmissorForm());
    }
}
