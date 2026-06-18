// Soft neon-green glow for active UI elements.
shader_type canvas_item;

uniform vec4 glow_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform float strength : hint_range(0.0, 2.0) = 0.4;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    COLOR = tex;
    COLOR.rgb += glow_color.rgb * strength * tex.a;
}
