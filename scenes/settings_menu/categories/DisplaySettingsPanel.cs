using Godot;
using System;
using System.Collections.Generic;

namespace BloodDragon
{
    public partial class DisplaySettingsPanel : BaseSettingsPanel
    {
        public static readonly Vector2I[] Resolutions =
        {
            new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440), new(3840, 2160),
        };

        private CycleOption _vsyncRow;

        protected override void Build()
        {
            int resIndex = Array.IndexOf(Resolutions, Pending.Resolution);
            if (resIndex < 0) resIndex = 1;
            // Labels derived from the resolution list so the two can never drift apart.
            var resLabels = Array.ConvertAll(Resolutions, r => $"{r.X}×{r.Y}");

            _vsyncRow = BoolCycle("set.vsync", Pending.VSync, v => Pending.VSync = v);

            var fov = new SliderRow("set.fov", 60, 120, 0.05, Pending.Fov, "F2") { LocalizedLabel = true };
            fov.ValueChanged += v => Pending.Fov = (float)v;

            var rows = new List<Control>
            {
                Cycle("set.resolution", resLabels, resIndex,
                    i => Pending.Resolution = Resolutions[i]),
                Cycle("set.window_mode", new[] { "val.windowed", "val.fullscreen" },
                    Pending.Fullscreen ? 1 : 0, i => Pending.Fullscreen = i == 1),
                _vsyncRow,
                Cycle("set.fps_limit", new[] { "30", "60", "90", "120", "144", "240" },
                    Pending.FpsLimit switch { 30 => 0, 90 => 2, 120 => 3, 144 => 4, 240 => 5, _ => 1 },
                    i => Pending.FpsLimit = i switch { 0 => 30, 2 => 90, 3 => 120, 4 => 144, 5 => 240, _ => 60 }),
                Cycle("set.gpu_frames", new[] { "1", "2", "3", "4" },
                    Mathf.Clamp(Pending.GpuFramesInFlight - 1, 0, 3), i => Pending.GpuFramesInFlight = i + 1),
                BoolCycle("set.letterbox", Pending.Letterbox, v => Pending.Letterbox = v),
                ApiRow(),
                Cycle("set.msaa", new[] { "val.off_f", "2", "4", "8" },
                    Pending.Msaa switch { 2 => 1, 4 => 2, 8 => 3, _ => 0 },
                    i => Pending.Msaa = i switch { 1 => 2, 2 => 4, 3 => 8, _ => 0 }),
                Cycle("set.a2c", new[] { "val.off_f", "val.standard", "val.extended" },
                    (int)Pending.AlphaToCoverage, i => Pending.AlphaToCoverage = (AlphaToCoverage)i),
                Cycle("set.ssao", new[] { "val.off_f", "val.ssao", "val.hbao" },
                    (int)Pending.Ssao, i => Pending.Ssao = (SsaoMethod)i),
                fov,
            };

            Scroll(rows);
            _vsyncRow.SetDisabled(Pending.DirectX == DirectXVersion.DirectX9);
        }

        private CycleOption ApiRow()
        {
            if (OS.GetName() == "Linux")
            {
                var row = Cycle("set.api", new[] { "val.opengl" }, 0, _ => { });
                row.SetDisabled(true);
                return row;
            }

            return Cycle("set.directx", new[] { "val.dx9", "val.dx11", "val.dx12" },
                (int)Pending.DirectX, i =>
                {
                    Pending.DirectX = (DirectXVersion)i;
                    _vsyncRow.SetDisabled(i == (int)DirectXVersion.DirectX9);
                });
        }
    }
}
