using System;
using System.Collections.Generic;

namespace Prgfx.Eel
{
    class Program
    {
        static void Main(string[] args)
        {
            var exp1 = "5 - 20 + 3";
            // var exp1 = "2 * 3 * 2";
            var exp2 = "variable + variable2";
            // var exp3 = "String.substring('something', 2, 3) + 2.34";
            var exp3 = "String.substr('something', 2, 3) + ' ' + String.substr('something', -3) + ' ' + String.substr('something', 3, 6)";
            // var exp3 = "Array.length([1, 2, 3, 'test'])";
            var exp4 = "dict.list[0]";

            var context = new Context(new Dictionary<string, object>(){
                { "String", new EelHelper.StringHelper() },
                { "Array", new EelHelper.ArrayHelper() },
                { "variable", "foo" },
                { "variable2", "bar" },
                { "dict", new Dictionary<string, object>(){
                    { "simple", 4 },
                    { "list", new List<object>(){ 1, "test" }},
                }},
            });
            var evaluator = new ParsingEelEvaluator();
            object res = null;
            // res = evaluator.Evaluate(exp1, context);
            // Console.WriteLine(res);
            // res = evaluator.Evaluate(exp2, context);
            // Console.WriteLine(res);
            res = evaluator.Evaluate(exp3, context);
            Console.WriteLine(res);
            res = evaluator.Evaluate(exp4, context);
            Console.WriteLine(res);

            // var tokenizer = new Tokenizer(exp3);
            // printTokenList(tokenizer.GetTokens());
        }

        static void printTokenList(List<Token> tokenList)
        {
            foreach (var token in tokenList)
            {
                Console.Write(token.Type);
                if (token.Payload!= null) {
                    Console.Write(": " + token.Payload);
                }
                Console.Write(", ");
            }
            Console.Write('\n');
        }
    }
}
