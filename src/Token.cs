namespace Prgfx.Eel
{
    public class Token
    {
        public TokenType Type;
        public object Payload;

        public Token(TokenType type, object payload = null)
        {
            this.Type = type;
            this.Payload = payload;
        }
    }

    public enum TokenType
    {
        ARROW,
        OP_EQUAL,
        OP_NOTEQUAL,
        OP_LESS,
        OP_LESSEQUAL,
        OP_GREATER,
        OP_GREATEREQUAL,
        OP_PLUS,
        OP_MINUS,
        OP_TIMES,
        OP_DIVIDE,
        OP_MODULO,
        OP_NOT,
        OP_AND,
        OP_OR,
        BRACE_OPEN,
        BRACE_CLOSE,
        PAREN_OPEN,
        PAREN_CLOSE,
        BRACKET_OPEN,
        BRACKET_CLOSE,
        QUESTIONMARK,
        COLON,
        COMMA,
        DOT,
        IDENTIFIER,
        NUMBER,
        STRING,
        TRUE,
        FALSE,
        EOF,

    }
}