#version 330 core

#ifdef USE_ALBEDO
uniform sampler2D albeloTextute;
in vec2 albedoCoords;
#endif

#if defined(USE_COLOR) || defined(USE_COLOR_5_BITS)
in vec4 color;
#endif

out vec4 FragColor;

void main()
{
#if defined(USE_COLOR) && defined(USE_ALBEDO)
    FragColor = mix(color, texture(albeloTextute, albedoCoords));
#elif defined(USE_COLOR) && defined(USE_ALBEDO) == false || defined(USE_COLOR_5_BITS)
    FragColor = color;
#elif defined(USE_ALBEDO)
    FragColor = texture(albeloTextute, albedoCoords);
#else
    FragColor = vec4(0.5, 0.5, 0.5, 1);
#endif
}
