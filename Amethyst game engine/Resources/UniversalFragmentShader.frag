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
uniform vec3 _baseColorFactor;
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
        FragColor = mix(FragColor, vec4(_baseColorFactor, 1.0), 0.5);
    #else
        FragColor = vec4(_baseColorFactor, 1.0);
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifndef FRAG_COLOR_INIT
FragColor = vec4(0.5, 0.5, 0.5, 1.0);
#endif

}

// It's just a note to me.

//      Attibs
//      [1] = "USE_VERTEX_COLORS", vertexColor
//      [1 << 1] = "USE_NORMALS", normals

//      TexUnits
//      [1 << 2] = "USE_ALBEDO_MAP_0", int
//      [1 << 3] = "USE_ALBEDO_MAP_1", int
//      [1 << 4] = "USE_ALBEDO_MAP_2", int
//      [1 << 5] = "USE_ALBEDO_MAP_3", int
//      [1 << 6] = "USE_METALLIC_ROUGHNESS_MAP", int
//      [1 << 7] = "USE_NORMAL_MAP", int
//      [1 << 8] = "USE_OCCLUSION_MAP", int
//      [1 << 9] = "USE_EMISSIVE_MAP", int

//      Uniforms
//      [1 << 10] = "USE_BASE_COLOR_FACTOR", vec3 baseColor
//      [1 << 11] = "USE_METALLIC_FACTOR", float metallicFactor
//      [1 << 12] = "USE_ROUGHNESS_FACTOR", float rougnessFactor
//      [1 << 13] = "USE_EMISSIVE_FACTOR", float emissiveFactor
//      [1 << 14] = "USE_OCCLUSION_STRENGTH", float occlusionStrength
//      [1 << 15] = "USE_NORMAL_SCALE", float normalScale
//      [1 << 24] = "USE_MESH_MATRIX", mat4
//

//      //------ModelSettings------//
//      [1 << 25] = "USE_COLOR_5_BITS",