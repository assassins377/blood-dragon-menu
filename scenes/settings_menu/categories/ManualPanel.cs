namespace BloodDragon
{
    public partial class ManualPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            RichPage(
                $"[b]{Localization.T("manual.controls")}[/b]\n"
              + Localization.T("manual.controls_body") + "\n\n"
              + $"[b]{Localization.T("manual.mechanics")}[/b]\n"
              + Localization.T("manual.mechanics_body") + "\n\n"
              + $"[b]{Localization.T("manual.tips")}[/b]\n"
              + Localization.T("manual.tips_body"));
        }
    }
}
