shader_type canvas_item;

uniform float offset: hint_range(0.0, 10.0);
uniform sampler2D scene_color : hint_screen_texture, filter_linear_mipmap;

void fragment() {
	vec2 halfpixel = SCREEN_PIXEL_SIZE / 2.0;
	
	vec4 sum = texture(scene_color, UV) * 4.0;
	sum += texture(scene_color, UV - halfpixel.xy * offset);
	sum += texture(scene_color, UV + halfpixel.xy * offset);
	sum += texture(scene_color, UV + vec2(halfpixel.x, -halfpixel.y) * offset);
	sum += texture(scene_color, UV - vec2(halfpixel.x, -halfpixel.y) * offset);

	COLOR = sum / 8.0;
}