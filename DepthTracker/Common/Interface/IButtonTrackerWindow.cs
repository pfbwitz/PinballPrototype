using System.Collections.Generic;
using WindowsInput.Native;

namespace DepthTracker.Common.Interface
{
    public interface IButtonTrackerWindow
    {
        Dictionary<VirtualKeyCode, bool> Keys { get; }
    }
}
