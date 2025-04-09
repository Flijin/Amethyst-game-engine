﻿#version 330 core


#pragma optimize(off)

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

layout(std140, binding = 0) uniform DirectionalLights{
    DirectionalLight[1] directionalLights;
};

layout(std140, binding = 1) uniform PointLights{
    PointLight[1] pointLights;
};

layout(std140, binding = 2) uniform SpotLights{
    SpotLights[1] spotlights;
};

#endif

uniform vec3 _cameraPos;

out vec4 FragColor;

vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewPos, float specularStrengh) {
//Потом убери
    if (light.constant != 0.32 || light.linear != 0.1 || light.quadratic != 0.35) {
        light.color = vec3(1.0, 0.0, 0.0);
    }

    vec3 L = normalize(light.position - fragPos);
    float dist = length(L);

    float attenuation = 1.0 / (light.constant + light.linear * dist +
    light.quadratic * dist * dist);

    vec3 N = normalize(normal);
    vec3 diffuse = max(dot(N, L), 0.0) * light.color * light.intensity;

    vec3 V = normalize(viewPos - fragPos);
    vec3 H = normalize(V + L);
    vec3 specular = pow(max(dot(N, H), 0.0), 32) * light.color * light.intensity * specularStrengh;

    return (diffuse + specular) * attenuation;


//    vec3 L = normalize(light.position - fragPos);
//    float dist = length(L);
//
//    float attenuation = 1.0 / (light.constant + light.linear * dist +
//    light.quadratic * dist * dist);
//
//    vec3 N = normalize(normal);
//    vec3 diffuse = max(dot(N, L), 0.0) * light.color * light.intensity;
//
//    vec3 V = normalize(viewPos - fragPos);
//    vec3 H = normalize(V + L);
//    vec3 specular = pow(max(dot(N, H), 0.0), 32) * light.color * light.intensity * specularStrengh;
//
//    return (diffuse + specular) * attenuation;
}

vec3 CalculateDirectionalLight(DirectionalLight light, vec3 normal, vec3 fragPos, vec3 viewPos, float specularStrengh) {
    vec3 N = normalize(normal);
    vec3 L = normalize(-light.direction);
    vec3 V = normalize(viewPos - fragPos);
    vec3 H = normalize(V + L);

    vec3 diffuse = max(dot(N, L), 0.0) * light.color * light.intensity;
    vec3 specular = pow(max(dot(N, H), 0.0), 32) * light.color * light.intensity * specularStrengh;

    return diffuse + specular;
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
        resultFragColor = mix(resultFragColor, _baseColorFactor, 0.5);
    #else
        resultFragColor = _baseColorFactor;
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifndef FRAG_COLOR_INIT
resultFragColor = vec4(0.5, 0.5, 0.5, 1.0);
#endif

#ifdef USE_NORMALS

float ambientStrengh = 0.1;

vec3 resColorVec3 = vec3(resultFragColor);

//vec3 test = CalculateDirectionalLight(directionalLights[0], Normal, FragPos, _cameraPos, 1.0);
//resColorVec3 *= test;

vec3 test = CalculatePointLight(pointLights[0], Normal, FragPos, _cameraPos, 1.0);
resColorVec3 *= test;

resColorVec3 += (ambientStrengh * pointLights[0].color);

resultFragColor = vec4(resColorVec3, resultFragColor.w);

#endif

FragColor = resultFragColor;

}
