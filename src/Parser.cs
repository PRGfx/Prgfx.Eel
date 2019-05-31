using System.Collections.Generic;
using Prgfx.Eel.Ast;

namespace Prgfx.Eel
{
    class Parser
    {
        protected List<Token> tokens;

        protected int currentToken;

        public Parser(string input)
        {
            var tokenizer = new Tokenizer(input);
            tokens = tokenizer.GetTokens();
            currentToken = -1;
        }

        protected Token NextToken()
        {
            var nextToken = PeekToken();
            if (nextToken.Type != TokenType.EOF)
            {
                currentToken++;
            }
            return nextToken;
        }

        protected void RewindToken()
        {
            currentToken--;
            if (currentToken < -1)
            {
                currentToken = -1;
            }
        }

        /// <summary>
        /// we create a dummy token so we don't have to handle null everywhere
        /// </summary>
        protected Token PeekToken()
        {
            return currentToken < tokens.Count - 1 ? tokens[currentToken + 1] : new Token(TokenType.EOF);
        }

        public ExpressionNode Parse()
        {
            return ParseExpression();
        }

        protected ExpressionNode ParseExpression()
        {
            var exp = ParseConditionalExpression();
            return new ExpressionNode() { exp = exp };
        }

        protected ConditionalExpressionNode ParseConditionalExpression()
        {
            var cond = ParseDisjunction();
            ExpressionNode success = null;
            ExpressionNode failure = null;
            if (PeekToken().Type == TokenType.QUESTIONMARK)
            {
                success = ParseExpression();
                if (NextToken().Type != TokenType.COLON)
                {
                    throw new ParserException("Missing : in ternary condition");
                }
                failure = ParseExpression();
            }
            return new ConditionalExpressionNode()
            {
                cond = cond,
                yes = success,
                no = failure,
            };
        }

        protected DisjunctionNode ParseDisjunction()
        {
            var lft = ParseConjunction();
            var rgts = new List<ConjunctionNode>();
            while (PeekToken().Type == TokenType.OP_OR)
            {
                NextToken();
                var rgt = ParseConjunction();
                rgts.Add(rgt);
            }
            return new DisjunctionNode()
            {
                lft = lft,
                rgt = rgts.ToArray(),
            };
        }

        protected ConjunctionNode ParseConjunction()
        {
            var lft = ParseComparison();
            var rgts = new List<ComparisonNode>();
            while (PeekToken().Type == TokenType.OP_AND)
            {
                var rgt = ParseComparison();
                rgts.Add(rgt);
            }
            return new ConjunctionNode()
            {
                lft = lft,
                rgt = rgts.ToArray(),
            };
        }

        protected ComparisonNode ParseComparison()
        {
            var lft = ParseSumCalculation();
            string comparisonOperator = null;
            switch (PeekToken().Type)
            {
                case TokenType.OP_EQUAL:
                    comparisonOperator = "==";
                    break;
                case TokenType.OP_NOTEQUAL:
                    comparisonOperator = "!=";
                    break;
                case TokenType.OP_LESS:
                    comparisonOperator = "<";
                    break;
                case TokenType.OP_LESSEQUAL:
                    comparisonOperator = "<=";
                    break;
                case TokenType.OP_GREATER:
                    comparisonOperator = ">";
                    break;
                case TokenType.OP_GREATEREQUAL:
                    comparisonOperator = ">=";
                    break;
            }
            SumCalculationNode rgt = null;
            if (comparisonOperator != null)
            {
                NextToken();
                rgt = ParseSumCalculation();
            }
            return new ComparisonNode()
            {
                lft = lft,
                comp = comparisonOperator,
                rgt = rgt,
            };
        }

