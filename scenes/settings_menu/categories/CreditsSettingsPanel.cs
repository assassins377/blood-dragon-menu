namespace BloodDragon
{
    public partial class CreditsSettingsPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            RichPage(
                "[b]BLOOD DRAGON[/b]\n"
              + "Версия: v1.0.0\n"
              + "Разработчик: [Имя студии]\n"
              + "Движок: Godot 4.6 .NET\n"
              + "Год выпуска: 2026\n\n"
              + "[b]ЛИЦЕНЗИИ[/b]\nGodot Engine (MIT)\n\n"
              + "[b]КОНТАКТЫ[/b]\nСайт • email • соцсети");
        }
    }
}
