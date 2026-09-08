using Godot;

namespace BloodDragon
{
    /// <summary>Helpers for the CRT scanline / vignette overlay and the left-side menu buttons.</summary>
    public static class MenuOverlay
    {
        private static StyleBoxFlat _selectedStylebox;
        private static StyleBoxFlat _idleStylebox;

        /// <summary>Adds a top CanvasLayer with the CRT scanline + vignette shader over everything.</summary>
        public static void AddCrt(Node root)
        {
            var shader = GD.Load<Shader>("res://shaders/crt_effect.gdshader");
            if (shader == null) return;

            var layer = new CanvasLayer { Layer = 100 };
            var rect = new ColorRect { Color = new Color(1, 1, 1, 1) };
            rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            rect.MouseFilter = Control.MouseFilterEnum.Ignore;
            var mat = new ShaderMaterial { Shader = shader };
            // Scanlines/noise sit on top of glyphs and make text look muddy.
            mat.SetShaderParameter("scanline_intensity", 0.0f);
            mat.SetShaderParameter("noise_intensity", 0.0f);
            mat.SetShaderParameter("vignette_radius", 0.98f);
            mat.SetShaderParameter("vignette_softness", 0.35f);
            rect.Material = mat;
            layer.AddChild(rect);
            root.AddChild(layer);
        }

        /// <summary>Menu-button background. Selected = left magenta bar + faint phosphor fill, not a lime slab.
        /// Instances are cached so repeated styling (category switches, hover states) does not churn the GC.</summary>
        public static StyleBoxFlat ButtonStylebox(bool selected)
        {
            if (!selected && _idleStylebox != null) return _idleStylebox;
            if (selected && _selectedStylebox != null) return _selectedStylebox;

            var sb = new StyleBoxFlat { BgColor = selected ? MenuTheme.RowFill : MenuTheme.Transparent };
            sb.SetContentMarginAll(10);
            sb.ContentMarginLeft = 18;
            if (selected)
            {
                sb.BorderColor = MenuTheme.Accent;
                sb.BorderWidthLeft = 3;
                sb.BorderWidthTop = 0;
                sb.BorderWidthBottom = 0;
                sb.BorderWidthRight = 0;
            }

            if (selected) _selectedStylebox = sb;
            else _idleStylebox = sb;
            return sb;
        }

        /// <summary>Left-aligned menu button. Hover/focus keep phosphor text on a dark row.</summary>
        public static Button MakeMenuButton(string text, int fontSize = 30)
        {
            var b = new Button
            {
                Text = Localization.T(text).ToUpper(),
                Alignment = HorizontalAlignment.Left,
                FocusMode = Control.FocusModeEnum.All,
            };
            b.AddThemeFontOverride("font", MenuTheme.Mono);
            b.AddThemeFontSizeOverride("font_size", fontSize);

            var active = ButtonStylebox(selected: true);
            var idle = ButtonStylebox(selected: false);
            b.AddThemeStyleboxOverride("normal", idle);
            b.AddThemeStyleboxOverride("hover", active);
            b.AddThemeStyleboxOverride("pressed", active);
            b.AddThemeStyleboxOverride("focus", active);

            b.AddThemeColorOverride("font_color", MenuTheme.TextNormal);
            b.AddThemeColorOverride("font_hover_color", MenuTheme.TextActive);
            b.AddThemeColorOverride("font_focus_color", MenuTheme.TextActive);
            b.AddThemeColorOverride("font_pressed_color", MenuTheme.TextActive);

            b.MouseEntered += () => AudioManager.Instance?.PlayHover();
            return b;
        }
    }
}
