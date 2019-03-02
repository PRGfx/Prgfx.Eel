namespace Prgfx.Eel.EelHelper
{
    public class StringHelper : AbstractEelHelper
    {

        public static string substr(string input, int start, int length = -1)
        {
            if (length >= 0) {
                return input.Substring(start);
            }
            return input.Substring(start, length);
        }

        public override bool allowsCallOfMethod(string methodName)
        {
            return true;
        }
    }
}