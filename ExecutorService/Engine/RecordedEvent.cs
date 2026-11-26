
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace ReferenciaFactory.Engine
{
    public class RecordedEvent
    {
        public enum EventKind { MouseClick, MouseMove, KeyDown, KeyUp, Scroll, WindowFocus }
        public EventKind Kind { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public MouseButtons Button { get; set; }
        public bool IsDoubleClick { get; set; }
        public Keys Key { get; set; }
        public string KeyChar { get; set; } = "";
        public bool IsCtrl { get; set; }
        public bool IsAlt { get; set; }
        public bool IsShift { get; set; }
        public int ScrollDelta { get; set; }
        public string WindowTitle { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public TimeSpan DelayFromPrev { get; set; }

        public string GetLabel() => Kind switch
        {
            EventKind.MouseClick => $"{(IsDoubleClick ? "Duplo " : "")}Clique {Button}",
            EventKind.MouseMove => "Mover mouse",
            EventKind.KeyDown => BuildKeyLabel(),
            EventKind.Scroll => $"Scroll {(ScrollDelta > 0 ? "↑" : "↓")}",
            EventKind.WindowFocus => $"Foco: {WindowTitle}",
            _ => Kind.ToString()
        };

        public string GetDetail() => Kind switch
        {
            EventKind.MouseClick => $"X:{X}  Y:{Y}",
            EventKind.MouseMove => $"X:{X}  Y:{Y}",
            EventKind.KeyDown => BuildKeyLabel(),
            EventKind.Scroll => $"X:{X}  Y:{Y}  Δ:{ScrollDelta}",
            EventKind.WindowFocus => WindowTitle,
            _ => ""
        };

        private string BuildKeyLabel()
        {
            string k = IsCtrl ? "Ctrl+" : "";
            k += IsAlt ? "Alt+" : "";
            k += IsShift ? "Shift+" : "";
            k += KeyChar.Length > 0 ? KeyChar : Key.ToString();
            return k;
        }
    }

    public class GlobalInputHook : IDisposable
    {
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags);

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public int X, Y;
            public uint mouseData, flags, time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode, scanCode, flags, time;
            public IntPtr dwExtraInfo;
        }

        private IntPtr _mouseHook = IntPtr.Zero;
        private IntPtr _keyHook = IntPtr.Zero;

        private readonly HookProc _mouseProc;
        private readonly HookProc _keyProc;
        private GCHandle _mouseProcHandle;
        private GCHandle _keyProcHandle;

        private DateTime _lastEvent = DateTime.Now;
        private System.Drawing.Point _lastMousePos;
        private string _lastWindowTitle = "";
        private DateTime _lastClickTime = DateTime.MinValue;
        private System.Drawing.Point _lastClickPos;

        private volatile bool _stopped = true;

        public event EventHandler<RecordedEvent>? EventCaptured;

        public bool CaptureMouseClicks { get; set; } = true;
        public bool CaptureMouseMove { get; set; } = false;
        public bool CaptureKeyboard { get; set; } = true;
        public bool CaptureScroll { get; set; } = false;
        public bool DetectPauses { get; set; } = true;
        public bool TrackWindowFocus { get; set; } = true;

        public GlobalInputHook()
        {
            _mouseProc = MouseHookCallback;
            _keyProc = KeyboardHookCallback;
        }

        public void Start()
        {
            Stop();
            _stopped = false;

            _mouseProcHandle = GCHandle.Alloc(_mouseProc);
            _keyProcHandle = GCHandle.Alloc(_keyProc);

            using var proc = Process.GetCurrentProcess();
            using var mod = proc.MainModule!;
            var hMod = GetModuleHandle(mod.ModuleName!);

            _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, hMod, 0);
            _keyHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyProc, hMod, 0);
            _lastEvent = DateTime.Now;
        }

        public void Stop()
        {
            _stopped = true;

            if (_mouseHook != IntPtr.Zero) { UnhookWindowsHookEx(_mouseHook); _mouseHook = IntPtr.Zero; }
            if (_keyHook != IntPtr.Zero) { UnhookWindowsHookEx(_keyHook); _keyHook = IntPtr.Zero; }
            if (_mouseProcHandle.IsAllocated) _mouseProcHandle.Free();
            if (_keyProcHandle.IsAllocated) _keyProcHandle.Free();
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_stopped || nCode < 0)
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

            try
            {
                var ms = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                int msg = (int)wParam;

                CheckWindowFocus();

                if (CaptureMouseClicks &&
                    (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN ||
                     msg == WM_MBUTTONDOWN || msg == WM_LBUTTONDBLCLK))
                {
                    var btn = msg == WM_RBUTTONDOWN ? MouseButtons.Right :
                              msg == WM_MBUTTONDOWN ? MouseButtons.Middle : MouseButtons.Left;

                    bool isDbl = false;
                    var now = DateTime.Now;
                    var pos = new System.Drawing.Point(ms.X, ms.Y);
                    if (btn == MouseButtons.Left &&
                        (now - _lastClickTime).TotalMilliseconds < 500 &&
                        Math.Abs(pos.X - _lastClickPos.X) < 5 &&
                        Math.Abs(pos.Y - _lastClickPos.Y) < 5)
                        isDbl = true;
                    _lastClickTime = now;
                    _lastClickPos = pos;

                    Emit(new RecordedEvent
                    {
                        Kind = RecordedEvent.EventKind.MouseClick,
                        X = ms.X,
                        Y = ms.Y,
                        Button = btn,
                        IsDoubleClick = isDbl
                    });
                }

                if (CaptureMouseMove && msg == WM_MOUSEMOVE)
                {
                    var pos = new System.Drawing.Point(ms.X, ms.Y);
                    if (Math.Abs(pos.X - _lastMousePos.X) > 5 || Math.Abs(pos.Y - _lastMousePos.Y) > 5)
                    {
                        _lastMousePos = pos;
                        Emit(new RecordedEvent { Kind = RecordedEvent.EventKind.MouseMove, X = ms.X, Y = ms.Y });
                    }
                }

                if (CaptureScroll && msg == WM_MOUSEWHEEL)
                {
                    int delta = (short)(ms.mouseData >> 16);
                    Emit(new RecordedEvent { Kind = RecordedEvent.EventKind.Scroll, X = ms.X, Y = ms.Y, ScrollDelta = delta });
                }
            }
            catch { }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_stopped || nCode < 0)
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

            try
            {
                if (CaptureKeyboard)
                {
                    int msg = (int)wParam;
                    if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                    {
                        var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                        var key = (Keys)kb.vkCode;

                        bool ctrl = (Control.ModifierKeys & Keys.Control) != 0;
                        bool alt = (Control.ModifierKeys & Keys.Alt) != 0;
                        bool shift = (Control.ModifierKeys & Keys.Shift) != 0;

                        if (key == Keys.LControlKey || key == Keys.RControlKey ||
                            key == Keys.LMenu || key == Keys.RMenu ||
                            key == Keys.LShiftKey || key == Keys.RShiftKey)
                            goto done;



                        string keyChar = GetKeyChar(kb.vkCode, kb.scanCode);

                        CheckWindowFocus();
                        Emit(new RecordedEvent
                        {
                            Kind = RecordedEvent.EventKind.KeyDown,
                            Key = key,
                            KeyChar = keyChar,
                            IsCtrl = ctrl,
                            IsAlt = alt,
                            IsShift = shift,
                        });
                    }
                }
            }
            catch { }

        done:
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static string GetKeyChar(uint vkCode, uint scanCode)
        {
            byte[] keyState = new byte[256];
            GetKeyboardState(keyState);

            var sb = new System.Text.StringBuilder(4);
            int result = ToUnicode(vkCode, scanCode, keyState, sb, sb.Capacity, 0);

            if (result == 1)
            {
                return sb.ToString();
            }
            return HotkeyTranslator.KeyCodeCollection(vkCode);
        }

        private void CheckWindowFocus()
        {
            if (!TrackWindowFocus) return;
            var hwnd = GetForegroundWindow();
            var sb = new System.Text.StringBuilder(256);
            GetWindowText(hwnd, sb, 256);
            string title = sb.ToString();
            if (title.Length > 0 && title != _lastWindowTitle)
            {
                _lastWindowTitle = title;
                Emit(new RecordedEvent { Kind = RecordedEvent.EventKind.WindowFocus, WindowTitle = title });
            }
        }

        private void Emit(RecordedEvent e)
        {
            if (_stopped) return;
            var now = DateTime.Now;
            e.DelayFromPrev = now - _lastEvent;
            _lastEvent = now;
            EventCaptured?.Invoke(this, e);
        }

        public void Dispose() => Stop();
    }
}
