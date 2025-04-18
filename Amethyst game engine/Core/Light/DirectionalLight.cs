﻿using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Core.Light;

[StructLayout(LayoutKind.Explicit, Size = 32)]
internal struct DirectionalLight
{
    [FieldOffset(0)]
    public Vector3 direction;

    [FieldOffset(16)]
    public Vector3 color;

    [FieldOffset(28)]
    public float intensity;
}
