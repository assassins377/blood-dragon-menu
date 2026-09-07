using Godot;

namespace BloodDragon
{
    public partial class CalibrationSettingsPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            var brightness = new SliderRow("set.brightness", 0, 100, 1, Pending.Brightness * 100, "F0", "%") { LocalizedLabel = true };
            brightness.ValueChanged += v => Pending.Brightness = (float)(v / 100.0);

            var contrast = new SliderRow("set.contrast", 0, 100, 1, Pending.Contrast * 100, "F0", "%") { LocalizedLabel = true };
            contrast.ValueChanged += v => Pending.Contrast = (float)(v / 100.0);

            var gamma = new SliderRow("set.gamma", 1.8, 3.0, 0.05, Pending.Gamma, "F2") { LocalizedLabel = true };
            gamma.ValueChanged += v => Pending.Gamma = (float)v;

            Scroll(new Control[] { brightness, contrast, gamma });
        }
    }
}
