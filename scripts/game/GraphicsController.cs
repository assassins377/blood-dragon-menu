using Godot;

namespace BloodDragon
{
    /// <summary>
    /// Pushes the current <see cref="GameSettings"/> into the live 3D scene
    /// (environment, light, camera). Called on load and whenever settings change.
    /// </summary>
    public static class GraphicsController
    {
        public static void Apply(WorldEnvironment worldEnv, DirectionalLight3D sun, Camera3D cam, GameSettings s)
        {
            if (worldEnv?.Environment is { } env)
            {
                // Brightness / contrast — calibration sliders (0..1, neutral ~0.5 / ~1.0).
                env.AdjustmentEnabled = true;
                env.AdjustmentBrightness = Mathf.Lerp(0.3f, 1.7f, s.Brightness);
                env.AdjustmentContrast = Mathf.Lerp(0.6f, 1.6f, s.Contrast);

                // Post-processing → glow; SSAO method → SSAO on/off.
                env.GlowEnabled = s.PostProcessing;
                env.SsaoEnabled = s.Ssao != SsaoMethod.Off;
            }

            if (sun != null)
                sun.ShadowEnabled = s.Shadows;

            if (cam != null)
            {
                cam.Fov = s.Fov;
                cam.Far = s.DrawDistance switch
                {
                    DrawDistance.Low => 120f,
                    DrawDistance.Medium => 400f,
                    _ => 1500f,
                };
            }
        }
    }
}
