using Godot;

namespace BloodDragon
{
    /// <summary>
    /// Pushes the current <see cref="GameSettings"/> into the live 3D scene
    /// (environment, light, camera, viewport, calibration). Called on load and
    /// whenever settings change. Documents which settings have no runtime API.
    /// </summary>
    public static class GraphicsController
    {
        public static void Apply(WorldEnvironment worldEnv, DirectionalLight3D sun, Camera3D cam,
                                 Viewport viewport, ShaderMaterial calibration, GameSettings s)
        {
            if (worldEnv?.Environment is { } env)
            {
                env.GlowEnabled = s.PostProcessing;          // ПОСТ-ОБРАБОТКА
                env.SsaoEnabled = s.Ssao != SsaoMethod.Off;  // МЕТОД SSAO (on/off)
                env.SsilEnabled = s.Lighting == QualityLevel.High; // ОСВЕЩЕНИЕ → SSIL on high
            }

            if (sun != null)
            {
                sun.ShadowEnabled = s.Shadows;               // ТЕНИ
                sun.ShadowBlur = s.ShadowQuality switch       // КАЧЕСТВО ТЕНЕЙ
                {
                    QualityLevel.Low => 1.5f,
                    QualityLevel.Medium => 1.0f,
                    _ => 0.5f,
                };
            }

            // КАЧЕСТВО ТЕНЕЙ → global soft-shadow filter quality.
            var sq = s.ShadowQuality switch
            {
                QualityLevel.Low => RenderingServer.ShadowQuality.SoftLow,
                QualityLevel.Medium => RenderingServer.ShadowQuality.SoftMedium,
                _ => RenderingServer.ShadowQuality.SoftHigh,
            };
            RenderingServer.DirectionalSoftShadowFilterSetQuality(sq);
            RenderingServer.PositionalSoftShadowFilterSetQuality(sq);

            if (cam != null)
            {
                cam.Fov = s.Fov;                              // ПОЛЕ ЗРЕНИЯ
                cam.Far = s.DrawDistance switch               // ДАЛЬНОСТЬ ПРОРИСОВКИ
                {
                    DrawDistance.Low => 120f,
                    DrawDistance.Medium => 400f,
                    _ => 1500f,
                };
            }

            if (viewport != null)
                viewport.Scaling3DScale = s.OverallQuality switch // ОБЩЕЕ КАЧЕСТВО → render scale
                {
                    QualityLevel.Low => 0.7f,
                    QualityLevel.Medium => 0.85f,
                    _ => 1.0f,
                };

            if (calibration != null)
            {
                // КАЛИБРОВКА: neutral at brightness 0.5 / contrast 1.0 / gamma 2.2.
                calibration.SetShaderParameter("brightness", s.Brightness * 2f);
                calibration.SetShaderParameter("contrast", s.Contrast);
                calibration.SetShaderParameter("gamma", s.Gamma);
            }

            // No runtime API in Godot (documented, applied elsewhere or restart-only):
            //   DirectX (render backend, startup), GpuFramesInFlight (startup),
            //   AlphaToCoverage (per-material), TextureQuality / WaterQuality
            //   (no textured/water assets here), AudioOutput (speaker mode, startup).
        }
    }
}
