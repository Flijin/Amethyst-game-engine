using Amethyst_game_engine.Core.Managers;
using System.Drawing;
using System.Windows.Forms;

namespace Amethyst_game_engine.Core;

internal partial class EditorWindow : Form
{
    private static Color PANEL_COLOR = Color.FromArgb(40, 40, 40);
    private static Color BUTTONS_COLOR = Color.FromArgb(60, 60, 60);

    private SceneManager _sceneManager;

    private Panel _mainPanel;

    protected override bool ShowWithoutActivation => true;

    public EditorWindow(SceneManager manager)
    {
        _mainPanel = new()
        {
            BackColor = PANEL_COLOR,
            Size = new Size(400, 800),
            Location = new Point(30, 50)
        };

        Controls.Add(_mainPanel);

        _sceneManager = manager;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.Black;
        TransparencyKey = Color.Black;

        SetStyle(ControlStyles.Selectable, false);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        Button testButton = new()
        {
            Size = new Size(100, 25),
            BackColor = Color.RebeccaPurple,
            ForeColor = Color.White,
            Text = "Загрузить модель",
            Location = new Point(100, 100)
        };

        Controls.Add(testButton);
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

    private void AddMainPanel()
    {
        Panel panel = new();

        panel.BackColor = PANEL_COLOR;
        panel.Size = new Size(400, 800);
        panel.Location = new Point(100, 100);

        Controls.Add(panel);
    }
}
