namespace Prgfx.Eel.EelHelper
{
    public class StringHelper : AbstractEelHelper
    {

        public static string substr(string input, int start, int length = -1)
        {
            if (start < 0) {
                start = System.Math.Max(0, input.Length + start);
            }
            if (length < 0) {
                return input.Substring(start);
            }
            return input.Substring(start, System.Math.Min(length, input.Length - start));
        }

        public override bool AllowsCallOfMethod(string methodName)
        {
            return true;
        }
    }
}