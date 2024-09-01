#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace HularionPlugin.Route
{
    /// <summary>
    /// A plugin's resonse message to a request.
    /// </summary>
    public class RoutedResponseMessage
    {
        /// <summary>
        /// A summary of the message.
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// The human-readable message.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// True iff this message describes an error.
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// The request that led to the response.
        /// </summary>
        public string RequestKey { get; set; }
        /// <summary>
        /// Any additional information related to the response.
        /// </summary>
        public object Other { get; set; }
        /// <summary>
        /// Indicates the kind of message.
        /// </summary>
        public RoutedResponseMessageType Type { get; set; }


        public RoutedResponseMessage()
        {

        }

        public RoutedResponseMessage(string? requestKey = null, bool? isError = null, RoutedResponseMessageType? type = null, string? header = null, string? message = null)
        {
            if (requestKey != null) { RequestKey = (string)requestKey; }
            if (isError != null) { IsError = (bool)isError; }
            if (type != null) { Type = (RoutedResponseMessageType)type; }
            if (header != null) { Header = (string)header; }
            if (message != null) { Message = (string)message; }
        }

    }

}