        protected SumCalculationNode ParseSumCalculation()
        {
            var lft = ParseProdCalculation();
            var args = new List<NodeWithArgsArg<ProdCalculationNode>>();
            string op = null;
            do
            {
                op = null;
                var next = PeekToken();
                if (next.Type == TokenType.OP_PLUS)
                {
                    op = "+";
                }
                else if (next.Type == TokenType.OP_MINUS)
                {
                    op = "-";
                }
                else
                {
                    break;
                }
                NextToken();
                var rgt = ParseProdCalculation();
                args.Add(new NodeWithArgsArg<ProdCalculationNode>()
                {
                    op = op,
                    rgt = rgt,
                });
            } while (op != null);
            return new SumCalculationNode()
            {
                lft = lft,
                args = args.ToArray(),
            };
        }

        protected ProdCalculationNode ParseProdCalculation()
        {
            var lft = ParseSimpleExpression();
            var args = new List<NodeWithArgsArg<SimpleExpressionNode>>();
            string op = null;
            do
            {
                op = null;
                var next = PeekToken();
                if (next.Type == TokenType.OP_DIVIDE)
                {
                    op = "/";
                }
                else if (next.Type == TokenType.OP_TIMES)
                {
                    op = "*";
                }
                else if (next.Type == TokenType.OP_MODULO)
                {
                    op = "%";
                }
                else
                {
                    break;
                }
                NextToken();
                var rgt = ParseSimpleExpression();
                args.Add(new NodeWithArgsArg<SimpleExpressionNode>()
                {
                    op = op,
                    rgt = rgt,
                });
            } while (op != null);
            return new ProdCalculationNode()
            {
                lft = lft,
                args = args.ToArray(),
            };
        }

        protected SimpleExpressionNode ParseSimpleExpression()
        {
            var arrowFunctionTerm = ReturnOrBacktrack(ParseArrowFunction);
            if (arrowFunctionTerm != null)
            {
                return arrowFunctionTerm;
            }
            var wrappedExpressionTerm = ReturnOrBacktrack(ParseWrappedExpression);
            if (wrappedExpressionTerm != null)
            {
                return wrappedExpressionTerm;
            }
            var notExpression = ReturnOrBacktrack(ParseNotExpression);
            if (notExpression != null)
            {
                return notExpression;
            }
            var arrayLiteral = ReturnOrBacktrack(ParseArrayLiteral);
            if (arrayLiteral != null)
            {
                return arrayLiteral;
            }
            var objectLiteral = ReturnOrBacktrack(ParseObjectLiteral);
            if (objectLiteral != null)
            {
                return objectLiteral;
            }
            var term = ParseTerm();
            // var term = ReturnOrBacktrack(ParseTerm);
            if (term != null)
            {
                return term;
            }
            throw new ParserException("Error parsing simple expression " + PeekToken().Type + " (" + this.currentToken + ")");
        }

        protected BooleanLiteralNode ParseBoolean()
        {
            var nextToken = NextToken();
            if (nextToken.Type == TokenType.TRUE)
            {
                return new BooleanLiteralNode() { value = true };
            }
            if (nextToken.Type == TokenType.FALSE)
            {
                return new BooleanLiteralNode() { value = false };
            }
            RewindToken();
            return null;
        }

        protected NumberLiteralNode ParseNumberLiteral()
        {
            var nextToken = PeekToken();
            if (nextToken.Type != TokenType.NUMBER)
            {
                return null;
            }
            NextToken();
            return new NumberLiteralNode() { value = (float)nextToken.Payload };
        }

        protected StringLiteralNode ParseStringLiteral()
        {
            var nextToken = PeekToken();
            if (nextToken.Type != TokenType.STRING)
            {
                return null;
            }
            NextToken();
            return new StringLiteralNode() { value = (string)nextToken.Payload };
        }

