#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.General;
using HularionCore.Pattern.Functional;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionPlugin.Route
{
    /// <summary>
    /// A routed request.
    /// </summary>
    public class RoutedRequest
    {
        /// <summary>
        /// The key of the request.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The key of the user making the request.
        /// </summary>
        public string UserKey { get; set; }
        /// <summary>
        /// Determines which request handler is used. 
        /// </summary>
        public string Route { get; set; }
        /// <summary>
        /// The encoding of the request.
        /// </summary>
        public string Encoding { get; set; }

        /// <summary>
        /// The query parameters.
        /// </summary>
        public Dictionary<string, string> Variables { get { return GetBaseVariables(); } }

        private const char equals = '=';
        private readonly char[] ampersandSplit = new char[] { '&' };
        private const char questionMark = '?';
        private readonly char[] equalsMarkSplit = new char[] { equals };

        /// <summary>
        /// Gets the variables from the route string.
        /// </summary>
        /// <returns>The variables represented as key/value pairs.</returns>
        public Dictionary<string, string> GetBaseVariables()
        {
            var variables = new Dictionary<string, string>();
            if (Route.IndexOf(questionMark) > -1)
            {
                var r = Route.Substring(Route.IndexOf(questionMark) + 1, Route.Length - Route.IndexOf(questionMark) - 1);
                var splits = r.Split(ampersandSplit);
                foreach (var split in splits)
                {
                    if (split.IndexOf(equals) > -1)
                    {
                        var s2 = split.Split(equalsMarkSplit);
                        if (!variables.ContainsKey(s2[0])) { variables.Add(s2[0], s2[1]); }
                    }
                    else
                    {
                        if (!variables.ContainsKey(split)) { variables.Add(split, split); }
                    }
                }
            }
            return variables;
        }

        /// <summary>
        /// Gets the base route from the route string.
        /// </summary>
        /// <param name="route">The complete route string.</param>
        /// <returns>The base route string.</returns>
        public static string GetBaseRoute(string route)
        {
            if (String.IsNullOrWhiteSpace(route)) { return string.Empty; }
            if (route.IndexOf(questionMark) > -1)
            {
                return route.Substring(0, route.IndexOf(questionMark)).ToLower();
            }
            return route.ToLower();
        }

        public RoutedResponse CreateResponse()
        {
            return new RoutedResponse()
            {
                RequestKey = Key
            };
        }


        public RoutedResponseMessage CreateMessage(bool? isError = null, RoutedResponseMessageType? type = null, string? header = null, string? message = null)
        {
            return new RoutedResponseMessage(requestKey: Key, isError: isError, type: type, header: header, message: message);
        }

        public RoutedResponseMessage CreateErrorMessage(string? header = null, string? message = null)
        {
            return new RoutedResponseMessage(requestKey: Key, isError: true, type:  RoutedResponseMessageType.Error, header: header, message: message);
        }

    }

    /// <summary>
    /// A message with the message type.
    /// </summary>
    /// <typeparam name="MessageType"></typeparam>
    public class RoutedRequest<MessageType> : RoutedRequest
    {
        /// <summary>
        /// The object containing the request message.
        /// </summary>
        public MessageType Detail { get; set; }

        public RoutedResponse<MessageType> CreateResponse<MessageType>()
        {
            return new RoutedResponse<MessageType>()
            {
                RequestKey = Key,
                Detail = Activator.CreateInstance<MessageType>()
            };
        }

        public RoutedRequest<object> MakeAnonymous()
        {
            var mapper = new MemberMapper();
            mapper.CreateMap<RoutedRequest<MessageType>, RoutedRequest<object>>();
            var result = new RoutedRequest<object>();
            mapper.Map(this, result);
            return result;
        }
    }

}
