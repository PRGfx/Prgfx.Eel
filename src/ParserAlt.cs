using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Prgfx.Eel.Ast;

namespace Prgfx.Eel
{
    public class ParserAlt
    {
        public ExpressionNode Parse(string input)
        {
            var lexer = new Lexer(input);
            return ParseExpression(lexer);
        }

        private T ReturnOrBacktrack<T>(Func<Lexer, T> parser, Lexer lexer)
        {
            var snapshot = lexer.Snapshot();
            try
            {
                var result = parser.DynamicInvoke(lexer);
                if (result == null)
                {
                    lexer.Reset(snapshot);
                }
                return (T)result;
            }
            catch (ParserException)
            {
                return (T)(object)null;
            }
            catch (System.Exception)
            {
                return (T)(object)null;
            }
        }

        private T ReturnOrFail<T>(Func<Lexer, T> parser, Lexer lexer, System.Exception exception)
        {
            var result = ReturnOrBacktrack<T>(parser, lexer);
            if (result == null)
            {
                throw exception;
            }
            return result;
        }

        protected ExpressionNode ParseExpression(Lexer lexer)
        {
            SkipWhiteSpace(lexer);
            var exp = ParseConditionalExpression(lexer);
            SkipWhiteSpace(lexer);
            return new ExpressionNode() { exp = exp };
        }

        protected ConditionalExpressionNode ParseConditionalExpression(Lexer lexer)
        {
            var cond = ParseDisjunction(lexer);
            ExpressionNode success = null;
            ExpressionNode failure = null;
            if (lexer.Is('?'))
            {
                success = ParseExpression(lexer);
                if (!lexer.Is(':'))
                {
                    throw new ParserException("Missing : in ternary condition");
                }
                failure = ParseExpression(lexer);
            }
            return new ConditionalExpressionNode()
            {
                cond = cond,
                yes = success,
                no = failure,
            };
        }

        protected DisjunctionNode ParseDisjunction(Lexer lexer)
        {
            var lft = ParseConjunction(lexer);
            var rgts = new List<ConjunctionNode>();
            while (lexer.Peek(2) == "||" || lexer.Peek(3) == "or ")
            {
                lexer.Consume();
                lexer.Consume();
                var rgt = ParseConjunction(lexer);
                rgts.Add(rgt);
            }
            return new DisjunctionNode()
            {
                lft = lft,
                rgt = rgts.ToArray(),
            };
        }

        protected ConjunctionNode ParseConjunction(Lexer lexer)
        {
            var lft = ParseComparison(lexer);
            var rgts = new List<ComparisonNode>();
            while (lexer.Peek(2) == "&&" || lexer.Peek(3) == "and ")
            {
                lexer.Consume();
                lexer.Consume();
                if (lexer.Is('d'))
                {
                    lexer.Consume();
                }
                var rgt = ParseComparison(lexer);
                rgts.Add(rgt);
            }
            return new ConjunctionNode()
            {
                lft = lft,
                rgt = rgts.ToArray(),
            };
        }

        protected ComparisonNode ParseComparison(Lexer lexer)
        {
            var lft = ParseSumCalculation(lexer);
            SkipWhiteSpace(lexer);
            Match m = Regex.Match(lexer.Peek(2), "==|!=|<=|>=|<|>");
            string comp = null;
            SumCalculationNode rgt = null;
            if (m.Success)
            {
                comp = m.Groups[0].Value;
                lexer.Consume();
                if (comp.Length == 2)
                {
                    lexer.Consume();
                }
                SkipWhiteSpace(lexer);
                rgt = ParseSumCalculation(lexer);
            }
            return new ComparisonNode()
            {
                lft = lft,
                comp = comp,
                rgt = rgt,
            };
        }

        protected SumCalculationNode ParseSumCalculation(Lexer lexer)
        {
            var lft = ParseProdCalculation(lexer);
            var args = new List<NodeWithArgsArg<ProdCalculationNode>>();
            if (lexer.Peek(1) == "+" || lexer.Peek(1) == "-")
            {
                var op = lexer.Consume();
                SkipWhiteSpace(lexer);
                var rgt = ReturnOrFail(
                    ParseProdCalculation,
                    lexer,
                    new ParserException("Error parsing operand in sum expression")
                );
                SkipWhiteSpace(lexer);
                args.Add(new NodeWithArgsArg<ProdCalculationNode>()
                {
                    op = op.ToString(),
                    rgt = rgt,
                });
            }
            return new SumCalculationNode()
            {
                lft = lft,
                args = args.ToArray(),
            };
        }

