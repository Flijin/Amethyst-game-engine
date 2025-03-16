#version 330 core


layout (location = 0) in vec3 _position;

#ifdef USE_MESH_MATRIX
uniform mat4 mesh;
#endif

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

#ifdef USE_VERTEX_COLORS
out vec4 vertexColor;
layout (location = 1) in vec3 _vertexColor;
#endif

#ifdef USE_ALBEDO_MAP
out vec2 albedoCoords;
layout (location = 2) in vec2 _albedoCoords;
#endif

#ifdef USE_NORMALS
out vec3 normal;
out vec3 fragPos;
layout (location = 3) in vec3 _normal;
#endif

void main()
{
#ifdef USE_NORMALS
normal = _normal;
#endif

#ifdef USE_ALBEDO_MAP
    albedoCoords = _albedoCoords;
#endif

#ifdef USE_VERTEX_COLORS
    vertexColor = vec4(_vertexColor, 1.0);
#endif

#ifdef USE_MESH_MATRIX
    gl_Position = vec4(_position, 1.0) * mesh * model * view * projection;
    #ifdef USE_NORMALS
        fragPos = vec3(mesh * model * vec4(_position, 1.0));
    #endif
#else
    gl_Position = vec4(_position, 1.0) * model * view * projection;
    #ifdef USE_NORMALS
        fragPos = vec3(model * vec4(_position, 1.0));
    #endif
#endif

}
