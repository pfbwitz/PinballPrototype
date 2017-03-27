﻿using DepthTracker.Common;
using DepthTracker.UI;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DepthTrackerClicks.Common
{
    public class MouseOperations
    {
        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000001
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public static void SetCursorPosition(int X, int Y)
        {
            SetCursorPos(X, Y);
        }

        public static void SetCursorPosition(MousePoint point)
        {
            SetCursorPos(point.X, point.Y);
        }

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void MouseEvent(MouseEventFlags value)
        {
            var position = GetCursorPosition();

            mouse_event
                ((int)value,
                 position.X,
                 position.Y,
                 0,
                 0)
                ;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public static void PushButton(ButtonDirection buttonDirection, ITracker window)
        {
            try
            {
                var w = (ClicksTracker)window.Instance;
                switch (buttonDirection)
                {
                    case ButtonDirection.Up:
                        //w.TrackerMouseDown = false;
                        MouseEvent(MouseEventFlags.LeftUp);
                        Task.Run(async () => {
                            await Task.Delay(1000);
                            await w.Dispatcher.BeginInvoke(new Action(() => w.TrackerMouseDown = false));
                        });
                        break;
                    case ButtonDirection.Down:
                        w.TrackerMouseDown = true;
                        MouseEvent(MouseEventFlags.LeftDown);
                        break;
                }
            }
            catch
            {
            }
        }
    }
}