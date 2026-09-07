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
                Cycle("set.overall_quality", Quality, (int)Pending.OverallQuality, i => Pending.OverallQuality = (QualityLevel)i),
                Cycle("set.texture_quality", Quality, (int)Pending.TextureQuality, i => Pending.TextureQuality = (QualityLevel)i),
                BoolCycle("set.shadows", Pending.Shadows, v => Pending.Shadows = v),
                Cycle("set.shadow_quality", Quality, (int)Pending.ShadowQuality, i => Pending.ShadowQuality = (QualityLevel)i),
                Cycle("set.lighting", Quality, (int)Pending.Lighting, i => Pending.Lighting = (QualityLevel)i),
                BoolCycle("set.post_processing", Pending.PostProcessing, v => Pending.PostProcessing = v),
                Cycle("set.water_quality", Quality, (int)Pending.WaterQuality, i => Pending.WaterQuality = (QualityLevel)i),
                Cycle("set.draw_distance", new[] { "val.low_f", "val.medium_f", "val.high_f" },
                    (int)Pending.DrawDistance, i => Pending.DrawDistance = (DrawDistance)i),
            };
            Scroll(rows);
        }
    }
}
