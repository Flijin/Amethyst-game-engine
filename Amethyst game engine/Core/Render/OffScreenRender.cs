using Amethyst_game_engine.Core.CameraModule;
using Amethyst_game_engine.Core.GameObjects;
using Amethyst_game_engine.Core.GameObjects.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

namespace Amethyst_game_engine.Core.Render;

internal class OffScreenRender
{
    private readonly int _fbo;
    private readonly int _colorTexture;
    private readonly int _depthRenderbuffer;
    private readonly int _width;
    private readonly int _height;

    public OffScreenRender(int width, int height)
    {
        _width = width;
        _height = height;

        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

        _colorTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _colorTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _colorTexture, 0);

        _depthRenderbuffer = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRenderbuffer);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthRenderbuffer);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            throw new Exception("FBO is not ready");

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public Bitmap Render(DrawableObject obj)
    {
        var tempCamera = CreatePreviewCamera(obj);
        int previousFBO = GL.GetInteger(GetPName.DrawFramebufferBinding);

        int[] previousViewport = new int[4];
        GL.GetInteger(GetPName.Viewport, previousViewport);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.Viewport(0, 0, _width, _height);

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        obj.DrawObjectWithCamera(tempCamera);

        Bitmap bmp = new(_width, _height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        BitmapData data = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        GL.ReadPixels(0, 0, _width, _height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

        bmp.UnlockBits(data);

        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFBO);
        GL.Viewport(previousViewport[0], previousViewport[1], previousViewport[2], previousViewport[3]);

        return bmp;
    }

    private static Camera CreatePreviewCamera(DrawableObject obj, float fov = 45f)
    {
        BoundingBox worldBox = obj.Transform.Box;
        Vector3 center = worldBox.Center;
        float radius = worldBox.Size.Length / 2.0f;

        float fovRad = fov * MathF.PI / 180.0f;
        float distance = radius / MathF.Tan(fovRad / 2.0f);
        distance *= 0.9f;

        float horizontalAngle = 45.0f;
        float verticalAngle = 25.0f;

        float hRad = horizontalAngle * MathF.PI / 180.0f;
        float vRad = verticalAngle * MathF.PI / 180.0f;

        Vector3 cameraPosition = center + new Vector3(
            MathF.Cos(hRad) * MathF.Cos(vRad) * distance,
            MathF.Sin(vRad) * distance,
            MathF.Sin(hRad) * MathF.Cos(vRad) * distance
        );

        Camera camera = new(CameraTypes.Perspective, cameraPosition, 1.0f)
        {
            Fov = fov,
            Near = 0.01f,
            Far = distance + radius * 2.0f
        };

        Vector3 direction = center - cameraPosition;
        camera.Yaw = MathF.Atan2(direction.X, direction.Z) * 180.0f / MathF.PI;
        camera.Pitch = MathF.Asin(direction.Y / direction.Length) * 180.0f / MathF.PI;

        return camera;
    }

    public void Dispose()
    {
        GL.DeleteFramebuffer(_fbo);
        GL.DeleteTexture(_colorTexture);
        GL.DeleteRenderbuffer(_depthRenderbuffer);
    }
}
