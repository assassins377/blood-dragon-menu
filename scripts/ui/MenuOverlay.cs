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

        /// <summary>Menu-button background stylebox with the shared padding.</summary>
        public static StyleBoxFlat ButtonStylebox(Color bg)
        {
            var sb = new StyleBoxFlat { BgColor = bg };
            sb.SetContentMarginAll(10);
            sb.ContentMarginLeft = 18;
            return sb;
        }

        /// <summary>Left-aligned menu button: transparent normally, full green fill on hover/focus.
        /// Plays the hover blip automatically, so callers never wire that up themselves.</summary>
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

            var active = ButtonStylebox(MenuTheme.Accent);
            b.AddThemeStyleboxOverride("normal", ButtonStylebox(MenuTheme.Transparent));
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

        /// <summary>Bottom legend like Blood Dragon: ВЫБРАТЬ / ПРИНЯТЬ / НАЗАД.</summary>
        public static NavBar MakeNavBar(BoxContainer.AlignmentMode align = BoxContainer.AlignmentMode.End)
        {
            var bar = new NavBar();
            bar.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
            bar.AnchorTop = 1;
            bar.AnchorBottom = 1;
            bar.AnchorLeft = 0;
            bar.AnchorRight = 1;
            bar.OffsetLeft = 80;
            bar.OffsetRight = -80;
            bar.OffsetTop = -64;
            bar.OffsetBottom = -24;
            bar.Alignment = align;
            bar.AddThemeConstantOverride("separation", 36);
            bar.MouseFilter = Control.MouseFilterEnum.Ignore;
            return bar;
        }
    }

    public partial class NavBar : HBoxContainer
    {
        private readonly Label _select;
        private readonly Label _accept;
        private readonly Label _back;

        public NavBar()
        {
            _select = Hint("ENTER", "ВЫБРАТЬ");
            _accept = Hint("ENTER", "ПРИНЯТЬ");
            _back = Hint("ESC", "НАЗАД");
            AddChild(_select);
            AddChild(_accept);
            AddChild(_back);
        }

        public void SetHints(bool select, bool back, bool accept = false)
        {
            _select.Visible = select;
            _accept.Visible = accept;
            _back.Visible = back;
            Visible = select || back || accept;
        }

        private static Label Hint(string key, string action)
        {
            var l = MenuTheme.MakeLabel($"{key}  {action}", 18);
            l.AddThemeColorOverride("font_color", MenuTheme.Accent);
            l.MouseFilter = MouseFilterEnum.Ignore;
            return l;
        }
    }
}
