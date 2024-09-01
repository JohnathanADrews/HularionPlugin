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
using HularionText.Language.Json;
using HularionText.Language.Json.Elements;
using HularionText.StringCase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace HularionPlugin.Route
{
    /// <summary>
    /// Routes requests to the registered handler.
    /// </summary>
    public class HularionRouter
    {

        private Dictionary<string, HularionRoute> routes = new Dictionary<string, HularionRoute>();
        private Dictionary<string, HularionRoute> anonymousRoutes = new Dictionary<string, HularionRoute>();

        private Dictionary<string, IRouteProvider> routeProviders = new Dictionary<string, IRouteProvider>();
        private Dictionary<HularionRoute, IRouteProvider> routeToProvider = new Dictionary<HularionRoute, IRouteProvider>();

        public JsonSerializer Serializer = new JsonSerializer(new StringCaseModifier(StringCaseDefinition.StartLower));
        public JsonSerializer Deserializer = new JsonSerializer(new StringCaseModifier(StringCaseDefinition.StartLower));

        public IParameterizedProvider<HularionRoute, IRouteProvider> RouteToProvider { get { return routeToProvider.MakeParameterizedProvider(); } }
        public IProvider<HularionRoute[]> RoutesProvider { get { return new ProviderFunction<HularionRoute[]>(() => routes.Values.ToArray()); } }
        public IParameterizedProvider<string, bool> HasProvider { get { return ParameterizedProvider.FromSingle<string, bool>(providerName => routeProviders.ContainsKey(providerName)); } }
        public IParameterizedProvider<string, IRouteProvider> ProviderProvider
        {
            get
            {
                return ParameterizedProvider.FromSingle<string, IRouteProvider>(providerName =>
                  {
                      if (routeProviders.ContainsKey(providerName)) 
                      { 
                          return routeProviders[providerName]; 
                      }
                      return null;
                  });
            }
        }

        private string[] routePath = new string[] { nameof(HularionRoute.Route) };


        public IEnumerable<HularionRoute> Routes => routes.Values.ToList();

        public readonly string BaseRoutePath;
        private readonly string systemRouteProvider;

        public HularionRouter(string baseRoutePath, string systemRouteProviderName = "system", StringCaseModifier? serializationCaseModifier = null, StringCaseModifier? deserializationCaseModifier = null)
        {
            BaseRoutePath = baseRoutePath;
            this.systemRouteProvider = systemRouteProviderName;
            if (serializationCaseModifier != null) { this.Serializer = new JsonSerializer(serializationCaseModifier); }
            if(deserializationCaseModifier != null) { this.Deserializer = new JsonSerializer(deserializationCaseModifier); }

        }


        /// <summary>
        /// Registers the route.
        /// </summary>
        /// <typeparam name="RequestMessageType">The type of the request message.</typeparam>
        /// <typeparam name="ResponseMessageType">The type of the response message.</typeparam>
        /// <param name="route">The route information for processing requests.</param>
        public void RegisterRoute<RequestMessageType, ResponseMessageType>(HularionRoute<RequestMessageType, ResponseMessageType> route)
        {
            lock (routes) { routes[route.Route] = route; }
        }

        public void RegisterRoutes(params HularionRoute[] hularionRoutes)
        {
            lock (routes) 
            { 
                foreach(var route in hularionRoutes)
                {
                    routes[route.Route] = route;
                }
            }
        }

        /// <summary>
        /// Registers the route provider and all the routes.
        /// </summary>
        /// <param name="routeProvider">The route provider.</param>
        public void RegisterRouteProvider(IRouteProvider routeProvider)
        {
            routeProviders[routeProvider.Key.ToLower()] = routeProvider;
            RegisterRoutes(routeProvider.Routes.ToArray());
            foreach (var route in routeProvider.Routes)
            {
                routeToProvider[route] = routeProvider;
            }
        }

        /// <summary>
        /// Processes the provided json request and returns the response.
        /// </summary>
        /// <param name="requestJson">The JSON contianing the request.</param>
        /// <returns>The request response.</returns>
        public RoutedResponse ProcessJsonRequest(string requestJson)
        {
            var jsonDocument = Serializer.ToJsonDocument(requestJson);
            var route = jsonDocument.GetFirstPathValue<string>(routePath);
            var baseRoute = RoutedRequest.GetBaseRoute(route);

            if (!routes.ContainsKey(baseRoute))
            {
                var unknownRequest = (RoutedRequest)Deserializer.Deserialize(typeof(RoutedRequest), requestJson);
                var routeNotAvailableResponse = unknownRequest.CreateResponse();
                routeNotAvailableResponse.State = RoutedResponseState.Failure;
                routeNotAvailableResponse.Messages.Add(new RoutedResponseMessage(message: String.Format("The provided route, '{0}', is unknown (xLUyKbpqcUSzeotmQvAkxA).", baseRoute)) { });
                return routeNotAvailableResponse;
            }

            var processor = routes[baseRoute];
            var requestType = typeof(RoutedRequest<>).MakeGenericType(processor.MessageType);
            var request = (RoutedRequest)Deserializer.Deserialize(requestType, requestJson);
            var response = processor.Handler.Process(request);
            return response;
        }

        /// <summary>
        /// Processes the provided json request and returns the response.
        /// </summary>
        /// <param name="requestJson">The JSON contianing the request.</param>
        /// <returns>The request response serialized to JSON.</returns>
        public string ProcessJsonRequestToJsonResult(string requestJson)
        {
            if (String.IsNullOrWhiteSpace(requestJson))
            {
                var routeNotAvailableResponse = new RoutedResponse();
                routeNotAvailableResponse.State = RoutedResponseState.Failure;
                routeNotAvailableResponse.Messages.Add(new RoutedResponseMessage(message: String.Format("The request had no content (G3gdnJ2WaU6xdEFBinvJaA).")) { });
                return Serializer.Serialize(routeNotAvailableResponse);
            }
            var jsonDocument = Deserializer.ToJsonDocument(requestJson);
            var route = jsonDocument.GetFirstPathValue<string>(routePath);
            var baseRoute = RoutedRequest.GetBaseRoute(route);

            if (!routes.ContainsKey(baseRoute))
            {
                var unknownRequest = (RoutedRequest)Deserializer.Deserialize(typeof(RoutedRequest), requestJson);
                var routeNotAvailableResponse = unknownRequest.CreateResponse();
                routeNotAvailableResponse.State = RoutedResponseState.Failure;
                routeNotAvailableResponse.Messages.Add(new RoutedResponseMessage(message: String.Format("The provided route, '{0}', is unknown (rFV2tlrdVkGn7yLhrMuuHw).", baseRoute)) { });
                return Serializer.Serialize(routeNotAvailableResponse);
            }

            var processor = routes[baseRoute];
            var requestType = typeof(RoutedRequest<>).MakeGenericType(processor.MessageType);
            var request = (RoutedRequest)Deserializer.Deserialize(requestType, requestJson);
            var response = processor.Handler.Process(request);
            return Serializer.Serialize(response);
        }

        /// <summary>
        /// Processes the request and returns the response.
        /// </summary>
        /// <typeparam name="RequestType">The request type.</typeparam>
        /// <typeparam name="ResponseType">The response type.</typeparam>
        /// <param name="request">The request to process.</param>
        /// <returns>The response.</returns>
        public RoutedResponse<ResponseType> ProcessRequest<RequestType, ResponseType>(RoutedRequest<RequestType> request)
        {
            var baseRoute = RoutedRequest.GetBaseRoute(request.Route);

            if (!routes.ContainsKey(baseRoute))
            {
                var routeNotAvailableResponse = request.CreateResponse<ResponseType>();
                routeNotAvailableResponse.State = RoutedResponseState.Failure;
                routeNotAvailableResponse.Messages.Add(new RoutedResponseMessage(message: String.Format("The provided route, '{0}', is unknown (s6fEn0zj2EWIqTgo5IOqrQ).", baseRoute)) { });
                return routeNotAvailableResponse;
            }
            if (anonymousRoutes.ContainsKey(baseRoute))
            {
                var processor = anonymousRoutes[baseRoute];
                var response = RoutedResponse<ResponseType>.FromAnonymous((RoutedResponse<object>)processor.Handler.Process(request.MakeAnonymous()));
                return response;
            }
            else
            {
                var processor = routes[baseRoute];
                var response = (RoutedResponse<ResponseType>)processor.Handler.Process(request);
                return response;
            }
        }

        /// <summary>
        /// Gets the route associated with the provided route string.
        /// </summary>
        /// <param name="route">The route string. RoutedRequest.GetBaseRoute is called: no need to call prior.</param>
        /// <returns>The route associated with the provided route string or null if the route string is not registered.</returns>
        public HularionRoute GetRoute(string route)
        {
            route = RoutedRequest.GetBaseRoute(route);
            if (routes.ContainsKey(route))
            {
                return routes[route];
            }
            return null;
        }

        public void RegisterAnonymousRoute(HularionRoute route)
        {
            var routeString = RoutedRequest.GetBaseRoute(route.Route);
            if (routes.ContainsKey(routeString))
            {
                var registred = routes[routeString];
                if (!registred.IsOpen)
                {
                    throw new ArgumentException(String.Format("The route {0} is not open. No anonymous route may be registered. Fn40aHxrm0el1UgXykdbUA", routeString));
                }
                anonymousRoutes[routeString] = route;
                var generics = route.GetType().GetGenericArguments();
            }
            else
            {
                routes[routeString] = route;
            }
        }

    }
}