        // PathPart: [<identifier>(<arrayAccess>)(<methodArguments>)]
        // ^- is this correct? can we call a function from array access?
        // PathPart(.PathPart)*
        protected ObjectPathNode ParseObjectPath()
        {
            var parts = new List<ObjectPathPartNode>();
            ObjectPathPartNode first = ReturnOrBacktrack(ParseMethodCall);
            if (first == null)
            {
                if (PeekToken().Type == TokenType.IDENTIFIER)
                {
                    first = new StaticObjectPathPartNode() { name = (string)NextToken().Payload };
                }
                // else
                // {
                //     throw new ParserException("Could not parse object path, unexpected Token " + PeekToken().Type);
                // }
            }
            if (first == null)
            {
                throw new ParserException("Could not parse object path, unexpected Token " + PeekToken().Type);
            }
            else
            {
                parts.Add(first);
            }
            while (PeekToken().Type == TokenType.DOT)
            {
                NextToken();
                var methodCall = ParseMethodCall();
                if (methodCall == null)
                {
                    if (PeekToken().Type == TokenType.IDENTIFIER)
                    {
                        var part = new StaticObjectPathPartNode() { name = (string)NextToken().Payload };
                        parts.Add(part);
                    }
                }
                else
                {
                    parts.Add(methodCall);
                }
                var objectAccess = ParseOffsetAccess();
                if (objectAccess != null)
                {
                    parts.Add(objectAccess);
                }
            }
            return new ObjectPathNode() { path = parts.ToArray() };
        }

        protected OffsetAccessNode ParseOffsetAccess()
        {
            if (PeekToken().Type != TokenType.BRACKET_OPEN)
            {
                return null;
            }
            NextToken();
            var expression = ParseExpression();
            if (NextToken().Type != TokenType.BRACE_CLOSE)
            {
                throw new ParserException("Missing ] after offset access");
            }
            return new OffsetAccessNode()
            {
                exp = expression,
            };
        }

        protected MethodCallNode ParseMethodCall()
        {
            var identifier = NextToken();
            if (identifier.Type != TokenType.IDENTIFIER)
            {
                RewindToken();
                return null;
            }
            var name = (string)identifier.Payload;
            if (NextToken().Type != TokenType.PAREN_OPEN)
            {
                RewindToken();
                return null;
            }
            var arguments = ParseNodeList(ParseExpression);
            RewindToken();
            var next = NextToken();
            if (next.Type != TokenType.PAREN_CLOSE)
            {
                System.Console.WriteLine(next.Type);
                throw new ParserException("Missing ) after argument list");
            }
            return new MethodCallNode()
            {
                name = name,
                arguments = arguments
            };
        }

        protected TermNode ParseTerm()
        {
            TermNode term = ReturnOrBacktrack(ParseBoolean);
            if (term != null)
            {
                return term;
            }
            term = ReturnOrBacktrack(ParseNumberLiteral);
            if (term != null)
            {
                return term;
            }
            term = ReturnOrBacktrack(ParseStringLiteral);
            if (term != null)
            {
                return term;
            }
            term = ReturnOrBacktrack(ParseObjectPath);
            if (term != null)
            {
                return term;
            }
            throw new ParserException("Could not parse Term");
        }

        protected ObjectLiteralNode ParseObjectLiteral()
        {
            if (NextToken().Type != TokenType.BRACE_OPEN)
            {
                return null;
            }
            // var values = new List<ObjectLiteralPropertyNode>();
            // var value = ParseObjectLiteralProperty();
            // if (value != null)
            // {
            //     values.Add(value);
            //     while (NextToken().Type == TokenType.COMMA)
            //     {
            //         value = ParseObjectLiteralProperty();
            //         if (value != null)
            //         {
            //             values.Add(value);
            //         }
            //         else
            //         {
            //             break;
            //         }
            //     }
            // }
            var values = ParseNodeList<ObjectLiteralPropertyNode>(ParseObjectLiteralProperty);
            if (NextToken().Type != TokenType.BRACE_CLOSE)
            {
                throw new ParserException("Missing } after object literal");
            }
            return new ObjectLiteralNode()
            {
                // properties = values.ToArray()
                properties = values
            };
        }

        protected ObjectLiteralPropertyNode ParseObjectLiteralProperty()
        {
            var nextToken = NextToken();
            string key = null;
            if (nextToken.Type == TokenType.STRING || nextToken.Type == TokenType.IDENTIFIER)
            {
                key = (string)nextToken.Payload;
            }
            else
            {
                new ParserException("Expecting string or identifier as object key");
            }
            if (NextToken().Type != TokenType.COLON)
            {
                throw new ParserException("Expecting : after object key");
            }
            var expression = ParseExpression();
            return new ObjectLiteralPropertyNode()
            {
                key = key,
                value = expression
            };
        }

