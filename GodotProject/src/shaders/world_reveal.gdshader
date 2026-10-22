shader_type canvas_item;

global uniform sampler2D paint_mask;
global uniform vec4 paint_zone;
uniform float tint_strength : hint_range(0.0, 1.0) = 0.25;

varying vec2 world_pos;

void vertex() 
{
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() 
{
    vec4 base = texture(TEXTURE, UV);
    float lum = dot(base.rgb, vec3(0.299, 0.587, 0.114));

    vec2 mask_uv = (world_pos - paint_zone.xy) / paint_zone.zw;
    vec4 paint = texture(paint_mask, mask_uv);
    float painted = step(0.5, paint.a);

    vec3 revealed = mix(base.rgb, paint.rgb, tint_strength);
    COLOR = vec4(mix(vec3(lum), revealed, painted), base.a);
}