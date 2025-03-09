#version 330 core

layout (location = 0) in vec3 _position;

#ifdef USE_MESH_MATRIX
uniform mat4 mesh;
#endif

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

#ifdef USE_VERTEX_COLORS
out vec4 color;
layout (location = 1) in vec4 _color;
#endif

#ifdef USE_ALBEDO_MAP_0
out vec2 albedoCoords_0;
layout (location = 2) in vec2 _albedoCoords_0;
#endif

#ifdef USE_ALBEDO_MAP_1
out vec2 albedoCoords_1;
layout (location = 3) in vec2 _albedoCoords_1;
#endif

#ifdef USE_ALBEDO_MAP_2
out vec2 albedoCoords_2;
layout (location = 4) in vec2 _albedoCoords_2;
#endif

#ifdef USE_ALBEDO_MAP_3
out vec2 albedoCoords_3;
layout (location = 5) in vec2 _albedoCoords_3;
#endif

void main()
{
#ifdef USE_ALBEDO_MAP_0
    albedoCoords_0 = _albedoCoords_0;
#endif

#if defined(USE_COLOR_5_BITS) && defined(USE_VERTEX_COLORS)
    color = vec4(_color.x / 32.0, _color.y / 32.0, _color.z / 32.0, _color.w);
#elif defined(USE_VERTEX_COLORS)
    color = _color;
#endif

#ifdef USE_MESH_MATRIX
    gl_Position = vec4(_position, 1.0) * mesh * model * view * projection;
#else
    gl_Position = vec4(_position, 1.0) * model * view * projection;
#endif
}


// It's just a note to me.

//        [1] = "USE_VERTEX_COLORS",
//        [1 << 1] = "USE_NORMALS",
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