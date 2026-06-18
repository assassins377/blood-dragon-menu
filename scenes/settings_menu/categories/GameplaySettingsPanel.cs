using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    public partial class GameplaySettingsPanel : BaseSettingsPanel
    {
        protected override void Build()
        {
            var rows = new List<Control>
            {
                Cycle("СЛОЖНОСТЬ", new[] { "ЛЁГКАЯ", "НОРМАЛЬНАЯ", "СЛОЖНАЯ", "КОШМАР" },
                    (int)Pending.Difficulty, i => Pending.Difficulty = (DifficultyLevel)i),
                BoolCycle("ПОДСКАЗКИ", Pending.Hints, v => Pending.Hints = v),
                BoolCycle("АВТОПРИЦЕЛИВАНИЕ", Pending.AutoAim, v => Pending.AutoAim = v),
            };
            Scroll(rows);
        }
    }
}
