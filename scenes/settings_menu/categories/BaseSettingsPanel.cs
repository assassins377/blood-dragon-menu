using Godot;
using System;
using System.Collections.Generic;

namespace BloodDragon
{
    /// <summary>
    /// Base for every settings category panel. Each concrete panel is its own
    /// scene (.tscn) with this-derived script; it builds its rows in
    /// <see cref="Build"/> against the shared pending-settings buffer.
    /// </summary>
    public abstract partial class BaseSettingsPanel : Control
    {
        protected static readonly string[] OnOff = { "ВКЛ", "ВЫКЛ" };
        protected static readonly string[] Quality = { "НИЗКОЕ", "СРЕДНЕЕ", "ВЫСОКОЕ" };

        protected GameSettings Pending;
        private bool _built;

        /// <summary>Called by the settings menu to hand over the pending buffer.</summary>
        public void Setup(GameSettings pending)
        {
            Pending = pending;
            TryBuild();
        }

        public override void _Ready() => TryBuild();

        private void TryBuild()
        {
            if (_built || Pending == null) return;
            _built = true;
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            Build();
        }

        protected abstract void Build();

        // ── Shared row factories ────────────────────────────────────────────

        protected ScrollContainer Scroll(IEnumerable<Control> rows)
        {
            var scroll = new ScrollContainer();
            scroll.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            vbox.AddThemeConstantOverride("separation", 4);
            scroll.AddChild(vbox);
            foreach (var r in rows)
            {
                r.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                vbox.AddChild(r);
            }
            AddChild(scroll);
            return scroll;
        }

        protected CycleOption Cycle(string label, string[] opts, int index, Action<int> onChange)
        {
            var row = new CycleOption(label, opts, index);
            row.ValueChanged += idx => onChange(idx);
            return row;
        }

        protected CycleOption BoolCycle(string label, bool value, Action<bool> onChange)
            => Cycle(label, OnOff, value ? 0 : 1, idx => onChange(idx == 0));

        protected SliderRow Percent(string label, float value, Action<float> onChange)
        {
            var s = new SliderRow(label, 0, 100, 1, value * 100, "F0", "%");
            s.ValueChanged += v => onChange((float)(v / 100.0));
            return s;
        }

        protected RichTextLabel RichPage(string bbcode)
        {
            var rt = new RichTextLabel
            {
                BbcodeEnabled = true,
                FitContent = true,
                ScrollActive = true,
                Text = bbcode,
            };
            rt.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            rt.AddThemeFontOverride("normal_font", MenuTheme.Mono);
            rt.AddThemeFontOverride("bold_font", MenuTheme.Mono);
            rt.AddThemeColorOverride("default_color", MenuTheme.TextNormal);
            AddChild(rt);
            return rt;
        }
    }
}
