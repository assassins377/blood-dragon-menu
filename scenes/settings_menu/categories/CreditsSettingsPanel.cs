namespace BloodDragon
{
    public partial class CreditsSettingsPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            RichPage(
                "[b]BLOOD DRAGON[/b]\n"
              + "Версия: v1.0.0\n"
              + "Разработчик: open-source\n"
              + "Движок: Godot 4.7 .NET\n"
              + "Год выпуска: 2026\n\n"
              + "[b]ЛИЦЕНЗИИ[/b]\nGodot Engine (MIT)\nЭтот проект (MIT)\n\n"
              + "[b]КОНТАКТЫ[/b]\nРепозиторий на GitHub");
        }
    }
}
