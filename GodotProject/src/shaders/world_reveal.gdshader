shader_type canvas_item;

global uniform sampler2D paint_mask;
global uniform vec4 paint_zone;
global uniform float world_reveal;

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
    ivec2 mask_size = textureSize(paint_mask, 0);
    ivec2 p = ivec2(floor(mask_uv * vec2(mask_size)));

    vec3 color = vec3(lum); // unpainted = grayscale
    if (p.x >= 0 && p.y >= 0 && p.x < mask_size.x && p.y < mask_size.y)
    {
        vec4 paint = texelFetch(paint_mask, p, 0);
		if (paint.a > 0.5)
		{
		    vec3 tinted = paint.rgb * (0.5 + lum);          // paint color, shaded by tile detail
		    color = mix(vec3(lum), tinted, tint_strength);   // blend from the gray look, not the original color
		}
    }

    COLOR = vec4(mix(color, base.rgb, world_reveal), base.a);
}