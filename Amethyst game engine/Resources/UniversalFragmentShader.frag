#version 330 core

#ifdef USE_ALBEDO_MAP
uniform sampler2D _albedoTexture;
in vec2 AlbedoCoords;
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
in vec4 VertexColor;
#endif

#if defined(USE_LIGHTING) && defined(USE_GOURAND_SHADING_MODEL) == false

int shininess = MAX_SHININESS;

in vec3 Normal;
in vec3 FragPos;

uniform int _numSpotlights;
uniform int _numPointLights;
uniform int _numDirectionalLights;
uniform vec3 _cameraPos;

layout(std140, binding = 0) uniform DirectionalLights{
    DirectionalLight[DIRECTIONAL_LIGHTS_COUNT] _directionalLights;
};

layout(std140, binding = 1) uniform PointLights{
    PointLight[POINT_LIGHTS_COUNT] _pointLights;
};

layout(std140, binding = 2) uniform Spotlights{
    Spotlight[SPOTLIGHT_COUNT] _spotlights;
};

#elif defined(USE_GOURAND_SHADING_MODEL)

in vec3 DiffuseSpecular;

#endif

out vec4 FragColor;

void main() {
vec4 resultFragColor = vec4(0);

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

float ambientStrength = AMBIENT_STHENGTH;
vec3 resColorVec3 = vec3(0);

    #ifdef USE_BLINN_PHONG_SHADING_MODEL
        vec3 temp;

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

        for (int i = 0; i < _numSpotlights; i++) {
            if (_spotlights[i].color.x != -1.0) {
                temp = CalculateSpotlight(_spotlights[i], Normal, FragPos, _cameraPos, 1.0, shininess);
                temp *= vec3(resultFragColor);
                resColorVec3 += temp;
            }
        }
    #elif defined(USE_lAMBERTIAN_SHADING_MODEL)
        vec3 temp;

        for (int i = 0; i < _numDirectionalLights; i++) {
            if (_directionalLights[i].color.x != -1.0) {
                temp = CalculateDirectionalLight(_directionalLights[i], Normal);
                temp *= vec3(resultFragColor);
                resColorVec3 += temp;
            }
        }

        for (int i = 0; i < _numPointLights; i++) {
            if (_pointLights[i].color.x != -1.0) {
                temp = CalculatePointLight(_pointLights[i], Normal, FragPos);
                temp *= vec3(resultFragColor);
                resColorVec3 += temp;
            }
        }

        for (int i = 0; i < _numSpotlights; i++) {
            if (_spotlights[i].color.x != -1.0) {
                temp = CalculateSpotlight(_spotlights[i], Normal, FragPos);
                temp *= vec3(resultFragColor);
                resColorVec3 += temp;
            }
        }

    #elif defined(USE_GOURAND_SHADING_MODEL)
        resColorVec3 = vec3(resultFragColor) * DiffuseSpecular;
    #endif

    #if USE_MONOCHROME_AMBIENT == True
    resColorVec3 += (ambientStrength * vec3(1));
    #else
    resColorVec3 += (ambientStrength * vec3(resultFragColor));
    #endif

    resultFragColor = vec4(resColorVec3, resultFragColor.w);

#endif


FragColor = resultFragColor;

}

//0 => "#define USE_BLINN_PHONG_SHADING_MODEL",
//1 => "#define USE_GOURAND_SHADING_MODEL",
//2 => "#define USE_lAMBERTIAN_SHADING_MODEL",
//3 => "#define USE_OREN_NAYAR_SHADING_MODEL",
//4 => "#define USE_DISNEY_BRDF_SHADING_MODEL",