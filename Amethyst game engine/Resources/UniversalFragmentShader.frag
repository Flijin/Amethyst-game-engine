#version 330 core


#ifdef USE_ALBEDO_MAP_0
uniform sampler2D _albeloTexture_0;
in vec2 albedoCoords_0;
#endif

#ifdef USE_ALBEDO_MAP_1
uniform sampler2D _albeloTexture_1;
in vec2 albedoCoords_1;
#endif

#ifdef USE_ALBEDO_MAP_2
uniform sampler2D _albeloTexture_2;
in vec2 albedoCoords_2;
#endif

#ifdef USE_ALBEDO_MAP_3
uniform sampler2D _albeloTexture_3;
in vec2 albedoCoords_3;
#endif

#ifdef USE_BASE_COLOR_FACTOR
uniform vec4 _baseColorFactor;
#endif

#ifdef USE_VERTEX_COLORS
in vec4 vertexColor;
#endif

out vec4 FragColor;

void main()
{

#ifdef USE_VERTEX_COLORS
    FragColor = vertexColor;
    #define FRAG_COLOR_INIT;
#endif

#ifdef USE_ALBEDO_MAP_0
    #ifdef FRAG_COLOR_INIT
        FragColor = mix(vertexColor, texture(albeloTextute_0, albedoCoords_0));
    #else
        FragColor = texture(albeloTextute_0, albedoCoords_0, 0.5);
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

#ifndef FRAG_COLOR_INIT
FragColor = vec4(0.5, 0.5, 0.5, 1.0);
#endif

}
