using Amethyst_game_engine.CameraModule;
using Amethyst_game_engine.Core.CameraModule;
using Amethyst_game_engine.Core.GameObjects;
using Amethyst_game_engine.Core.Managers;
using Amethyst_game_engine.Core.Render;
using Amethyst_game_engine.Models.GLBModule;
using Amethyst_game_engine.Models.STLModule;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Panel = System.Windows.Forms.Panel;

namespace Amethyst_game_engine.Core;

internal partial class EditorWindow : Form
{
    private struct GameObjectData
    {
        public string Tag { get; set; }
        public Guid ID { get; set; }
        public Bitmap Preview { get; set; }
    }

    private static readonly Color PANEL_COLOR = Color.FromArgb(40, 40, 40);
    private static readonly Color PANEL_COLOR_2 = Color.FromArgb(65, 65, 65);
    private static readonly Color BUTTONS_PRESSED_COLOR = Color.FromArgb(70, 70, 70);

    private readonly GameObjectManager _gameObjectManager;
    private readonly FreeCameraController _editorController;
    private readonly GameApplication _app;
    private readonly Camera _cam;

    private readonly Label _fpsLabel = new();
    private readonly Panel _gameObjectsPanel = new();
    private readonly int _gameObjectsElementPadding = 15;
    private int _gameObjectsElementOffset = 30;

    private Panel _mainPanel = new();
    private Panel? _objectInspector = null;

    private readonly OffScreenRender _renderer;
    private readonly List<Panel> _gameObjectsItems = [];

    protected override bool ShowWithoutActivation => true;

    public EditorWindow(SceneManager manager)
    {
        _app = manager.Application!;
        _gameObjectManager = manager.CurrentScene!.GameObjectManager;

        _renderer = new(256, 256);

        _cam = new Camera(CameraTypes.Perspective, Vector3.Zero, _app.WindowAspectRatio)
        {
            Far = 1000.0f,
            Near = 0.03f
        };

        _app.ShadingModel = Render.Settings.ShadingModels.Unlit;

        _editorController = new(3.0f, 0.2f);
        _editorController.BindCamera(_cam);
        manager.CurrentScene!.CameraManager.AddCamera(_cam);

        _app.MouseDown += OnMouseDown;
        _app.KeyDown += OnKeyDown;
        _app.MouseMove += OnMouseMove;
        _app.UpdateFrame += FixedUpdate;
        _app.MouseUp += OnMouseUp;

        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.FromArgb(0, 0, 1);
        TransparencyKey = Color.FromArgb(0, 0, 1);

        SetStyle(ControlStyles.Selectable, false);
        System.Windows.Forms.Timer timer = new();

        timer.Tick += TimerTick;
        timer.Start();
    }

    public void InitGameObjects()
    {
        var gameObjects = _gameObjectManager.GameObjects;

        for (int i = 0; i < gameObjects.Count; i++)
        {
            AddGameObjectToPanel(gameObjects[i].Tag!, _renderer.Render(gameObjects[i]), gameObjects[i].ID);
        }
    }

    private void OnMouseUp(MouseButtonEventArgs e) => _app.CursorState = CursorState.Normal;

