using DepthTracker.Common.Enum;
using DepthTracker.Common.Interface;
using WindowsInput;
using WindowsInput.Native;

namespace DepthTracker.Common.Worker
{
    public static class ButtonWorker
    {
        public static void PushButton(this IButtonTrackerWindow window, VirtualKeyCode key, 
            ButtonDirection buttonDirection, InputSimulator simulator)
        {
            if (key != VirtualKeyCode.RETURN && key != VirtualKeyCode.LEFT && key != VirtualKeyCode.RIGHT)
                return;

            switch (buttonDirection)
            {
                case ButtonDirection.Up:
                    window.Keys[key] = false;
                    simulator.Keyboard.KeyUp(key);
                    break;
                case ButtonDirection.Down:
                    simulator.Keyboard.KeyDown(key);
                    break;
            }
        }
    }
}
