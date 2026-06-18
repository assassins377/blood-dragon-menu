using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    /// <summary>
    /// "Справка и параметры" — two-column settings screen.
    /// Left: category list. Right: green-bordered panel hosting the active
    /// category scene. Changes are buffered in a pending copy and only
    /// committed on ПРИНЯТЬ.
    /// </summary>
    public partial class SettingsMenu : Control
    {
        private static readonly (string Name, string Scene)[] Categories =
        {
            ("ИЗОБРАЖЕНИЕ",          "res://scenes/settings_menu/categories/DisplaySettingsPanel.tscn"),
            ("КАЛИБРОВКА",          "res://scenes/settings_menu/categories/CalibrationSettingsPanel.tscn"),
            ("КАЧЕСТВО ВИДЕО",      "res://scenes/settings_menu/categories/VideoQualitySettingsPanel.tscn"),
            ("ЭЛЕМЕНТЫ УПРАВЛЕНИЯ", "res://scenes/settings_menu/categories/InputSettingsPanel.tscn"),
            ("ИГРОВОЙ ПРОЦЕСС",     "res://scenes/settings_menu/categories/GameplaySettingsPanel.tscn"),
            ("ЗВУК",                "res://scenes/settings_menu/categories/AudioSettingsPanel.tscn"),
            ("РУКОВОДСТВО",         "res://scenes/settings_menu/categories/ManualPanel.tscn"),
            ("ТИТРЫ",               "res://scenes/settings_menu/categories/CreditsSettingsPanel.tscn"),
        };

        private GameSettings _pending;

        private VBoxContainer _categoryList;
        private Label _panelHeader;
        private Control _panelHost;
        private readonly Dictionary<string, Control> _panels = new();
        private readonly List<Button> _categoryButtons = new();

        public override void _Ready()
        {
            _pending = SettingsManager.Instance.Current.Clone();
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var bg = new ColorRect { Color = MenuTheme.Background };
            bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            bg.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(bg);

            BuildLayout();
            BuildCategories();
            MenuOverlay.AddCrt(this);
            AudioManager.Instance?.StartMenuMusic();

            AnimateFadeIn();
            SelectCategory(Categories[0].Name);
        }

        // ── Layout ────────────────────────────────────────────────────────

        private void BuildLayout()
        {
            var margin = new MarginContainer();
            margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_left", 70);
            margin.AddThemeConstantOverride("margin_right", 70);
            margin.AddThemeConstantOverride("margin_top", 60);
            margin.AddThemeConstantOverride("margin_bottom", 60);
            AddChild(margin);

            var root = new VBoxContainer();
            root.AddThemeConstantOverride("separation", 18);
            margin.AddChild(root);

            var heading = MenuTheme.MakeLabel("СПРАВКА И ПАРАМЕТРЫ", 34);
            heading.AddThemeColorOverride("font_color", MenuTheme.Accent);
            root.AddChild(heading);

            var columns = new HBoxContainer();
            columns.SizeFlagsVertical = SizeFlags.ExpandFill;
            columns.AddThemeConstantOverride("separation", 28);
            root.AddChild(columns);

            _categoryList = new VBoxContainer { CustomMinimumSize = new Vector2(420, 0) };
            _categoryList.AddThemeConstantOverride("separation", 6);
            columns.AddChild(_categoryList);

            var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddThemeStyleboxOverride("panel", MenuTheme.Panel(border: true));
            columns.AddChild(panel);

            var panelBox = new VBoxContainer();
            panelBox.AddThemeConstantOverride("separation", 12);
            panel.AddChild(panelBox);

            _panelHeader = MenuTheme.MakeLabel("", 26);
            _panelHeader.AddThemeColorOverride("font_color", MenuTheme.Accent);
            panelBox.AddChild(_panelHeader);

            _panelHost = new Control
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                ClipContents = true,
            };
            panelBox.AddChild(_panelHost);

            var bottom = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.End };
            bottom.AddThemeConstantOverride("separation", 16);
            root.AddChild(bottom);

            var accept = MenuOverlay.MakeMenuButton("Принять", 26);
            accept.CustomMinimumSize = new Vector2(220, 48);
            accept.Pressed += OnAccept;
            bottom.AddChild(accept);

            var back = MenuOverlay.MakeMenuButton("Назад", 26);
            back.CustomMinimumSize = new Vector2(220, 48);
            back.Pressed += OnBack;
            bottom.AddChild(back);
        }

        private void BuildCategories()
        {
            foreach (var (name, scenePath) in Categories)
            {
                var scene = GD.Load<PackedScene>(scenePath);
                var panel = scene.Instantiate<BaseSettingsPanel>();
                panel.Visible = false;
                _panelHost.AddChild(panel);
                panel.Setup(_pending);
                _panels[name] = panel;

                var btn = MenuOverlay.MakeMenuButton(name, 24);
                btn.CustomMinimumSize = new Vector2(0, 46);
                btn.Pressed += () => { AudioManager.Instance?.PlaySelect(); SelectCategory(name); };
                _categoryButtons.Add(btn);
                _categoryList.AddChild(btn);
            }
        }

        private void SelectCategory(string name)
        {
            _panelHeader.Text = name;
            foreach (var kv in _panels)
                kv.Value.Visible = kv.Key == name;

            foreach (var b in _categoryButtons)
            {
                bool sel = b.Text == name;
                b.AddThemeStyleboxOverride("normal",
                    MenuOverlay.ButtonStylebox(sel ? MenuTheme.Accent : MenuTheme.Transparent));
                b.AddThemeColorOverride("font_color", sel ? MenuTheme.TextActive : MenuTheme.TextNormal);
            }
        }

        // ── Actions ─────────────────────────────────────────────────────────

        private void OnAccept()
        {
            AudioManager.Instance?.PlaySelect();
            SettingsManager.Instance.ApplyFromPending(_pending);
            GetTree().ChangeSceneToFile("res://scenes/main_menu/MainMenu.tscn");
        }

        private void OnBack()
        {
            AudioManager.Instance?.PlaySelect();
            GetTree().ChangeSceneToFile("res://scenes/main_menu/MainMenu.tscn");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnBack();
                GetViewport().SetInputAsHandled();
            }
        }

        private void AnimateFadeIn()
        {
            Modulate = new Color(1, 1, 1, 0);
            CreateTween().TweenProperty(this, "modulate:a", 1.0f, 0.3f);
        }
    }
}
