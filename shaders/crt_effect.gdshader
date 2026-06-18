// CRT overlay: scanlines + vignette + noise.
// Designed to sit on a full-screen ColorRect ABOVE the UI: it outputs black with
// a computed alpha so it darkens the picture subtly instead of replacing it.
shader_type canvas_item;
render_mode blend_mix;

uniform float scanline_count     : hint_range(100.0, 1200.0) = 600.0;
uniform float scanline_intensity : hint_range(0.0, 1.0)      = 0.15;
uniform float vignette_radius    : hint_range(0.0, 1.0)      = 0.85;
uniform float vignette_softness  : hint_range(0.0, 1.0)      = 0.5;
uniform float noise_intensity    : hint_range(0.0, 0.2)      = 0.04;

float rand(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
    // Scanlines: darken on every other line.
    float line = sin(UV.y * scanline_count * PI) * 0.5 + 0.5;
    float scan = line * scanline_intensity;

    // Vignette: 0 in the centre, growing toward the corners.
    vec2 centered = UV - 0.5;
    float vig = smoothstep(vignette_radius - vignette_softness, vignette_radius, length(centered));

    // Animated noise.
    float noise = (rand(UV + TIME * 0.1) - 0.5) * noise_intensity;

    float alpha = clamp(scan + vig * 0.7 + abs(noise), 0.0, 1.0);
    COLOR = vec4(0.0, 0.0, 0.0, alpha);
}
