#version 330 core

#ifdef USE_ALBEDO_MAP_0
uniform sampler2D albeloTextute_0;
in vec2 albedoCoords_0;
#endif

#ifdef USE_ALBEDO_MAP_1
uniform sampler2D albeloTextute_1;
in vec2 albedoCoords_1;
#endif

#ifdef USE_ALBEDO_MAP_2
uniform sampler2D albeloTextute_2;
in vec2 albedoCoords_2;
#endif

#ifdef USE_ALBEDO_MAP_3
uniform sampler2D albeloTextute_3;
in vec2 albedoCoords_3;
#endif

#ifdef USE_VERTEX_COLORS
in vec4 color;
#endif

out vec4 FragColor;

void main()
{

#ifdef USE_VERTEX_COLORS
    FragColor = color;
    #define FRAG_COLOR_INIT
#endif

#ifdef USE_ALBEDO_MAP_0
    #ifdef FRAG_COLOR_INIT
        FragColor = mix(color, texture(albeloTextute_0, albedoCoords_0));
    #else
        FragColor = texture(albeloTextute_0, albedoCoords_0);
        #define FRAG_COLOR_INIT
    #endif
#endif

#ifndef FRAG_COLOR_INIT
    FragColor = vec4(0.5, 0.5, 0.5, 1);
#endif

}

// It's just a note to me.

//          [1] = "USE_VERTEX_COLORS",
//          [1 << 1] = "USE_NORMALS",
//        [1 << 2] = "USE_ALBEDO_MAP_0",
//        [1 << 3] = "USE_ALBEDO_MAP_1",
//        [1 << 4] = "USE_ALBEDO_MAP_2",
//        [1 << 5] = "USE_ALBEDO_MAP_3",
//        [1 << 6] = "USE_METALLIC_ROUGHNESS_MAP",
//        [1 << 7] = "USE_NORMAL_MAP",
//        [1 << 8] = "USE_OCCLUSION_MAP",
//        [1 << 9] = "USE_EMISSIVE_MAP",
//        [1 << 10] = "USE_BASE_COLOR_FACTOR",
//        [1 << 11] = "USE_METALLIC_FACTOR",
//        [1 << 12] = "USE_ROUGHNESS_FACTOR",
//        [1 << 13] = "USE_EMISSIVE_FACTOR",
//        [1 << 14] = "USE_OCCLUSION_STRENGTH",
//        [1 << 15] = "USE_NORMAL_SCALE",
//
//        //------ModelSettings------//
//        [1 << 24] = "USE_MESH_MATRIX",
//        [1 << 25] = "USE_COLOR_5_BITS",