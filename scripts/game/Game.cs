using Godot;
using System.Linq;

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
        private CanvasLayer _calibrationLayer;
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
            ApplyAlphaToCoverage();
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.Applied += ApplyGraphics;
                SettingsManager.Instance.Applied += ApplyAlphaToCoverage;
            }
        }

        public override void _ExitTree()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.Applied -= ApplyGraphics;
                SettingsManager.Instance.Applied -= ApplyAlphaToCoverage;
            }
        }

        private void ApplyGraphics()
            => GraphicsController.Apply(_worldEnv, _sun, _player?.Camera, GetViewport(),
                                        _calibration, _calibrationLayer, SettingsManager.Instance.Current);

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
                    MaterialOverride = MatteMaterial(new Color(0.16f, 0.30f, 0.19f)),
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

        /// <summary>Creates a flat non-emissive material so arena blocks stay matte.</summary>
        private static StandardMaterial3D MatteMaterial(Color albedo)
            => new() { AlbedoColor = albedo };

        /// <summary>Applies the ALPHA TO COVERAGE setting to every arena material.</summary>
        private void ApplyAlphaToCoverage()
        {
            var s = SettingsManager.Instance?.Current;
            if (s == null) return;

            var mode = s.AlphaToCoverage switch
            {
                AlphaToCoverage.Standard => BaseMaterial3D.AlphaAntiAliasing.AlphaToCoverage,
                AlphaToCoverage.Extended => BaseMaterial3D.AlphaAntiAliasing.AlphaToCoverageAndToOne,
                _ => BaseMaterial3D.AlphaAntiAliasing.Off,
            };

            foreach (var mi in GetChildren().OfType<MeshInstance3D>())
                if (mi.MaterialOverride is StandardMaterial3D mat)
                    mat.AlphaAntialiasingMode = mode;
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
            cross.AddThemeColorOverride("font_color", new Color(0.85f, 1f, 0.85f));
            cross.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            layer.AddChild(cross);

            var hint = MenuTheme.MakeLabel("game.hint", 18);
            hint.AddThemeColorOverride("font_color", new Color(0.92f, 0.96f, 0.90f));
            hint.Position = new Vector2(40, 30);
            layer.AddChild(hint);
        }

        private void BuildCalibration()
        {
            var shader = GD.Load<Shader>("res://shaders/calibration.gdshader");
            if (shader == null) return;

            _calibration = new ShaderMaterial { Shader = shader };
            _calibrationLayer = new CanvasLayer { Layer = 80 };
            var rect = new ColorRect { Color = new Color(1, 1, 1, 1), Material = _calibration };
            rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            rect.MouseFilter = Control.MouseFilterEnum.Ignore;
            _calibrationLayer.AddChild(rect);
            AddChild(_calibrationLayer);
        }

        private void BuildPauseMenu()
        {
            _pauseLayer = new CanvasLayer { Layer = 50, Visible = false };
            _pauseLayer.ProcessMode = ProcessModeEnum.Always;
            AddChild(_pauseLayer);

            var dim = new ColorRect { Color = new Color(0, 0, 0, 0.45f) };
            dim.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            _pauseLayer.AddChild(dim);

            var center = new CenterContainer();
            center.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            center.MouseFilter = Control.MouseFilterEnum.Ignore;
            _pauseLayer.AddChild(center);

            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 16);
            vbox.Alignment = BoxContainer.AlignmentMode.Center;
            center.AddChild(vbox);

            var title = MenuTheme.MakeLabel("menu.pause", 40);
            title.AddThemeColorOverride("font_color", new Color(0.92f, 0.96f, 0.90f));
            title.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(title);

            var resume = MenuOverlay.MakeMenuButton("menu.resume", 26);
            resume.CustomMinimumSize = new Vector2(420, 50);
            StylePauseButton(resume);
            resume.Pressed += () => { AudioManager.Instance?.PlaySelect(); SetPaused(false); };
            vbox.AddChild(resume);

            var toMenu = MenuOverlay.MakeMenuButton("menu.to_main_menu", 26);
            toMenu.CustomMinimumSize = new Vector2(420, 50);
            StylePauseButton(toMenu);
            toMenu.Pressed += OnQuitToMenu;
            vbox.AddChild(toMenu);
        }

        private static void StylePauseButton(Button b)
        {
            var idle = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0) };
            idle.SetContentMarginAll(10);
            idle.ContentMarginLeft = 18;
            var focus = new StyleBoxFlat { BgColor = new Color(1, 1, 1, 0.08f) };
            focus.SetContentMarginAll(10);
            focus.ContentMarginLeft = 18;
            focus.BorderColor = new Color(0.85f, 1f, 0.85f);
            focus.BorderWidthLeft = 3;
            b.AddThemeStyleboxOverride("normal", idle);
            b.AddThemeStyleboxOverride("hover", focus);
            b.AddThemeStyleboxOverride("pressed", focus);
            b.AddThemeStyleboxOverride("focus", focus);
            var on = new Color(0.92f, 0.96f, 0.90f);
            b.AddThemeColorOverride("font_color", on);
            b.AddThemeColorOverride("font_hover_color", on);
            b.AddThemeColorOverride("font_focus_color", on);
            b.AddThemeColorOverride("font_pressed_color", on);
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
