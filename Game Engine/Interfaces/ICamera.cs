using System.Drawing;

namespace Game_Engine.Interfaces
{
    internal interface ICamera
    {
        public SizeF GlobalSize { get; }
        public PointF GlobalPos { get; }

        public SizeF ViewSize { get; }
        public PointF ViewPos { get; }

        public void DrawViewField();
    }
}
