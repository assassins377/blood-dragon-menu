using Godot;
using System;
using System.Collections.Generic;

namespace BloodDragon
{
    /// <summary>
    /// A settings row of the form:  LABEL NAME        ◄  VALUE  ►
    /// Arrows appear only while focused. Green fill on focus, transparent otherwise.
    /// Built fully in code so it needs no scene file.
    /// </summary>
    public partial class CycleOption : Control
    {
        [Signal] public delegate void ValueChangedEventHandler(int index);

        private Label _name, _left, _value, _right;
        private ColorRect _bg;

        private readonly List<string> _options = new();
        private int _index;
        private bool _disabled;
        private string _labelText = "НАСТРОЙКА";

        public CycleOption() { }

        public CycleOption(string label, IEnumerable<string> options, int defaultIndex = 0)
        {
            _labelText = label;
            _options.AddRange(options);
            _index = Mathf.Clamp(defaultIndex, 0, Math.Max(0, _options.Count - 1));
        }

        public int CurrentIndex => _index;
        public string CurrentValue => _options.Count > 0 ? _options[_index] : "";

        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(0, 46);
            FocusMode = FocusModeEnum.All;
            MouseFilter = MouseFilterEnum.Stop;

            _bg = new ColorRect { Color = MenuTheme.Transparent };
            _bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            _bg.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(_bg);

            var hbox = new HBoxContainer();
            hbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            hbox.OffsetLeft = 16;
            hbox.OffsetRight = -16;
            hbox.MouseFilter = MouseFilterEnum.Ignore;
            hbox.AddThemeConstantOverride("separation", 10);
            AddChild(hbox);

            _name = MenuTheme.MakeLabel(_labelText);
            _name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(_name);

            _left = MenuTheme.MakeLabel("◄", 22, false);
            _value = MenuTheme.MakeLabel(CurrentValue, 22, false);
            _value.HorizontalAlignment = HorizontalAlignment.Right;
            _value.CustomMinimumSize = new Vector2(170, 0);
            _right = MenuTheme.MakeLabel("►", 22, false);
            hbox.AddChild(_left);
            hbox.AddChild(_value);
            hbox.AddChild(_right);

            FocusEntered += UpdateVisuals;
            FocusExited += UpdateVisuals;
            UpdateVisuals();
        }

        public void SetOptions(IEnumerable<string> options, int defaultIndex = 0)
        {
            _options.Clear();
            _options.AddRange(options);
            _index = Mathf.Clamp(defaultIndex, 0, Math.Max(0, _options.Count - 1));
            UpdateVisuals();
        }

        public void SetDisabled(bool disabled)
        {
            _disabled = disabled;
            FocusMode = disabled ? FocusModeEnum.None : FocusModeEnum.All;
            UpdateVisuals();
        }

        private void CycleNext()
        {
            if (_disabled || _options.Count == 0) return;
            _index = (_index + 1) % _options.Count;
            UpdateVisuals();
            AudioManager.Instance?.PlayTick();
            EmitSignal(SignalName.ValueChanged, _index);
        }

        private void CyclePrev()
        {
            if (_disabled || _options.Count == 0) return;
            _index = (_index - 1 + _options.Count) % _options.Count;
            UpdateVisuals();
            AudioManager.Instance?.PlayTick();
            EmitSignal(SignalName.ValueChanged, _index);
        }

        private void UpdateVisuals()
        {
            if (_value != null) _value.Text = CurrentValue;
            if (_bg == null) return;

            bool focused = HasFocus() && !_disabled;
            Color text = _disabled ? MenuTheme.TextDisabled
                       : focused   ? MenuTheme.TextActive
                                   : MenuTheme.TextNormal;

            _bg.Color = focused ? MenuTheme.Accent : MenuTheme.Transparent;
            _name.AddThemeColorOverride("font_color", text);
            _value.AddThemeColorOverride("font_color", text);
            _left.AddThemeColorOverride("font_color", text);
            _right.AddThemeColorOverride("font_color", text);
            _left.Visible = focused;
            _right.Visible = focused;
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (_disabled) return;

            if (@event is InputEventMouseButton mb && mb.Pressed)
            {
                GrabFocus();
                if (mb.ButtonIndex == MouseButton.Left) { CycleNext(); AcceptEvent(); }
                else if (mb.ButtonIndex == MouseButton.Right) { CyclePrev(); AcceptEvent(); }
                return;
            }

            if (@event.IsActionPressed("ui_right") || @event.IsActionPressed("ui_accept"))
            {
                CycleNext();
                AcceptEvent();
            }
            else if (@event.IsActionPressed("ui_left"))
            {
                CyclePrev();
                AcceptEvent();
            }
        }
    }
}