        protected ProdCalculationNode ParseProdCalculation(Lexer lexer)
        {
            var lft = ParseSimpleExpression(lexer);
            var args = new List<NodeWithArgsArg<SimpleExpressionNode>>();
            SkipWhiteSpace(lexer);
            while (lexer.Is('/') || lexer.Is('*') || lexer.Is('%'))
            {
                var op = lexer.Consume();
                SkipWhiteSpace(lexer);
                var rgt = ReturnOrFail<SimpleExpressionNode>(
                    ParseSimpleExpression,
                    lexer,
                    new ParserException("Error parsing operand in product expression")
                );
                SkipWhiteSpace(lexer);
                args.Add(new NodeWithArgsArg<SimpleExpressionNode>()
                {
                    op = op.ToString(),
                    rgt = rgt,
                });
            }
            return new ProdCalculationNode()
            {
                lft = lft,
                args = args.ToArray(),
            };
        }

        protected SimpleExpressionNode ParseSimpleExpression(Lexer lexer)
        {
            var arrowFunctionTerm = ReturnOrBacktrack<ArrowFunctionNode>(ParseArrowFunction, lexer);
            if (arrowFunctionTerm != null)
            {
                return arrowFunctionTerm;
            }
            var wrappedExpressionTerm = ReturnOrBacktrack<WrappedExpressionNode>(ParseWrappedExpression, lexer);
            if (wrappedExpressionTerm != null)
            {
                return wrappedExpressionTerm;
            }
            var notExpression = ReturnOrBacktrack<NotExpressionNode>(ParseNotExpression, lexer);
            if (notExpression != null)
            {
                return notExpression;
            }
            var arrayLiteral = ReturnOrBacktrack<ArrayLiteralNode>(ParseArrayLiteral, lexer);
            if (arrayLiteral != null)
            {
                return arrayLiteral;
            }
            var objectLiteral = ReturnOrBacktrack<ObjectLiteralNode>(ParseObjectLiteral, lexer);
            if (objectLiteral != null)
            {
                return objectLiteral;
            }
            return ReturnOrFail<TermNode>(ParseTerm, lexer, new ParserException("Error parsing simple expression"));
        }

        private TermNode ParseTerm(Lexer lexer)
        {
            TermNode term = ReturnOrBacktrack<BooleanLiteralNode>(ParseBoolean, lexer);
            if (term != null)
            {
                return term;
            }
            term = ReturnOrBacktrack<NumberLiteralNode>(ParseNumberLiteral, lexer);
            if (term != null)
            {
                return term;
            }
            term = ReturnOrBacktrack<StringLiteralNode>(ParseStringLiteral, lexer);
            if (term != null)
            {
                return term;
            }
            return ReturnOrFail<ObjectPathNode>(
                ParseObjectPath,
                lexer,
                new ParserException("Could not parse Term")
            );
        }

        private ObjectPathNode ParseObjectPath(Lexer lexer)
        {
            var parts = new List<ObjectPathPartNode>();
            ObjectPathPartNode first = null;
            var methodCall = ReturnOrBacktrack<MethodCallNode>(ParseMethodCall, lexer);
            if (methodCall == null)
            {
                var identifier = ParseIdentifier(lexer);
                first = new StaticObjectPathPartNode() { name = identifier };
            }
            else
            {
                first = methodCall;
            }
            parts.Add(first);
            while (lexer.Is('.'))
            {
                ObjectPathPartNode part = null;
                part = ReturnOrBacktrack<MethodCallNode>(ParseMethodCall, lexer);
                if (part != null) {
                    parts.Add(part);
                }
                if (part == null) {
                    part = new StaticObjectPathPartNode() { name = ParseIdentifier(lexer) };
                    parts.Add(part);
                }
                part = ReturnOrBacktrack<OffsetAccessNode>(ParseOffsetAccess, lexer);
            }
            return new ObjectPathNode() { path = parts.ToArray() };
        }

