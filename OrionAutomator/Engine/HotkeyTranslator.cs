
using System.Text.RegularExpressions;

namespace OrionAutomator.Engine
{
    public static class HotkeyTranslator
    {
        public static readonly Dictionary<string, string> SpecialKeyMaps = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Backspace"] = "{BACKSPACE}",
            ["Enter"] = "{ENTER}",
            ["Tab"] = "{TAB}",
            ["Delete"] = "{DELETE}",
            ["Esc"] = "{ESC}",
            ["Escape"] = "{ESC}",
            ["Space"] = "{SPACE}",
            ["Up"] = "{UP}",
            ["Down"] = "{DOWN}",
            ["Left"] = "{LEFT}",
            ["Right"] = "{RIGHT}",
            ["Home"] = "{HOME}",
            ["End"] = "{END}",
            ["PageUp"] = "{PGUP}",
            ["PageDown"] = "{PGDN}",
            ["Insert"] = "{INSERT}",
            ["F1"] = "{F1}",
            ["F2"] = "{F2}",
            ["F3"] = "{F3}",
            ["F4"] = "{F4}",
            ["F5"] = "{F5}",
            ["F6"] = "{F6}",
            ["F7"] = "{F7}",
            ["F8"] = "{F8}",
            ["F9"] = "{F9}",
            ["F10"] = "{F10}",
            ["F11"] = "{F11}",
            ["F12"] = "{F12}",
            ["OemSemicolon"] = ";",
            ["Oem7"] = "'",
            ["OemComma"] = ",",
            ["OemPeriod"] = ".",
            ["OemMinus"] = "-",
            ["OemPlus"] = "=",
            ["OemQuestion"] = "/",
            ["OemOpenBrackets"] = "[",
            ["OemCloseBrackets"] = "]",
            ["OemPipe"] = "\\",
            ["OemTilde"] = "`",
        };

        private static readonly Regex _keyNameRegex = new(string.Join("|", SpecialKeyMaps.Keys.OrderByDescending(k => k.Length).Select(Regex.Escape)), RegexOptions.Compiled);

        public static string Translate(string keys)
        {
            string result = keys
                .Replace("Ctrl+", "^")
                .Replace("Alt+", "!")
                .Replace("Shift+", "+");
            result = _keyNameRegex.Replace(result, m => SpecialKeyMaps[m.Value]);
            result = LowercaseOutsideBraces(result);

            return result;

        }


        private static string LowercaseOutsideBraces(string s)
        {
            var sb = new System.Text.StringBuilder(s.Length);
            bool inBraces = false;
            foreach (char c in s)
            {
                if (c == '{') inBraces = true;
                else if (c == '}') inBraces = false;

                sb.Append(inBraces ? c : char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        public static string KeyCodeCollection(uint vkCode)
        {
            return (Keys)vkCode switch
            {
                Keys.Space => " ",
                Keys.Return => "Enter",
                Keys.Tab => "Tab",
                Keys.Back => "Backspace",
                Keys.Escape => "Esc",
                Keys.Delete => "Delete",
                Keys.Up => "Up",
                Keys.Down => "Down",
                Keys.Left => "Left",
                Keys.Right => "Right",
                Keys.Home => "Home",
                Keys.End => "End",
                Keys.Prior => "PageUp",
                Keys.Next => "PageDown",
                Keys.Insert => "Insert",
                Keys.F1 => "F1",
                Keys.F2 => "F2",
                Keys.F3 => "F3",
                Keys.F4 => "F4",
                Keys.F5 => "F5",
                Keys.F6 => "F6",
                Keys.F7 => "F7",
                Keys.F8 => "F8",
                Keys.F9 => "F9",
                Keys.F10 => "F10",
                Keys.F11 => "F11",
                Keys.F12 => "F12",

                _ => ((Keys)vkCode).ToString()
            };
        }
    }
}
