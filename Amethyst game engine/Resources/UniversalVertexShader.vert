#version 330 core

layout (location = 0) in vec3 _position;

#ifdef USE_MESH_MATRIX
uniform mat4 _mesh;
#endif

uniform mat4 _model;
uniform mat4 _view;
uniform mat4 _projection;

#ifdef USE_VERTEX_COLORS
out vec4 VertexColor;
layout (location = 1) in vec3 _vertexColor;
#endif

#ifdef USE_ALBEDO_MAP
out vec2 AlbedoCoords;
layout (location = 2) in vec2 _albedoCoords;
#endif

#ifdef USE_LIGHTING
    layout (location = 3) in vec3 _normal;

    #ifdef USE_GOURAND_SHADING_MODEL

        int shininess = MAX_SHININESS;

        uniform int _numSpotlights;
        uniform int _numPointLights;
        uniform int _numDirectionalLights;
        uniform vec3 _cameraPos;

        layout(std140, binding = 0) uniform DirectionalLights{
            DirectionalLight[DIRECTIONAL_LIGHTS_COUNT] _directionalLights;
        };

        layout(std140, binding = 1) uniform PointLights{
            PointLight[POINT_LIGHTS_COUNT] _pointLights;
        };

        layout(std140, binding = 2) uniform Spotlights{
            Spotlight[SPOTLIGHT_COUNT] _spotlights;
        };

        out vec3 DiffuseSpecular;
    #else
        out vec3 Normal;
        out vec3 FragPos;
    #endif

#endif

void main() {

#ifdef USE_GOURAND_SHADING_MODEL
    vec3 Normal;
    vec3 FragPos;
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

#ifdef USE_ALBEDO_MAP
    AlbedoCoords = _albedoCoords;
#endif

#ifdef USE_VERTEX_COLORS
    VertexColor = vec4(_vertexColor, 1.0);
#endif

#ifdef USE_GOURAND_SHADING_MODEL

    DiffuseSpecular = vec3(0);

    for (int i = 0; i < _numDirectionalLights; i++) {
        if (_directionalLights[i].color.x != -1.0) {
            DiffuseSpecular += CalculateDirectionalLight(_directionalLights[i], Normal, FragPos, _cameraPos, 1.0, shininess);
        }
    }

    for (int i = 0; i < _numPointLights; i++) {
        if (_pointLights[i].color.x != -1.0) {
            DiffuseSpecular += CalculatePointLight(_pointLights[i], Normal, FragPos, _cameraPos, 1.0, shininess);
        }
    }

    for (int i = 0; i < _numSpotlights; i++) {
        if (_spotlights[i].color.x != -1.0) {
            DiffuseSpecular += CalculateSpotlight(_spotlights[i], Normal, FragPos, _cameraPos, 1.0, shininess);
        }
    }

#endif

}