        private OffsetAccessNode ParseOffsetAccess(Lexer lexer)
        {
            if (!lexer.Is('['))
            {
                return null;
            }
            lexer.Consume();
            SkipWhiteSpace(lexer);
            var expression = ParseExpression(lexer);
            if (!lexer.Is(']'))
            {
                throw new ParserException("Missing ] after offset access");
            }
            lexer.Consume();
            return new OffsetAccessNode()
            {
                exp = expression,
            };
        }

        private MethodCallNode ParseMethodCall(Lexer lexer)
        {
            var name = ParseIdentifier(lexer);
            if (!lexer.Is('('))
            {
                return null;
            }
            lexer.Consume();
            var arguments = new List<ExpressionNode>();
            arguments.Add(ParseExpression(lexer));
            while (lexer.Is(','))
            {
                arguments.Add(ParseExpression(lexer));
            }
            if (!lexer.Is(')'))
            {
                throw new ParserException("Missing ) after argument list");
            }
            lexer.Consume();
            return new MethodCallNode()
            {
                name = name,
                arguments = arguments.ToArray(),
            };
        }

        private NumberLiteralNode ParseNumberLiteral(Lexer lexer)
        {
            var sb = new System.Text.StringBuilder();
            var acceptSign = true;
            var acceptDecimals = true;
            if (!lexer.Is('-') && !lexer.IsNumeric())
            {
                throw new ParserException("Not a number");
            }
            if (lexer.Is('-'))
            {
                if (acceptSign)
                {
                    sb.Append(lexer.Consume());
                    acceptSign = false;
                }
                else
                {
                    throw new ParserException("Unexpected sign -");
                }
            }
            while (true)
            {
                if (lexer.Is('.'))
                {
                    if (!acceptDecimals)
                    {
                        throw new ParserException("Unexpected decimal separator");
                    }
                    acceptDecimals = false;
                    sb.Append(lexer.Consume());
                }
                else if (lexer.IsNumeric())
                {
                    sb.Append(lexer.Consume());
                }
                else if (lexer.IsAlpha())
                {
                    var c = lexer.Consume();
                    lexer.Rewind();
                    throw new ParserException($"Unexpected character in number: {c}");
                }
                else if (lexer.IsEnd())
                {
                    break;
                }
                else
                {
                    break;
                }
            }
            if (!float.TryParse(sb.ToString(), out float value))
            {
                return null;
            }
            return new NumberLiteralNode() { value = value };
        }

        private BooleanLiteralNode ParseBoolean(Lexer lexer)
        {
            Match m = Regex.Match(lexer.Peek(6), @"^(true|false|TRUE|FALSE)[^a-zA-Z0-9_]");
            if (m.Success)
            {
                var value = m.Groups[1].Value.ToLower();
                for (int i = 0; i < value.Length; i++)
                {
                    lexer.Consume();
                }
                return new BooleanLiteralNode()
                {
                    value = value == "true",
                };
            }
            return null;
        }

        private ObjectLiteralNode ParseObjectLiteral(Lexer lexer)
        {
            if (!lexer.Is('{'))
            {
                return null;
            }
            lexer.Consume();
            SkipWhiteSpace(lexer);
            var values = new List<ObjectLiteralPropertyNode>();
            values.Add(ParseObjectLiteralProperty(lexer));
            if (!lexer.Is('}'))
            {
                throw new ParserException("Missing } after object literal");
            }
            lexer.Consume();
            return new ObjectLiteralNode() { properties = values.ToArray() };
        }

        private ObjectLiteralPropertyNode ParseObjectLiteralProperty(Lexer lexer)
        {
            string key = null;
            var stringKey = ReturnOrBacktrack<StringLiteralNode>(ParseStringLiteral, lexer);
            if (stringKey != null)
            {
                key = stringKey.value;
            }
            if (key == null)
            {
                key = ReturnOrFail<string>(
                    ParseIdentifier,
                    lexer,
                    new ParserException("Expecting string or identifier as object key")
                );
            }
            if (!lexer.Is(':'))
            {
                throw new ParserException("Expecting : after object key");
            }
            lexer.Consume();
            var exp = ParseExpression(lexer);
            return new ObjectLiteralPropertyNode()
            {
                key = key,
                value = exp,
            };
        }

