using Godot;

namespace BloodDragon
{
    /// <summary>
    /// Autoload. Plays menu SFX (hover / select / tick) on the SFX bus and the
    /// looping ambient track on the Ambient bus. Menu blips respect the
    /// "ОЗВУЧКА МЕНЮ" (MenuVoice) setting.
    /// </summary>
    public partial class AudioManager : Node
    {
        public static AudioManager Instance { get; private set; }

        private AudioStreamPlayer _hover, _select, _tick, _music;

        public override void _Ready()
        {
            Instance = this;
            ProcessMode = ProcessModeEnum.Always; // keep audio alive while the tree is paused
            _hover  = MakePlayer("res://audio/sfx/menu_hover.wav", "SFX");
            _select = MakePlayer("res://audio/sfx/menu_select.wav", "SFX");
            _tick   = MakePlayer("res://audio/sfx/menu_tick.wav", "SFX");
            _music  = MakePlayer("res://audio/music/menu_ambient.wav", "Ambient");

            // Loop the ambient track by restarting it when it ends.
            _music.Finished += () => { if (_music.Stream != null) _music.Play(); };
        }

        private AudioStreamPlayer MakePlayer(string path, string bus)
        {
            var p = new AudioStreamPlayer
            {
                Stream = GD.Load<AudioStream>(path),
                Bus = AudioServer.GetBusIndex(bus) >= 0 ? bus : "Master",
            };
            AddChild(p);
            return p;
        }

        private static bool MenuVoiceOn =>
            SettingsManager.Instance == null || SettingsManager.Instance.Current.MenuVoice;

        public void PlayHover()  { if (MenuVoiceOn) _hover?.Play(); }
        public void PlaySelect() { if (MenuVoiceOn) _select?.Play(); }
        public void PlayTick()   { if (MenuVoiceOn) _tick?.Play(); }

        public void StartMenuMusic() { if (_music != null && !_music.Playing) _music.Play(); }
        public void StopMenuMusic()  { _music?.Stop(); }
    }
}
