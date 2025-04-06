#version 430 core


struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float constant;
    float linear;
    float quadratic;
};

struct SpotLight {
    vec3 position;
    vec3 direction;
    vec3 color;
    float constant;
    float linear;
    float quadratic;
    float innerCuttof;
    float outerCuttof;
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

#ifdef USE_METALLIC_FACTOR
uniform float _metallicFactor;
#endif

#ifdef USE_ROUGHNESS_FACTOR
uniform float _roughnessFactor;
#endif

#ifdef USE_VERTEX_COLORS
in vec4 vertexColor;
#endif

#ifdef USE_NORMALS
in vec3 Normal;
in vec3 FragPos;

uniform int _numPointLights;
uniform int _numSpotLights;
uniform int _numDirectionalLights;

layout(std430, binding = 0) buffer Lights{
    int numPointLights;
    int numSpotLights;
    int numDirectionalLights;
    DirectionalLight directionalLights[];
    PointLight pointLights[];
    SpotLight spotLights[];
}
#endif

out vec4 FragColor;

vec3 CalculateDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir, float shininess) {
    vec3 lightDir = normalize(-light.direction);
    vec3 diffuse = light.color * max(dot(normal, lightDir), 0.0) * light.intensity;
    vec3 halfwayDir = normalize(lightDir + viewDir);
    vec3 specular = light.color * pow(max(dot(normal, halfwayDir), 0.0), shininess);

    return diffuse + specular;
}

vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {
    vec3 lightDir = normalize(light.position - fragPos);
    float dist = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * dist + light.quadratic
    * dist * dist);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float diffuse = max(dot(normal, lightDir), 0.0);
    float specular = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    return (diffuse + specular) * light.color * light.intensity + attenuation;
}

vec3 CalculateSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, float shininess) {
    vec3 lightDir = normalize(light.position - fragPos);

    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.innerCuttof - light.outerCuttof;
    float intensity = clamp((theta - light.outerCuttof) / epsilon, 0.0, 1.0);

    float dist = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * dist + light.quadratic
    * dist * dist);

    vec3 diffuse = light.color * max(dot(normal, lightDir), 0.0) * intensity;
    vec3 halfwayDir = normalize(lightDir + viewDir);
    vec3 specular = light.color * pow(max(dot(normal, halfwayDir), 0.0), shininess) * intensity;

    return (diffuse + specular) * attenuation;
}

void main() {
vec4 resultFragColor;

#ifdef USE_VERTEX_COLORS
    resultFragColor = vertexColor;
    #define FRAG_COLOR_INIT;
#endif

#ifdef USE_ALBEDO_MAP
    #ifdef FRAG_COLOR_INIT
        resultFragColor = mix(vertexColor, texture(_albedoTexture, albedoCoords), 0.5);
    #else
        resultFragColor = texture(_albedoTexture, albedoCoords);
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifdef USE_BASE_COLOR_FACTOR
    #ifdef FRAG_COLOR_INIT
        resultFragColor = mix(FragColor, _baseColorFactor, 0.5);
    #else
        resultFragColor = _baseColorFactor;
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifndef FRAG_COLOR_INIT
resultFragColor = vec4(0.5, 0.5, 0.5, 1.0);
#endif

#ifdef USE_NORMALS

    float ambientStrength = 0.1;
    resultFragColor *= ambientStrength;

    vec3 N = normalize(Normal);
    vec3 L = normalize(lightPos - FragPos);
    vec3 V = normalize(-FragPos);

    for (int i = 0; i < _numDirectionalLights; i++) {
        
    }

    for (int i = 0; i < _numPointLights; i++) {

    }

    for (int i = 0; i < _numSpotLights; i++) {

    }

#endif

FragColor = resultFragColor;

}
