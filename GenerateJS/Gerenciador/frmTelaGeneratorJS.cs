using System.Diagnostics;
using System.Runtime.InteropServices;
using ExecutorService.GeneratorJs;
using Microsoft.Web.WebView2.Core;


namespace GenerateJS.Gerenciador
{
    /// <summary>Janela unica: hospeda o WebView2 com a interface (wwwroot) e liga a ponte JS &lt;-&gt; C#.</summary>
    public partial class frmTelaGeneratorJS : Form
    {
        private const string Host = "app.generatorjs";
        private static readonly string WebRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        private static readonly string WebViewData = Path.Combine(ReferenciaStandard.Repository.AppPaths.GeneratorJs, "WebView2");
        private Bridge? _bridge;

        public frmTelaGeneratorJS()
        {
            InitializeComponent();   // controles e propriedades da janela: frmTelaGeneratorJS.Designer.cs
        }

        private async void frmTelaGeneratorJS_Load(object? sender, EventArgs e)
        {
            await InitAsync();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetTitleBar(dark: true, caption: Color.Black);
        }

        private async Task InitAsync()
        {
            try
            {
                Directory.CreateDirectory(WebViewData);
                var env = await CoreWebView2Environment.CreateAsync(null, WebViewData);
                await _web.EnsureCoreWebView2Async(env);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                MessageBox.Show("O WebView2 Runtime nao esta instalado.\nBaixe em https://developer.microsoft.com/microsoft-edge/webview2/",
                    "GeneratorJS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            var core = _web.CoreWebView2;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            core.Settings.IsPasswordAutosaveEnabled = false;
            core.Settings.IsGeneralAutofillEnabled = false;

            core.SetVirtualHostNameToFolderMapping(Host, WebRoot, CoreWebView2HostResourceAccessKind.Deny);
            core.NavigationStarting += (_, e) => { if (!e.Uri.StartsWith($"https://{Host}/", StringComparison.OrdinalIgnoreCase)) e.Cancel = true; };
            core.NewWindowRequested += (_, e) => e.Handled = true;

            _bridge = new Bridge(core, this, new GeneratorJsExecutor());
            core.WebMessageReceived += (_, e) => _ = _bridge.HandleAsync(e.WebMessageAsJson);

            core.Navigate($"https://{Host}/index.html");
        }

        // ---- barra de titulo acompanha o tema da tela (Windows 10 20H1+ / Windows 11)
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        internal void SetTitleBar(bool dark, Color caption)
        {
            if (!IsHandleCreated) return;
            var d = dark ? 1 : 0;
            DwmSetWindowAttribute(Handle, 20, ref d, sizeof(int));                    // DWMWA_USE_IMMERSIVE_DARK_MODE
            var bgr = caption.R | (caption.G << 8) | (caption.B << 16);
            DwmSetWindowAttribute(Handle, 35, ref bgr, sizeof(int));                  // DWMWA_CAPTION_COLOR (Windows 11)
        }
    }
}

