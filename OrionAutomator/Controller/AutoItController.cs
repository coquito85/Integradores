
using AutoIt;
using OrionAutomator.Engine;

namespace ReferenciaFactory.Controller
{
    public class AutoItController
    {
        private const int SW_RESTORE = 9;


        private static readonly Dictionary<string, string> SpecialKeys = HotkeyTranslator.SpecialKeyMaps;
        public void Dispose()
        {
            GC.SuppressFinalize(this);

        }
        public async Task MoveMouseAsync(int x, int y, int speed = -15)
        {
            await Task.Run(() => AutoItX.MouseMove(x, y, speed));
        }

        public async Task ClickAsync(int x, int y, string button = "left", int clicks = 1, int speed = 5)
        {
            await Task.Run(() => AutoItX.MouseClick(button, x, y, clicks, speed));

        }

        public async Task DragAsync(int x1, int y1, int x2, int y2, int Speed = 5)
        {
            await Task.Run(() => AutoItX.MouseClickDrag("left", x1, y1, x2, y2, Speed));
        }

        public async Task TypeTextAsync(string text, int delay = 20)
        {
            await Task.Run(() =>
            {
                AutoItX.AutoItSetOption("SendKeyDelay", delay);

                if (SpecialKeys.TryGetValue(text.Trim(), out string? autoItKey))
                {
                    AutoItX.Send(autoItKey, 0);
                    return;
                }


                string escaped = text
                    .Replace("{", "{{}")
                    .Replace("}", "{}}");

                AutoItX.Send(escaped, 1);
            });
        }

        public async Task SendKeysAsync(string keys)
        {
            await Task.Run(() => AutoItX.Send(keys, 0));
        }

        public async Task HotKeyAsync(string modifiers, string key)
        {
            await Task.Run(() => AutoItX.Send($"{modifiers}{{{key}}}", 0));
        }

        public async Task<bool> ActivateWindowAsync(string title, CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                AutoItX.AutoItSetOption("WinTitleMatchMode", 2);
                AutoItX.WinActivate(title);

                var deadline = DateTime.UtcNow.AddSeconds(3);
                while (DateTime.UtcNow < deadline)
                {
                    if (ct.IsCancellationRequested) return false;
                    if (AutoItX.WinActive(title) == 1) return true;
                    ct.WaitHandle.WaitOne(150);
                }
                return !ct.IsCancellationRequested && AutoItX.WinActive(title) == 1;
            }, ct);
        }

        public async Task OpenAppAsync(string path, CancellationToken ct = default)
        {
            await Task.Run(() => AutoItX.Run(path, "", 1), ct);
            await Task.Delay(1500, ct);
        }

        public async Task WaitWindowAsync(string title, int timeout = 10, CancellationToken ct = default)
        {
            await Task.Run(() =>
            {
                AutoItX.AutoItSetOption("WinTitleMatchMode", 2);

                var deadline = DateTime.UtcNow.AddSeconds(timeout);
                while (DateTime.UtcNow < deadline)
                {
                    if (ct.IsCancellationRequested) return;
                    if (AutoItX.WinWait(title, "", 0) == 1) return;
                    ct.WaitHandle.WaitOne(150);
                }
            }, ct);
        }

        public async Task CloseWindowAsync(string title)
        {
            await Task.Run(() =>
            {
                AutoItX.AutoItSetOption("WinTitleMatchMode", 2);
                AutoItX.WinClose(title);
            });
        }
        public async Task SelectTextAsync(int x1, int y1, int x2, int y2, int speed = 5)
        {
            await Task.Run(() => AutoItX.MouseClickDrag("left", x1, y1, x2, y2, speed));
        }
        public async Task MoveWindowAsync(string title, int x, int y)
        {
            await Task.Run(() =>
            {
                AutoItX.AutoItSetOption("WinTitleMatchMode", 2);
                if (AutoItX.WinWait(title, "", 5) == 0) return;
                AutoItX.WinSetState(title, "", 9);
                Thread.Sleep(350);
                AutoItX.WinActivate(title);
                AutoItX.WinWaitActive(title, "", 4);
                AutoItX.WinMove(title, "", x, y);
                Thread.Sleep(150);
                AutoItX.WinMove(title, "", x, y);
            });
        }

        public async Task ResizeWindowAsync(string title, int w, int h)
        {
            await Task.Run(() =>
            {
                AutoItX.AutoItSetOption("WinTitleMatchMode", 2);
                AutoItX.WinSetState(title, "", SW_RESTORE);
                Thread.Sleep(200);
                AutoItX.WinActivate(title);
                AutoItX.WinWaitActive(title, "", 3);
                var pos = AutoItX.WinGetPos(title);
                AutoItX.WinMove(title, "", pos.X, pos.Y, w, h);
            });
        }

        public async Task ExecuteEventAsync(RecordedEvent ev)
        {
            switch (ev.Kind)
            {
                case RecordedEvent.EventKind.MouseClick:
                    if (ev.IsDoubleClick)
                        await ClickAsync(ev.X, ev.Y, ev.Button == MouseButtons.Right ? "right" : "left", 2);
                    else
                        await ClickAsync(ev.X, ev.Y,
                            ev.Button == MouseButtons.Right ? "right" :
                            ev.Button == MouseButtons.Middle ? "middle" : "left");
                    break;

                case RecordedEvent.EventKind.MouseMove:
                    await MoveMouseAsync(ev.X, ev.Y);
                    break;

                case RecordedEvent.EventKind.KeyDown:
                    await SendRecordedKeyAsync(ev);
                    break;

                case RecordedEvent.EventKind.Scroll:
                    string dir = ev.ScrollDelta > 0 ? "up" : "down";
                    int clicks = Math.Abs(ev.ScrollDelta) / 120;
                    await Task.Run(() => AutoItX.MouseWheel(dir, clicks > 0 ? clicks : 1));
                    break;

                case RecordedEvent.EventKind.WindowFocus:
                    await ActivateWindowAsync(ev.WindowTitle);
                    await Task.Delay(150);
                    break;

                default:
                    await Task.Delay(50);
                    break;
            }
        }


        private async Task SendRecordedKeyAsync(RecordedEvent ev)
        {
            string modifiers = "";
            if (ev.IsCtrl) modifiers += "^";
            if (ev.IsAlt) modifiers += "!";
            if (ev.IsShift) modifiers += "+";

            string rawKey = ev.KeyChar.Length > 0 ? ev.KeyChar : ev.Key.ToString();

            string keyPart;

            if (SpecialKeys.TryGetValue(rawKey, out string? mapped))
            {
                keyPart = mapped;
            }
            else if (rawKey.Length == 1)
            {
                keyPart = modifiers.Length > 0 ? rawKey.ToLower() : rawKey;
            }
            else
            {
                keyPart = $"{{{rawKey}}}";
            }

            string sequence = modifiers + keyPart;
            await SendKeysAsync(sequence);
        }

        public Bitmap CaptureScreen()
        {
            var screen = Screen.PrimaryScreen.Bounds;
            var bmp = new Bitmap(screen.Width, screen.Height);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(screen.Location, Point.Empty, screen.Size);
            return bmp;
        }
    }
}
