shader_type canvas_item;

uniform sampler2D scene_texture : hint_screen_texture;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec3 scene_sample = vec3(1.0) - texture(scene_texture, UV).rgb;
	
	scene_sample = round(scene_sample);
	
	COLOR = vec4(scene_sample, 1.0);
}