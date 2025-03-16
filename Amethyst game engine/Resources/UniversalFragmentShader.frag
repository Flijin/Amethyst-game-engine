#version 330 core

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
in vec3 normal;
in vec3 fragPos;
#endif

out vec4 FragColor;

void main()
{
//so far, these are constants
    float ambientStrength = 0.2;

    vec3 lightPos = vec3(100);
    vec3 lightColor = vec3(1.0);
    vec3 ambientColor = lightColor * ambientStrength;
//---------------------------

#ifdef USE_VERTEX_COLORS
    FragColor = vertexColor;
    #define FRAG_COLOR_INIT;
#endif

#ifdef USE_ALBEDO_MAP
    #ifdef FRAG_COLOR_INIT
        FragColor = mix(vertexColor, texture(_albedoTexture, albedoCoords), 0.5);
    #else
        FragColor = texture(_albedoTexture, albedoCoords);
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifdef USE_BASE_COLOR_FACTOR
    #ifdef FRAG_COLOR_INIT
        FragColor = mix(FragColor, _baseColorFactor, 0.5);
    #else
        FragColor = _baseColorFactor;
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifdef USE_NORMALS
vec3 lightDirection = normalize(lightPos - fragPos);
float diff = max(dot(normal, lightDirection), 0.0);
vec3 diffuse = diff * lightColor;
ambientColor = ambientColor + diffuse;
#endif

#ifndef FRAG_COLOR_INIT
FragColor = vec4(0.5, 0.5, 0.5, 1.0);
#else
FragColor *= vec4(ambientColor, 1.0);
#endif

}
