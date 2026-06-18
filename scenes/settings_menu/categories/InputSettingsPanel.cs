using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    public partial class InputSettingsPanel : BaseSettingsPanel
    {
        private static readonly (string action, string label)[] Bindings =
        {
            ("move_forward", "ДВИЖЕНИЕ ВПЕРЁД"), ("move_backward", "ДВИЖЕНИЕ НАЗАД"),
            ("move_left", "ДВИЖЕНИЕ ВЛЕВО"), ("move_right", "ДВИЖЕНИЕ ВПРАВО"),
            ("jump", "ПРЫЖОК"), ("crouch", "ПРИСЕСТЬ"), ("sprint", "БЕГ"),
            ("shoot", "СТРЕЛЬБА"), ("aim", "ПРИЦЕЛИВАНИЕ"), ("reload", "ПЕРЕЗАРЯДКА"),
            ("interact", "ВЗАИМОДЕЙСТВИЕ"), ("inventory", "ИНВЕНТАРЬ"),
            ("map", "КАРТА"), ("pause", "ПАУЗА"),
        };

        protected override void Build()
        {
            var rows = new List<Control>
            {
                Percent("ЧУВСТВИТЕЛЬНОСТЬ МЫШИ", Pending.MouseSensitivity, v => Pending.MouseSensitivity = v),
                BoolCycle("ИНВЕРСИЯ МЫШИ ПО Y", Pending.InvertY, v => Pending.InvertY = v),
                Percent("ЧУВСТВИТЕЛЬНОСТЬ КОНТРОЛЛЕРА", Pending.ControllerSensitivity, v => Pending.ControllerSensitivity = v),
            };

            foreach (var (action, label) in Bindings)
            {
                var row = new KeyBindingRow(action, label, Pending.KeyBindings[action]);
                row.Rebind += (a, key) => Pending.KeyBindings[a] = key;
                rows.Add(row);
            }
            Scroll(rows);
        }
    }
}
