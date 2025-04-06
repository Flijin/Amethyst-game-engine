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
out vec3 Normal;
out vec3 FragPos;
layout (location = 3) in vec3 _normal;
#endif

void main() {
#ifdef USE_ALBEDO_MAP
    albedoCoords = _albedoCoords;
#endif

#ifdef USE_VERTEX_COLORS
    vertexColor = vec4(_vertexColor, 1.0);
#endif

#ifdef USE_MESH_MATRIX
    gl_Position = projection * view * model * mesh * vec4(_position, 1.0);
    #ifdef USE_NORMALS
        Normal = mat3(transpose(inverse(view * model * mesh))) * _normal;
        FragPos = vec3(view * model * mesh * vec4(_position, 1.0));
    #endif
#else
    gl_Position = projection * view * model * vec4(_position, 1.0);
    #ifdef USE_NORMALS
        Normal = mat3(transpose(inverse(view * model))) * _normal;
        FragPos = vec3(view * model * vec4(_position, 1.0));
    #endif
#endif
}
