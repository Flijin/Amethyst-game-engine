using System.Runtime.InteropServices;
using Amethyst_game_engine.Core.Utilities;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.Managers;

internal unsafe sealed class ShaderStorageBufferManager<T> : IDisposable where T : struct
{
    private const int PADDING = 16;

    private void* _lightData;
    private int _lightCount;
    private int _lightCapacity;
    private int _freeSpaces;

    public int LightHandler { get; private set; }

    public ShaderStorageBufferManager(int lightCapacity, int bindingPoint)
    {
        _lightCapacity = lightCapacity;
        LightHandler = GL.GenBuffer();

        _lightData = NativeMemory.Alloc((nuint)(lightCapacity * Marshal.SizeOf<T>() + PADDING));
        *(int*)(_lightData) = 0;

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightHandler);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, lightCapacity * Marshal.SizeOf<T>() + PADDING,
                      (nint)_lightData, BufferUsageHint.DynamicDraw);

        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, LightHandler);
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void Clear()
    {
        *(int*)_lightData = 0;
        _lightCount = 0;

        GL.BufferData(BufferTarget.ShaderStorageBuffer, PADDING, (nint)_lightData, BufferUsageHint.DynamicDraw);
    }

    public void UpdateLight(T light, int index)
    {
        if (index < 0 || index >= _lightCount)
        {
            SystemCalls.PrintMessage($"Error, index of light {index} is incorrect", MessageTypes.ErrorMessage);
            return;
        }

        var offset = Marshal.SizeOf<T>() * index + PADDING;
        var dest = (nint)((byte*)_lightData + offset);
        Marshal.StructureToPtr(light, dest, false);

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightHandler);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, offset, Marshal.SizeOf<T>(), dest);
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void RemoveLight(int index)
    {
        if (index < 0 || index >= _lightCount)
        {
            SystemCalls.PrintMessage($"Error, index of light {index} is incorrect", MessageTypes.ErrorMessage);
            return;
        }

        _lightCount--;
        _freeSpaces++;

        *(int*)_lightData = _lightCount;

        var offset = Marshal.SizeOf<T>() * index + PADDING;
        var lightPtr = (byte*)_lightData + offset;
        var fieldOffset = Marshal.OffsetOf<T>("isActive").ToInt32();
        var isActivePtr = (int*)(lightPtr + fieldOffset);

        *isActivePtr = 0;

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightHandler);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, sizeof(int), (nint)_lightData);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, offset + fieldOffset, sizeof(int), (nint)isActivePtr);
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void AddLight(T light)
    {
        _lightCount++;

        var sizeofT = Marshal.SizeOf<T>();

        var dirty = false;

        if (_lightCount > _lightCapacity)
        {
            _lightCapacity *= 2;
            var newSize = (nuint)(sizeofT * _lightCapacity + PADDING);
            _lightData = NativeMemory.Realloc(_lightData, newSize);
            dirty = true;
        }

        var offset = PADDING;
        var fieldOffset = Marshal.OffsetOf<T>("isActive").ToInt32();
        var found = false;

        if (_freeSpaces > 0)
        {
            for (int i = 0; i < _lightCount; i++)
            {
                if (*(int*)((byte*)_lightData + offset + fieldOffset) == 0)
                {
                    found = true;
                    _freeSpaces--;

                    break;
                }

                offset += sizeofT;
            }
        }

        if (found == false)
            offset = sizeofT * (_lightCount - 1) + PADDING;

        var dest = (nint)((byte*)_lightData + offset);

        Marshal.StructureToPtr(light, dest, false);
        *(int*)_lightData = _lightCount;

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightHandler);

        if (dirty)
        {
            GL.BufferData(BufferTarget.ShaderStorageBuffer,
                          sizeofT * _lightCapacity + PADDING,
                          (nint)_lightData, BufferUsageHint.DynamicDraw);
        }
        else
        {
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, offset, sizeofT, dest);

            GL.BufferSubData(BufferTarget.ShaderStorageBuffer,
                             0, sizeof(int), (nint)_lightData);
        }

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void Dispose()
    {
        NativeMemory.Free(_lightData);
        GL.DeleteBuffer(LightHandler);
    }
}
