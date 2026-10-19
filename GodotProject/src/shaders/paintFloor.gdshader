shader_type canvas_item;

uniform sampler2D paint_mask : filter_linear, repeat_disable;
uniform float tint_strength : hint_range(0.0, 1.0) = 0.3;
uniform float brightness : hint_range(0.5, 2.5) = 1.6;
uniform float rim_darkness : hint_range(0.0, 1.0) = 0.35;

void fragment() 
{
    vec4 base = texture(TEXTURE, UV);
    float lum = dot(base.rgb, vec3(0.299, 0.587, 0.114));

    vec4 paint = texture(paint_mask, UV);
    vec3 paint_rgb = paint.rgb / max(paint.a, 0.001);
    float amount = smoothstep(0.45, 0.55, paint.a);

    vec3 tinted = paint_rgb * min(lum * brightness, 1.0);
    vec3 colored = mix(base.rgb, tinted, tint_strength);

    float rim = amount * (1.0 - smoothstep(0.55, 0.85, paint.a));
    colored *= 1.0 - rim * rim_darkness;

    COLOR = vec4(mix(vec3(lum), colored, amount), base.a);
}