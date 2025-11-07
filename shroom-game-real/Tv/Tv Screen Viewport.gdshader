shader_type spatial;

uniform sampler2D viewportTexture : filter_linear;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 sample = texture(viewportTexture, UV);
	
	EMISSION = sample.rgb;
}
