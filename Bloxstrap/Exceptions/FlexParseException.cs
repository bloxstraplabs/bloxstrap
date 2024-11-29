namespace Bloxstrap.Exceptions
{
    internal class FlexParseException : Exception
    {
        public FlexParseException(string expression, string message, string? position = null) 
            : base($"Invalid syntax encountered when parsing '{expression}' at position {position ?? "EOF"} ({message})") { }

        public FlexParseException(string expression, string message, int position)
            : this(expression, message, position.ToString()) { }

        public FlexParseException(string expression, string message, FlexToken? token)
            : this(expression, message, token?.Position.ToString()) { }
    }
}
