#version 330 core

uniform sampler2D sampler0;
uniform vec3 aColor;

in vec2 texCoord;
out vec4 FragColor;

void main()
{
    FragColor = texture(sampler0, texCoord);
    //FragColor = vec4(aColor, 1.0);
}