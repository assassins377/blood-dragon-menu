using Godot;

namespace BloodDragon
{
    /// <summary>Helpers for the CRT scanline / vignette overlay and the left-side menu buttons.</summary>
    public static class MenuOverlay
    {
        /// <summary>Adds a top CanvasLayer with the CRT scanline + vignette shader over everything.</summary>
        public static void AddCrt(Node root)
        {
            var shader = GD.Load<Shader>("res://shaders/crt_effect.gdshader");
            if (shader == null) return;

            var layer = new CanvasLayer { Layer = 100 };
            var rect = new ColorRect { Color = new Color(1, 1, 1, 1) };
            rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            rect.MouseFilter = Control.MouseFilterEnum.Ignore;
            rect.Material = new ShaderMaterial { Shader = shader };
            layer.AddChild(rect);
            root.AddChild(layer);
        }

        /// <summary>Left-aligned menu button: transparent normally, full green fill on hover/focus.</summary>
        public static Button MakeMenuButton(string text, int fontSize = 30)
        {
            var b = new Button
            {
                Text = text.ToUpper(),
                Alignment = HorizontalAlignment.Left,
                FocusMode = Control.FocusModeEnum.All,
            };
            b.AddThemeFontOverride("font", MenuTheme.Mono);
            b.AddThemeFontSizeOverride("font_size", fontSize);

            // Normal: transparent bg, light text.
            var normal = new StyleBoxFlat { BgColor = MenuTheme.Transparent };
            normal.SetContentMarginAll(10);
            normal.ContentMarginLeft = 18;

            // Hover / focus / pressed: solid green fill, dark text.
            var active = new StyleBoxFlat { BgColor = MenuTheme.Accent };
            active.SetContentMarginAll(10);
            active.ContentMarginLeft = 18;

            b.AddThemeStyleboxOverride("normal", normal);
            b.AddThemeStyleboxOverride("hover", active);
            b.AddThemeStyleboxOverride("pressed", active);
            b.AddThemeStyleboxOverride("focus", active);

            b.AddThemeColorOverride("font_color", MenuTheme.TextNormal);
            b.AddThemeColorOverride("font_hover_color", MenuTheme.TextActive);
            b.AddThemeColorOverride("font_focus_color", MenuTheme.TextActive);
            b.AddThemeColorOverride("font_pressed_color", MenuTheme.TextActive);
            return b;
        }
    }
}
