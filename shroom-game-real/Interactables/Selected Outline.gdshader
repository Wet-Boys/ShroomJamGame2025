shader_type canvas_item;

instance uniform vec2 rect_size;

uniform vec4 color: source_color = vec4(1.0);
uniform float line_thickness: hint_range(0.0, 0.5, 0.01) = 0.03;

void fragment() {
	vec2 pos = UV - vec2(0.5);
	float ratio = rect_size.y / rect_size.x;
	
	float rect_alpha = 1.0 - step(abs(pos.x), (0.5 - line_thickness * ratio)) * step(abs(pos.y), (0.5 - line_thickness));
	
	COLOR = vec4(color.rgb, color.a * rect_alpha);
}