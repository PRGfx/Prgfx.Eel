using System;
using System.Collections.Generic;

namespace Prgfx.Eel
{
    class Program
    {
        static void Main(string[] args)
        {
            var exp1 = "4+5";
            var exp2 = "variable+variable2";
            var exp3 = "String.substring('something', 2, 3)";

            var context = new Context(new Dictionary<string, object>(){
                { "String", new EelHelper.StringHelper() },
                { "variable", "foo" },
                { "variable2", "bar" },
            });
            var evaluator = new ParsingEelEvaluator();
            var res = evaluator.Evaluate(exp2, context);
            Console.WriteLine(res);
        }
    }
}
