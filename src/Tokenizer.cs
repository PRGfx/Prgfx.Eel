using System.Collections.Generic;
using System.Text;

namespace Prgfx.Eel
{
    class Tokenizer
    {
        protected string input;
        protected int cursor = 0;

        protected List<Token> tokens;

        protected bool done = false;

        public Tokenizer(string input)
        {
            this.input = input;
        }

        public List<Token> GetTokens()
        {
            if (!done) {
                generateTokens();
            }
            return tokens;
        }

        protected void generateTokens()
        {
            tokens = new List<Token>();
            char stringDelimiter = '\0';
            var isEscaped = false;
            var currentPayload = new StringBuilder();
            while (cursor < input.Length) {
                if (stringDelimiter == '\0') {
                    if (input[cursor] == '"' || input[cursor] == '\'') {
                        stringDelimiter = input[cursor];
                        cursor++;
                        continue;
                    }
                    parseIdentifier();
                    parseNumber();
                    if (cursor >= input.Length) {
                        break;
                    }
                    if (input.Length - cursor >= 2 && input.Substring(cursor, 2) == "=>")
                    {
                        tokens.Add(new Token(TokenType.ARROW));
                        cursor += 2;
                        continue;
                    }
                    if (input.Length - cursor >= 2 && input.Substring(cursor, 2) == "==")
                    {
                        tokens.Add(new Token(TokenType.OP_EQUAL));
                        cursor += 2;
                        continue;
                    }
                    if (input.Length - cursor >= 2 && input.Substring(cursor, 2) == "!=")
                    {
                        tokens.Add(new Token(TokenType.OP_NOTEQUAL));
                        cursor += 2;
                        continue;
                    }
                    if (input.Length - cursor >= 2 && input.Substring(cursor, 2) == "<=")
                    {
                        tokens.Add(new Token(TokenType.OP_LESSEQUAL));
                        cursor += 2;
                        continue;
                    }
                    if (input[cursor] == '<')
                    {
                        tokens.Add(new Token(TokenType.OP_LESS));
                        cursor++;
                        continue;
                    }
                    if (input.Length - cursor >= 2 && input.Substring(cursor, 2) == ">=")
                    {
                        tokens.Add(new Token(TokenType.OP_GREATEREQUAL));
                        cursor += 2;
                        continue;
                    }
                    if (input[cursor] == '>')
                    {
                        tokens.Add(new Token(TokenType.OP_GREATER));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '!')
                    {
                        tokens.Add(new Token(TokenType.OP_NOT));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '+')
                    {
                        tokens.Add(new Token(TokenType.OP_PLUS));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '-')
                    {
                        tokens.Add(new Token(TokenType.OP_MINUS));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '*')
                    {
                        tokens.Add(new Token(TokenType.OP_TIMES));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '/')
                    {
                        tokens.Add(new Token(TokenType.OP_DIVIDE));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '%')
                    {
                        tokens.Add(new Token(TokenType.OP_MODULO));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '?')
                    {
                        tokens.Add(new Token(TokenType.QUESTIONMARK));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == ':')
                    {
                        tokens.Add(new Token(TokenType.COLON));
                        cursor++;
                        continue;
                    }
                    if (input.Length - cursor >= 4 && input.Substring(cursor, 4) == "true")
                    {
                        tokens.Add(new Token(TokenType.TRUE));
                        cursor += 4;
                        continue;
                    }
                    if (input.Length - cursor >= 4 && input.Substring(cursor, 4) == "TRUE")
                    {
                        tokens.Add(new Token(TokenType.TRUE));
                        cursor += 4;
                        continue;
                    }
                    if (input.Length - cursor >= 5 && input.Substring(cursor, 5) == "false")
                    {
                        tokens.Add(new Token(TokenType.FALSE));
                        cursor += 5;
                        continue;
                    }
                    if (input.Length - cursor >= 5 && input.Substring(cursor, 5) == "FALSE")
                    {
                        tokens.Add(new Token(TokenType.FALSE));
                        cursor += 5;
                        continue;
                    }
                    if (input.Length - cursor >= 2 && input.Substring(cursor, 2) == "&&")
                    {
                        tokens.Add(new Token(TokenType.OP_AND));
                        cursor += 2;
                        continue;
                    }
                    if (input.Length - cursor >= 3 && input.Substring(cursor, 3) == "and")
                    {
                        tokens.Add(new Token(TokenType.OP_AND));
                        cursor += 3;
                        continue;
                    }
                    if (input.Length - cursor >= 2 && input.Substring(cursor, 2) == "||")
                    {
                        tokens.Add(new Token(TokenType.OP_OR));
                        cursor += 2;
                        continue;
                    }
                    if (input.Length - cursor >= 2 && input.Substring(cursor, 2) == "or")
                    {
                        tokens.Add(new Token(TokenType.OP_OR));
                        cursor += 2;
                        continue;
                    }
                    if (input[cursor] == '(')
                    {
                        tokens.Add(new Token(TokenType.PAREN_OPEN));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == ')')
                    {
                        tokens.Add(new Token(TokenType.PAREN_CLOSE));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '{')
                    {
                        tokens.Add(new Token(TokenType.BRACE_OPEN));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '}')
                    {
                        tokens.Add(new Token(TokenType.BRACE_CLOSE));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '[')
                    {
                        tokens.Add(new Token(TokenType.BRACKET_OPEN));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == ']')
                    {
                        tokens.Add(new Token(TokenType.BRACKET_CLOSE));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == ',')
                    {
                        tokens.Add(new Token(TokenType.COMMA));
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == '.')
                    {
                        tokens.Add(new Token(TokenType.DOT));
                        cursor++;
                        continue;
                    }
                    cursor++;
                } else {
                    if (isEscaped) {
                        isEscaped = false;
                        currentPayload.Append(input[cursor++]);
                        continue;
                    }
                    if (input[cursor] == '\\') {
                        isEscaped = true;
                        cursor++;
                        continue;
                    }
                    if (input[cursor] == stringDelimiter) {
                        stringDelimiter = '\0';
                        tokens.Add(new Token(TokenType.STRING, currentPayload.ToString()));
                        currentPayload = new StringBuilder();
                        cursor ++;
                        continue;
                    }
                    currentPayload.Append(input[cursor++]);
                }
            }
            done = true;
        }

        protected void parseIdentifier()
        {
            if (cursor >= input.Length || !char.IsLetter(input[cursor]) && input[cursor] != '_') {
                return;
            }
            var start = cursor;
            while (cursor < input.Length && (char.IsLetterOrDigit(input[cursor]) || input[cursor] == '_')) {
                cursor++;
            }
            if (cursor > start) {
                tokens.Add(new Token(TokenType.IDENTIFIER, input.Substring(start, cursor - start)));
            }
        }

        protected void parseNumber()
        {
            if (cursor >= input.Length) {
                return;
            }
            var isNegative = input[cursor] == '-';
            var start = cursor + (isNegative ? 1 : 0);
            if (!char.IsDigit(input[start])) {
                return;
            }
            cursor = start;
            var readDecimals = false;
            while (cursor < input.Length) {
                if (input[cursor] == '.' && !readDecimals) {
                    readDecimals = true;
                    cursor++;
                    continue;
                }
                if (!char.IsDigit(input[cursor])) {
                    break;
                }
                cursor++;
            }
            float digit;
            float.TryParse(input.Substring(start, cursor - start), out digit);
            if (isNegative) {
                digit *= -1;
            }
            tokens.Add(new Token(TokenType.NUMBER, digit));
            return;
        }
    }
}