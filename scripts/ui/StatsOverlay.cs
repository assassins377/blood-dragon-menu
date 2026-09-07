using Godot;

namespace BloodDragon
{
    /// <summary>
    /// In-game performance overlay driven by the МОНИТОРИНГ settings.
    /// Compact: FPS + frametime + draw calls on one line.
    /// Full: adds objects, primitives, video/buffer/texture memory,
    /// node/orphan counts and shader pipeline compilations.
    /// Values come from the engine's Performance singleton; anything the
    /// current renderer cannot report is shown as N/A.
    /// </summary>
    public partial class StatsOverlay : CanvasLayer
    {
        private Label _label;
        private Timer _timer;

        public override void _Ready()
        {
            Layer = 90; // Above gameplay and menus, below the CRT overlay (100).
            ProcessMode = ProcessModeEnum.Always;

            _label = new Label
            {
                Position = new Vector2(24, 24),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            _label.AddThemeFontOverride("font", MenuTheme.Mono);
            _label.AddThemeFontSizeOverride("font_size", 16);
            _label.AddThemeColorOverride("font_color", MenuTheme.Accent);
            _label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.8f));
            _label.AddThemeConstantOverride("outline_size", 2);
            AddChild(_label);

            // TIME_FPS is only refreshed once per second by the engine,
            // so updating more often than 2 Hz is pointless.
            _timer = new Timer
            {
                WaitTime = 0.5,
                Autostart = true,
            };
            _timer.Timeout += Refresh;
            AddChild(_timer);

            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Applied += OnSettingsApplied;

            Refresh();
        }

        public override void _ExitTree()
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Applied -= OnSettingsApplied;
        }

        private void OnSettingsApplied() => Refresh();

        private void Refresh()
        {
            var s = SettingsManager.Instance?.Current;
            bool show = s != null && s.ShowStats;
            Visible = show;
            if (!show)
            {
                _label.Text = "";
                return;
            }

            double fps = Performance.GetMonitor(Performance.Monitor.TimeFps);
            double frameMs = Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000.0f;

            if (s.StatsMode == StatsMode.Compact)
            {
                _label.Text = $"FPS {fps:0} | {frameMs:0.0} {T("stats.ms")} | {T("stats.draw")} {Monitor(Performance.Monitor.RenderTotalDrawCallsInFrame)}";
                return;
            }

            _label.Text =
                $"FPS {fps:0} | {frameMs:0.0} {T("stats.ms")}\n" +
                $"{T("stats.draw")} {Monitor(Performance.Monitor.RenderTotalDrawCallsInFrame)} | " +
                $"{T("stats.objects")} {Monitor(Performance.Monitor.RenderTotalObjectsInFrame)} | " +
                $"{T("stats.prims")} {Monitor(Performance.Monitor.RenderTotalPrimitivesInFrame)}\n" +
                $"{T("stats.video_mem")} {Mem(Performance.Monitor.RenderVideoMemUsed)} | " +
                $"{T("stats.buffer_mem")} {Mem(Performance.Monitor.RenderBufferMemUsed)} | " +
                $"{T("stats.texture_mem")} {Mem(Performance.Monitor.RenderTextureMemUsed)}\n" +
                $"{T("stats.nodes")} {Count(Performance.Monitor.ObjectNodeCount)} | " +
                $"{T("stats.orphans")} {Count(Performance.Monitor.ObjectOrphanNodeCount)} | " +
                $"{T("stats.shader_compiles")} {Compiles()}";
        }

        private static string T(string key) => Localization.T(key);

        /// <summary>Node counters: 0 is a real value, always shown as-is.</summary>
        private static string Count(Performance.Monitor m)
            => $"{Performance.GetMonitor(m):0}";

        /// <summary>Render metrics: some renderers report 0 when unsupported — show N/A then.</summary>
        private static string Monitor(Performance.Monitor m)
        {
            double v = Performance.GetMonitor(m);
            return v > 0 ? $"{v:0}" : T("stats.na");
        }

        private static string Mem(Performance.Monitor m)
        {
            double v = Performance.GetMonitor(m);
            return v > 0 ? $"{v / 1048576.0f:0.0} MB" : T("stats.na");
        }

        private static string Compiles()
        {
            double d = Performance.GetMonitor(Performance.Monitor.PipelineCompilationsDraw);
            double c = Performance.GetMonitor(Performance.Monitor.PipelineCompilationsCanvas);
            double s = Performance.GetMonitor(Performance.Monitor.PipelineCompilationsSpecialization);
            return $"{d + c + s:0}";
        }
    }
}