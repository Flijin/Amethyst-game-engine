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

layout (location = 0) in vec3 aPosition;

#ifdef USE_LIGHTING
layout (location = 1) in vec3 aNormal;

	#ifndef USE_GOURAUD
	out vec3 normal;
	out vec3 fragPos;
	#endif
#endif

#ifdef USE_MESH_MATRIX
uniform mat4 meshMatrix;
#endif

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

#ifdef USE_VERTEX_COLORS
layout (location = 2) in vec4 aVertexColor;
out vec4 vertexColor;
#endif

#ifdef USE_ALBEDO_MAP
layout (location = 3) in vec2 anAlbedoCoords;
out vec2 albedoCoords;
#endif

#ifdef USE_EMISSIVE_MAP
layout (location = 4) in vec2 anEmissiveCoords;
out vec2 emissiveCoords;
#endif

#ifdef USE_NORMAL_MAP
layout (location = 5) in vec2 aNormalCoords;
out vec2 normalCoords;
#endif

#ifdef USE_METALLIC_ROUGHNESS_MAP
layout (location = 6) in vec2 aMetallicRoughnessCoords;
out vec2 metallicRoughnessCoords;
#endif

#ifdef USE_OCCLUSION_MAP
layout (location = 7) in vec2 anOcclusionCoords;
out vec2 occlusionCoords;
#endif

#if defined(USE_LIGHTING) && defined(USE_GOURAUD)
int shininess = MAX_SHININESS;
uniform vec3 cameraPos;

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

out vec3 diffuseSpecular;

#endif

void main(void) {
	#ifdef USE_GOURAUD
		vec3 normal;
		vec3 vertexPos;
	#endif

	#ifdef USE_ALBEDO_MAP
		albedoCoords = anAlbedoCoords;
	#endif

	#ifdef USE_EMISSIVE_MAP
		emissiveCoords = anEmissiveCoords;
	#endif

	#ifdef USE_NORMAL_MAP
		normalCoords = aNormalCoords;
	#endif

	#ifdef USE_METALLIC_ROUGHNESS_MAP
		metallicRoughnessCoords = aMetallicRoughnessCoords;
	#endif

	#ifdef USE_OCCLUSION_MAP
		occlusionCoords = anOcclusionCoords;
	#endif

	#ifdef USE_MESH_MATRIX
		gl_Position = projectionMatrix * viewMatrix * modelMatrix * meshMatrix * vec4(aPosition, 1.0);
		#ifdef USE_LIGHTING
			mat4 modelViewMatrix = modelMatrix * meshMatrix;
			normal = mat3(transpose(inverse(modelViewMatrix))) * aNormal;

			#ifdef USE_GOURAUD
				vertexPos = vec3(modelViewMatrix * vec4(aPosition, 1.0));
			#else
				fragPos = vec3(modelViewMatrix * vec4(aPosition, 1.0));
			#endif
		#endif
	#else
		gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(aPosition, 1.0);
		#ifdef USE_LIGHTING
			normal = mat3(transpose(inverse(modelMatrix))) * aNormal;

			#ifdef USE_GOURAUD
				vertexPos = vec3(modelMatrix * vec4(aPosition, 1.0));
			#else
				fragPos = vec3(modelMatrix * vec4(aPosition, 1.0));
			#endif
		#endif
	#endif

	#ifdef USE_GOURAUD
    	diffuseSpecular = vec3(0);

		for (int i = 0; i < directionalLights.numOfDirectionalLights; i++) {
			diffuseSpecular += CalculateDirectionalLight(directionalLights.directionalLights[i],
														normal, vertexPos, cameraPos, 1.0, shininess);
		}

		for (int i = 0; i < pointLights.numOfPointLights; i++) {
			float dist = distance(vertexPos, pointLights.pointLights[i].position);

			if (dist <= pointLights.pointLights[i].radius) {
				diffuseSpecular += CalculatePointLight(pointLights.pointLights[i],
													   normal, vertexPos, cameraPos, 1.0, shininess);
			}
		}

		for (int i = 0; i < spotlights.numOfSpotlights; i++) {
			float dist = distance(vertexPos, spotlights.spotlights[i].position);

			if (dist <= spotlights.spotlights[i].radius) {
				diffuseSpecular += CalculateSpotlight(spotlights.spotlights[i],
													  normal, vertexPos, cameraPos, 1.0, shininess);
			}
        }
	#endif	
}