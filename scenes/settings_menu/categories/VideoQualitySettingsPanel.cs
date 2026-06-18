using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    public partial class VideoQualitySettingsPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            var rows = new List<Control>
            {
                Cycle("ОБЩЕЕ КАЧЕСТВО", Quality, (int)Pending.OverallQuality, i => Pending.OverallQuality = (QualityLevel)i),
                Cycle("КАЧЕСТВО ТЕКСТУР", Quality, (int)Pending.TextureQuality, i => Pending.TextureQuality = (QualityLevel)i),
                BoolCycle("ТЕНИ", Pending.Shadows, v => Pending.Shadows = v),
                Cycle("КАЧЕСТВО ТЕНЕЙ", Quality, (int)Pending.ShadowQuality, i => Pending.ShadowQuality = (QualityLevel)i),
                Cycle("ОСВЕЩЕНИЕ", Quality, (int)Pending.Lighting, i => Pending.Lighting = (QualityLevel)i),
                BoolCycle("ПОСТ-ОБРАБОТКА", Pending.PostProcessing, v => Pending.PostProcessing = v),
                Cycle("КАЧЕСТВО ВОДЫ", Quality, (int)Pending.WaterQuality, i => Pending.WaterQuality = (QualityLevel)i),
                Cycle("ДАЛЬНОСТЬ ПРОРИСОВКИ", new[] { "НИЗКАЯ", "СРЕДНЯЯ", "ВЫСОКАЯ" },
                    (int)Pending.DrawDistance, i => Pending.DrawDistance = (DrawDistance)i),
            };
            Scroll(rows);
        }
    }
}
