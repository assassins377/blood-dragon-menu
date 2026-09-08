using Godot;

namespace BloodDragon
{
    /// <summary>Shared palette, fonts and small factory helpers for the menu UI.</summary>
    public static class MenuTheme
    {
        public static readonly Color Background    = new("#eef3ea");
        public static readonly Color Accent        = new("#1f7a3a");
        public static readonly Color AccentSoft    = new("#b7d9bc");
        public static readonly Color Magenta       = new("#c43b6e");
        public static readonly Color RowFill       = new(0.78f, 0.90f, 0.80f, 1.0f);
        public static readonly Color Transparent   = new(0, 0, 0, 0);
        public static readonly Color TextNormal    = new("#2c332c");
        public static readonly Color TextActive    = new("#145c28");
        public static readonly Color TextDisabled  = new("#8a9488");

        private static Font _mono;
        private static StyleBoxFlat _panelBordered;
        private static StyleBoxFlat _panelPlain;

        /// <summary>A monospace system font (Courier/Consolas/DejaVu fallback chain).</summary>
        public static Font Mono
        {
            get
            {
                _mono ??= new SystemFont
                {
                    FontNames = new[] { "Ubuntu Mono", "DejaVu Sans Mono", "Liberation Mono", "Noto Sans Mono", "Consolas", "monospace" },
                    Antialiasing = TextServer.FontAntialiasing.Gray,
                    Hinting = TextServer.Hinting.Normal,
                    SubpixelPositioning = TextServer.SubpixelPositioning.Disabled,
                    Oversampling = 1.0f,
                };
                return _mono;
            }
        }

        public static Label MakeLabel(string text, int fontSize = 22, bool upper = true)
        {
            var l = new Label
            {
                Text = upper ? Localization.T(text).ToUpper() : Localization.T(text),
                VerticalAlignment = VerticalAlignment.Center,
            };
            l.AddThemeFontOverride("font", Mono);
            l.AddThemeFontSizeOverride("font_size", fontSize);
            l.AddThemeColorOverride("font_color", TextNormal);
            return l;
        }

        /// <summary>A flat StyleBox with an optional green border (right settings panel). Cached.</summary>
        public static StyleBoxFlat Panel(bool border)
        {
            if (border)
            {
                if (_panelBordered == null)
                {
                    var sb = new StyleBoxFlat { BgColor = new Color(1f, 1f, 1f, 1f) };
                    sb.BorderColor = Accent;
                    sb.SetBorderWidthAll(2);
                    sb.SetContentMarginAll(16);
                    _panelBordered = sb;
                }
                return _panelBordered;
            }

            if (_panelPlain == null)
            {
                var sb = new StyleBoxFlat { BgColor = new Color(1f, 1f, 1f, 1f) };
                sb.SetContentMarginAll(16);
                _panelPlain = sb;
            }
            return _panelPlain;
        }
    }
}
