using Godot;

namespace BloodDragon
{
    /// <summary>
    /// Main menu: dark green background, title top-left with a blinking cursor,
    /// vertical button list on the left, CRT overlay, fade-in on enter.
    /// </summary>
    public partial class MainMenu : Control
    {
        private const string Title = "BLOOD DRAGON 1.0 ";
        private Label _cursor;

        public override void _Ready()
        {
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var bg = new ColorRect { Color = MenuTheme.Background };
            bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            bg.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(bg);

            BuildTitle();
            BuildMenu();
            MenuOverlay.AddCrt(this);
            AudioManager.Instance?.StartMenuMusic();

            AnimateFadeIn();
        }

        private void BuildTitle()
        {
            var titleBox = new HBoxContainer();
            titleBox.Position = new Vector2(80, 70);
            titleBox.AddThemeConstantOverride("separation", 0);
            AddChild(titleBox);

            var title = MenuTheme.MakeLabel(Title, 46);
            title.AddThemeColorOverride("font_color", MenuTheme.Accent);
            titleBox.AddChild(title);

            _cursor = MenuTheme.MakeLabel("█", 46, false);
            _cursor.AddThemeColorOverride("font_color", MenuTheme.Accent);
            titleBox.AddChild(_cursor);

            // Blinking cursor: 1s cycle.
            var timer = new Timer { WaitTime = 0.5, Autostart = true };
            timer.Timeout += () => _cursor.Visible = !_cursor.Visible;
            AddChild(timer);
        }

        private void BuildMenu()
        {
            var vbox = new VBoxContainer();
            vbox.Position = new Vector2(80, 260);
            vbox.AddThemeConstantOverride("separation", 14);
            vbox.CustomMinimumSize = new Vector2(500, 0);
            AddChild(vbox);

            var campaign = MenuOverlay.MakeMenuButton("Кампания");
            var settings = MenuOverlay.MakeMenuButton("Справка и параметры");
            var quit = MenuOverlay.MakeMenuButton("Выйти из игры");

            foreach (var b in new[] { campaign, settings, quit })
            {
                b.CustomMinimumSize = new Vector2(500, 52);
                vbox.AddChild(b);
            }

            campaign.Pressed += OnCampaign;
            settings.Pressed += OnSettings;
            quit.Pressed += () => { AudioManager.Instance?.PlaySelect(); GetTree().Quit(); };

            campaign.GrabFocus();
        }

        private void OnCampaign()
        {
            AudioManager.Instance?.PlaySelect();
            AudioManager.Instance?.StopMenuMusic();
            GetTree().ChangeSceneToFile("res://scenes/game/Game.tscn");
        }

        private void OnSettings()
        {
            AudioManager.Instance?.PlaySelect();
            GetTree().ChangeSceneToFile("res://scenes/settings_menu/SettingsMenu.tscn");
        }

        private void AnimateFadeIn()
        {
            Modulate = new Color(1, 1, 1, 0);
            CreateTween().TweenProperty(this, "modulate:a", 1.0f, 0.3f);
        }
    }
}
