﻿using System;
using System.Windows;
using DepthTracker.Common.Interface;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace DepthTracker.UI
{
    public partial class Overlay : Window
    {
        public ITracker Tracker { get; private set; }

        public Overlay()
        {
            WindowStyle = WindowStyle.None;
            IsHitTestVisible = false;
            Topmost = true;
            AllowsTransparency = true;
            
            Deactivated += Window_Deactivated;

            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowsServices.SetWindowExTransparent(new WindowInteropHelper(this).Handle);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            ((Window)sender).Topmost = true;
        }
    }

    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
