using VRage.Utils;

namespace BaseMod
{
    public static class Logger
    {
        private const string Prefix = "[Mod][BaseMod]";
        public static bool DebugEnabled;

        public static void Debug(string text)
        {
            if (DebugEnabled)
                MyLog.Default.WriteLineAndConsole(string.Format("{0} - Debug - {1}", Prefix, text));
        }

        public static void LogInfo(string text)
        {
            MyLog.Default.WriteLineAndConsole(string.Format("{0} - Info - {1}", Prefix, text));
        }
    }
}
