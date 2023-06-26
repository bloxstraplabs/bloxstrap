using System;
using System.Windows;
using System.Windows.Forms;

namespace Bloxstrap.Utility
{
    public static class WindowScaling
    {
        public static double ScaleFactor => Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;

        public static int GetScaledNumber(int number)
        {
            return (int)Math.Ceiling(number * ScaleFactor);
        }

        public static System.Drawing.Size GetScaledSize(System.Drawing.Size size)
        {
            return new System.Drawing.Size(GetScaledNumber(size.Width), GetScaledNumber(size.Height));
        }

        public static System.Drawing.Point GetScaledPoint(System.Drawing.Point point)
        {
            return new System.Drawing.Point(GetScaledNumber(point.X), GetScaledNumber(point.Y));
        }

        public static Padding GetScaledPadding(Padding padding)
        {
            return new Padding(GetScaledNumber(padding.Left), GetScaledNumber(padding.Top), GetScaledNumber(padding.Right), GetScaledNumber(padding.Bottom));
        }
    }
}
