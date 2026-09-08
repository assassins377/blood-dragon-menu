using Godot;
using System.Collections.Generic;

namespace BloodDragon
{
    /// <summary>
    /// МОНИТОРИНГ: in-game performance overlay toggle + detail level.
    /// The overlay itself lives in the StatsOverlay autoload.
    /// </summary>
    public partial class PerformanceStatsPanel : BaseSettingsPanel
    {
        private CycleOption _modeRow;

        protected override void Build()
        {
            _modeRow = Cycle("set.stats_mode", new[] { "val.stats_compact", "val.stats_full" },
                (int)Pending.StatsMode == 2 ? 1 : 0, i => Pending.StatsMode = i == 1 ? StatsMode.Full : StatsMode.Compact);

            var rows = new List<Control>
            {
                BoolCycle("set.show_stats", Pending.ShowStats, v =>
                {
                    Pending.ShowStats = v;
                    _modeRow.SetDisabled(!v);
                }),
                _modeRow,
            };

            _modeRow.SetDisabled(!Pending.ShowStats);
            Scroll(rows);
        }
    }
}