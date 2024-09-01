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
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionPlugin.Route
{
    /// <summary>
    /// The response from a plugin for a request.
    /// </summary>
    public class RoutedResponse
    {
        /// <summary>
        /// The key of the request.
        /// </summary>
        public string RequestKey { get; set; }
        /// <summary>
        /// The messages from the handler.
        /// </summary>
        public List<RoutedResponseMessage> Messages { get; set; } = new List<RoutedResponseMessage>();
        /// <summary>
        /// The state of the response.
        /// </summary>
        public RoutedResponseState State { get; set; } = RoutedResponseState.Success;

        public bool IsFailure { get; set; } = false;


        public RoutedResponse()
        {

        }

        public void SetAsFailure(params RoutedResponseMessage[] messages)
        {
            State = RoutedResponseState.Failure;
            IsFailure = true;
            Messages.AddRange(messages);
        }

        public void SetAsSuccess(params RoutedResponseMessage[] messages)
        {
            State = RoutedResponseState.Success;
            IsFailure = false;
            Messages.AddRange(messages);
        }

        public void SetAsEncodingNotSupported(params RoutedResponseMessage[] messages)
        {
            State = RoutedResponseState.EncodingNotSupported;
            Messages.AddRange(messages);
        }

    }

    /// <summary>
    /// A message with the message type.
    /// </summary>
    /// <typeparam name="MessageType"></typeparam>
    public class RoutedResponse<MessageType> : RoutedResponse
    {
        /// <summary>
        /// The object containing the request message.
        /// </summary>
        public new MessageType Detail { get; set; }

        public RoutedResponse()
        {

        }

        public static RoutedResponse<MessageType> FromAnonymous(RoutedResponse<object> anonymous)
        {
            var result = new RoutedResponse<MessageType>();
            if (anonymous.Detail != null && anonymous.Detail.GetType() == typeof(MessageType))
            {
                result.Detail = (MessageType)anonymous.Detail;
            }
            return result;
        }
    }

}
