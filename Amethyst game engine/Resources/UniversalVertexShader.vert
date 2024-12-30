#version 330 core

layout (location = 0) in vec3 _position;

#if defined(USE_COLORS) || defined(USE_STL_COLORS)
layout (location = 1) in vec4 _color;
#endif

#ifdef USE_MESH_MATRIX
uniform mat4 mesh;
#endif

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

#if defined(USE_COLORS) || defined(USE_STL_COLORS)
out vec4 color;
#endif

#ifdef USE_ALBEDO
out vec2 albedoCoords;
layout (location = 2) in vec2 _albedoCoords;
#endif

void main()
{
#ifdef USE_ALBEDO
    albedoCoords = _albedoCoords;
#endif

#if defined(USE_COLORS)
    color = _color;
#elif defined(USE_STL_COLORS)
    color = vec4(_color.x / 32.0, _color.y / 32.0, _color.z / 32.0, _color.w);
#endif

#ifdef USE_MESH_MATRIX
    gl_Position = vec4(_position, 1.0) * mesh * model * view * projection;
#else
    gl_Position = vec4(_position, 1.0) * model * view * projection;
#endif
}
