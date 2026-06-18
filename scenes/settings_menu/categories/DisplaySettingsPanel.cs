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

            _vsyncRow = BoolCycle("ВЕРТ. СИНХРОНИЗАЦИЯ", Pending.VSync, v => Pending.VSync = v);

            var fov = new SliderRow("ПОЛЕ ЗРЕНИЯ", 60, 120, 0.05, Pending.Fov, "F2");
            fov.ValueChanged += v => Pending.Fov = (float)v;

            var rows = new List<Control>
            {
                Cycle("РАЗРЕШЕНИЕ ЭКРАНА",
                    new[] { "1280×720", "1600×900", "1920×1080", "2560×1440", "3840×2160" },
                    resIndex, i => Pending.Resolution = Resolutions[i]),
                Cycle("ОКОННЫЙ РЕЖИМ", new[] { "В ОКНЕ", "ПОЛНОЭКРАННЫЙ" },
                    Pending.Fullscreen ? 1 : 0, i => Pending.Fullscreen = i == 1),
                _vsyncRow,
                Cycle("ФРЕЙМОВ В БУФЕРЕ GPU", new[] { "1", "2", "3", "4" },
                    Mathf.Clamp(Pending.GpuFramesInFlight - 1, 0, 3), i => Pending.GpuFramesInFlight = i + 1),
                BoolCycle("FORMAT LETTERBOX", Pending.Letterbox, v => Pending.Letterbox = v),
                Cycle("DIRECTX", new[] { "DIRECTX 9", "DIRECTX 11", "DIRECTX 12" },
                    (int)Pending.DirectX, i =>
                    {
                        Pending.DirectX = (DirectXVersion)i;
                        _vsyncRow.SetDisabled(i == (int)DirectXVersion.DirectX9);
                    }),
                Cycle("MSAA СГЛАЖИВАНИЕ", new[] { "ВЫКЛ", "2", "4", "8" },
                    Pending.Msaa switch { 2 => 1, 4 => 2, 8 => 3, _ => 0 },
                    i => Pending.Msaa = i switch { 1 => 2, 2 => 4, 3 => 8, _ => 0 }),
                Cycle("ALPHA TO COVERAGE", new[] { "ВЫКЛ", "СТАНДАРТНОЕ", "РАСШИРЕННОЕ" },
                    (int)Pending.AlphaToCoverage, i => Pending.AlphaToCoverage = (AlphaToCoverage)i),
                Cycle("МЕТОД SSAO", new[] { "ВЫКЛ", "SSAO", "HBAO+" },
                    (int)Pending.Ssao, i => Pending.Ssao = (SsaoMethod)i),
                fov,
            };

            Scroll(rows);
            _vsyncRow.SetDisabled(Pending.DirectX == DirectXVersion.DirectX9);
        }
    }
}
