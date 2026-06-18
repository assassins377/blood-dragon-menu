using Godot;
using System;

namespace BloodDragon
{
    /// <summary>
    /// Autoload singleton. Holds the live <see cref="GameSettings"/>, persists it to
    /// user://settings.cfg and applies it to the engine (display / audio / language).
    /// Register in Project Settings → Autoload as "SettingsManager".
    /// </summary>
    public partial class SettingsManager : Node
    {
        public static SettingsManager Instance { get; private set; }
        public GameSettings Current { get; private set; } = new();

        /// <summary>Raised after settings are applied — listeners (e.g. the in-game
        /// graphics controller) refresh themselves from <see cref="Current"/>.</summary>
        public event Action Applied;

        private const string SavePath = "user://settings.cfg";

        public override void _Ready()
        {
            Instance = this;
            Load();
        }

        // ──────────────────────────────────────────────────────────────────
        //  Pending → Current
        // ──────────────────────────────────────────────────────────────────

        /// <summary>Commit a pending copy (from the settings menu), apply and save.</summary>
        public void ApplyFromPending(GameSettings pending)
        {
            Current.CopyFrom(pending);
            ApplyAll();
            Save();
        }

        // ──────────────────────────────────────────────────────────────────
        //  Persistence
        // ──────────────────────────────────────────────────────────────────

        public void Save()
        {
            var cfg = new ConfigFile();

            cfg.SetValue("display", "resolution_x", Current.Resolution.X);
            cfg.SetValue("display", "resolution_y", Current.Resolution.Y);
            cfg.SetValue("display", "fullscreen", Current.Fullscreen);
            cfg.SetValue("display", "vsync", Current.VSync);
            cfg.SetValue("display", "gpu_frames_in_flight", Current.GpuFramesInFlight);
            cfg.SetValue("display", "letterbox", Current.Letterbox);
            cfg.SetValue("display", "directx", (int)Current.DirectX);
            cfg.SetValue("display", "msaa", Current.Msaa);
            cfg.SetValue("display", "alpha_to_coverage", (int)Current.AlphaToCoverage);
            cfg.SetValue("display", "ssao", (int)Current.Ssao);
            cfg.SetValue("display", "fov", Current.Fov);

            cfg.SetValue("calibration", "brightness", Current.Brightness);
            cfg.SetValue("calibration", "contrast", Current.Contrast);
            cfg.SetValue("calibration", "gamma", Current.Gamma);

            cfg.SetValue("video_quality", "overall_quality", (int)Current.OverallQuality);
            cfg.SetValue("video_quality", "texture_quality", (int)Current.TextureQuality);
            cfg.SetValue("video_quality", "shadows", Current.Shadows);
            cfg.SetValue("video_quality", "shadow_quality", (int)Current.ShadowQuality);
            cfg.SetValue("video_quality", "lighting", (int)Current.Lighting);
            cfg.SetValue("video_quality", "post_processing", Current.PostProcessing);
            cfg.SetValue("video_quality", "water_quality", (int)Current.WaterQuality);
            cfg.SetValue("video_quality", "draw_distance", (int)Current.DrawDistance);

            cfg.SetValue("controls", "mouse_sensitivity", Current.MouseSensitivity);
            cfg.SetValue("controls", "invert_y", Current.InvertY);
            cfg.SetValue("controls", "controller_sensitivity", Current.ControllerSensitivity);
            foreach (var kv in Current.KeyBindings)
                cfg.SetValue("controls", "key_" + kv.Key, kv.Value);

            cfg.SetValue("gameplay", "difficulty", (int)Current.Difficulty);
            cfg.SetValue("gameplay", "hints", Current.Hints);
            cfg.SetValue("gameplay", "auto_aim", Current.AutoAim);

            cfg.SetValue("audio", "master_volume", Current.MasterVolume);
            cfg.SetValue("audio", "music_volume", Current.MusicVolume);
            cfg.SetValue("audio", "sfx_volume", Current.SfxVolume);
            cfg.SetValue("audio", "dialogue_volume", Current.DialogueVolume);
            cfg.SetValue("audio", "ambient_volume", Current.AmbientVolume);
            cfg.SetValue("audio", "dynamic_range", (int)Current.DynamicRange);
            cfg.SetValue("audio", "audio_output", (int)Current.AudioOutput);
            cfg.SetValue("audio", "menu_voice", Current.MenuVoice);

            cfg.SetValue("language", "ui_language", Current.UiLanguage);
            cfg.SetValue("language", "voice_language", Current.VoiceLanguage);
            cfg.SetValue("language", "subtitle_language", Current.SubtitleLanguage);
            cfg.SetValue("language", "font", (int)Current.FontSize);

            cfg.Save(SavePath);
        }

