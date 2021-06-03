using System;
using System.Collections.Generic;
using System.Web;
using Antlr4.Runtime;
using LibProtection.Injections.Internals;
using System.Text.RegularExpressions;

namespace LibProtection.Injections
{
    public sealed class Html : AntlrLanguageProvider
    {
        private enum HtmlTokenizerState
        {
            Insignificant,
            EventName,
            EventEqualSign,
            EventValue,
            ResourceName,
            ResourceEqualSign,
            ResourceValue
        }

        private readonly HashSet<string> _htmlUrlAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "href", "src", "manifest", "poster", "code", "codebase", "data", "xlink:href", "xml:base", "from", "to",
            "formaction", "action", "dynsrc", "lowsrc"
        };

        private Html() { }

        protected override Enum ConvertAntlrTokenType(int antlrTokenType)
        {
            return (HtmlTokenType)antlrTokenType;
        }

        protected override Lexer CreateLexer(string text)
        {
            return new HTMLLexer(new AntlrInputStream(text));
        }

        public override IEnumerable<Token> Tokenize(string text, int offset = 0)
        {
            var state = HtmlTokenizerState.Insignificant;
            var insideScriptTag = false;

            foreach (var token in base.Tokenize(text, offset))
            {
                if (token.LanguageProvider is Html)
                {
                    var htmlTokenType = (HtmlTokenType)token.Type;
                    switch (state)
                    {
                        case HtmlTokenizerState.EventName:
                            state = htmlTokenType == HtmlTokenType.TagEquals
                            ? HtmlTokenizerState.EventEqualSign
                            : HtmlTokenizerState.Insignificant;
                            break;

                        case HtmlTokenizerState.EventEqualSign:
                            state = htmlTokenType == HtmlTokenType.AttributeValue
                                ? HtmlTokenizerState.EventValue
                                : HtmlTokenizerState.Insignificant;
                            break;

                        case HtmlTokenizerState.ResourceName:
                            state = htmlTokenType == HtmlTokenType.TagEquals
                                ? HtmlTokenizerState.ResourceEqualSign
                                : HtmlTokenizerState.Insignificant;
                            break;

                        case HtmlTokenizerState.ResourceEqualSign:
                            state = htmlTokenType == HtmlTokenType.AttributeValue
                                ? HtmlTokenizerState.ResourceValue
                                : HtmlTokenizerState.Insignificant;
                            break;

                        default:
                            state = HtmlTokenizerState.Insignificant;
                            switch (htmlTokenType)
                            {
                                case HtmlTokenType.AttributeName:
                                    if (token.Text.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                                    {
                                        state = HtmlTokenizerState.EventName;
                                    }
                                    else if (_htmlUrlAttributes.Contains(token.Text))
                                    {
                                        state = HtmlTokenizerState.ResourceName;
                                    }
                                    break;

                                case HtmlTokenType.TagOpen:
                                    if (string.Equals(token.Text, "<script", StringComparison.OrdinalIgnoreCase))
                                    {
                                        insideScriptTag = true;
                                    }
                                    else
                                    if (string.Equals(token.Text, "</script", StringComparison.OrdinalIgnoreCase))
                                    {
                                        insideScriptTag = false;
                                    }

                                    break;
                            }

                            break;
                    }

                    if (IsContextChanged(token, state, insideScriptTag, out var islandData))
                    {
                        var islandTokens = islandData.LanguageProvider.Tokenize(islandData.Text, islandData.Offset);
                        foreach (var islandToken in islandTokens)
                        {
                            yield return islandToken;
                        }
                    }
                    else
                    {
                        yield return token;
                    }
                }
            }
        }

        private static bool IsContextChanged(Token htmlToken, HtmlTokenizerState context, bool insideScriptTag,
            out IslandDto islandData)
        {
            islandData = null;

            switch (context)
            {
                case HtmlTokenizerState.EventValue:
                    {
                        var islandText = TrimQuotes(htmlToken, out var offset);

                        if (!string.IsNullOrEmpty(islandText))
                        {
                            islandData = new IslandDto(Single<JavaScript>.Instance, offset, islandText);
                        }

                        break;
                    }

                case HtmlTokenizerState.ResourceValue:
                    {
                        var islandText = TrimQuotes(htmlToken, out var offset);

                        if (!string.IsNullOrEmpty(islandText))
                        {
                            islandData = new IslandDto(Single<Url>.Instance, offset, islandText);
                        }

                        break;
                    }

                default:
                    var htmlTokenType = (HtmlTokenType)htmlToken.Type;

                    if (insideScriptTag)
                    {
                        switch (htmlTokenType)
                        {
                            case HtmlTokenType.HtmlText:
                                islandData = new IslandDto(Single<JavaScript>.Instance, htmlToken.Range.LowerBound,
                                    htmlToken.Text);
                                break;

                            case HtmlTokenType.HtmlComment:
                                // TODO: implement
                                break;
                        }
                    }

                    break;
            }

            return islandData != null;
        }

        private static string TrimQuotes(Token token, out int offset)
        {
            var tokenText = token.Text;
            offset = token.Range.LowerBound;

            if (tokenText.Length == 0) { return string.Empty; }

            if (tokenText[0] == '\'' && tokenText[tokenText.Length - 1] == '\'' ||
                tokenText[0] == '"' && tokenText[tokenText.Length - 1] == '"')
            {
                if (tokenText.Length > 2)
                {
                    tokenText = tokenText.Substring(1, tokenText.Length - 2);
                    offset++;
                }
            }

            return tokenText;
        }

        public override bool TrySanitize(string text, Token context, out string sanitized)
        {
            switch (context.LanguageProvider)
            {
                case Html _:
                    sanitized = HtmlEncode(text, (HtmlTokenType)context.Type);
                    return true;

                case Url _:
                    if (Single<Url>.Instance.TrySanitize(text, context, out var urlSanitized))
                    {
                        sanitized = HtmlEncode(urlSanitized, HtmlTokenType.AttributeValue);
                        return true;
                    }
                    break;

                case JavaScript _:
                    if (Single<JavaScript>.Instance.TrySanitize(text, context, out var ecmaScriptSanitized))
                    {
                        sanitized = ecmaScriptSanitized;
                        return true;
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported HTML island: {context}");
            }

            sanitized = null;
            return false;
        }

        protected override bool IsTrivial(Enum type, string text)
        {
            switch ((HtmlTokenType)type)
            {
                case HtmlTokenType.HtmlComment:
                case HtmlTokenType.HtmlConditionalComment:
                case HtmlTokenType.HtmlText:
                case HtmlTokenType.Cdata:
                case HtmlTokenType.AttributeWhiteSpace:
                case HtmlTokenType.AttributeValue:
                    return true;
            }

            return false;
        }

        private static string HtmlEncode(string text, HtmlTokenType tokenType)
        {
            switch (tokenType)
            {
                case HtmlTokenType.AttributeValue:
                    return HttpUtility.HtmlAttributeEncode(text);

                default:
                    return HttpUtility.HtmlEncode(text);
            }
        }

        /// <summary>
        /// Escapes (decodes) html references with the need to take into account taint ranges
        /// </summary>
        /// <param name="text"></param>
        /// <param name="taintedRanges">List of original taint ranges that will be fixed after escaping</param>
        /// <returns>A string with escaped html references</returns>
        public static string HtmlUnescape(string text, ref List<Range> taintedRanges)
        {
            var charRefRegex = new Regex(@"&(#[0-9]+;?|#[xX][0-9a-fA-F]+;?|[^\t\n\f <&#;]{1,32};?)");
            var currentMatch = charRefRegex.Match(text);
            while (currentMatch.Success)
            {
                var decoded = HtmlCharRefDecoder.decodeCharRef(currentMatch.Value);
                text = text.Substring(0, currentMatch.Index) + decoded + text.Substring(currentMatch.Index + currentMatch.Length);

                for (var trIndex = 0; trIndex < taintedRanges.Count; trIndex++)
                {
                    taintedRanges[trIndex] = FixRange(currentMatch.Index, currentMatch.Length, decoded.Length, taintedRanges[trIndex]);
                }

                currentMatch = charRefRegex.Match(text);
            }

            return text;
        }

        /// <summary>
        /// Fixes taint ranges after html char reference decoding
        /// </summary>
        /// <param name="hrLowerBound">Index that points to start of html reference</param>
        /// <param name="hrLength">Length of html reference before decoding</param>
        /// <param name="decodedLength">Length of decoded html reference</param>
        /// <param name="taintRange">Original taint range that has been got from formatting</param>
        /// <returns>a new taint range with with corrected bounds</returns>
        private static Range FixRange(int hrLowerBound, int hrLength, int decodedLength, Range taintRange)
        {
            Range newTaintRange;
            var HR_1 = hrLowerBound;  // HR - Html Reference
            var L_HR = hrLength;
            var HR_2 = HR_1 + L_HR - 1;
            var L_HR_d = decodedLength;
            var T_1 = taintRange.LowerBound;
            var T_2 = taintRange.UpperBound;

            if (HR_1 < T_1 && HR_2 < T_2) {

                if (HR_2 < T_1) 
                {
                    /* Case 1.1
                     *                    HR_1    HR_2
                     *                    #### HR ####
                     * -------------------|-|------|-|-----|-|-------|-|------>
                     *                                     ### TAINT ###
                     *                                     T_1       T_2
                     */
                    newTaintRange = new Range(T_1 - (L_HR - L_HR_d), T_2 - (L_HR - L_HR_d));
                } 
                else {
                    /* Case 1.2
                     *                   HR_1      HR_2
                     *                    #### HR ####
                     * -------------------|-|------|-|-------|-|------>
                     *                             ### TAINT ###
                     *                             T_1      T_2
                     *       
                     * Case 1.3      
                     *                   HR_1              HR_2
                     *                    ######## HR ########
                     * -------------------|-|------|-|-----|-|-------|-|------>
                     *                             ####### TAINT #######
                     *                             T_1              T_2
                     */
                    newTaintRange = new Range(HR_1, T_2 - (L_HR - L_HR_d));
                }
            } 
            else if (HR_1 < T_1 && HR_2 == T_2) 
            {
                /* Case 2
                 *                    HR_1                  HR_2
                 *                    ######### HR ############
                 * -------------------|-|---------|-|-------|-|------>
                 *                                ### TAINT ###
                 *                                T_1      T_2
                 */
                newTaintRange = new Range(HR_1, HR_1 + L_HR_d);
            } 
            else if (HR_1 < T_1 && HR_2 > T_2) 
            {
                /* Case 3
                *               HR_1                           HR_2
                *               ################ HR ##############
                * --------------|-|------|-|---------|-|-------|-|------>
                *                        #### TAINT ####
                *                        T_1         T_2
                */
                newTaintRange = new Range(HR_1, HR_1 + L_HR_d);
            } 
            else if (HR_1 == T_1 && HR_2 < T_2) 
            {
                /* Case 4
                 *                    HR_1        HR_2
                 *                    ###### HR #####
                 * -------------------|-|---------|-|-------|-|------>
                 *                    ######### TAINT #########
                 *                    T_1                   T_2
                 */
                newTaintRange = new Range(HR_1, T_2 - (L_HR - L_HR_d));
            } 
            else if (HR_1 == T_1 && HR_2 == T_2) 
            {
                /* Case 5
                 *                   HR_1                  HR_2
                 *                    ######### HR ############
                 * -------------------|-|-------------------|-|------>
                 *                    ######### TAINT #########
                 *                    T_1                  T_2
                 */
                newTaintRange = new Range(HR_1, HR_1 + L_HR_d);
            } 
            else if (HR_1 == T_1 && HR_2 > T_2)
            {
                /* Case 6
                 *                   HR_1                   HR_2
                 *                    ########### HR ##########
                 * -------------------|-|---------|-|-------|-|------>
                 *                    #### TAINT ####
                 *                    T_1         T_2
                 */
                newTaintRange = new Range(HR_1, HR_1 + L_HR_d);
            }
            else if (HR_1 > T_1 && HR_2 < T_2)
            {
                /* Case 7
                *                        HR_1        HR_2
                *                        ##### HR ######
                * --------------|-|------|-|---------|-|-------|-|------>
                *               ############# TAINT ##############
                *               T_1                           T_2
                */
                newTaintRange = new Range(T_1, T_2 - (L_HR - L_HR_d));
            }
            else if (HR_1 > T_1 && HR_2 == T_2)
            {
                /* Case 8
                 *                                HR_1      HR_2
                 *                                ##### HR ####
                 * -------------------|-|---------|-|-------|-|------>
                 *                    ######### TAINT #########
                 *                    T_1                   T_2
                 */
                newTaintRange = new Range(T_1, T_2 - (L_HR - L_HR_d));
            }
            else
            {
                if (HR_1 < T_2)
                {
                    /* Case 9.1
                    *                       HR_1                   HR_2
                    *                        ########## HR ###########
                    * --------------|-|------|-|---------|-|-------|-|------>
                    *               ######## TAINT #########
                    *               T_1                  T_2
                    */
                    newTaintRange = new Range(T_1, HR_1 + L_HR_d);
                }
                else if (HR_1 == T_2)
                {
                    /* Case 9.2
                    *                                HR_1      HR_2
                    *                                ##### HR ####
                    * -------------------|-|---------|-|-------|-|------>
                    *                    ### TAINT #####
                    *                    T_1         T_2
                    *           
                    */
                    newTaintRange = new Range(T_1, T_2 + (L_HR_d - 1));
                } else
                {
                    /* Case 9.3
                    *                                   HR_1       HR_2
                    *                                    #### HR #####
                    * --------------|-|------|-|---------|-|-------|-|------>
                    *               ## TAINT ###
                    *               T_1      T_2
                    */
                    newTaintRange = new Range(T_1, T_2);
                }
            }

            return newTaintRange;
        }
    }
}