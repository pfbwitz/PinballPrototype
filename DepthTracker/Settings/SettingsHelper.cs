namespace DepthTracker.Settings
{
    public static class SettingsHelper<T> where T : ISettings, new()
    {
        public static T GetInstance()
        {
            return new T();
        }
    }
}
