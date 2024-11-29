namespace Bloxstrap.Enums
{
    public enum FlexTokenType
    {
        // data types
        NULL, 
        NUMBER, 
        BOOL, 
        STRING, 
        FLAG,

        // comparison operators
        COMPARE_EQ,
        COMPARE_NEQ,
        COMPARE_GT,
        COMPARE_LT,
        COMPARE_GEQ,
        COMPARE_LEQ,

        // boolean logic operators
        LOGIC_AND, 
        LOGIC_OR,

        // yes, "bracket" is technically not the correct term, don't care
        BRACKET_OPEN, 
        BRACKET_CLOSE
    }
}
