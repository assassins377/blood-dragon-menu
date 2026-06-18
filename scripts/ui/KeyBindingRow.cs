using Godot;

namespace BloodDragon
{
    /// <summary>
    /// A key-rebind row:  ACTION NAME        [ CURRENT KEY ]
    /// Click / Enter to enter capture mode, then press any key/mouse button.
    /// </summary>
    public partial class KeyBindingRow : Control
    {
        [Signal] public delegate void RebindEventHandler(string action, string key);

        private readonly string _action;
        private readonly string _display;
        private string _key;

        private Label _name;
        private Button _keyButton;
        private bool _capturing;

        public KeyBindingRow(string action, string displayName, string key)
        {
            _action = action;
            _display = displayName;
            _key = key;
        }

        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(0, 44);

            var hbox = new HBoxContainer();
            hbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            hbox.OffsetLeft = 16;
            hbox.OffsetRight = -16;
            hbox.AddThemeConstantOverride("separation", 16);
            AddChild(hbox);

            _name = MenuTheme.MakeLabel(_display);
            _name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(_name);

            _keyButton = new Button
            {
                Text = _key,
                CustomMinimumSize = new Vector2(180, 0),
            };
            _keyButton.AddThemeFontOverride("font", MenuTheme.Mono);
            _keyButton.Pressed += BeginCapture;
            hbox.AddChild(_keyButton);
        }

        private void BeginCapture()
        {
            _capturing = true;
            _keyButton.Text = "...";
        }

        public override void _Input(InputEvent @event)
        {
            if (!_capturing) return;

            string captured = null;
            if (@event is InputEventKey k && k.Pressed && !k.Echo)
                captured = OS.GetKeycodeString(k.PhysicalKeycode != Key.None ? k.PhysicalKeycode : k.Keycode).ToUpper();
            else if (@event is InputEventMouseButton mb && mb.Pressed)
                captured = mb.ButtonIndex switch
                {
                    MouseButton.Left => "MOUSE_LEFT",
                    MouseButton.Right => "MOUSE_RIGHT",
                    MouseButton.Middle => "MOUSE_MIDDLE",
                    _ => null,
                };

            if (captured == null) return;

            _capturing = false;
            _key = captured;
            _keyButton.Text = _key;
            EmitSignal(SignalName.Rebind, _action, _key);
            AcceptEvent();
        }
    }
}
