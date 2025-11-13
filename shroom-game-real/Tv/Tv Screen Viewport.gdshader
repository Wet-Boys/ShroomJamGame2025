shader_type spatial;

uniform sampler2D viewportTexture : filter_linear;
uniform float emissionMultiplier : hint_range(0, 1) = 1;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 uv = vec2(-UV.y, UV.x);
	vec4 sample = texture(viewportTexture, uv);

	EMISSION = sample.rgb * emissionMultiplier;
	ALBEDO = sample.rgb;
}
