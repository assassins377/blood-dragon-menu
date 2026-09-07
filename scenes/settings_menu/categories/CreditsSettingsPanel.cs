namespace BloodDragon
{
    public partial class CreditsSettingsPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            RichPage(
                $"[b]{Localization.T("credits.title")}[/b]\n"
              + Localization.T("credits.version") + "\n"
              + Localization.T("credits.developer") + "\n"
              + Localization.T("credits.engine") + "\n"
              + Localization.T("credits.year") + "\n\n"
              + $"[b]{Localization.T("credits.licenses")}[/b]\n"
              + Localization.T("credits.licenses_body") + "\n\n"
              + $"[b]{Localization.T("credits.contacts")}[/b]\n"
              + Localization.T("credits.contacts_body"));
        }
    }
}
