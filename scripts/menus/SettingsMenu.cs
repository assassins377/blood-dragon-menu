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
            ("cat.display",          "res://scenes/settings_menu/categories/DisplaySettingsPanel.tscn"),
            ("cat.calibration",      "res://scenes/settings_menu/categories/CalibrationSettingsPanel.tscn"),
            ("cat.video_quality",    "res://scenes/settings_menu/categories/VideoQualitySettingsPanel.tscn"),
            ("cat.controls",         "res://scenes/settings_menu/categories/InputSettingsPanel.tscn"),
            ("cat.gameplay",         "res://scenes/settings_menu/categories/GameplaySettingsPanel.tscn"),
            ("cat.language",         "res://scenes/settings_menu/categories/LanguageSettingsPanel.tscn"),
            ("cat.audio",            "res://scenes/settings_menu/categories/AudioSettingsPanel.tscn"),
            ("cat.manual",           "res://scenes/settings_menu/categories/ManualPanel.tscn"),
            ("cat.credits",          "res://scenes/settings_menu/categories/CreditsSettingsPanel.tscn"),
            ("cat.stats",            "res://scenes/settings_menu/categories/PerformanceStatsPanel.tscn"),
        };

        private GameSettings _pending;

        private VBoxContainer _categoryList;
        private Label _panelHeader;
        private Control _panelHost;
        private Label _hintLabel;
        private Timer _hintTimer;
        private Control _detailLayer;
        private Label _detailTitle;
        private Label _detailBody;
        private string _pendingHint = "";
        private string _pendingDetail = "";
        private bool _clearHint;
        private readonly Dictionary<string, Control> _panels = new();
        private readonly List<Button> _categoryButtons = new();
        private Button _selectedCategoryButton;

        public override void _Ready()
        {
            _pending = SettingsManager.Instance.Current.Clone();
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var bg = new ColorRect { Color = MenuTheme.Background };
            bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            bg.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(bg);

            BuildLayout();
            BuildHintChrome();
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

            var heading = MenuTheme.MakeLabel("menu.settings_title", 34);
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

            _hintLabel = MenuTheme.MakeLabel("", 16, false);
            _hintLabel.AddThemeColorOverride("font_color", MenuTheme.TextDisabled);
            _hintLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            _hintLabel.CustomMinimumSize = new Vector2(0, 44);
            panelBox.AddChild(_hintLabel);

            var bottom = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.End };
            bottom.AddThemeConstantOverride("separation", 16);
            root.AddChild(bottom);

            var accept = MenuOverlay.MakeMenuButton("menu.accept", 26);
            accept.CustomMinimumSize = new Vector2(220, 48);
            accept.Pressed += OnAccept;
            bottom.AddChild(accept);

            var back = MenuOverlay.MakeMenuButton("menu.back", 26);
            back.CustomMinimumSize = new Vector2(220, 48);
            back.Pressed += OnBack;
            bottom.AddChild(back);
        }

        private void BuildCategories()
        {
            // Panels are instantiated lazily on first select, so entering the
            // settings screen does not build and localize all ten categories.
            foreach (var (name, _) in Categories)
            {
                var btn = MenuOverlay.MakeMenuButton(name, 24);
                btn.CustomMinimumSize = new Vector2(0, 46);
                btn.Pressed += () => { AudioManager.Instance?.PlaySelect(); SelectCategory(name); };
                _categoryButtons.Add(btn);
                _categoryList.AddChild(btn);
            }
        }

        private void EnsurePanel(string name)
        {
            if (_panels.ContainsKey(name)) return;

            string path = null;
            foreach (var c in Categories)
            {
                if (c.Name == name) { path = c.Scene; break; }
            }
            if (path == null) return;

            var scene = GD.Load<PackedScene>(path);
            var panel = scene.Instantiate<BaseSettingsPanel>();
            panel.Visible = false;
            _panelHost.AddChild(panel);
            panel.Setup(_pending);
            _panels[name] = panel;
        }

        private void SelectCategory(string name)
        {
            EnsurePanel(name);

            string display = Localization.T(name).ToUpper();
            _panelHeader.Text = display;
            foreach (var kv in _panels)
                kv.Value.Visible = kv.Key == name;

            // Restyle only the previously and newly selected buttons instead of
            // rebuilding every category stylebox on each switch.
            if (_selectedCategoryButton != null)
            {
                _selectedCategoryButton.AddThemeStyleboxOverride("normal", MenuOverlay.ButtonStylebox(false));
                _selectedCategoryButton.AddThemeColorOverride("font_color", MenuTheme.TextNormal);
            }

            Button next = null;
            foreach (var b in _categoryButtons)
            {
                if (b.Text == display)
                {
                    next = b;
                    b.AddThemeStyleboxOverride("normal", MenuOverlay.ButtonStylebox(true));
                    b.AddThemeColorOverride("font_color", MenuTheme.TextActive);
                }
            }
            _selectedCategoryButton = next;

            ShowHint("", "");
        }

        private void BuildHintChrome()
        {
            _hintTimer = new Timer { OneShot = true };
            _hintTimer.Timeout += FlushHint;
            AddChild(_hintTimer);

            SettingsHint.Focused += OnHintFocused;
            SettingsHint.Cleared += OnHintCleared;
            SettingsHint.DetailRequested += OpenDetail;

            var detailCanvas = new CanvasLayer { Layer = 110 };
            AddChild(detailCanvas);
            _detailLayer = new Control { Visible = false };
            _detailLayer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            _detailLayer.MouseFilter = MouseFilterEnum.Stop;
            detailCanvas.AddChild(_detailLayer);

            var dim = new ColorRect { Color = new Color(0, 0, 0, 0.35f) };
            dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            dim.GuiInput += e =>
            {
                if (e is InputEventMouseButton mb && mb.Pressed)
                {
                    CloseDetail();
                    _detailLayer.AcceptEvent();
                }
            };
            _detailLayer.AddChild(dim);

            var center = new CenterContainer();
            center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            center.MouseFilter = MouseFilterEnum.Ignore;
            _detailLayer.AddChild(center);

            var card = new PanelContainer { CustomMinimumSize = new Vector2(520, 0) };
            card.AddThemeStyleboxOverride("panel", MenuTheme.Panel(border: true));
            center.AddChild(card);

            var box = new VBoxContainer();
            box.AddThemeConstantOverride("separation", 12);
            card.AddChild(box);

            _detailTitle = MenuTheme.MakeLabel("", 22);
            _detailTitle.AddThemeColorOverride("font_color", MenuTheme.Accent);
            box.AddChild(_detailTitle);

            _detailBody = MenuTheme.MakeLabel("", 18, false);
            _detailBody.AddThemeColorOverride("font_color", MenuTheme.TextNormal);
            _detailBody.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            _detailBody.CustomMinimumSize = new Vector2(480, 0);
            box.AddChild(_detailBody);

            var close = MenuOverlay.MakeMenuButton("menu.back", 22);
            close.CustomMinimumSize = new Vector2(180, 40);
            close.Pressed += CloseDetail;
            box.AddChild(close);
        }

        public override void _ExitTree()
        {
            SettingsHint.Focused -= OnHintFocused;
            SettingsHint.Cleared -= OnHintCleared;
            SettingsHint.DetailRequested -= OpenDetail;
        }

        private void OnHintFocused(string hint, string detail)
        {
            _clearHint = false;
            _pendingHint = hint;
            _pendingDetail = detail;
            _hintTimer.Stop();
            _hintTimer.WaitTime = 0.4;
            _hintTimer.Start();
        }

        private void OnHintCleared()
        {
            _clearHint = true;
            _hintTimer.Stop();
            _hintTimer.WaitTime = 0.12;
            _hintTimer.Start();
        }

        private void FlushHint()
        {
            if (_clearHint)
                ShowHint("", "");
            else
                ShowHint(_pendingHint, _pendingDetail);
        }

        private void ShowHint(string hintKey, string detailKey)
        {
            if (!Localization.Has(hintKey))
            {
                _hintLabel.Text = "";
                return;
            }

            string text = Localization.T(hintKey);
            if (!string.IsNullOrEmpty(detailKey))
                text += "   [? / F1]";
            _hintLabel.Text = text;
        }

        private void OpenDetail(string detailKey)
        {
            if (!Localization.Has(detailKey)) return;
            string id = detailKey.StartsWith("info.") ? detailKey.Substring(5) : detailKey;
            string titleKey = "set." + id;
            _detailTitle.Text = Localization.Has(titleKey)
                ? Localization.T(titleKey).ToUpper()
                : Localization.T(detailKey).ToUpper();
            _detailBody.Text = Localization.T(detailKey);
            _detailLayer.Visible = true;
        }

        private void CloseDetail() => _detailLayer.Visible = false;

        // ── Actions ─────────────────────────────────────────────────────────

        private void OnAccept()
        {
            AudioManager.Instance?.PlaySelect();
            SettingsManager.Instance.ApplyFromPending(_pending);
            GoMainMenu();
        }

        private void OnBack()
        {
            AudioManager.Instance?.PlaySelect();
            GoMainMenu();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event == null || !@event.IsActionPressed("ui_cancel"))
                return;

            if (_detailLayer != null && _detailLayer.Visible)
            {
                CloseDetail();
                GetViewport()?.SetInputAsHandled();
                return;
            }

            GetViewport()?.SetInputAsHandled();
            // Godot 4.4+: change_scene_to_file removes this node from the tree
            // immediately (get_tree / get_viewport become null). Defer so we
            // don't run ChangeSceneToFile in the middle of input dispatch.
            // https://docs.godotengine.org/en/4.4/classes/class_scenetree.html
            CallDeferred(MethodName.OnBack);
        }

        private void GoMainMenu()
        {
            var tree = GetTree();
            if (tree == null) return;
            tree.CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/main_menu/MainMenu.tscn");
        }

        private void AnimateFadeIn()
        {
            Modulate = new Color(1, 1, 1, 0);
            CreateTween().TweenProperty(this, "modulate:a", 1.0f, 0.3f);
        }
    }
}
