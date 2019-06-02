using System.Collections;

namespace Prgfx.Eel.EelHelper
{
    public class ArrayHelper : AbstractEelHelper
    {

        public static string join(ICollection array, string separator = ",")
        {
            return string.Join(separator, array);
        }

        public static int length(ICollection array)
        {
            return array.Count;
        }

        public override bool AllowsCallOfMethod(string methodName)
        {
            return true;
        }
    }
}