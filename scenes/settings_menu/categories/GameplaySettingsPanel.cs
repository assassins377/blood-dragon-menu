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
                Cycle("set.difficulty", new[] { "val.easy", "val.normal", "val.hard", "val.nightmare" },
                    (int)Pending.Difficulty, i => Pending.Difficulty = (DifficultyLevel)i),
                BoolCycle("set.hints", Pending.Hints, v => Pending.Hints = v),
                BoolCycle("set.auto_aim", Pending.AutoAim, v => Pending.AutoAim = v),
            };
            Scroll(rows);
        }
    }
}
