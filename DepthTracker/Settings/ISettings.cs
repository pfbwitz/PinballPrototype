﻿namespace DepthTracker.Settings
{
    public interface ISettings
    {
        int X { get; set; }

        int Y { get; set; }

        int Width { get; set; }

        int Height { get; set; }

        int ZMin { get; set; }

        int ZMax { get; set; }

        bool Flip { get; set; }

        bool Run { get; set; }

        void Save();
    }
}