        protected ArrayLiteralNode ParseArrayLiteral()
        {
            if (NextToken().Type != TokenType.BRACKET_OPEN)
            {
                return null;
            }
            var values = ParseNodeList<ExpressionNode>(ParseExpression);
            // var values = new List<ExpressionNode>();
            // var value = ParseExpression();
            // if (value != null)
            // {
            //     values.Add(value);
            //     while (NextToken().Type == TokenType.COMMA)
            //     {
            //         value = ParseExpression();
            //         if (value != null)
            //         {
            //             values.Add(value);
            //         }
            //         else
            //         {
            //             break;
            //         }
            //     }
            // }
            if (NextToken().Type != TokenType.BRACKET_CLOSE)
            {
                throw new ParserException("Missing ] after array literal");
            }
            return new ArrayLiteralNode()
            {
                // values = values.ToArray()
                values = values
            };
        }

        protected NotExpressionNode ParseNotExpression()
        {
            if (NextToken().Type != TokenType.OP_NOT)
            {
                return null;
            }
            var expression = ParseSimpleExpression();
            return (NotExpressionNode)expression;
        }

        protected WrappedExpressionNode ParseWrappedExpression()
        {
            if (NextToken().Type != TokenType.BRACE_OPEN)
            {
                return null;
            }
            var expression = ParseExpression();
            if (NextToken().Type != TokenType.BRACE_CLOSE)
            {
                throw new ParserException("Missing closing parenthesis after wrapped expression");
            }
            return (WrappedExpressionNode)expression;
        }

        protected ArrowFunctionNode ParseArrowFunction()
        {
            var arguments = ParseMethodArguments();
            if (NextToken().Type != TokenType.ARROW)
            {
                return null;
            }
            var exp = ParseExpression();
            return new ArrowFunctionNode()
            {
                arguments = arguments,
                exp = exp
            };
        }

        protected MethodArgumentsNode ParseMethodArguments()
        {
            var allowMultiple = false;
            // var args = new List<string>();
            if (PeekToken().Type == TokenType.PAREN_OPEN)
            {
                NextToken();
                allowMultiple = true;
            }
            var args = ParseNodeList<string>(() => {
                var token = NextToken();
                if (token.Type == TokenType.IDENTIFIER) {
                    return (string)token.Payload;
                }
                RewindToken();
                return null;
            });
            // var arg = NextToken();
            // if (arg.Type == TokenType.IDENTIFIER)
            // {
            //     args.Add((string)arg.Payload);
            // }
            // while (PeekToken().Type == TokenType.COMMA)
            // {
            //     NextToken();
            //     arg = NextToken();
            //     if (arg.Type == TokenType.IDENTIFIER)
            //     {
            //         args.Add((string)arg.Payload);
            //     }
            //     else
            //     {
            //         break;
            //     }
            // }
            if (allowMultiple && PeekToken().Type != TokenType.PAREN_CLOSE)
            {
                throw new ParserException("Missing closing parenthesis after argument list");
            }
            return new MethodArgumentsNode()
            {
                // arguments = args.ToArray()
                arguments = args
            };
        }

        protected T[] ParseNodeList<T>(System.Func<T> callback)
        {
            var results = new List<T>();
            var first = callback();
            if (first != null)
            {
                results.Add(first);
                while (NextToken().Type == TokenType.COMMA)
                {
                    var item = callback();
                    if (item == null)
                    {
                        break;
                    }
                    results.Add(item);
                }
            }
            return results.ToArray();
        }

        protected T ReturnOrBacktrack<T>(System.Func<T> callback)
        {
            var currentTokenBefore = this.currentToken;
            try
            {
                T result = callback();
                if (result != null)
                {
                    return result;
                }
            }
            catch (ParserException) { }
            this.currentToken = currentTokenBefore;
            return (T)(object)null;
        }
    }
}