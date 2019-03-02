using System;

namespace Prgfx.Eel
{
    public class Lexer
    {
        private string input;

        private int charPos;

        private char currentChar;

        public Lexer(string input)
        {
            this.input = input;
            this.charPos = 0;
            this.currentChar = input.Length > 0 ? input[0] : '\0';
        }

        public int Snapshot()
        {
            return this.charPos;
        }

        public void Reset(int cursor)
        {
            this.charPos = cursor;
            this.currentChar = input.Length > cursor ? input[cursor] : '\0';
        }

		public void Rewind()
		{
			if (charPos > 0) {
				currentChar = input[--charPos];
			}
		}

		public string Peek(int length)
		{
			if (charPos < input.Length - 1)
            {
                return input.Substring(charPos, Math.Max(length, input.Length - charPos));
            }
            return string.Empty;
		}

        public char Consume()
        {
            var c = currentChar;
            if (charPos < input.Length - 1)
            {
                currentChar = input[++charPos];
            }
            else
            {
                currentChar = '\0';
            }
            return c;
        }

		public bool IsWhiteSpace()
        {
            return char.IsWhiteSpace(currentChar);
        }
		public bool IsAlpha()
        {
            return char.IsLetter(currentChar);
        }

        public bool IsAlphaNumeric()
        {
            return char.IsLetterOrDigit(currentChar);
        }

        public bool IsNumeric()
        {
            return char.IsDigit(currentChar);
        }

        internal bool Is(char v)
        {
            return currentChar == v;
        }

		internal bool IsEnd()
		{
			return currentChar == '\0';
		}
    }
}
