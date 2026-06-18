using Godot;
using System;

namespace BloodDragon
{
    /// <summary>
    /// A settings row of the form:  LABEL : VALUE      [====slider====]
    /// The whole row shows the green focus fill while the slider is focused.
    /// </summary>
    public partial class SliderRow : Control
    {
        [Signal] public delegate void ValueChangedEventHandler(double value);

        private Label _name;
        private HSlider _slider;
        private ColorRect _bg;

        private readonly string _labelText;
        private readonly string _suffix;     // e.g. "%" or ""
        private readonly string _format;     // e.g. "F0", "F2"
        private readonly double _min, _max, _step, _initial;

        public SliderRow(string label, double min, double max, double step, double initial,
                         string format = "F0", string suffix = "")
        {
            _labelText = label;
            _min = min; _max = max; _step = step; _initial = initial;
            _format = format; _suffix = suffix;
        }

        public double Value => _slider?.Value ?? _initial;

        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(0, 46);
            MouseFilter = MouseFilterEnum.Pass;

            _bg = new ColorRect { Color = MenuTheme.Transparent };
            _bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            _bg.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(_bg);

            var hbox = new HBoxContainer();
            hbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            hbox.OffsetLeft = 16;
            hbox.OffsetRight = -16;
            hbox.AddThemeConstantOverride("separation", 16);
            AddChild(hbox);

            _name = MenuTheme.MakeLabel(_labelText);
            _name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(_name);

            _slider = new HSlider
            {
                MinValue = _min,
                MaxValue = _max,
                Step = _step,
                Value = _initial,
                CustomMinimumSize = new Vector2(220, 0),
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            hbox.AddChild(_slider);

            _slider.FocusEntered += UpdateVisuals;
            _slider.FocusExited += UpdateVisuals;
            _slider.ValueChanged += v =>
            {
                UpdateLabel();
                EmitSignal(SignalName.ValueChanged, v);
            };

            UpdateLabel();
            UpdateVisuals();
        }

        private void UpdateLabel()
        {
            string v = _slider.Value.ToString(_format, System.Globalization.CultureInfo.InvariantCulture);
            _name.Text = $"{_labelText} : {v}{_suffix}".ToUpper();
        }

        private void UpdateVisuals()
        {
            bool focused = _slider.HasFocus();
            _bg.Color = focused ? MenuTheme.Accent : MenuTheme.Transparent;
            _name.AddThemeColorOverride("font_color", focused ? MenuTheme.TextActive : MenuTheme.TextNormal);
        }
    }
}
