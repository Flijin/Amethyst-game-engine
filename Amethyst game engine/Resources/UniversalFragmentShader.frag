#version 330 core

#ifdef USE_ALBEDO
uniform sampler2D albeloTextute;
in vec2 albedoCoords;
#endif

#if defined(USE_COLORS) || defined(USE_STL_COLORS)
in vec4 color;
#endif

out vec4 FragColor;

void main()
{
#if defined(USE_COLORS) && defined(USE_ALBEDO)
    FragColor = mix(color, texture(albeloTextute, albedoCoords));
#elif defined(USE_COLORS) || defined(USE_STL_COLORS)
    FragColor = color;
#elif defined(USE_ALBEDO)
    FragColor = texture(albeloTextute, albedoCoords);
#else
    FragColor = vec4(0.5, 0.5, 0.5, 1);
#endif
}
