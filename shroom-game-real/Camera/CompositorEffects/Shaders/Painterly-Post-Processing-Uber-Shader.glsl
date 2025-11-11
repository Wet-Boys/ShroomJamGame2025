#[compute]
#version 450

///////////////////////
// Parameters
///////////////////////
layout(set = 0, binding = 0) uniform ParametersUniform {
	float kuwahara_radius;
	float painterly_noise_amplitude;
	float painterly_noise_scale;
	float painterly_noise_speed;
} params;

///////////////////////
// Texture Inputs
///////////////////////
layout(set = 1, binding = 0) uniform sampler2D src_screen_texture;
layout(rgba16f, set = 1, binding = 1) uniform image2D depth_stencil_texture;
layout(rgba16f, set = 1, binding = 2) uniform image2D dst_screen_texture;

///////////////////////
// Push Constants
///////////////////////
layout(push_constant, std430) uniform PushConstants {
	vec2 raster_size;
	double time;
} push_constants;

///////////////////////
// Painterly Functions
///////////////////////
float random(vec2 c) {
  return fract(sin(dot(c.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

vec4 fromLinear(vec4 linearRGB) {
    bvec3 cutoff = lessThan(linearRGB.rgb, vec3(0.0031308));
    vec3 higher = vec3(1.055)*pow(linearRGB.rgb, vec3(1.0/2.4)) - vec3(0.055);
    vec3 lower = linearRGB.rgb * vec3(12.92);

    return vec4(mix(higher, lower, cutoff), linearRGB.a);
}

float hash(vec2 p) {
  return fract(sin(dot(p, vec2(127.1,311.7))) * 43758.5453123);
}

float noise(vec2 p) {
  vec2 i = floor(p);
  vec2 f = fract(p);
  float a = hash(i + vec2(0.0,0.0));
  float b = hash(i + vec2(1.0,0.0));
  float c = hash(i + vec2(0.0,1.0));
  float d = hash(i + vec2(1.0,1.0));
  vec2 u = f*f*(3.0-2.0*f);
  return mix(a, b, u.x) +
  (c - a) * u.y * (1.0 - u.x) +
  (d - b) * u.x * u.y;
}

///////////////////////
// Kuwahara Functions
///////////////////////
vec3 sampleColor(vec2 offset, vec2 fragcoord, vec2 resolution) {
    vec2 coord = fragcoord.xy + (offset / resolution);
    return texture(src_screen_texture, coord).rgb;
}

vec4 getDominantOrientation(vec4 structureTensor) {
    float Jxx = structureTensor.r; 
    float Jyy = structureTensor.g; 
    float Jxy = structureTensor.b; 

    float trace = Jxx + Jyy;
    float determinant = Jxx * Jyy - Jxy * Jxy;

    float lambda1 = trace * 0.5 + sqrt(trace * trace * 0.25 - determinant);
    float lambda2 = trace * 0.5 - sqrt(trace * trace * 0.25 - determinant);
    
    float jxyStrength = abs(Jxy) / (abs(Jxx) + abs(Jyy) + abs(Jxy) + 1e-6);

    vec2 v;
    
    if (jxyStrength > 0.0) {
        v = normalize(vec2(-Jxy, Jxx - lambda1));
    } else {
        v = vec2(0.0, 1.0);
    }

    return vec4(normalize(v), lambda1, lambda2);
}

float polynomialWeight(float x, float y, float eta, float lambda) {
    float polyValue = (x + eta) - lambda * (y * y);
    return max(0.0, polyValue * polyValue);
}

void getSectorVarianceAndAverageColor(mat2 anisotropyMat, float angle, float in_radius, vec2 fragcoord, vec2 resolution, out vec3 avgColor, out float variance) {
    vec3 weightedColorSum = vec3(0.0);
    vec3 weightedSquaredColorSum = vec3(0.0);
    float totalWeight = 0.0;

    float eta = 0.1;
    float lambda = 0.5;

    for (float r = 1.0; r <= in_radius; r += 1.0) {
        for (float a = -0.392699; a <= 0.392699; a += 0.196349) {
            vec2 sampleOffset = r * vec2(cos(angle + a), sin(angle + a));
            sampleOffset *= anisotropyMat;

            vec3 color = sampleColor(sampleOffset, fragcoord, resolution);
            float weight = polynomialWeight(sampleOffset.x, sampleOffset.y, eta, lambda);

            weightedColorSum += color * weight;
            weightedSquaredColorSum += color * color * weight;
            totalWeight += weight;
        }
    }

    // Calculate average color and variance
    avgColor = weightedColorSum / totalWeight;
    vec3 varianceRes = (weightedSquaredColorSum / totalWeight) - (avgColor * avgColor);
    variance = dot(varianceRes, vec3(0.299, 0.587, 0.114)); // Convert to luminance
}

const int SECTOR_COUNT = 8;

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;
void main() {
	ivec2 id = ivec2(gl_GlobalInvocationID.xy);
    ivec2 size = ivec2(push_constants.raster_size);
    
    vec2 uv = vec2(float(id.x) / float(size.x), float(id.y) / float(size.y));
    
	if (id.x >= size.x || id.y >= size.y) {
		return;
   	}
   	
	// vec4 structureTensor = texture(inputBuffer, UV);

    vec3 sectorAvgColors[SECTOR_COUNT];
    float sectorVariances[SECTOR_COUNT];
    
    vec4 orientationAndAnisotropy = getDominantOrientation(vec4(0.0));
    vec2 orientation = orientationAndAnisotropy.xy;

    float anisotropy = (orientationAndAnisotropy.z - orientationAndAnisotropy.w) / (orientationAndAnisotropy.z + orientationAndAnisotropy.w + 1e-6);
    
    float alpha = 25.0;
    float scaleX = alpha / (anisotropy + alpha);
    float scaleY = (anisotropy + alpha) / alpha;
    
	
    mat2 anisotropyMat = mat2(vec2(orientation.x, -orientation.y), vec2(orientation.y, orientation.x)) * mat2(vec2(scaleX, 0.0), vec2(0.0, scaleY));
	
	// vec2 resolution = vec2(textureSize(src_screen_texture, 0));
	// vec2 resolution = vec2(1.0) / SCREEN_PIXEL_SIZE;
	
	double time = push_constants.time;
	
	// Generate two noise values to construct a random direction.
    float nx = noise(uv * params.painterly_noise_scale + vec2(floor(time * params.painterly_noise_speed), 0.0));
    float ny = noise(uv * params.painterly_noise_scale + vec2(0.0, floor(time * params.painterly_noise_speed)));
    // Map [0,1] to [-1,1]
    vec2 rand_dir = normalize(vec2(nx * 2.0 - 1.0, ny * 2.0 - 1.0));

    // Use another noise value to determine the jitter amplitude sign.
    float n = noise(uv.xy * params.painterly_noise_scale + vec2(floor(time * params.painterly_noise_speed)));
    float off = ((n * 2.0 - 1.0) * params.painterly_noise_amplitude);
    
    vec2 pixel_size = vec2(1.0) / push_constants.raster_size;
    
    // Offset along a random direction
    //vec2 uv2 = uv.xy + rand_dir * off * TEXTURE_PIXEL_SIZE;
	vec2 uv2 = uv.xy + rand_dir * off * pixel_size;

    for (int i = 0; i < SECTOR_COUNT; i++) {
    	float angle = float(i) * 6.28318 / float(SECTOR_COUNT); // 2PI / SECTOR_COUNT
      	getSectorVarianceAndAverageColor(anisotropyMat, angle, float(params.kuwahara_radius), uv2, push_constants.raster_size, sectorAvgColors[i], sectorVariances[i]);
    }

    float minVariance = sectorVariances[0];
    vec3 finalColor = sectorAvgColors[0];

    for (int i = 1; i < SECTOR_COUNT; i++) {
        if (sectorVariances[i] < minVariance) {
            minVariance = sectorVariances[i];
            finalColor = sectorAvgColors[i];
        }
    }
    
    vec4 depth = imageLoad(depth_stencil_texture, id);
    
    vec4 final_result = vec4(finalColor, 1.0);
    imageStore(dst_screen_texture, id, vec4(depth.g * 100));
}