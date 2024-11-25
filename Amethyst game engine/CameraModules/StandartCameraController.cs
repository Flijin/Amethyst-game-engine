using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = Amethyst_game_engine.Core.Window;

namespace Amethyst_game_engine.CameraModules;

public class StandartCameraController
{
    private Camera? _camera;
    private Vector2 _lastMousePosition;
    private bool _isFirstMove = true;
    private float _speed;
    private float _sensivity;

    public float Speed
    {
        get => _speed;

        set
        {
            if (value >= 0)
                _speed = value;
            else
                _speed = 0;
        }
    }

    public float Sensivity
    {
        get => _sensivity;

        set
        {
            if (value >= 0)
                _sensivity = value;
            else
                _sensivity = 0;
        }
    }

    public StandartCameraController(float speed, float sensivity)
    {
        Speed = speed;
        Sensivity = sensivity;

        Window.KeyPressedEvent += MoveCamera;
        Window.MouseMoveEvent += RotateCamera;
        Window.ResetFirstMoveEvent += ResetFirstMove;
    }

    internal void BindCamera(Camera cam) => _camera = cam;
    internal void ResetFirstMove() => _isFirstMove = true;

    internal void MoveCamera(KeyboardState inputKey, float delta)
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

    internal void RotateCamera(Vector2 mousePos)
    {
        if (_isFirstMove == true)
        {
            _isFirstMove = false;
            _lastMousePosition = mousePos;
        }
        else
        {
            var delta = _lastMousePosition - mousePos;
            _lastMousePosition = mousePos;

            if (_camera is not null)
            {
                _camera.Yaw -= delta.X * Sensivity;
                _camera.Pitch += delta.Y * Sensivity;
            }
        }
    }
}
