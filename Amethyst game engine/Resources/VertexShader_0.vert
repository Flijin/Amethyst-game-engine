#version 330 core

layout (location = 0) in vec3 _position;
layout (location = 1) in vec3 _color;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 color;

void main()
{
    gl_Position = vec4(_position, 1.0) * model * view * projection;
    color = _color / 32.0;
}
