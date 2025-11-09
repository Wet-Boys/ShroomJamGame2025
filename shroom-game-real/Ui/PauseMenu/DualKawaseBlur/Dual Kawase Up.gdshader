shader_type canvas_item;


uniform float offset: hint_range(0.0, 10.0);
uniform sampler2D scene_color : hint_screen_texture, filter_linear_mipmap;

void fragment() {
	vec2 halfpixel = SCREEN_PIXEL_SIZE / 2.0;
	
	vec4 sum = texture(scene_color, UV + vec2(-halfpixel.x * 2.0, 0.0) * offset);
	sum += texture(scene_color, UV + vec2(-halfpixel.x, halfpixel.y) * offset) * 2.0;
	sum += texture(scene_color, UV + vec2(0.0, halfpixel.y * 2.0) * offset);
	sum += texture(scene_color, UV + vec2(halfpixel.x, halfpixel.y) * offset) * 2.0;
	sum += texture(scene_color, UV + vec2(halfpixel.x * 2.0, 0.0) * offset);
	sum += texture(scene_color, UV + vec2(halfpixel.x, -halfpixel.y) * offset) * 2.0;
	sum += texture(scene_color, UV + vec2(0.0, -halfpixel.y * 2.0) * offset);
	sum += texture(scene_color, UV + vec2(-halfpixel.x, -halfpixel.y) * offset) * 2.0;

	COLOR = sum / 12.0;
}
