using Godot;

namespace BloodDragon
{
    public partial class CalibrationSettingsPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            var brightness = new SliderRow("ЯРКОСТЬ", 0, 100, 1, Pending.Brightness * 100, "F0", "%");
            brightness.ValueChanged += v => Pending.Brightness = (float)(v / 100.0);

            var contrast = new SliderRow("КОНТРАСТНОСТЬ", 0, 100, 1, Pending.Contrast * 100, "F0", "%");
            contrast.ValueChanged += v => Pending.Contrast = (float)(v / 100.0);

            var gamma = new SliderRow("GAMMA", 1.8, 3.0, 0.05, Pending.Gamma, "F2");
            gamma.ValueChanged += v => Pending.Gamma = (float)v;

            Scroll(new Control[] { brightness, contrast, gamma });
        }
    }
}
