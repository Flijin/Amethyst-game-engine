#version 330 core

layout (location = 0) in vec3 _position;
layout (location = 1) in vec4 _color;

uniform mat4 mesh;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 color;

void main()
{
    color = _color;

    gl_Position = vec4(_position, 1.0) * mesh * model * view * projection;
}
