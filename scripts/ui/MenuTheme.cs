using Godot;

namespace BloodDragon
{
    /// <summary>Shared palette, fonts and small factory helpers for the menu UI.</summary>
    public static class MenuTheme
    {
        public static readonly Color Background    = new("#020d05"); // very dark green
        public static readonly Color Accent        = new("#00ff00"); // neon green
        public static readonly Color AccentSoft    = new("#39ff14");
        public static readonly Color Transparent   = new(0, 0, 0, 0);
        public static readonly Color TextNormal    = new("#cccccc");
        public static readonly Color TextActive     = new("#0a0a0a");
        public static readonly Color TextDisabled  = new("#555555");

        private static Font _mono;

        /// <summary>A monospace system font (Courier/Consolas/DejaVu fallback chain).</summary>
        public static Font Mono
        {
            get
            {
                _mono ??= new SystemFont
                {
                    FontNames = new[] { "Courier New", "Consolas", "DejaVu Sans Mono", "Liberation Mono", "monospace" },
                };
                return _mono;
            }
        }

        public static Label MakeLabel(string text, int fontSize = 22, bool upper = true)
        {
            var l = new Label
            {
                Text = upper ? text.ToUpper() : text,
                VerticalAlignment = VerticalAlignment.Center,
            };
            l.AddThemeFontOverride("font", Mono);
            l.AddThemeFontSizeOverride("font_size", fontSize);
            l.AddThemeColorOverride("font_color", TextNormal);
            return l;
        }

        /// <summary>A flat StyleBox with an optional green border (right settings panel).</summary>
        public static StyleBoxFlat Panel(bool border)
        {
            var sb = new StyleBoxFlat { BgColor = new Color(0.01f, 0.05f, 0.02f, 0.6f) };
            if (border)
            {
                sb.BorderColor = Accent;
                sb.SetBorderWidthAll(2);
            }
            sb.SetContentMarginAll(16);
            return sb;
        }
    }
}
