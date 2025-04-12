#version 330 core


layout (location = 0) in vec3 _position;

#ifdef USE_MESH_MATRIX
uniform mat4 _mesh;
#endif

uniform mat4 _model;
uniform mat4 _view;
uniform mat4 _projection;

#ifdef USE_VERTEX_COLORS
lowp out vec4 VertexColor;
lowp layout (location = 1) in vec3 _vertexColor;
#endif

#ifdef USE_ALBEDO_MAP
mediump out vec2 AlbedoCoords;
mediump layout (location = 2) in vec2 _albedoCoords;
#endif

#ifdef USE_LIGHTING
out vec3 Normal;
out vec3 FragPos;

layout (location = 3) in vec3 _normal;
#endif

void main() {
#ifdef USE_ALBEDO_MAP
    AlbedoCoords = _albedoCoords;
#endif

#ifdef USE_VERTEX_COLORS
    VertexColor = vec4(_vertexColor, 1.0);
#endif

#ifdef USE_MESH_MATRIX
    gl_Position = _projection * _view * _model * _mesh * vec4(_position, 1.0);
    #ifdef USE_LIGHTING
        mat4 modelView = _model * _mesh;
        Normal = mat3(transpose(inverse(modelView))) * _normal;
        FragPos = vec3(modelView * vec4(_position, 1.0));
    #endif
#else
    gl_Position = _projection * _view * _model * vec4(_position, 1.0);
    #ifdef USE_LIGHTING
        Normal = mat3(transpose(inverse(_model))) * _normal;
        FragPos = vec3(_model * vec4(_position, 1.0));
    #endif
#endif
}
