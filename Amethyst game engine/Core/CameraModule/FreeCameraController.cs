using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Amethyst_game_engine.Core.CameraModule;

public class FreeCameraController
{
    private Camera? _camera;
    private Vector2 _lastMousePosition;
    private bool _isFirstMove = true;
    private float _speed;
    private float _sensivity;

    public float Speed
    {
        get => _speed;
        set => _speed = MathF.Max(0.0f, value);
    }

    public float Sensivity
    {
        get => _sensivity;
        set => _sensivity = MathF.Max(0.0f, value);
    }

    public FreeCameraController(float speed, float sensivity)
    {
        Speed = speed;
        Sensivity = sensivity;
    }

    public void BindCamera(Camera cam) => _camera = cam;
    public void ResetFirstMove() => _isFirstMove = true;
        
    public void MoveCamera(KeyboardState inputKey, float delta)
    {
        if (_camera is not null)
        {
            if (inputKey.IsKeyDown(Keys.W)) _camera.Position += _camera.Front * Speed * delta;
            if (inputKey.IsKeyDown(Keys.S)) _camera.Position -= _camera.Front * Speed * delta;
            if (inputKey.IsKeyDown(Keys.A)) _camera.Position -= _camera.RightVector * Speed * delta;
            if (inputKey.IsKeyDown(Keys.D)) _camera.Position += _camera.RightVector * Speed * delta;
            if (inputKey.IsKeyDown(Keys.Space)) _camera.Position += Vector3.UnitY * Speed * delta;
            if (inputKey.IsKeyDown(Keys.LeftShift)) _camera.Position -= Vector3.UnitY * Speed * delta;
        }
    }

    public void RotateCamera(Vector2 mousePos)
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