        private StringLiteralNode ParseStringLiteral(Lexer lexer)
        {
            char openingQuote = lexer.Consume();
            if (openingQuote != '\'' && openingQuote != '"')
            {
                lexer.Rewind();
                return null;
            }
            var isEscaped = false;
            var sb = new System.Text.StringBuilder();
            while (true)
            {
                if (lexer.IsEnd())
                {
                    throw new ParserException($"Unfinished string literal \"{sb.ToString()}\"");
                }
                if (lexer.Is('\\') && !isEscaped)
                {
                    isEscaped = true;
                    lexer.Consume();
                    continue;
                }
                if (lexer.Is('\'') || lexer.Is('"'))
                {
                    var closingQuote = lexer.Consume();
                    if (!isEscaped && closingQuote == openingQuote)
                    {
                        return new StringLiteralNode() { value = sb.ToString() };
                    }
                    sb.Append(closingQuote);
                    isEscaped = false;
                    continue;
                }
                sb.Append(lexer.Consume());
                isEscaped = false;
            }
        }

        private ArrayLiteralNode ParseArrayLiteral(Lexer lexer)
        {
            if (!lexer.Is('['))
            {
                return null;
            }
            lexer.Consume();
            SkipWhiteSpace(lexer);
            var values = new List<ExpressionNode>();
            values.Add(ParseExpression(lexer));
            while (lexer.Is(','))
            {
                SkipWhiteSpace(lexer);
                values.Add(ParseExpression(lexer));
            }
            if (!lexer.Is(']'))
            {
                throw new ParserException("Missing ] after array literal");
            }
            lexer.Consume();
            return new ArrayLiteralNode()
            {
                values = values.ToArray(),
            };
        }

        private NotExpressionNode ParseNotExpression(Lexer lexer)
        {
            if (lexer.Is('!'))
            {
                lexer.Consume();
                if (lexer.IsWhiteSpace())
                {
                    lexer.Rewind();
                    throw new ParserException("Unexpected '!'");
                }
                return (NotExpressionNode)ParseSimpleExpression(lexer);
            }
            return null;
        }

        protected ArrowFunctionNode ParseArrowFunction(Lexer lexer)
        {
            var arguments = ParseMethodArguments(lexer);
            if (lexer.Peek(2) != "=>")
            {
                return null;
            }
            var exp = ParseExpression(lexer);
            return new ArrowFunctionNode()
            {
                arguments = arguments,
                exp = exp,
            };
        }

        protected MethodArgumentsNode ParseMethodArguments(Lexer lexer)
        {
            var args = new List<string>();
            var allowMultiple = false;
            if (lexer.Is('('))
            {
                lexer.Consume();
                allowMultiple = true;
            }
            args.Add(ParseIdentifier(lexer));
            while (allowMultiple && lexer.Is(','))
            {
                lexer.Consume();
                args.Add(ParseIdentifier(lexer));
            }
            if (allowMultiple && !lexer.Is(')'))
            {
                throw new ParserException("Missing closing parenthesis after argument list");
            }
            return new MethodArgumentsNode()
            {
                arguments = args.ToArray(),
            };
        }

        protected WrappedExpressionNode ParseWrappedExpression(Lexer lexer)
        {
            if (lexer.Is('('))
            {
                lexer.Consume();
            }
            else
            {
                return null;
            }
            var expression = ParseExpression(lexer);
            if (!lexer.Is(')'))
            {
                throw new ParserException("Missing closing parenthesis after wrapped expression");
            }
            lexer.Consume();
            return (WrappedExpressionNode)expression;
        }

        protected string ParseIdentifier(Lexer lexer)
        {
            if (!lexer.IsAlpha() && !lexer.Is('_'))
            {
                throw new ParserException("Identifier not starting with a character");
            }
            var sB = new System.Text.StringBuilder();
            sB.Append(lexer.Consume());
            while (lexer.IsAlphaNumeric() || lexer.Is('_'))
            {
                sB.Append(lexer.Consume());
            }
            return sB.ToString();
        }

        protected void SkipWhiteSpace(Lexer lexer)
        {
            while (lexer.IsWhiteSpace())
            {
                lexer.Consume();
            }
        }
    }

    [Serializable]
    internal class ParserException : Exception
    {
        public ParserException()
        {
        }

        public ParserException(string message) : base(message)
        {
        }

        public ParserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
