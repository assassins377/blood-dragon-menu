using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    public partial class InputSettingsPanel : BaseSettingsPanel
    {
        private static readonly (string action, string label)[] Bindings =
        {
            ("move_forward", "bind.move_forward"), ("move_backward", "bind.move_backward"),
            ("move_left", "bind.move_left"), ("move_right", "bind.move_right"),
            ("jump", "bind.jump"), ("crouch", "bind.crouch"), ("sprint", "bind.sprint"),
            ("shoot", "bind.shoot"), ("aim", "bind.aim"), ("reload", "bind.reload"),
            ("interact", "bind.interact"), ("inventory", "bind.inventory"),
            ("map", "bind.map"), ("pause", "bind.pause"),
        };

        protected override void Build()
        {
            var rows = new List<Control>
            {
                Percent("set.mouse_sens", Pending.MouseSensitivity, v => Pending.MouseSensitivity = v),
                BoolCycle("set.invert_y", Pending.InvertY, v => Pending.InvertY = v),
                Percent("set.controller_sens", Pending.ControllerSensitivity, v => Pending.ControllerSensitivity = v),
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