        public void Load()
        {
            var cfg = new ConfigFile();
            if (cfg.Load(SavePath) != Error.Ok)
            {
                // No file yet — apply defaults so the window matches the saved model.
                ApplyAll();
                return;
            }

            Current.Resolution = new Vector2I(
                (int)cfg.GetValue("display", "resolution_x", 1600),
                (int)cfg.GetValue("display", "resolution_y", 900));
            Current.Fullscreen = (bool)cfg.GetValue("display", "fullscreen", false);
            Current.VSync = (bool)cfg.GetValue("display", "vsync", false);
            Current.GpuFramesInFlight = (int)cfg.GetValue("display", "gpu_frames_in_flight", 4);
            Current.Letterbox = (bool)cfg.GetValue("display", "letterbox", true);
            Current.DirectX = (DirectXVersion)(int)cfg.GetValue("display", "directx", (int)DirectXVersion.DirectX11);
            Current.Msaa = (int)cfg.GetValue("display", "msaa", 8);
            Current.AlphaToCoverage = (AlphaToCoverage)(int)cfg.GetValue("display", "alpha_to_coverage", (int)AlphaToCoverage.Extended);
            Current.Ssao = (SsaoMethod)(int)cfg.GetValue("display", "ssao", (int)SsaoMethod.Ssao);
            Current.Fov = (float)cfg.GetValue("display", "fov", 73.15f);

            Current.Brightness = (float)cfg.GetValue("calibration", "brightness", 0.5f);
            Current.Contrast = (float)cfg.GetValue("calibration", "contrast", 1.0f);
            Current.Gamma = (float)cfg.GetValue("calibration", "gamma", 2.2f);

            Current.OverallQuality = (QualityLevel)(int)cfg.GetValue("video_quality", "overall_quality", (int)QualityLevel.High);
            Current.TextureQuality = (QualityLevel)(int)cfg.GetValue("video_quality", "texture_quality", (int)QualityLevel.High);
            Current.Shadows = (bool)cfg.GetValue("video_quality", "shadows", true);
            Current.ShadowQuality = (QualityLevel)(int)cfg.GetValue("video_quality", "shadow_quality", (int)QualityLevel.High);
            Current.Lighting = (QualityLevel)(int)cfg.GetValue("video_quality", "lighting", (int)QualityLevel.High);
            Current.PostProcessing = (bool)cfg.GetValue("video_quality", "post_processing", true);
            Current.WaterQuality = (QualityLevel)(int)cfg.GetValue("video_quality", "water_quality", (int)QualityLevel.Medium);
            Current.DrawDistance = (DrawDistance)(int)cfg.GetValue("video_quality", "draw_distance", (int)DrawDistance.Medium);

            Current.MouseSensitivity = (float)cfg.GetValue("controls", "mouse_sensitivity", 0.5f);
            Current.InvertY = (bool)cfg.GetValue("controls", "invert_y", false);
            Current.ControllerSensitivity = (float)cfg.GetValue("controls", "controller_sensitivity", 0.5f);
            foreach (var action in new System.Collections.Generic.List<string>(Current.KeyBindings.Keys))
                Current.KeyBindings[action] = (string)cfg.GetValue("controls", "key_" + action, Current.KeyBindings[action]);

            Current.Difficulty = (DifficultyLevel)(int)cfg.GetValue("gameplay", "difficulty", (int)DifficultyLevel.Normal);
            Current.Hints = (bool)cfg.GetValue("gameplay", "hints", true);
            Current.AutoAim = (bool)cfg.GetValue("gameplay", "auto_aim", false);

            Current.MasterVolume = (float)cfg.GetValue("audio", "master_volume", 0.8f);
            Current.MusicVolume = (float)cfg.GetValue("audio", "music_volume", 0.7f);
            Current.SfxVolume = (float)cfg.GetValue("audio", "sfx_volume", 0.9f);
            Current.DialogueVolume = (float)cfg.GetValue("audio", "dialogue_volume", 0.85f);
            Current.AmbientVolume = (float)cfg.GetValue("audio", "ambient_volume", 0.75f);
            Current.DynamicRange = (DynamicRange)(int)cfg.GetValue("audio", "dynamic_range", (int)DynamicRange.Medium);
            Current.AudioOutput = (AudioOutput)(int)cfg.GetValue("audio", "audio_output", (int)AudioOutput.Stereo);
            Current.MenuVoice = (bool)cfg.GetValue("audio", "menu_voice", true);

            Current.UiLanguage = (string)cfg.GetValue("language", "ui_language", "ru");
            Current.VoiceLanguage = (string)cfg.GetValue("language", "voice_language", "ru");
            Current.SubtitleLanguage = (string)cfg.GetValue("language", "subtitle_language", "ru");
            Current.FontSize = (FontSize)(int)cfg.GetValue("language", "font", (int)FontSize.Standard);

            ApplyAll();
        }

