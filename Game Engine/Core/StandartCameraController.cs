using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game_Engine.Core;

internal class StandartCameraController
{
    private Camera? _camera;
    private bool _isFirstMove = true;
    private Vector2 _lastMousePosition;

    public float Speed { get; set; }
    public float Sensivity { get; set; }

    public StandartCameraController(float speed, float sensivity)
    {
        Speed = speed;
        Sensivity = sensivity;

        Window.KeyPressedEvent += MoveCamera;
        Window.MouseMoveEvent += RotateCamera;
        Window.ResetFirstMoveEvent += ResetFirstMove;
    }

    public void BindCamera(Camera cam) => _camera = cam;
    public void ResetFirstMove() => _isFirstMove = true;

    public void MoveCamera(KeyboardState inputKey, float delta)
    {
        if (_camera is not null)
        {
            if (inputKey.IsKeyDown(Keys.W)) _camera.Position += _camera.Front * Speed * delta;
            if (inputKey.IsKeyDown(Keys.S)) _camera.Position -= _camera.Front * Speed * delta;
            if (inputKey.IsKeyDown(Keys.A)) _camera.Position -= Vector3.Normalize(Vector3.Cross(_camera.Front, Vector3.UnitY)) * Speed * delta;
            if (inputKey.IsKeyDown(Keys.D)) _camera.Position += Vector3.Normalize(Vector3.Cross(_camera.Front, Vector3.UnitY)) * Speed * delta;
            if (inputKey.IsKeyDown(Keys.Space)) _camera.Position += Vector3.UnitY * Speed * delta;
            if (inputKey.IsKeyDown(Keys.LeftShift)) _camera.Position -= Vector3.UnitY * Speed * delta;
        }
    }

    public void RotateCamera(Vector2 pos)
    {
        if (_isFirstMove == true)
        {
            _isFirstMove = false;
            _lastMousePosition = pos;
        }
        else
        {
            var delta = _lastMousePosition - pos;
            _lastMousePosition = pos;

            if (_camera is not null)
            {
                _camera.Yaw -= delta.X * Sensivity;
                _camera.Pitch += delta.Y * Sensivity;
            }
        }
    }
}
