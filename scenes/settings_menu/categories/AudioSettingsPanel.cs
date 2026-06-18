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
                Percent("ОБЩАЯ ГРОМКОСТЬ", Pending.MasterVolume, v => Pending.MasterVolume = v),
                Percent("ГРОМКОСТЬ МУЗЫКИ", Pending.MusicVolume, v => Pending.MusicVolume = v),
                Percent("ГРОМКОСТЬ ЭФФЕКТОВ", Pending.SfxVolume, v => Pending.SfxVolume = v),
                Percent("ГРОМКОСТЬ ДИАЛОГОВ", Pending.DialogueVolume, v => Pending.DialogueVolume = v),
                Percent("ГРОМКОСТЬ ОКРУЖЕНИЯ", Pending.AmbientVolume, v => Pending.AmbientVolume = v),
                Cycle("ДИНАМИЧЕСКИЙ ДИАПАЗОН", new[] { "НИЗКИЙ", "СРЕДНИЙ", "ВЫСОКИЙ" },
                    (int)Pending.DynamicRange, i => Pending.DynamicRange = (DynamicRange)i),
                Cycle("РЕЖИМ ВЫВОДА ЗВУКА", new[] { "СТЕРЕО", "5.1", "7.1" },
                    (int)Pending.AudioOutput, i => Pending.AudioOutput = (AudioOutput)i),
                BoolCycle("ОЗВУЧКА МЕНЮ", Pending.MenuVoice, v => Pending.MenuVoice = v),
            };
            Scroll(rows);
        }
    }
}
