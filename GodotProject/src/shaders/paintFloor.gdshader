shader_type canvas_item;

uniform sampler2D paint_mask : filter_linear, repeat_disable;
uniform float edge_softness : hint_range(0.0, 0.5) = 0.15;
uniform float tint_strength : hint_range(0.0, 1.0) = 0.65;

void fragment() 
{
    vec4 base = texture(TEXTURE, UV);
    float gray = dot(base.rgb, vec3(0.299, 0.587, 0.114));
    vec4 paint = texture(paint_mask, UV);
    vec3 paint_rgb = paint.rgb / max(paint.a, 0.001);
    float amount = smoothstep(0.5 - edge_softness, 0.5 + edge_softness, paint.a);
    vec3 colored = mix(base.rgb, paint_rgb, tint_strength);
    COLOR = vec4(mix(vec3(gray), colored, amount), base.a);
}