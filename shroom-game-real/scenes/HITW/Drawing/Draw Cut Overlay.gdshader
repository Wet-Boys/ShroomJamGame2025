shader_type spatial;
render_mode unshaded;

uniform float alpha : hint_range(0.0, 1.0, 0.01) = 0.9;
uniform float hint_visibility : hint_range(0.0, 1.0, 0.01) = 0.75f;
uniform sampler2D overlay_texture : hint_default_black, filter_linear;
uniform sampler2D cut_out : hint_default_white, filter_linear;

uniform bool show_mistakes;
uniform vec3 mistakes_color : source_color; 

void vertex() {
	// Called for every vertex the material is visible on.
}

float sample_texture(sampler2D sampler, vec2 uv) {
	return round(texture(sampler, uv).r);
}

void fragment() {
	float overlay = 1.0 - sample_texture(overlay_texture, UV);
	float sample = 1.0 - sample_texture(cut_out, UV);
	
	if (sample <= 0.0) {
		discard;
	}
	
	if (show_mistakes) {
		float mask = 1.0 - overlay;
		ALBEDO = vec3(overlay) + (mask * mistakes_color);
		ALPHA = 1.0;
	}
	else {
		ALBEDO = mix(vec3(1.0), vec3(overlay), hint_visibility);
		ALPHA = alpha;
	}
}
