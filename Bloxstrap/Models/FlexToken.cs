namespace Bloxstrap.Models
{
    public class FlexToken
    {
        public FlexTokenType Type;
        public string? Value;
        public int Position;

        public bool IsDataType =>
            Type == FlexTokenType.FLAG
            || Type == FlexTokenType.NUMBER
            || Type == FlexTokenType.BOOL
            || Type == FlexTokenType.STRING
            || Type == FlexTokenType.NULL;

        public bool IsLogicToken => 
            Type == FlexTokenType.LOGIC_AND 
            || Type == FlexTokenType.LOGIC_OR;

        public bool IsInequalityOperator =>
            Type == FlexTokenType.COMPARE_GT
            || Type == FlexTokenType.COMPARE_LT
            || Type == FlexTokenType.COMPARE_GEQ
            || Type == FlexTokenType.COMPARE_LEQ;

        public bool IsComparisonOperator =>
            IsInequalityOperator
            || Type == FlexTokenType.COMPARE_EQ
            || Type == FlexTokenType.COMPARE_NEQ;

        public bool BoolValue => Type == FlexTokenType.BOOL && Value == "true";

        public FlexToken(FlexTokenType type, string? value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public string? GetActualValue()
        {
            if (Value is null)
                return null;
            
            if (Type == FlexTokenType.STRING)
                return Value.Substring(1, Value.Length - 2);

            if (Type == FlexTokenType.FLAG)
                return App.FastFlags.GetValue(Value);

            return Value;
    }

        public override string ToString() => $"{Type}: {Value}";
    }
}