        // ──────────────────────────────────────────────────────────────────
        //  Apply to engine
        // ──────────────────────────────────────────────────────────────────

        public void ApplyAll()
        {
            ApplyDisplay();
            ApplyAudio();
            ApplyLanguage();
            ApplyControls();
            Applied?.Invoke();
        }

        /// <summary>Rebuild the InputMap actions from the saved key bindings.</summary>
        public void ApplyControls()
        {
            foreach (var kv in Current.KeyBindings)
            {
                if (!InputMap.HasAction(kv.Key))
                    InputMap.AddAction(kv.Key);
                InputMap.ActionEraseEvents(kv.Key);
                var ev = KeyStringToEvent(kv.Value);
                if (ev != null)
                    InputMap.ActionAddEvent(kv.Key, ev);
            }
        }

        private static InputEvent KeyStringToEvent(string s)
        {
            switch (s)
            {
                case "MOUSE_LEFT":   return new InputEventMouseButton { ButtonIndex = MouseButton.Left };
                case "MOUSE_RIGHT":  return new InputEventMouseButton { ButtonIndex = MouseButton.Right };
                case "MOUSE_MIDDLE": return new InputEventMouseButton { ButtonIndex = MouseButton.Middle };
            }

            Key key = s switch
            {
                "SPACE"  => Key.Space,
                "CTRL"   => Key.Ctrl,
                "SHIFT"  => Key.Shift,
                "ALT"    => Key.Alt,
                "ESCAPE" => Key.Escape,
                "ENTER"  => Key.Enter,
                "TAB"    => Key.Tab,
                _        => (Key)OS.FindKeycodeFromString(s),
            };
            if (key == Key.None) return null;
            return new InputEventKey { PhysicalKeycode = key, Keycode = key };
        }

        public void ApplyDisplay()
        {
            DisplayServer.WindowSetMode(Current.Fullscreen
                ? DisplayServer.WindowMode.Fullscreen
                : DisplayServer.WindowMode.Windowed);

            // Only resize in windowed mode; fullscreen drives the resolution itself.
            if (!Current.Fullscreen)
                DisplayServer.WindowSetSize(Current.Resolution);

            DisplayServer.WindowSetVsyncMode(Current.VSync
                ? DisplayServer.VSyncMode.Enabled
                : DisplayServer.VSyncMode.Disabled);

            var vp = (SceneTree)Engine.GetMainLoop() is { } tree ? tree.Root : null;
            if (vp != null)
                vp.Msaa3D = Current.Msaa switch
                {
                    2 => Viewport.Msaa.Msaa2X,
                    4 => Viewport.Msaa.Msaa4X,
                    8 => Viewport.Msaa.Msaa8X,
                    _ => Viewport.Msaa.Disabled,
                };
        }

        public void ApplyAudio()
        {
            SetBus("Master", Current.MasterVolume);
            SetBus("Music", Current.MusicVolume);
            SetBus("SFX", Current.SfxVolume);
            SetBus("Dialogue", Current.DialogueVolume);
            SetBus("Ambient", Current.AmbientVolume);
        }

        private static void SetBus(string name, float linear)
        {
            int idx = AudioServer.GetBusIndex(name);
            if (idx >= 0)
                AudioServer.SetBusVolumeDb(idx, Mathf.LinearToDb(Mathf.Max(linear, 0.0001f)));
        }

        public void ApplyLanguage()
        {
            TranslationServer.SetLocale(Current.UiLanguage);
        }
    }
}
