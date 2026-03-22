#version 430


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

#ifndef USE_UNLIT
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

#ifdef USE_GOURAUD
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

out vec3 diffuseSpecular;

#endif

vec3 CalculateSpotlight(Spotlight light, vec3 normal, vec3 fragPos, vec3 viewPos, float specularStrength, int shininess) {
    vec3 L = normalize(light.position - fragPos);
    float theta = dot(L, normalize(light.direction));

    float epsilon = light.innerCutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    float dist = length(vec3(light.position) - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * dist +
    light.quadratic * (dist * dist));

    vec3 N = normalize(normal);
    vec3 diffuse = max(dot(N, L), 0.0) * light.color * light.intensity;

    vec3 V = normalize(viewPos - fragPos);
    vec3 H = normalize(L + V);
    vec3 specular = pow(max(dot(N, H), 0.0), shininess) * light.color * light.intensity * specularStrength;

    return (diffuse + specular) * attenuation * intensity;
}

vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewPos, float specularStrength, int shininess) {
    vec3 lightDir = light.position - fragPos;
    float dist = length(lightDir);
    vec3 L = normalize(lightDir);
    
    float attenuation = 1.0 / (light.constant + light.linear * dist + light.quadratic * dist * dist);
    
    vec3 N = normalize(normal);
    vec3 diffuse = max(dot(N, L), 0.0) * light.color * light.intensity;
    
    vec3 V = normalize(viewPos - fragPos);
    vec3 H = normalize(V + L);
    vec3 specular = pow(max(dot(N, H), 0.0), shininess) * light.color * light.intensity * specularStrength;
    
    return (diffuse + specular) * attenuation;
}

vec3 CalculateDirectionalLight(DirectionalLight light, vec3 normal, vec3 fragPos, vec3 viewPos, float specularStrength, int shininess) {
    vec3 N = normalize(normal);
    vec3 L = normalize(-light.direction);
    vec3 V = normalize(viewPos - fragPos);
    vec3 H = normalize(V + L);

    vec3 diffuse = max(dot(N, L), 0.0) * light.color * light.intensity;
    vec3 specular = pow(max(dot(N, H), 0.0), shininess) * light.color * light.intensity * specularStrength;

    return diffuse + specular;
}

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
		#ifndef USE_UNLIT
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
		#ifndef USE_UNLIT
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
														normal, vertexPos, _cameraPos, 1.0, shininess);
		}

		for (int i = 0; i < pointLights.numOfPointLights; i++) {
			float dist = distance(vertexPos, pointLights.pointLights[i].position);

			if (dist <= pointLights.pointLights[i].radius) {
				diffuseSpecular += CalculatePointLight(pointLights.pointLights[i],
													   normal, vertexPos, _cameraPos, 1.0, shininess);
			}
		}

		for (int i = 0; i < spotlights.numOfSpotlights; i++) {
			float dist = distance(vertexPos, spotlights.spotlights[i].position);

			if (dist <= spotlights.spotlights[i].radius) {
				diffuseSpecular += CalculateSpotlight(spotlights.spotlights[i],
													  normal, vertexPos, _cameraPos, 1.0, shininess);
			}
        }
	#endif

	#ifdef USE_VERTEX_COLORS
		vertexColor = aVertexColor;
	#endif
}
