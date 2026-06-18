// Full-screen calibration: brightness / contrast / gamma.
// Sits on a CanvasLayer above the 3D scene and samples the screen texture.
// Tuned so the default settings (brightness 0.5, contrast 1.0, gamma 2.2)
// produce an identity transform — i.e. they don't alter the image at neutral.
shader_type canvas_item;

uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;
uniform float brightness : hint_range(0.0, 2.0) = 1.0; // 1.0 = neutral
uniform float contrast   : hint_range(0.0, 2.0) = 1.0; // 1.0 = neutral
uniform float gamma      : hint_range(1.8, 3.0) = 2.2; // 2.2 = neutral

void fragment() {
    vec3 c = texture(screen_tex, SCREEN_UV).rgb;
    c *= brightness;
    c = (c - 0.5) * contrast + 0.5;
    c = pow(max(c, vec3(0.0)), vec3(2.2 / gamma)); // 2.2/2.2 = 1.0 → identity
    COLOR = vec4(clamp(c, 0.0, 1.0), 1.0);
}
