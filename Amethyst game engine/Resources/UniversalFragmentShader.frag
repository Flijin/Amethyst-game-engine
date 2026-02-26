#version 420

#pragma optimize(on)
#pragma debug(on)

struct Spotlight {
    vec3 position;
    vec3 direction;
    vec3 color;
    float intensity;
    float innerCutOff;
    float outerCutOff;
    float constant;
    float linear;
    float quadratic;
	float radius;
};

struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float constant;
    float linear;
    float quadratic;
	float radius;
};

struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;
};

#ifdef USE_ALBEDO_MAP
uniform sampler2D _albedoTexture;
in vec2 albedoCoords;
#endif

#ifdef USE_BASE_COLOR_FACTOR
uniform vec4 _baseColorFactor;
#endif

#ifdef USE_VERTEX_COLORS
in vec4 vertexColor;
#endif

#ifdef USE_LIGHTING
    #ifdef USE_EMISSIVE_MAP
        uniform sampler2D _emissiveTexture;
        in vec2 emissiveCoords;
    #endif

    #ifdef USE_NORMAL_MAP
        uniform sampler2D _normalTexture;
        in vec2 normalCoords;
    #endif

    #ifdef USE_OCCLUSION_MAP
        uniform sampler2D _occlusionTexture;
        in vec2 occlusionCoords;
    #endif

    #ifdef USE_EMISSIVE_FACTOR
        uniform vec3 _emissiveFactor;
    #endif

    #ifdef NORMAL_SCALE
        uniform float normalScale;
    #endif

    #ifdef USE_OCCLUSION_STRENGTH
        uniform float occlusionStrength;
    #endif

    #ifndef USE_GOURAUD
        in vec3 normal;
        in vec3 fragPos;
    #endif
#endif

#if defined(USE_LIGHTING) && defined(USE_PBR_METALLIC_ROUGHNESS)
    #ifdef USE_METALLIC_ROUGHNESS_MAP
        uniform sampler2D _metallicRoughnessTexture;
        in vec2 metallicRoughnessCoords;
    #endif

    #ifdef USE_METALLIC_FACTOR
        uniform float _metallicFactor;
    #endif

    #ifdef USE_ROUGHNESS_FACTOR
        uniform float _roughnessFactor;
    #endif
#endif

#if defined(USE_LIGHTING) && defined(USE_GOURAUD)
    in vec3 diffuseSpecular;
#endif

#if defined(USE_LIGHTING) && defined(USE_GOURAUD) == false
int shininess = MAX_SHININESS;
uniform vec3 _cameraPos;

layout (std430, binding = 0) buffer DirectionLights {
	int numOfDirectionalLights;
	DirectionalLight directionalLights[];
} directionalLights;

layout (std430, binding = 1) buffer PointLights {
	int numOfPointLights;
	PointLight pointLights[];
} pointLights;

layout (std430, binding = 2) buffer Spotlights {
	int numOfSpotlights;
	Spotlight spotlights[];
} spotlights;

#endif

out vec4 fragColor;

vec4 GetPixelBaseColor() {
    vec4 baseColor = vec4(0.5, 0.5, 0.5, 1.0);

    #ifdef USE_ALBEDO_MAP
        baseColor = texture(_albedoTexture, albedoCoords);
    #endif

    #ifdef USE_BASE_COLOR_FACTOR
        #ifdef USE_ALBEDO_MAP
            baseColor = mix(baseColor, _baseColorFactor, 0.5);
        #else
            baseColor = _baseColorFactor;
        #endif
    #endif

    #ifdef USE_VERTEX_COLORS
        #if defined(USE_ALBEDO_MAP) || defined(USE_BASE_COLOR_FACTOR)
            baseColor = mix(baseColor, vertexColor, 0.5);
        #else
            baseColor = vertexColor;
        #endif
    #endif

    return baseColor;
}

void main(void) {
    vec4 fragColorVar;

    fragColorVar = GetPixelBaseColor();

    #ifdef USE_LIGHTING
        vec3 diffuseSpecular = vec3(0.0);

        #ifdef USE_BLINN_PHONG
        	for (int i = 0; i < directionalLights.numOfDirectionalLights; i++) {
		        diffuseSpecular += CalculateDirectionalLight(directionalLights.directionalLights[i],
													        normal, fragPos, _cameraPos, 1.0, shininess);
	        }

            for (int i = 0; i < pointLights.numOfPointLights; i++) {
			    float dist = distance(fragPos, pointLights.pointLights[i].position);

			    if (dist <= pointLights.pointLights[i].radius) {
				    diffuseSpecular += CalculatePointLight(pointLights.pointLights[i],
													       normal, fragPos, _cameraPos, 1.0, shininess);
			    }
		    }

		    for (int i = 0; i < spotlights.numOfSpotlights; i++) {
			    float dist = distance(fragPos, spotlights.spotlights[i].position);

			    if (dist <= spotlights.spotlights[i].radius) {
				    diffuseSpecular += CalculateSpotlight(spotlights.spotlights[i],
													      normal, fragPos, _cameraPos, 1.0, shininess);
			    }
            }
        #endif

        fragColorVar.rgb *= diffuseSpecular;

        #if defined(USE_EMISSIVE_MAP) || defined(USE_EMISSIVE_FACTOR)
            vec3 emissive = vec3(0.0);
            #ifdef USE_EMISSIVE_MAP
                emissive = texture(_emissiveTexture, emissiveCoords).rgb;
            #endif

            #ifdef USE_EMISSIVE_FACTOR
                #ifdef USE_EMISSIVE_MAP
                    emissive *= _emissiveFactor;
                #else
                    emissive = _emissiveFactor;
                #endif
            #endif
                fragColorVar.rgb += emissive;
        #endif
    #endif
    
    fragColor = fragColorVar;
}