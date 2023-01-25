﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace OsEngine.Layout
{
    public class MouseCoordinates
    {

        public static double YmousePos(System.Windows.Window ui)
        {
            var point = System.Windows.Forms.Control.MousePosition;
            Point newPoint = new Point(point.X, point.Y);

            var transform = PresentationSource.FromVisual(ui).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(newPoint);

            return mouse.Y;
        }

        public static double XmousePos(System.Windows.Window ui)
        {
            var point = System.Windows.Forms.Control.MousePosition;
            Point newPoint = new Point(point.X, point.Y);

            var transform = PresentationSource.FromVisual(ui).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(newPoint);

            return mouse.X;
        }

    }
    public struct POINT
    {
        public int X;
        public int Y;
    }
}
