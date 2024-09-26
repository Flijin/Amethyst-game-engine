﻿#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;

out vec4 Color;

void main()
{
    gl_Position = vec4(aPosition, 1.0f);
    Color = vec4(aColor, 1.0f);
}