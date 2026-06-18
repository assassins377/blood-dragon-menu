using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    /// <summary>
    /// Plain data model holding every configurable game setting.
    /// Defaults follow game_menu_specification.md.
    /// </summary>
    public class GameSettings
    {
        // ── Display (ИЗОБРАЖЕНИЕ) ──────────────────────────────────────────
        public Vector2I Resolution { get; set; } = new(1600, 900);
        public bool Fullscreen { get; set; } = false;
        public bool VSync { get; set; } = false;
        public int GpuFramesInFlight { get; set; } = 4;
        public bool Letterbox { get; set; } = true;
        public DirectXVersion DirectX { get; set; } = DirectXVersion.DirectX11;
        public int Msaa { get; set; } = 8;                          // 0 / 2 / 4 / 8
        public AlphaToCoverage AlphaToCoverage { get; set; } = AlphaToCoverage.Extended;
        public SsaoMethod Ssao { get; set; } = SsaoMethod.Ssao;
        public float Fov { get; set; } = 73.15f;

        // ── Calibration (КАЛИБРОВКА) ──────────────────────────────────────
        public float Brightness { get; set; } = 0.5f;
        public float Contrast { get; set; } = 1.0f;
        public float Gamma { get; set; } = 2.2f;

        // ── Video quality (КАЧЕСТВО ВИДЕО) ────────────────────────────────
        public QualityLevel OverallQuality { get; set; } = QualityLevel.High;
        public QualityLevel TextureQuality { get; set; } = QualityLevel.High;
        public bool Shadows { get; set; } = true;
        public QualityLevel ShadowQuality { get; set; } = QualityLevel.High;
        public QualityLevel Lighting { get; set; } = QualityLevel.High;
        public bool PostProcessing { get; set; } = true;
        public QualityLevel WaterQuality { get; set; } = QualityLevel.Medium;
        public DrawDistance DrawDistance { get; set; } = DrawDistance.Medium;

        // ── Controls (ЭЛЕМЕНТЫ УПРАВЛЕНИЯ) ─────────────────────────────────
        public float MouseSensitivity { get; set; } = 0.5f;
        public bool InvertY { get; set; } = false;
        public float ControllerSensitivity { get; set; } = 0.5f;
        public Dictionary<string, string> KeyBindings { get; set; } = new()
        {
            ["move_forward"]  = "W",
            ["move_backward"] = "S",
            ["move_left"]     = "A",
            ["move_right"]    = "D",
            ["jump"]          = "SPACE",
            ["crouch"]        = "CTRL",
            ["sprint"]        = "SHIFT",
            ["shoot"]         = "MOUSE_LEFT",
            ["aim"]           = "MOUSE_RIGHT",
            ["reload"]        = "R",
            ["interact"]      = "E",
            ["inventory"]     = "I",
            ["map"]           = "M",
            ["pause"]         = "ESCAPE",
        };

        // ── Gameplay (ИГРОВОЙ ПРОЦЕСС) ─────────────────────────────────────
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;
        public bool Hints { get; set; } = true;
        public bool AutoAim { get; set; } = false;

        // ── Audio (ЗВУК) ──────────────────────────────────────────────────
        public float MasterVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.7f;
        public float SfxVolume { get; set; } = 0.9f;
        public float DialogueVolume { get; set; } = 0.85f;
        public float AmbientVolume { get; set; } = 0.75f;
        public DynamicRange DynamicRange { get; set; } = DynamicRange.Medium;
        public AudioOutput AudioOutput { get; set; } = AudioOutput.Stereo;
        public bool MenuVoice { get; set; } = true;

        // ── Language (ЯЗЫК) ───────────────────────────────────────────────
        public string UiLanguage { get; set; } = "ru";
        public string VoiceLanguage { get; set; } = "ru";
        public string SubtitleLanguage { get; set; } = "ru";
        public FontSize FontSize { get; set; } = FontSize.Standard;

        /// <summary>
        /// Deep copy — used both for the settings menu's "pending changes" buffer and
        /// to commit those changes back. <see cref="MemberwiseClone"/> copies every field
        /// automatically, so new settings never need to be added to a hand-written copy.
        /// </summary>
        public GameSettings Clone()
        {
            var c = (GameSettings)MemberwiseClone();
            c.KeyBindings = new Dictionary<string, string>(KeyBindings);
            return c;
        }
    }
}
