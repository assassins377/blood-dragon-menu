using Godot;

namespace BloodDragon
{
    /// <summary>
    /// Minimal playable "campaign" scene: a small 3D arena with an FPS player,
    /// driven by the live graphics/control settings, plus an Esc pause menu.
    /// The world is built in code so it needs no hand-authored 3D scene.
    /// </summary>
    public partial class Game : Node3D
    {
        private WorldEnvironment _worldEnv;
        private DirectionalLight3D _sun;
        private Player _player;
        private CanvasLayer _pauseLayer;
        private ShaderMaterial _calibration;
        private bool _paused;

        public override void _Ready()
        {
            BuildEnvironment();
            BuildArena();
            BuildPlayer();
            BuildHud();
            BuildCalibration();
            BuildPauseMenu();

            ApplyGraphics();
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Applied += ApplyGraphics;
        }

        public override void _ExitTree()
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Applied -= ApplyGraphics;
        }

        private void ApplyGraphics()
            => GraphicsController.Apply(_worldEnv, _sun, _player?.Camera, GetViewport(),
                                        _calibration, SettingsManager.Instance.Current);

        // ── World ────────────────────────────────────────────────────────

        private void BuildEnvironment()
        {
            var env = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Sky,
                Sky = new Sky { SkyMaterial = new ProceduralSkyMaterial() },
                TonemapMode = Godot.Environment.ToneMapper.Filmic,
                GlowEnabled = true,
                FogEnabled = true,
                FogDensity = 0.01f,
                FogLightColor = new Color(0.02f, 0.12f, 0.05f),
            };
            _worldEnv = new WorldEnvironment { Environment = env };
            AddChild(_worldEnv);

            _sun = new DirectionalLight3D
            {
                RotationDegrees = new Vector3(-55, -50, 0),
                ShadowEnabled = true,
                LightColor = new Color(0.8f, 1.0f, 0.85f),
            };
            AddChild(_sun);
        }

        private void BuildArena()
        {
            // Floor (infinite collision plane + visible green grid mesh).
            var floorBody = new StaticBody3D();
            floorBody.AddChild(new CollisionShape3D { Shape = new WorldBoundaryShape3D() });
            var floorMesh = new MeshInstance3D
            {
                Mesh = new PlaneMesh { Size = new Vector2(120, 120) },
                MaterialOverride = SolidMaterial(new Color(0.03f, 0.10f, 0.05f), 0.0f),
            };
            floorBody.AddChild(floorMesh);
            AddChild(floorBody);

            // Scatter some neon-green blocks to look at / collide with.
            var rng = new RandomNumberGenerator { Seed = 1337 };
            for (int i = 0; i < 24; i++)
            {
                float h = rng.RandfRange(1.5f, 6f);
                var box = new StaticBody3D
                {
                    Position = new Vector3(rng.RandfRange(-40, 40), h / 2f, rng.RandfRange(-40, 40)),
                };
                var shape = new BoxShape3D { Size = new Vector3(3, h, 3) };
                box.AddChild(new CollisionShape3D { Shape = shape });
                box.AddChild(new MeshInstance3D
                {
                    Mesh = new BoxMesh { Size = shape.Size },
                    MaterialOverride = SolidMaterial(new Color(0.05f, 0.15f, 0.07f), 0.25f),
                });
                AddChild(box);
            }
        }

        private static StandardMaterial3D SolidMaterial(Color albedo, float emission)
        {
            var m = new StandardMaterial3D { AlbedoColor = albedo };
            if (emission > 0)
            {
                m.EmissionEnabled = true;
                m.Emission = MenuTheme.Accent;
                m.EmissionEnergyMultiplier = emission;
            }
            return m;
        }

        private void BuildPlayer()
        {
            _player = new Player { Position = new Vector3(0, 2, 8) };
            AddChild(_player);
        }

        // ── UI ───────────────────────────────────────────────────────────

        private void BuildHud()
        {
            var layer = new CanvasLayer();
            AddChild(layer);

            var cross = MenuTheme.MakeLabel("+", 26, false);
            cross.AddThemeColorOverride("font_color", MenuTheme.Accent);
            cross.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            layer.AddChild(cross);

            var hint = MenuTheme.MakeLabel("WASD — движение   SHIFT — бег   ПРОБЕЛ — прыжок   ESC — пауза", 18);
            hint.Position = new Vector2(40, 30);
            layer.AddChild(hint);
        }

        private void BuildCalibration()
        {
            var shader = GD.Load<Shader>("res://shaders/calibration.gdshader");
            if (shader == null) return;

            _calibration = new ShaderMaterial { Shader = shader };
            var layer = new CanvasLayer { Layer = 80 };
            var rect = new ColorRect { Color = new Color(1, 1, 1, 1), Material = _calibration };
            rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            rect.MouseFilter = Control.MouseFilterEnum.Ignore;
            layer.AddChild(rect);
            AddChild(layer);
        }

        private void BuildPauseMenu()
        {
            _pauseLayer = new CanvasLayer { Layer = 50, Visible = false };
            _pauseLayer.ProcessMode = ProcessModeEnum.Always;
            AddChild(_pauseLayer);

            var dim = new ColorRect { Color = new Color(0, 0, 0, 0.7f) };
            dim.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            _pauseLayer.AddChild(dim);

            var vbox = new VBoxContainer();
            vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            vbox.AddThemeConstantOverride("separation", 16);
            _pauseLayer.AddChild(vbox);

            var title = MenuTheme.MakeLabel("ПАУЗА", 40);
            title.AddThemeColorOverride("font_color", MenuTheme.Accent);
            title.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(title);

            var resume = MenuOverlay.MakeMenuButton("Продолжить", 26);
            resume.CustomMinimumSize = new Vector2(420, 50);
            resume.Pressed += () => { AudioManager.Instance?.PlaySelect(); SetPaused(false); };
            vbox.AddChild(resume);

            var toMenu = MenuOverlay.MakeMenuButton("В главное меню", 26);
            toMenu.CustomMinimumSize = new Vector2(420, 50);
            toMenu.Pressed += OnQuitToMenu;
            vbox.AddChild(toMenu);
        }

        // ── Pause ──────────────────────────────────────────────────────────

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("pause") || @event.IsActionPressed("ui_cancel"))
            {
                SetPaused(!_paused);
                GetViewport().SetInputAsHandled();
            }
        }

        private void SetPaused(bool paused)
        {
            _paused = paused;
            GetTree().Paused = paused;
            _pauseLayer.Visible = paused;
            Input.MouseMode = paused ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }

        private void OnQuitToMenu()
        {
            AudioManager.Instance?.PlaySelect();
            GetTree().Paused = false;
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().ChangeSceneToFile("res://scenes/main_menu/MainMenu.tscn");
        }
    }
}
