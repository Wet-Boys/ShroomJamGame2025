shader_type spatial;
render_mode shadows_disabled, unshaded, depth_prepass_alpha;
stencil_mode write, compare_always, 0;

uniform vec4 blackout_color : source_color;
uniform float blackout_mix : hint_range(0.0, 1.0) = 0.0;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	if (blackout_mix == 0.0) {
		discard;
	}
	
	ALBEDO = blackout_color.rgb;
	ALPHA = blackout_color.a * blackout_mix;
}
