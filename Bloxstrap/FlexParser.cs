namespace Bloxstrap
{
    /// <summary>
    /// FastFlag Expression Parser
    /// </summary>
    public class FlexParser
    {
        private readonly string _expression;

        private readonly List<FlexToken> _tokens = new();

        private int _tokenPos = 0;

        private static readonly Dictionary<string, FlexTokenType> _staticTokenMap = new()
        {
            { "null", FlexTokenType.NULL },

            { "true", FlexTokenType.BOOL },
            { "false", FlexTokenType.BOOL },

            { "==", FlexTokenType.COMPARE_EQ },
            { "!=", FlexTokenType.COMPARE_NEQ },

            { ">", FlexTokenType.COMPARE_GT },
            { "<", FlexTokenType.COMPARE_LT },
            { ">=", FlexTokenType.COMPARE_GEQ },
            { "<=", FlexTokenType.COMPARE_LEQ },

            { "&&", FlexTokenType.LOGIC_AND },
            { "||", FlexTokenType.LOGIC_OR },

            { "(", FlexTokenType.BRACKET_OPEN },
            { ")", FlexTokenType.BRACKET_CLOSE }
        };

        private static readonly Dictionary<string, FlexTokenType> _regexTokenMap = new()
        {
            { @"^\d+", FlexTokenType.NUMBER },
            { @"^('[^']+')", FlexTokenType.STRING },
            { @"^([a-zA-Z0-9_\.]+)", FlexTokenType.FLAG }
        };

        public FlexParser(string expression)
        {
            _expression = expression;

            Tokenize();
        }

        public bool Evaluate() => EvaluateExpression();

        private void Tokenize()
        {
            int position = 0;

            while (position < _expression.Length)
            {
                string exprSlice = _expression.Substring(position);

                if (exprSlice[0] == ' ')
                {
                    position++;
                    continue;
                }

                string exprSliceLower = exprSlice.ToLowerInvariant();

                var mapMatch = _staticTokenMap.FirstOrDefault(x => exprSliceLower.StartsWith(x.Key));

                if (mapMatch.Key is null)
                {
                    bool matched = false;

                    foreach (var entry in _regexTokenMap)
                    {
                        var match = Regex.Match(exprSlice, entry.Key);

                        if (match.Success)
                        {
                            matched = true;

                            string phrase = match.Groups[match.Groups.Count > 1 ? 1 : 0].Value;
                            _tokens.Add(new(entry.Value, phrase, position));
                            position += phrase.Length;

                            break;
                        }
                    }

                    if (!matched)
                        throw new FlexParseException(_expression, "unknown identifier", position);
                }
                else
                {
                    _tokens.Add(new(mapMatch.Value, mapMatch.Key, position));
                    position += mapMatch.Key.Length;
                }
            }
        }

        /// <summary>
        /// The brackets in this example expression are instances of subexpressions: "[FLogNetwork == 7] || [false]"
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FlexParseException"></exception>
        private bool EvaluateSubExpression()
        {
            var token = _tokens.ElementAtOrDefault(_tokenPos++);

            if (token?.Type == FlexTokenType.FLAG)
            {
                var compToken = _tokens.ElementAtOrDefault(_tokenPos++);

                if (compToken is null || compToken.Value is null || !compToken.IsComparisonOperator)
                    throw new FlexParseException(_expression, "expected comparison operator", compToken);

                var valueToken = _tokens.ElementAtOrDefault(_tokenPos++);

                if (valueToken is null || !valueToken.IsDataType)
                    throw new FlexParseException(_expression, "expected data", valueToken);

                string? flagValue = token.GetActualValue();

                if (compToken.IsInequalityOperator)
                {
                    if (flagValue is null || valueToken.Value is null)
                        return false;

                    if (valueToken.Type != FlexTokenType.NUMBER)
                        throw new FlexParseException(_expression, "expected integer", valueToken);

                    if (!long.TryParse(flagValue, out long intFlagValue))
                        return false;

                    long intValue = long.Parse(valueToken.Value);

                    switch (compToken.Type)
                    {
                        case FlexTokenType.COMPARE_GT:
                            return intFlagValue > intValue;
                        case FlexTokenType.COMPARE_LT:
                           return intFlagValue < intValue;
                        case FlexTokenType.COMPARE_GEQ:
                            return intFlagValue >= intValue;
                        case FlexTokenType.COMPARE_LEQ:
                            return intFlagValue <= intValue;
                    }
                }
                else
                {
                    if (valueToken.Type == FlexTokenType.NULL)
                        return flagValue is null;

                    bool result = string.Compare(flagValue, valueToken.GetActualValue(), StringComparison.InvariantCultureIgnoreCase) == 0;

                    if (compToken.Type == FlexTokenType.COMPARE_EQ)
                        return result;
                    else
                        return !result;
                }
            }
            else if (token?.Type == FlexTokenType.BOOL)
            {
                return token.BoolValue;
            }

            return false;
        }

        private bool EvaluateExpression(int finalPos = 0)
        {
            bool result = false;
            
            if (finalPos == 0)
                finalPos = _tokens.Count;

            while (_tokenPos < finalPos)
            {
                var token = _tokens.ElementAtOrDefault(_tokenPos);

                if (token is null)
                    break;

                if (token.Type == FlexTokenType.FLAG || token.Type == FlexTokenType.BOOL)
                {
                    result = EvaluateSubExpression();
                }
                else if (token.Type == FlexTokenType.BRACKET_OPEN)
                {
                    var closeBracketToken = _tokens.Find(x => x.Type == FlexTokenType.BRACKET_CLOSE);

                    if (closeBracketToken is null)
                        throw new FlexParseException(_expression, "expected closing bracket");

                    _tokenPos++;

                    result = EvaluateExpression(_tokens.IndexOf(closeBracketToken));

                    _tokenPos++;
                }
                else
                {
                    throw new FlexParseException(_expression, "identifier was unexpected here", token);
                }

                var nextToken = _tokens.ElementAtOrDefault(_tokenPos++);

                if (nextToken is null)
                    break;

                if (!nextToken.IsLogicToken)
                    throw new FlexParseException(_expression, "expected boolean operator", nextToken);

                if (nextToken.Type == FlexTokenType.LOGIC_AND)
                {
                    if (result)
                    {
                        continue;
                    }
                    else
                    {
                        int bracketNesting = 0;

                        while (_tokenPos < finalPos)
                        {
                            token = _tokens[_tokenPos++];

                            if (token.Type == FlexTokenType.BRACKET_OPEN)
                                bracketNesting++;
                            else if (token.Type == FlexTokenType.BRACKET_CLOSE)
                                bracketNesting--;
                            else if (bracketNesting == 0 && token.Type == FlexTokenType.LOGIC_OR)
                                break;
                        }

                        if (bracketNesting != 0)
                            throw new FlexParseException(_expression, "unclosed bracket");
                    }
                }
                else if (nextToken.Type == FlexTokenType.LOGIC_OR && result)
                {
                    break;
                }
            }

            return result;
        }
    }
}
