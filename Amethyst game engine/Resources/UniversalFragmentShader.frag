#version 330 core


#ifdef USE_ALBEDO_MAP
uniform sampler2D _albedoTexture;
in vec2 albedoCoords;
#endif

#ifdef USE_BASE_COLOR_FACTOR
uniform vec4 _baseColorFactor;
#endif

#ifdef USE_METALLIC_FACTOR
uniform float _metallicFactor;
#endif

#ifdef USE_ROUGHNESS_FACTOR
uniform float _roughnessFactor;
#endif

#ifdef USE_VERTEX_COLORS
in vec4 vertexColor;
#endif

#ifdef USE_NORMALS
in vec3 Normal;
in vec3 FragPos;
in vec3 LightPos;
#endif

out vec4 FragColor;

//uniform vec3 lightPos;
//uniform vec3 lightColor;
//uniform vec3 diffuseColor;
//uniform vec3 specularColor;
//uniform float shininess;

vec3 BlinnPhong(vec3 N, vec3 L, vec3 V, vec3 lightColor, vec3 baseColor, float shininess) {
    float diff = max(dot(N, L), 0.0);
    vec3 diffuse = diff * baseColor * lightColor;
    vec3 H = normalize(L + V);
    float spec = pow(max(dot(N, H), 0.0), shininess);
    vec3 specular = spec * lightColor;
    vec3 ambient = 0.1 * lightColor;

    return ambient + diffuse + specular;
}

void main() {
vec4 resultFragColor;

#ifdef USE_VERTEX_COLORS
    resultFragColor = vertexColor;
    #define FRAG_COLOR_INIT;
#endif

#ifdef USE_ALBEDO_MAP
    #ifdef FRAG_COLOR_INIT
        resultFragColor = mix(vertexColor, texture(_albedoTexture, albedoCoords), 0.5);
    #else
        resultFragColor = texture(_albedoTexture, albedoCoords);
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifdef USE_BASE_COLOR_FACTOR
    #ifdef FRAG_COLOR_INIT
        resultFragColor = mix(FragColor, _baseColorFactor, 0.5);
    #else
        resultFragColor = _baseColorFactor;
        #define FRAG_COLOR_INIT;
    #endif
#endif

#ifndef FRAG_COLOR_INIT
resultFragColor = vec4(0.5, 0.5, 0.5, 1.0);
#endif

#ifdef USE_NORMALS
    vec3 lightColor = vec3(1);

    vec3 N = normalize(Normal);
    vec3 L = normalize(LightPos - FragPos);
    vec3 V = normalize(-FragPos);

    resultFragColor = vec4(BlinnPhong(N, L, FragPos, lightColor, vec3(resultFragColor), 32), resultFragColor.w);
#endif

FragColor = resultFragColor;

}
