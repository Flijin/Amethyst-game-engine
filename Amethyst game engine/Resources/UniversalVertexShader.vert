#version 330 core

layout (location = 0) in vec3 _position;

#if defined(USE_COLOR)
layout (location = 1) in vec4 _color;
#elif defined(USE_COLOR_5_BITS)
layout (location = 1) in vec3 _color;
#endif

#ifdef USE_MESH_MATRIX
uniform mat4 mesh;
#endif

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

#if defined(USE_COLOR) || defined(USE_COLOR_5_BITS)
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

#if defined(USE_COLOR)
    color = _color;
#elif defined(USE_COLOR_5_BITS)
    color = vec4(_color / 32.0, 1);
#endif

#ifdef USE_MESH_MATRIX
    gl_Position = vec4(_position, 1.0) * mesh * model * view * projection;
#else
    gl_Position = vec4(_position, 1.0) * model * view * projection;
#endif
}
