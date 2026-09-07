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
            "val.off_sub", "РУССКИЙ", "ENGLISH", "DEUTSCH", "FRANÇAIS", "ESPAÑOL", "ITALIANO", "日本語", "中文",
        };
        private static readonly string[] FontLabels = { "val.standard_font", "val.large_font", "val.accessibility_font" };

        protected override void Build()
        {
            int ui = IndexOf(LocaleIds, Pending.UiLanguage, 0);
            int voice = IndexOf(LocaleIds, Pending.VoiceLanguage, 0);
            int sub = IndexOf(SubtitleIds, Pending.SubtitleLanguage, 1);

            var rows = new List<Control>
            {
                Cycle("set.ui_language", LocaleLabels, ui, i => Pending.UiLanguage = LocaleIds[i]),
                Cycle("set.voice_language", LocaleLabels, voice, i => Pending.VoiceLanguage = LocaleIds[i]),
                Cycle("set.subtitle_language", SubtitleLabels, sub, i => Pending.SubtitleLanguage = SubtitleIds[i]),
                Cycle("set.font", FontLabels, (int)Pending.FontSize, i => Pending.FontSize = (FontSize)i),
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