    private void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Middle)
        {
            _editorController.ResetFirstMove();
            _app.CursorState = CursorState.Grabbed;
        }
    }

    private void OnMouseMove(MouseMoveEventArgs e)
    {
        if (_app.MouseState.IsButtonDown(MouseButton.Middle))
            _editorController.RotateCamera(e.Position);
    }

    private void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)
        {
            _app.CursorState = CursorState.Normal;
            _editorController.ResetFirstMove();
        }
    }

    private void FixedUpdate(FrameEventArgs e)
    {
        _editorController.MoveCamera(_app.KeyboardState, (float)e.Time);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _fpsLabel.Font = new Font(_fpsLabel.Font.FontFamily, 16.0f);
        _fpsLabel.ForeColor = Color.Red;
        _fpsLabel.Location = new Point(Width - 300, 50);
        _fpsLabel.AutoSize = true;

        Controls.Add(_fpsLabel);
        _mainPanel = CreateMainPanel();
        Controls.Add(_mainPanel);
    }

    private void OnLoadButtonClick(object? sender, EventArgs e)
    {
        using OpenFileDialog openFileDialog = new();

        openFileDialog.Title = "Выберите файл модели";
        openFileDialog.Filter = "GLB files (*.glb)|*.glb|STL files (*.stl)|*.stl|All files (*.*)|*.*";
        openFileDialog.FilterIndex = 1;
        openFileDialog.InitialDirectory = Environment.CurrentDirectory;
        openFileDialog.Multiselect = false;

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = openFileDialog.FileName;
            AddGameObject(filePath);
        }

        void AddGameObject(string modelPath)
        {
            if (modelPath.Contains(".glb"))
            {
                var scene = GLBImporter.LoadModel(modelPath)![0];

                foreach (var model in scene.Models)
                {
                    GLBGameObject gameObj = new(model)
                    {
                        Tag = "Untagged"
                    };

                    _gameObjectManager.AddGameObject(gameObj);

                    BeginInvoke(new Action(() =>
                    {
                        AddGameObjectToPanel(gameObj.Tag, _renderer.Render(gameObj), gameObj.ID);    
                    }));
                }
            }
            else
            {
                var model = STLImporter.LoadModel(modelPath);

                STLGameObject gameObj = new(model!)
                {
                    Tag = "Untagged",
                };

                _gameObjectManager.AddGameObject(gameObj);

                BeginInvoke(new Action(() =>
                {
                    AddGameObjectToPanel(gameObj.Tag, _renderer.Render(gameObj), gameObj.ID);
                }));
            }
        }
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        _fpsLabel.Text = $"FPS: {_app.FPS}";
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= 0x80000;
            cp.ExStyle |= 0x80;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x84;
        const int HTTRANSPARENT = -1;
        const int HTCLIENT = 1;

        if (m.Msg == WM_NCHITTEST)
        {
            int x = (int)(m.LParam.ToInt64() & 0xFFFF);
            int y = (int)((m.LParam.ToInt64() >> 16) & 0xFFFF);
            Point clientPoint = PointToClient(new Point(x, y));

            Control? control = GetChildAtPoint(clientPoint);

            if (control != null && control != this)
            {
                m.Result = HTCLIENT;
                return;
            }
            else
            {
                m.Result = HTTRANSPARENT;
                return;
            }
        }

        base.WndProc(ref m);
    }

    private void UpdateObjectTag(Guid id, string newTag)
    {
        var item = _gameObjectsItems.Where((panel) => (Guid)panel.Tag! == id).First();
        var targetControl = item.Controls.OfType<Label>().First();
        targetControl.Text = newTag;
    }

    private void RemoveObjectFromPanel(Guid id)
    {
        var targetContlrol = _gameObjectsItems.Where((panel) => (Guid)panel.Tag! == id).First();
        _gameObjectsPanel.Controls.Remove(targetContlrol);

        UpdateObjectsItems();
    }

    private void UpdateObjectsItems()
    {
        _gameObjectsElementOffset = 0;

        foreach (Control item in _gameObjectsPanel.Controls)
        {
            item.Location = new Point(5, _gameObjectsElementOffset);
            _gameObjectsElementOffset += item.Height + _gameObjectsElementPadding;
        }
    }

    private void AddGameObjectToPanel(string tag, Bitmap preview, Guid id)
    {
        int imageSize = 128;

        Panel itemPanel = new()
        {
            Size = new Size(_gameObjectsPanel.Width - 36, imageSize + 20),
            Location = new Point(5, _gameObjectsElementOffset),
            BorderStyle = BorderStyle.None,
            BackColor = PANEL_COLOR,
            Tag = id
        };

        _ = new PictureBox()
        {
            BackgroundImage = new Bitmap(preview, imageSize, imageSize),
            SizeMode = PictureBoxSizeMode.AutoSize,
            Size = new Size(imageSize, imageSize),
            Location = new Point(5, 5),
            Parent = itemPanel,
        };

        _ = new Label()
        {
            Text = tag,
            ForeColor = Color.White,
            Location = new Point(imageSize + 10, (imageSize - 20) / 2),
            AutoSize = true,
            Parent = itemPanel
        };

        itemPanel.Click += (s, e) =>
        {
            if (_objectInspector is not null)
            {
                Controls.Remove(_objectInspector);
                _objectInspector = null;
            }

            _objectInspector = CreateInspectorPanel(
                _gameObjectManager.GameObjects.Where(obj => obj.ID == (Guid)itemPanel.Tag).First(),
                preview);

            Panel inspectorTopPanel = null!;
            Panel inspectorBottomPanel = null!;

            foreach (Control control in _objectInspector.Controls)
            {
                if (control.Tag is not null && control.Tag.ToString() == "TopPanel")
                    inspectorTopPanel = (Panel)control;
                else if (control.Tag is not null && control.Tag.ToString() == "BottomPanel")
                    inspectorBottomPanel = (Panel)control;
            }

            foreach (Control control in inspectorTopPanel.Controls)
            {
                if (control.Tag is not null)
                {
                    if (control.Tag.ToString() == "BackButton")
                    {
                        control.Click += BackClick;
                    }
                    else if (control.Tag.ToString() == "DeleteButton")
                    {
                        control.Click += DeleteObjectClick;
                    }
                }
            }

            foreach (Control control in inspectorBottomPanel.Controls)
            {
                if (control.Tag is not null && control.Tag.ToString() == "TagBox")
                    ((TextBox)control).TextChanged += (s, e) => UpdateObjectTag((Guid)_objectInspector.Tag!, control.Text);
            }

            Controls.Add(_objectInspector);
        };

        void BackClick(object? sender, EventArgs e)
        {
            Controls.Remove(_objectInspector);
            _objectInspector = null;
        }

        void DeleteObjectClick(object? sender, EventArgs e)
        {
            var objectId = (Guid)_objectInspector!.Tag!;

            RemoveObjectFromPanel(objectId);
            _gameObjectManager.RemoveGameObjects(obj => obj.ID == objectId);

            Controls.Remove(_objectInspector);
            _objectInspector = null;
        }

        _gameObjectsPanel.Controls.Add(itemPanel);
        _gameObjectsElementOffset += itemPanel.Height + _gameObjectsElementPadding;
        _gameObjectsItems.Add(itemPanel);
    }

    private static void TextBoxKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        if (e.KeyChar == (char)System.Windows.Forms.Keys.Back)
            return;

        if (e.KeyChar == '-')
        {
            if (textBox.Text.Contains('-') || textBox.SelectionStart != 0)
                e.Handled = true;
            return;
        }

        if (e.KeyChar == ',')
        {
            if (textBox.Text.Contains(','))
                e.Handled = true;
            return;
        }

        if (!char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private Panel CreateMainPanel()
    {
        Panel mainPanel = new()
        {
            BackColor = PANEL_COLOR,
            Size = new Size(400, 1000),
            Location = new Point(0, 35),
            BorderStyle = BorderStyle.FixedSingle,
        };

        Panel topPanel = new()
        {
            BackColor = PANEL_COLOR_2,
            Size = new Size(350, 125),
            Location = new Point(25, 25),
            Parent = mainPanel,
            BorderStyle = BorderStyle.None
        };

        Button loadModelButton = new()
        {
            Location = new Point(75, 36),
            Size = new Size(200, 50),
            BackColor = PANEL_COLOR,
            Text = "Загрузить модель",
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Parent = topPanel
        };

        loadModelButton.FlatAppearance.BorderSize = 0;

        loadModelButton.MouseUp += (s, e) => loadModelButton.BackColor = PANEL_COLOR_2;
        loadModelButton.MouseDown += (s, e) => loadModelButton.BackColor = BUTTONS_PRESSED_COLOR;
        loadModelButton.Click += OnLoadButtonClick;

        Panel cameraPanel = new()
        {
            Location = new Point(25, 200),
            Size = new Size(350, 300),
            BackColor = PANEL_COLOR_2,
            Parent = mainPanel
        };

        Label cameraPanelLable = new()
        {
            ForeColor = Color.White,
            Text = "Настройки камеры",
            Parent = cameraPanel,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.TopCenter
        };

        _gameObjectsPanel.Location = new Point(25, 550);
        _gameObjectsPanel.Size = new Size(350, 400);
        _gameObjectsPanel.BackColor = PANEL_COLOR_2;
        _gameObjectsPanel.Parent = mainPanel;
        _gameObjectsPanel.AutoScroll = true;
        _gameObjectsPanel.AutoScrollMinSize = new Size(0, 0);
        _gameObjectsPanel.HorizontalScroll.Enabled = false;
        _gameObjectsPanel.HorizontalScroll.Visible = false;


        Label ObjectsPanelLable = new()
        {
            ForeColor = Color.White,
            Text = "Объекты сцены",
            Parent = _gameObjectsPanel,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.TopCenter
        };

        TextBox far = new()
        {
            Width = 160,
            Height = 50,
            BackColor = PANEL_COLOR,
            Location = new Point(170, 80),
            Parent = cameraPanel,
            ForeColor = Color.White,
            Text = _cam.Far.ToString(),
        };

        far.TextChanged += (s, e) =>
        {
            if (float.TryParse(far.Text, out float result))
                _cam.Far = result;
        };

        far.KeyPress += TextBoxKeyPress;

        TextBox near = new()
        {
            Width = 160,
            Height = 50,
            BackColor = PANEL_COLOR,
            Location = new Point(170, 120),
            Parent = cameraPanel,
            ForeColor = Color.White,
            Text = _cam.Near.ToString(),
        };

        near.TextChanged += (s, e) =>
        {
            if (float.TryParse(near.Text, out float result))
                _cam.Near = result;
        };

        near.KeyPress += TextBoxKeyPress;

        TextBox speed = new()
        {
            Width = 160,
            Height = 50,
            BackColor = PANEL_COLOR,
            Location = new Point(170, 160),
            Parent = cameraPanel,
            ForeColor = Color.White,
            Text = _editorController.Speed.ToString(),
        };

        speed.TextChanged += (s, e) =>
        {
            if (float.TryParse(speed.Text, out float result))
                _editorController.Speed = result;
        };

        speed.KeyPress += TextBoxKeyPress;

        TextBox sensivity = new()
        {
            Width = 160,
            Height = 50,
            BackColor = PANEL_COLOR,
            Location = new Point(170, 200),
            Parent = cameraPanel,
            ForeColor = Color.White,
            Text = _editorController.Sensivity.ToString(),
        };

        sensivity.TextChanged += (s, e) =>
        {
            if (float.TryParse(sensivity.Text, out float result))
                _editorController.Sensivity = result;
        };

        sensivity.KeyPress += TextBoxKeyPress;

        Label farLabel = new()
        {
            AutoSize = true,
            Text = "Дальняя граница",
            Parent = cameraPanel,
            ForeColor = Color.White,
            Location = new Point(0, 81)
        };

        Label nearLabel = new()
        {
            AutoSize = true,
            Text = "Ближняя граница",
            Parent = cameraPanel,
            ForeColor = Color.White,
            Location = new Point(0, 121)
        };

        Label speedLabel = new()
        {
            AutoSize = true,
            Text = "Скорость",
            Parent = cameraPanel,
            ForeColor = Color.White,
            Location = new Point(0, 161)
        };

        Label sensivityLabel = new()
        {
            AutoSize = true,
            Text = "Чувствительность",
            Parent = cameraPanel,
            ForeColor = Color.White,
            Location = new Point(0, 201)
        };

        return mainPanel;
    }

    private static Panel CreateInspectorPanel(DrawableObject obj, Bitmap preview)
    {
        Panel result = new()
        {
            BackColor = PANEL_COLOR,
            Location = new Point(415, 35),
            Size = new Size(700, 390),
            BorderStyle = BorderStyle.FixedSingle,
            Tag = obj.ID
        };

        Panel topPanel = new()
        {
            BackColor = PANEL_COLOR_2,
            Location = new Point(30, 30),
            Size = new Size(640, 100),
            Parent = result,
            Tag = "TopPanel"
        };

        Panel bottomPanel = new()
        {
            BackColor = PANEL_COLOR_2,
            Location = new Point(30, 145),
            Size = new Size(640, 210),
            Parent = result,
            Tag = "BottomPanel"
        };

        Button backButton = new()
        {
            BackColor = PANEL_COLOR,
            Location = new Point(15, 15),
            Size = new Size(200, 70),
            Parent = topPanel,
            Text = "Назад",
            ForeColor = Color.White,
            Tag = "BackButton"
        };

        Button deleteButton = new()
        {
            BackColor = PANEL_COLOR,
            Location = new Point(230, 15),
            Size = new Size(200, 70),
            Parent = topPanel,
            Text = "Удалить объект",
            ForeColor = Color.White,
            Tag = "DeleteButton"
        };

        CheckBox visibleBox = new()
        {
            Location = new Point(525, 25),
            Parent = topPanel,
            Checked = obj.Visible
        };

        _ = new Label()
        {
            Location = new Point(450, 55),
            AutoSize = true,
            Parent = topPanel,
            Text = "Отображать объект",
            ForeColor = Color.White
        };

        _ = new Label()
        {
            Location = new Point(15, 15),
            AutoSize = true,
            Parent = bottomPanel,
            Text = "Позиция",
            ForeColor = Color.White
        };

        _ = new Label()
        {
            Location = new Point(15, 65),
            AutoSize = true,
            Parent = bottomPanel,
            Text = "Поворот",
            ForeColor = Color.White
        };

        _ = new Label()
        {
            Location = new Point(15, 115),
            AutoSize = true,
            Parent = bottomPanel,
            Text = "Масштаб",
            ForeColor = Color.White
        };

        _ = new Label()
        {
            Location = new Point(62, 165),
            AutoSize = true,
            Parent = bottomPanel,
            Text = "Тэг",
            ForeColor = Color.White
        };

        var verticalOffset = 0;

        for (int i = 0; i < 3; i++)
        {
            _ = new Label()
            {
                Location = new Point(120, 15 + verticalOffset),
                AutoSize = true,
                Parent = bottomPanel,
                Text = "X",
                ForeColor = Color.White
            };

            verticalOffset += 50;
        }

        verticalOffset = 0;
        for (int i = 0; i < 3; i++)
        {
            _ = new Label()
            {
                Location = new Point(220, 15 + verticalOffset),
                AutoSize = true,
                Parent = bottomPanel,
                Text = "Y",
                ForeColor = Color.White
            };

            verticalOffset += 50;
        }


        verticalOffset = 0;
        for (int i = 0; i < 3; i++)
        {
            _ = new Label()
            {
                Location = new Point(320, 15 + verticalOffset),
                AutoSize = true,
                Parent = bottomPanel,
                Text = "Z",
                ForeColor = Color.White
            };

            verticalOffset += 50;
        }

        TextBox positionXBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(150, 15),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Position.X.ToString()
        };

        TextBox rotationXBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(150, 65),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Rotation.X.ToString()
        };

        TextBox scaleXBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(150, 115),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Scale.X.ToString()
        };

        TextBox positionYBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(250, 15),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Position.Y.ToString()
        };

        TextBox rotationYBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(250, 65),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Rotation.Y.ToString()
        };

        TextBox scaleYBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(250, 115),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Scale.Y.ToString()
        };

        TextBox positionZBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(350, 15),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Position.Z.ToString()
        };

        TextBox rotationZBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(350, 65),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Rotation.Z.ToString()
        };

        TextBox scaleZBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(350, 115),
            Size = new Size(60, 20),
            Parent = bottomPanel,
            Text = obj.Transform.Scale.Z.ToString()
        };

        TextBox tagBox = new()
        {
            BackColor = PANEL_COLOR,
            ForeColor = Color.White,
            Location = new Point(112, 165),
            Size = new Size(150, 20),
            Parent = bottomPanel,
            Text = obj.Tag,
            Tag = "TagBox"
        };

        PictureBox previewBox = new()
        {
            BackColor = PANEL_COLOR,
            Location = new Point(446, 15),
            Size = new Size(180, 180),
            Parent = bottomPanel,
            BackgroundImage = new Bitmap(preview, 180, 180),
            SizeMode = PictureBoxSizeMode.AutoSize
        };

        positionXBox.KeyPress += TextBoxKeyPress;
        positionXBox.TextChanged += (s, e) =>
        {
            var pos = obj.Transform.Position;

            if (float.TryParse(positionXBox.Text, out float result))
                obj.Transform.Position = new(result, pos.Y, pos.Z);
        };

        positionYBox.KeyPress += TextBoxKeyPress;
        positionYBox.TextChanged += (s, e) =>
        {
            var pos = obj.Transform.Position;

            if (float.TryParse(positionYBox.Text, out float result))
                obj.Transform.Position = new(pos.X, result, pos.Z);
        };

        positionZBox.KeyPress += TextBoxKeyPress;
        positionZBox.TextChanged += (s, e) =>
        {
            var pos = obj.Transform.Position;

            if (float.TryParse(positionZBox.Text, out float result))
                obj.Transform.Position = new(pos.X, pos.Y, result);
        };

        rotationXBox.KeyPress += TextBoxKeyPress;
        rotationXBox.TextChanged += (s, e) =>
        {
            var rot = obj.Transform.Rotation;

            if (float.TryParse(rotationXBox.Text, out float result))
                obj.Transform.Rotation = new(result, rot.Y, rot.Z);
        };

        rotationYBox.KeyPress += TextBoxKeyPress;
        rotationYBox.TextChanged += (s, e) =>
        {
            var rot = obj.Transform.Rotation;

            if (float.TryParse(rotationYBox.Text, out float result))
                obj.Transform.Rotation = new(rot.X, result, rot.Z);
        };

        rotationZBox.KeyPress += TextBoxKeyPress;
        rotationZBox.TextChanged += (s, e) =>
        {
            var rot = obj.Transform.Rotation;

            if (float.TryParse(rotationZBox.Text, out float result))
                obj.Transform.Rotation = new(rot.X, rot.Y, result);
        };

        scaleXBox.KeyPress += TextBoxKeyPress;
        scaleXBox.TextChanged += (s, e) =>
        {
            var scale = obj.Transform.Scale;

            if (float.TryParse(scaleXBox.Text, out float result))
                obj.Transform.Scale = new(result, scale.Y, scale.Z);
        };

        scaleYBox.KeyPress += TextBoxKeyPress;
        scaleYBox.TextChanged += (s, e) =>
        {
            var scale = obj.Transform.Scale;

            if (float.TryParse(scaleYBox.Text, out float result))
                obj.Transform.Scale = new(scale.X, result, scale.Z);
        };

        scaleZBox.KeyPress += TextBoxKeyPress;
        scaleZBox.TextChanged += (s, e) =>
        {
            var scale = obj.Transform.Scale;

            if (float.TryParse(scaleZBox.Text, out float result))
                obj.Transform.Scale = new(scale.X, scale.Y, result);
        };

        visibleBox.CheckedChanged += (s, e) => obj.Visible = visibleBox.Checked;
        tagBox.TextChanged += (s, e) => obj.Tag = tagBox.Text;

        return result;
    }
}
