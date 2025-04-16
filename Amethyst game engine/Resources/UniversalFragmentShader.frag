#version 330 core


struct SpotLight {
    highp vec3 position;
    highp vec3 direction;
    lowp vec3 color;
    mediump float intensity;
    mediump float innerCutOff;
    mediump float outerCutOff;
    mediump float constant;
    mediump float linear;
    mediump float quadratic;
};

struct PointLight {
    highp vec3 position;
    lowp vec3 color;
    mediump float intensity;
    mediump float constant;
    mediump float linear;
    mediump float quadratic;
};

struct DirectionalLight {
    highp vec3 direction;
    lowp vec3 color;
    mediump float intensity;
};

#ifdef USE_ALBEDO_MAP
lowp uniform sampler2D _albedoTexture;
highp in vec2 AlbedoCoords;
#endif

#ifdef USE_BASE_COLOR_FACTOR
lowp uniform vec4 _baseColorFactor;
#endif

#ifdef USE_METALLIC_FACTOR
lowp uniform float _metallicFactor;
#endif

#ifdef USE_ROUGHNESS_FACTOR
lowp uniform float _roughnessFactor;
#endif

#ifdef USE_VERTEX_COLORS
in vec4 VertexColor;
#endif

#ifdef USE_LIGHTING

mediump int shininess = MAX_SHININESS;

highp in vec3 Normal;
highp in vec3 FragPos;

mediump uniform int _numSpotLights;
mediump uniform int _numPointLights;
mediump uniform int _numDirectionalLights;
highp uniform vec3 _cameraPos;

layout(std140, binding = 0) uniform DirectionalLights{
    DirectionalLight[DIRECTIONAL_LIGHTS_COUNT] _directionalLights;
};

layout(std140, binding = 1) uniform PointLights{
    PointLight[POINT_LIGHTS_COUNT] _pointLights;
};

layout(std140, binding = 2) uniform SpotLights{
    SpotLight[SPOT_LIGHT_COUNT] _spotlights;
};

#endif

lowp out vec4 FragColor;

vec3 CalculateSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewPos, float specularStrength, mediump int shininess) {
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
    vec3 L = normalize(light.position - fragPos);
    float dist = length(L);

    float attenuation = 1.0 / (light.constant + light.linear * dist +
    light.quadratic * dist * dist);

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

void main() {
vec4 resultFragColor;

#ifdef USE_VERTEX_COLORS
    resultFragColor = VertexColor;
    #define FRAG_COLOR_INIT;
#endif

#ifdef USE_ALBEDO_MAP
    #ifdef FRAG_COLOR_INIT
        resultFragColor = mix(VertexColor, texture(_albedoTexture, AlbedoCoords), 0.5);
    #else
        resultFragColor = texture(_albedoTexture, AlbedoCoords);
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifdef USE_BASE_COLOR_FACTOR
    #ifdef FRAG_COLOR_INIT
        resultFragColor = mix(resultFragColor, _baseColorFactor, 0.5);
    #else
        resultFragColor = _baseColorFactor;
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifndef FRAG_COLOR_INIT
resultFragColor = vec4(0.5, 0.5, 0.5, 1.0);
#endif

#ifdef USE_LIGHTING

lowp float ambientStrength = AMBIENT_STHENGTH;

lowp vec3 resColorVec3 = vec3(0);
lowp vec3 temp;

for (int i = 0; i < _numDirectionalLights; i++) {
    if (_directionalLights[i].color.x != -1.0) {
        temp = CalculateDirectionalLight(_directionalLights[i], Normal, FragPos, _cameraPos, 1.0, shininess);
        temp *= vec3(resultFragColor);
        resColorVec3 += temp;
    }
}

for (int i = 0; i < _numPointLights; i++) {
    if (_pointLights[i].color.x != -1.0) {
        temp = CalculatePointLight(_pointLights[i], Normal, FragPos, _cameraPos, 1.0, shininess);
        temp *= vec3(resultFragColor);
        resColorVec3 += temp;
    }
}

for (int i = 0; i < _numSpotLights; i++) {
    if (_spotlights[i].color.x != -1.0) {
        temp = CalculateSpotLight(_spotlights[i], Normal, FragPos, _cameraPos, 1.0, shininess);
        temp *= vec3(resultFragColor);
        resColorVec3 += temp;
    }
}

#if USE_MONOCHROME_AMBIENT == True
resColorVec3 += (ambientStrength * vec3(1));
#else
resColorVec3 += (ambientStrength * vec3(resultFragColor));
#endif

resultFragColor = vec4(resColorVec3, resultFragColor.w);

#endif

FragColor = resultFragColor;

}
