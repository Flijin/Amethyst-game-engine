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

#ifdef USE_VERTEX_COLORS
    vertexColor = vec4(_vertexColor, 1.0);
#endif

#ifdef USE_MESH_MATRIX
    gl_Position = vec4(_position, 1.0) * mesh * model * view * projection;
#else
    gl_Position = vec4(_position, 1.0) * model * view * projection;
#endif

}
