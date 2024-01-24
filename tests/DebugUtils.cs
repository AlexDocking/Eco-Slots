using Eco.Shared.Localization;
using Eco.Shared.Utils;
namespace Parts.Tests
{
    public class DebugUtils
    {
        public static bool AssertEquals<T>(T expected, T actual, string message = null) => Assert(object.Equals(expected, actual), message + $":'{actual}' is not equal to '{expected}'");
        public static bool Assert(bool val, string message)
        {
            if (!val)
            {
                Fail(message);
                return false;
            }
            return true;
        }
        public static void Fail(string message)
        {
            if (string.IsNullOrEmpty(message)) message = "<no message>";
            message += "\n" + System.Environment.StackTrace;
            Log.WriteErrorLine(Localizer.Do($"Assertion failed: {message}"));
        }
    }
}
