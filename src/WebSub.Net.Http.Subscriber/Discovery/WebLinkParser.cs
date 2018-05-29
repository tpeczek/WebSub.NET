using System;
using System.Collections.Generic;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    // Based on https://github.com/JornWildt/Ramone/blob/master/Ramone/Utility/WebLinkParser.cs
    internal class WebLinkParser
    {
        private const string HUB_RELATION = "hub";
        private const string TOPIC_RELATION = "self";

        private enum TokenType { None, Url, Semicolon, Comma, Assignment, Identifier, ExtendedIdentifier, String, EOF }

        private struct Token
        {
            public TokenType Type { get; set; }

            public string Value { get; set; }
        }

        public static WebSubDiscovery ParseWebLinkHeaders(IEnumerable<string> linkHeaders)
        {
            WebSubDiscovery webSubDiscovery = new WebSubDiscovery();

            foreach (string linkHeader in linkHeaders)
            {
                webSubDiscovery = ParseWebLinkHeader(webSubDiscovery, linkHeader);
            }

            return webSubDiscovery;
        }

        private static WebSubDiscovery ParseWebLinkHeader(WebSubDiscovery webSubDiscovery, string linkHeader)
        {
            string inputString = linkHeader;
            int inputPos = 0;

            Token nextToken = new Token { Type = TokenType.None };
            while (true)
            {
                try
                {
                    nextToken = ReadToken(inputString, ref inputPos);

                    if (nextToken.Type == TokenType.Url)
                    {
                        webSubDiscovery = ParseWebLinkHeaderValue(webSubDiscovery, inputString, ref inputPos, ref nextToken);
                    }
                    else if (nextToken.Type == TokenType.EOF)
                    {
                        break;
                    }
                    else
                    {
                        ThrowFormatException(String.Format("Unexpected token '{0}' (expected URL)", nextToken.Type), inputString, inputPos);
                    }

                    if (nextToken.Type == TokenType.Comma)
                    {
                        continue;
                    }
                    else if (nextToken.Type == TokenType.EOF)
                    {
                        break;
                    }
                    else
                    {
                        ThrowFormatException(String.Format("Unexpected token '{0}' (expected comma)", nextToken.Type), inputString, inputPos);
                    }
                }
                catch (FormatException)
                {
                    while (nextToken.Type != TokenType.Comma && nextToken.Type != TokenType.EOF)
                    {
                        try
                        {
                            nextToken = ReadToken(inputString, ref inputPos);
                        }
                        catch (FormatException)
                        { }
                    }
                }
            }

            return webSubDiscovery;
        }

        private static WebSubDiscovery ParseWebLinkHeaderValue(WebSubDiscovery webSubDiscovery, string inputString, ref int inputPos, ref Token nextToken)
        {
            string url = nextToken.Value;
            string rel = null;

            nextToken = ReadToken(inputString, ref inputPos);

            while (nextToken.Type == TokenType.Semicolon)
            {
                try
                {
                    nextToken = ReadToken(inputString, ref inputPos);

                    var (id, value) = ParseParameter(inputString, ref inputPos, ref nextToken);

                    if (id == "rel" && rel == null)
                    {
                        rel = value;
                    }
                }
                catch (FormatException)
                {
                    while (nextToken.Type != TokenType.Semicolon && nextToken.Type != TokenType.Comma && nextToken.Type != TokenType.EOF)
                    {
                        try
                        {
                            nextToken = ReadToken(inputString, ref inputPos);
                        }
                        catch (FormatException)
                        {
                        }
                    }
                }
            }

            if (rel == HUB_RELATION)
            {
                if (webSubDiscovery.Hubs == null)
                {
                    webSubDiscovery.Hubs = new List<string>();
                }

                webSubDiscovery.Hubs.Add(url);
            }
            else if (rel == TOPIC_RELATION)
            {
                if (!String.IsNullOrWhiteSpace(webSubDiscovery.Topic))
                {
                    throw new WebSubDiscoveryException("Multiple canonical URLs for the topic have been found.");
                }

                webSubDiscovery.Topic = url;
            }


            return webSubDiscovery;
        }

        private static (string id, string value) ParseParameter(string inputString, ref int inputPos, ref Token nextToken)
        {
            if (nextToken.Type != TokenType.Identifier && nextToken.Type != TokenType.ExtendedIdentifier)
            {
                ThrowFormatException(String.Format("Unexpected token '{0}' (expected an identifier)", nextToken.Type), inputString, inputPos);
            }

            string id = nextToken.Value;

            nextToken = ReadToken(inputString, ref inputPos);

            if (nextToken.Type != TokenType.Assignment)
            {
                ThrowFormatException(String.Format("Unexpected token '{0}' (expected an assignment)", nextToken.Type), inputString, inputPos);
            }

            if (id == "rel")
            {
                nextToken = ReadNextStringOrRelType(inputString, ref inputPos);
            }
            else
            {
                nextToken = ReadToken(inputString, ref inputPos);
            }

            if (nextToken.Type != TokenType.String)
            {
                ThrowFormatException(String.Format("Unexpected token '{0}' (expected an string)", nextToken.Type), inputString, inputPos);
            }

            string value = nextToken.Value;

            nextToken = ReadToken(inputString, ref inputPos);

            return (id, value);
        }

        private static Token ReadToken(string inputString, ref int inputPos)
        {
            while (true)
            {
                char? c = ReadNextChar(inputString, ref inputPos);

                if (c == null)
                {
                    return new Token { Type = TokenType.EOF };
                }

                if (c == ';')
                {
                    return new Token { Type = TokenType.Semicolon };
                }

                if (c == ',')
                {
                    return new Token { Type = TokenType.Comma };
                }

                if (c == '=')
                {
                    return new Token { Type = TokenType.Assignment };
                }

                if (c == '"')
                {
                    return new Token { Type = TokenType.String, Value = ReadString(inputString, ref inputPos) };
                }

                if (c == '<')
                {
                    return new Token { Type = TokenType.Url, Value = ReadUrl(inputString, ref inputPos) };
                }

                if (Char.IsWhiteSpace(c.Value))
                {
                    continue;
                }

                if (Char.IsLetter(c.Value))
                {
                    return ReadIdentifier(c.Value, inputString, ref inputPos);
                }

                ThrowFormatException(String.Format("Unrecognized character '{0}'", c), inputString, inputPos);
            }
        }

        private static char? ReadNextChar(string inputString, ref int inputPos)
        {
            if (inputPos == inputString.Length)
            {
                return null;
            }

            return inputString[inputPos++];
        }

        private static string ReadString(string inputString, ref int inputPos)
        {
            string result = String.Empty;

            while (true)
            {
                char? c = ReadNextChar(inputString, ref inputPos);

                if ((c == null) || (c == '"'))
                {
                    break;
                }

                result += c;
            }

            return result;
        }

        private static string ReadUrl(string inputString, ref int inputPos)
        {
            string result = String.Empty;

            while (true)
            {
                char? c = ReadNextChar(inputString, ref inputPos);

                if ((c == null) || (c == '>'))
                {
                    break;
                }

                result += c;
            }

            return result;
        }

        private static Token ReadIdentifier(char c, string inputString, ref int inputPos)
        {
            string id = String.Empty + c;

            while (Char.IsLetterOrDigit(inputString[inputPos]))
            {
                id += inputString[inputPos++];
            }

            if (inputString[inputPos] == '*')
            {
                inputPos++;
                return new Token { Type = TokenType.ExtendedIdentifier, Value = id };
            }
            else
            {
                return new Token { Type = TokenType.Identifier, Value = id };
            }
        }

        private static Token ReadNextStringOrRelType(string inputString, ref int inputPos)
        {
            while (true)
            {
                char? c = ReadNextChar(inputString, ref inputPos);

                if (c == null)
                {
                    return new Token { Type = TokenType.EOF };
                }

                if (c == '"')
                {
                    return new Token { Type = TokenType.String, Value = ReadString(inputString, ref inputPos) };
                }

                if (Char.IsLetter(c.Value))
                {
                    return new Token { Type = TokenType.String, Value = ReadRelType(c.Value, inputString, ref inputPos) };
                }

                ThrowFormatException(String.Format("Unrecognized character '{0}' for string or rel-type", c), inputString, inputPos);
            }
        }

        private static string ReadRelType(char c, string inputString, ref int inputPos)
        {
            string id = String.Empty + c;

            while (Char.IsLetterOrDigit(inputString[inputPos]) || inputString[inputPos] == '.' || inputString[inputPos] == '-')
            {
                id += inputString[inputPos++];
            }

            return id;
        }

        private static void ThrowFormatException(string msg, string inputString, int inputPos)
        {
            throw new FormatException(string.Format("Invalid HTTP Web Link. {0} in '{1}' (around pos {2}).", msg, inputString, inputPos));
        }
    }

}
