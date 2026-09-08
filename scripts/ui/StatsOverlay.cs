using Godot;

namespace BloodDragon
{
    /// <summary>
    /// In-game overlay: FPS, frametime, draw calls, memory, nodes.
    /// Compact = one line; Full = four lines. Lives above the CRT layer,
    /// top-right so it never overlaps the game hint (top-left).
    /// Refreshes at most 5 Hz while visible and does no work while hidden.
    /// </summary>
    public partial class StatsOverlay : CanvasLayer
    {
        private const float RefreshInterval = 0.2f; // 5 Hz: enough for human reading
        private const float Margin = 24f;

        private Label _label;
        private float _frameMs;
        private float _refreshAccum;
        private float _sizeCheckAccum;
        private bool _wasShown;

        public override void _Ready()
        {
            // CRT is layer 100; stats must sit above it or scanlines eat the text.
            Layer = 110;
            ProcessMode = ProcessModeEnum.Always;
            _label = new Label
            {
                MouseFilter = Control.MouseFilterEnum.Ignore,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            _label.AddThemeFontOverride("font", MenuTheme.Mono);
            _label.AddThemeFontSizeOverride("font_size", 16);
            _label.AddThemeColorOverride("font_color", MenuTheme.TextActive);
            _label.AddThemeColorOverride("font_outline_color", new Color(1, 1, 1, 1));
            _label.AddThemeConstantOverride("outline_size", 1);
            AddChild(_label);

            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Applied += OnSettingsApplied;

            Refresh();
        }

        public override void _ExitTree()
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Applied -= OnSettingsApplied;
        }

        public override void _Process(double delta)
        {
            // Re-anchor cheaply at most once per second; window size rarely changes.
            _sizeCheckAccum += (float)delta;
            if (_sizeCheckAccum >= 1f)
            {
                _sizeCheckAccum = 0f;
                SetLabelAnchored();
            }

            var s = SettingsManager.Instance?.Current;
            bool show = s != null && s.ShowStats;

            if (!show)
            {
                if (_wasShown)
                {
                    _wasShown = false;
                    Visible = false;
                    _label.Text = "";
                }
                return;
            }

            if (delta > 0)
            {
                float sample = (float)(delta * 1000.0);
                _frameMs = _frameMs <= 0 ? sample : Mathf.Lerp(_frameMs, sample, 0.2f);
            }

            // Throttled text rebuild: avoids per-frame string allocation and
            // canvas rebatching while the numbers barely change.
            _refreshAccum += (float)delta;
            if (_refreshAccum < RefreshInterval)
                return;
            _refreshAccum = 0f;

            Refresh();
        }

        private void OnSettingsApplied() => Refresh();

        // Top-right anchored: the label grows from the right edge so multiline
        // text stays clear of the game hint at the top-left.
        private void SetLabelAnchored()
        {
            var vp = GetViewport();
            float width = vp?.GetVisibleRect().Size.X ?? 1920;
            _label.AnchorLeft = 1f;
            _label.AnchorRight = 1f;
            _label.OffsetLeft = -width + Margin;
            _label.OffsetRight = -Margin;
            _label.OffsetTop = Margin;
        }

        public void OnViewportSizeChanged() => SetLabelAnchored();

        private void Refresh()
        {
            var s = SettingsManager.Instance?.Current;
            bool show = s != null && s.ShowStats;
            _wasShown = show;
            Visible = show;
            if (!show)
            {
                _label.Text = "";
                return;
            }

            double fps = Engine.GetFramesPerSecond();
            if (fps <= 0)
                fps = Performance.GetMonitor(Performance.Monitor.TimeFps);

            ulong draws = Info(RenderingServer.RenderingInfo.TotalDrawCallsInFrame);
            ulong objects = Info(RenderingServer.RenderingInfo.TotalObjectsInFrame);
            ulong prims = Info(RenderingServer.RenderingInfo.TotalPrimitivesInFrame);

            if (s.StatsMode != StatsMode.Full)
            {
                _label.Text = $"FPS {fps:0} | {_frameMs:0.0} {T("stats.ms")} | {T("stats.draw")} {draws}";
                return;
            }

            _label.Text =
                $"FPS {fps:0} | {_frameMs:0.0} {T("stats.ms")}\n" +
                $"{T("stats.draw")} {draws} | {T("stats.objects")} {objects} | {T("stats.prims")} {prims}\n" +
                $"{T("stats.video_mem")} {Mem(RenderingServer.RenderingInfo.VideoMemUsed)} | " +
                $"{T("stats.buffer_mem")} {Mem(RenderingServer.RenderingInfo.BufferMemUsed)} | " +
                $"{T("stats.texture_mem")} {Mem(RenderingServer.RenderingInfo.TextureMemUsed)}\n" +
                $"{T("stats.nodes")} {Performance.GetMonitor(Performance.Monitor.ObjectNodeCount):0} | " +
                $"{T("stats.orphans")} {Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount):0} | " +
                $"{T("stats.shader_compiles")} {Compiles()}";
        }

        private static string T(string key) => Localization.T(key);

        private static ulong Info(RenderingServer.RenderingInfo key)
            => RenderingServer.GetRenderingInfo(key);

        private static string Mem(RenderingServer.RenderingInfo key)
        {
            ulong v = RenderingServer.GetRenderingInfo(key);
            return v > 0 ? $"{v / 1048576.0:0.0} MB" : T("stats.na");
        }

        private static string Compiles()
        {
            ulong n =
                Info(RenderingServer.RenderingInfo.PipelineCompilationsCanvas) +
                Info(RenderingServer.RenderingInfo.PipelineCompilationsMesh) +
                Info(RenderingServer.RenderingInfo.PipelineCompilationsSurface) +
                Info(RenderingServer.RenderingInfo.PipelineCompilationsDraw) +
                Info(RenderingServer.RenderingInfo.PipelineCompilationsSpecialization);
            return $"{n}";
        }
    }
}
