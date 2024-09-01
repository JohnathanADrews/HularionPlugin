#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionPlugin.Route
{
    /// <summary>
    /// Contains the routing and processing information for a request.
    /// </summary>
    public abstract class HularionRoute
    {
        /// <summary>
        /// The unique name to identify this route among others in the plugin.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The string indicating how the request should be routed.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// An optional method name for a caller. Case may be modified as appropriate.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Indicates how the route should be used.
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// true iff the route is open for setting.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// The type of the route message.
        /// </summary>
        public abstract Type MessageType { get; protected set; }

        /// <summary>
        /// The function that processes the request.
        /// </summary>
        public IParameterizedFacade<RoutedRequest, RoutedResponse> Handler { get; set; }

    }

    /// <summary>
    /// A route with the typed message indicated.
    /// </summary>
    /// <typeparam name="RouteRequestMessageType"></typeparam>
    public class HularionRoute<RouteRequestMessageType, RouteResponseMessageType> : HularionRoute
    {
        /// <summary>
        /// The type of the route message.
        /// </summary>
        public override Type MessageType { get; protected set; }

        /// <summary>
        /// The function that processes the request.
        /// </summary>
        public new IParameterizedFacade<RoutedRequest<RouteRequestMessageType>, RoutedResponse<RouteResponseMessageType>> Handler { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public HularionRoute()
        {
            this.MessageType = typeof(RouteRequestMessageType);
            base.Handler = ParameterizedFacade.FromSingle<RoutedRequest, RoutedResponse>(request => Handler.Process((RoutedRequest<RouteRequestMessageType>)request));
        }

        /// <summary>
        /// Sets the handler using a Func.
        /// </summary>
        /// <param name="handler">The Func to handle the request.</param>
        public void SetHandler(Func<RoutedRequest<RouteRequestMessageType>, RoutedResponse<RouteResponseMessageType>> handler)
        {
            Handler = ParameterizedFacade.FromSingle<RoutedRequest<RouteRequestMessageType>, RoutedResponse<RouteResponseMessageType>>(request => handler(request));
        }

        /// <summary>
        /// Sets the handler using a IParameterizedFacade.
        /// </summary>
        /// <param name="handler">The IParameterizedFacade to handle the request.</param>
        public void SetHandlerFacade(IParameterizedFacade<RoutedRequest<RouteRequestMessageType>, RoutedResponse<RouteResponseMessageType>> handler)
        {
            Handler = handler;
        }

    }
}
