using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    /// <summary>
    /// Attract screen ("press any key") then the left-side menu.
    /// Campaign opens a submenu (new / continue / load). Returning from
    /// settings skips the attract screen.
    /// </summary>
    public partial class MainMenu : Control
    {
        private const string Title = "menu.title";

        /// <summary>Once the attract screen is dismissed, stay dismissed for this session.</summary>
        private static bool AttractDone;

        private Label _cursor;
        private VBoxContainer _list;
        private Control _attract;
        private enum Page { Root, Campaign }
        private Page _page = Page.Root;

        public override void _Ready()
        {
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var bg = new ColorRect { Color = MenuTheme.Background };
            bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            bg.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(bg);

            BuildTitle();
            BuildList();
            MenuOverlay.AddCrt(this);
            AudioManager.Instance?.StartMenuMusic();

            if (AttractDone)
                ShowRoot(animate: false);
            else
                ShowAttract();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_attract == null || !_attract.Visible)
            {
                if (_page == Page.Campaign && @event.IsActionPressed("ui_cancel"))
                {
                    AudioManager.Instance?.PlaySelect();
                    ShowRoot(animate: true);
                    GetViewport().SetInputAsHandled();
                }
                return;
            }

            bool go = @event is InputEventKey k && k.Pressed && !k.Echo
                   || @event is InputEventMouseButton mb && mb.Pressed
                   || @event is InputEventJoypadButton jb && jb.Pressed
                   || @event.IsActionPressed("ui_accept");
            if (!go) return;

            AttractDone = true;
            AudioManager.Instance?.PlaySelect();
            HideAttract();
            ShowRoot(animate: true);
            GetViewport().SetInputAsHandled();
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

            var timer = new Timer { WaitTime = 0.5, Autostart = true };
            timer.Timeout += () => _cursor.Visible = !_cursor.Visible;
            AddChild(timer);
        }

        private void BuildList()
        {
            _list = new VBoxContainer();
            _list.Position = new Vector2(80, 260);
            _list.AddThemeConstantOverride("separation", 14);
            _list.CustomMinimumSize = new Vector2(500, 0);
            _list.Visible = false;
            AddChild(_list);
        }

        private void ShowAttract()
        {
            _list.Visible = false;
            _attract = new Control();
            _attract.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            _attract.MouseFilter = MouseFilterEnum.Stop;
            AddChild(_attract);

            var prompt = MenuTheme.MakeLabel("menu.press_any_key", 28);
            prompt.HorizontalAlignment = HorizontalAlignment.Center;
            prompt.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
            prompt.OffsetTop = 80;
            prompt.AddThemeColorOverride("font_color", MenuTheme.Accent);
            _attract.AddChild(prompt);

            var blink = new Timer { WaitTime = 0.7, Autostart = true };
            blink.Timeout += () => prompt.Visible = !prompt.Visible;
            _attract.AddChild(blink);
        }

        private void HideAttract()
        {
            _attract?.QueueFree();
            _attract = null;
        }

        private void ShowRoot(bool animate)
        {
            _page = Page.Root;
            FillList(new (string Label, System.Action OnPress)[]
            {
                ("menu.campaign", OnCampaignMenu),
                ("menu.settings", OnSettings),
                ("menu.quit", () => { AudioManager.Instance?.PlaySelect(); GetTree().Quit(); }),
            }, animate);
        }

        private void OnCampaignMenu()
        {
            AudioManager.Instance?.PlaySelect();
            ShowCampaign();
        }

        private void ShowCampaign()
        {
            _page = Page.Campaign;
            FillList(new (string Label, System.Action OnPress)[]
            {
                ("menu.new_game", StartGame),
                ("menu.continue", StartGame),
                ("menu.load_game", StartGame),
                ("menu.back", () => { AudioManager.Instance?.PlaySelect(); ShowRoot(animate: true); }),
            }, animate: true);
        }

        private void StartGame()
        {
            AudioManager.Instance?.PlaySelect();
            AudioManager.Instance?.StopMenuMusic();
            GetTree().ChangeSceneToFile("res://scenes/game/Game.tscn");
        }

        private void OnSettings()
        {
            AudioManager.Instance?.PlaySelect();
            AttractDone = true;
            GetTree().ChangeSceneToFile("res://scenes/settings_menu/SettingsMenu.tscn");
        }

        private void FillList((string Label, System.Action OnPress)[] items, bool animate)
        {
            // QueueFree: buttons stay locked while their Pressed callback is running.
            // Free() in a while(GetChildCount) loop fails and never removes the node,
            // so the engine logs the same error until the disk fills up.
            foreach (Node child in _list.GetChildren())
            {
                _list.RemoveChild(child);
                child.QueueFree();
            }

            _list.Visible = true;
            var buttons = new List<Button>();
            foreach (var item in items)
            {
                var b = MenuOverlay.MakeMenuButton(item.Label);
                b.CustomMinimumSize = new Vector2(500, 52);
                var press = item.OnPress;
                b.Pressed += press;
                _list.AddChild(b);
                buttons.Add(b);
            }

            buttons[0].GrabFocus();
            if (animate)
                AnimateButtons(buttons.ToArray());
        }

        private void AnimateButtons(Button[] buttons)
        {
            var tween = CreateTween().SetParallel();
            float delay = 0f;
            foreach (var b in buttons)
            {
                b.OffsetTransformEnabled = true;
                b.OffsetTransformVisualOnly = true;
                b.OffsetTransformPosition = new Vector2(-72, 0);
                tween.TweenProperty(b, "offset_transform_position", Vector2.Zero, 0.28f)
                    .SetDelay(delay)
                    .SetTrans(Tween.TransitionType.Sine)
                    .SetEase(Tween.EaseType.Out);
                delay += 0.05f;
            }
        }
    }
}
