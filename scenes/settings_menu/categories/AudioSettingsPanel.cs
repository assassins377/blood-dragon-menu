using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    public partial class AudioSettingsPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            var rows = new List<Control>
            {
                Percent("set.master_volume", Pending.MasterVolume, v => Pending.MasterVolume = v),
                Percent("set.music_volume", Pending.MusicVolume, v => Pending.MusicVolume = v),
                Percent("set.sfx_volume", Pending.SfxVolume, v => Pending.SfxVolume = v),
                Percent("set.dialogue_volume", Pending.DialogueVolume, v => Pending.DialogueVolume = v),
                Percent("set.ambient_volume", Pending.AmbientVolume, v => Pending.AmbientVolume = v),
                Cycle("set.dynamic_range", new[] { "val.low_dr", "val.medium_dr", "val.high_dr" },
                    (int)Pending.DynamicRange, i => Pending.DynamicRange = (DynamicRange)i),
                Cycle("set.audio_output", new[] { "val.stereo", "val.surround51", "val.surround71" },
                    (int)Pending.AudioOutput, i => Pending.AudioOutput = (AudioOutput)i),
                BoolCycle("set.menu_voice", Pending.MenuVoice, v => Pending.MenuVoice = v),
            };
            Scroll(rows);
        }
    }
}
