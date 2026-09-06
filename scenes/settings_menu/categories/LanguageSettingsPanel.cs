using Godot;
using System;
using System.Collections.Generic;

namespace BloodDragon
{
    public partial class LanguageSettingsPanel : BaseSettingsPanel
    {
        private static readonly string[] LocaleIds = { "ru", "en", "de", "fr", "es", "it", "ja", "zh" };
        private static readonly string[] LocaleLabels =
        {
            "РУССКИЙ", "ENGLISH", "DEUTSCH", "FRANÇAIS", "ESPAÑOL", "ITALIANO", "日本語", "中文",
        };
        private static readonly string[] SubtitleIds = { "off", "ru", "en", "de", "fr", "es", "it", "ja", "zh" };
        private static readonly string[] SubtitleLabels =
        {
            "ВЫКЛ", "РУССКИЙ", "ENGLISH", "DEUTSCH", "FRANÇAIS", "ESPAÑOL", "ITALIANO", "日本語", "中文",
        };
        private static readonly string[] FontLabels = { "СТАНДАРТНЫЙ", "КРУПНЫЙ", "ДОСТУПНЫЙ" };

        protected override void Build()
        {
            int ui = IndexOf(LocaleIds, Pending.UiLanguage, 0);
            int voice = IndexOf(LocaleIds, Pending.VoiceLanguage, 0);
            int sub = IndexOf(SubtitleIds, Pending.SubtitleLanguage, 1);

            var rows = new List<Control>
            {
                Cycle("ЯЗЫК ИНТЕРФЕЙСА", LocaleLabels, ui, i => Pending.UiLanguage = LocaleIds[i]),
                Cycle("ЯЗЫК ОЗВУЧКИ", LocaleLabels, voice, i => Pending.VoiceLanguage = LocaleIds[i]),
                Cycle("ЯЗЫК СУБТИТРОВ", SubtitleLabels, sub, i => Pending.SubtitleLanguage = SubtitleIds[i]),
                Cycle("ШРИФТ", FontLabels, (int)Pending.FontSize, i => Pending.FontSize = (FontSize)i),
            };
            Scroll(rows);
        }

        private static int IndexOf(string[] ids, string value, int fallback)
        {
            int i = Array.IndexOf(ids, value);
            return i >= 0 ? i : fallback;
        }
    }
}
