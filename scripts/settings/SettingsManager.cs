using Godot;
using System;
using System.Collections.Generic;

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

        // Single source of truth for (de)serialization: every scalar setting is
        // declared exactly once below (section + key + getter + setter). Save() and
        // Load() are just loops over this registry, so adding a field is one line and
        // can never get out of sync between writing and reading.
        private readonly List<(string Section, string Key, Func<Variant> Get, Action<Variant> Set)> _fields = new();

        public override void _Ready()
        {
            Instance = this;
            BuildFieldRegistry();
            Load();
        }

        // ──────────────────────────────────────────────────────────────────
        //  Field registry
        // ──────────────────────────────────────────────────────────────────

        private void BuildFieldRegistry()
        {
            void F(string sec, string key, Func<float> g, Action<float> set)  => _fields.Add((sec, key, () => g(), v => set(v.AsSingle())));
            void B(string sec, string key, Func<bool> g, Action<bool> set)    => _fields.Add((sec, key, () => g(), v => set(v.AsBool())));
            void I(string sec, string key, Func<int> g, Action<int> set)      => _fields.Add((sec, key, () => g(), v => set(v.AsInt32())));
            void Str(string sec, string key, Func<string> g, Action<string> set) => _fields.Add((sec, key, () => g(), v => set(v.AsString())));
            void E<T>(string sec, string key, Func<T> g, Action<T> set) where T : struct, Enum
                => _fields.Add((sec, key, () => Convert.ToInt32(g()), v => set((T)(object)v.AsInt32())));

            // Display
            I("display", "resolution_x", () => Current.Resolution.X, v => Current.Resolution = new Vector2I(v, Current.Resolution.Y));
            I("display", "resolution_y", () => Current.Resolution.Y, v => Current.Resolution = new Vector2I(Current.Resolution.X, v));
            B("display", "fullscreen", () => Current.Fullscreen, v => Current.Fullscreen = v);
            B("display", "vsync", () => Current.VSync, v => Current.VSync = v);
            I("display", "gpu_frames_in_flight", () => Current.GpuFramesInFlight, v => Current.GpuFramesInFlight = v);
            B("display", "letterbox", () => Current.Letterbox, v => Current.Letterbox = v);
            E("display", "directx", () => Current.DirectX, v => Current.DirectX = v);
            I("display", "msaa", () => Current.Msaa, v => Current.Msaa = v);
            E("display", "alpha_to_coverage", () => Current.AlphaToCoverage, v => Current.AlphaToCoverage = v);
            E("display", "ssao", () => Current.Ssao, v => Current.Ssao = v);
            F("display", "fov", () => Current.Fov, v => Current.Fov = v);

            // Calibration
            F("calibration", "brightness", () => Current.Brightness, v => Current.Brightness = v);
            F("calibration", "contrast", () => Current.Contrast, v => Current.Contrast = v);
            F("calibration", "gamma", () => Current.Gamma, v => Current.Gamma = v);

            // Video quality
            E("video_quality", "overall_quality", () => Current.OverallQuality, v => Current.OverallQuality = v);
            E("video_quality", "texture_quality", () => Current.TextureQuality, v => Current.TextureQuality = v);
            B("video_quality", "shadows", () => Current.Shadows, v => Current.Shadows = v);
            E("video_quality", "shadow_quality", () => Current.ShadowQuality, v => Current.ShadowQuality = v);
            E("video_quality", "lighting", () => Current.Lighting, v => Current.Lighting = v);
            B("video_quality", "post_processing", () => Current.PostProcessing, v => Current.PostProcessing = v);
            E("video_quality", "water_quality", () => Current.WaterQuality, v => Current.WaterQuality = v);
            E("video_quality", "draw_distance", () => Current.DrawDistance, v => Current.DrawDistance = v);

            // Controls (key bindings are handled separately — they are a dictionary)
            F("controls", "mouse_sensitivity", () => Current.MouseSensitivity, v => Current.MouseSensitivity = v);
            B("controls", "invert_y", () => Current.InvertY, v => Current.InvertY = v);
            F("controls", "controller_sensitivity", () => Current.ControllerSensitivity, v => Current.ControllerSensitivity = v);

            // Gameplay
            E("gameplay", "difficulty", () => Current.Difficulty, v => Current.Difficulty = v);
            B("gameplay", "hints", () => Current.Hints, v => Current.Hints = v);
            B("gameplay", "auto_aim", () => Current.AutoAim, v => Current.AutoAim = v);

            // Audio
            F("audio", "master_volume", () => Current.MasterVolume, v => Current.MasterVolume = v);
            F("audio", "music_volume", () => Current.MusicVolume, v => Current.MusicVolume = v);
            F("audio", "sfx_volume", () => Current.SfxVolume, v => Current.SfxVolume = v);
            F("audio", "dialogue_volume", () => Current.DialogueVolume, v => Current.DialogueVolume = v);
            F("audio", "ambient_volume", () => Current.AmbientVolume, v => Current.AmbientVolume = v);
            E("audio", "dynamic_range", () => Current.DynamicRange, v => Current.DynamicRange = v);
            E("audio", "audio_output", () => Current.AudioOutput, v => Current.AudioOutput = v);
            B("audio", "menu_voice", () => Current.MenuVoice, v => Current.MenuVoice = v);

            // Language
            Str("language", "ui_language", () => Current.UiLanguage, v => Current.UiLanguage = v);
            Str("language", "voice_language", () => Current.VoiceLanguage, v => Current.VoiceLanguage = v);
            Str("language", "subtitle_language", () => Current.SubtitleLanguage, v => Current.SubtitleLanguage = v);
            E("language", "font", () => Current.FontSize, v => Current.FontSize = v);
        }

        // ──────────────────────────────────────────────────────────────────
        //  Pending → Current
        // ──────────────────────────────────────────────────────────────────

        /// <summary>Commit a pending copy (from the settings menu), apply and save.</summary>
        public void ApplyFromPending(GameSettings pending)
        {
            // Take an independent copy so the menu's buffer and the live settings
            // don't alias (Clone gives a fresh KeyBindings dictionary too).
            Current = pending.Clone();
            ApplyAll();
            Save();
        }

        // ──────────────────────────────────────────────────────────────────
        //  Persistence
        // ──────────────────────────────────────────────────────────────────

        public void Save()
        {
            var cfg = new ConfigFile();
            foreach (var f in _fields)
                cfg.SetValue(f.Section, f.Key, f.Get());
            foreach (var kv in Current.KeyBindings)
                cfg.SetValue("controls", "key_" + kv.Key, kv.Value);
            cfg.Save(SavePath);
        }

        public void Load()
        {
            var cfg = new ConfigFile();
            if (cfg.Load(SavePath) != Error.Ok)
            {
                // No file yet — apply the model defaults so the engine matches them.
                ApplyAll();
                return;
            }

            // Each field falls back to its current (default) value when the key is absent.
            foreach (var f in _fields)
                f.Set(cfg.GetValue(f.Section, f.Key, f.Get()));

            foreach (var action in new List<string>(Current.KeyBindings.Keys))
                Current.KeyBindings[action] =
                    cfg.GetValue("controls", "key_" + action, Current.KeyBindings[action]).AsString();

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

            var win = (SceneTree)Engine.GetMainLoop() is { } tree ? tree.Root : null;
            if (win != null)
            {
                win.Msaa3D = Current.Msaa switch
                {
                    2 => Viewport.Msaa.Msaa2X,
                    4 => Viewport.Msaa.Msaa4X,
                    8 => Viewport.Msaa.Msaa8X,
                    _ => Viewport.Msaa.Disabled,
                };

                // FORMAT LETTERBOX: keep aspect with black bars, or expand to fill.
                win.ContentScaleAspect = Current.Letterbox
                    ? Window.ContentScaleAspectEnum.Keep
                    : Window.ContentScaleAspectEnum.Expand;
            }
        }

        private AudioEffectCompressor _masterCompressor;

        public void ApplyAudio()
        {
            SetBus("Master", Current.MasterVolume);
            SetBus("Music", Current.MusicVolume);
            SetBus("SFX", Current.SfxVolume);
            SetBus("Dialogue", Current.DialogueVolume);
            SetBus("Ambient", Current.AmbientVolume);
            ApplyDynamicRange();
        }

        /// <summary>ДИНАМИЧЕСКИЙ ДИАПАЗОН → a compressor on the Master bus.
        /// Narrow range = stronger compression (night mode), wide = transparent.</summary>
        private void ApplyDynamicRange()
        {
            int master = AudioServer.GetBusIndex("Master");
            if (master < 0) return;

            if (_masterCompressor == null)
            {
                _masterCompressor = new AudioEffectCompressor();
                AudioServer.AddBusEffect(master, _masterCompressor);
            }

            (_masterCompressor.Ratio, _masterCompressor.Threshold) = Current.DynamicRange switch
            {
                DynamicRange.Low => (8f, -24f),    // heavily compressed
                DynamicRange.Medium => (3f, -16f), // mild
                _ => (1f, 0f),                     // wide → effectively bypassed
            };
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
