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
using HularionPlugin.PluginMeta.Request;
using HularionPlugin.PluginMeta.Response;
using HularionPlugin.Route;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionPlugin.PluginMeta
{
    public class MetaRouter : IRouteProvider
    {
        public string Name => "Meta Route Provider";

        public string Key => "HularionRoute.Meta";

        public string Purpose => "Provides meta data about routes.";

        private const string routeProviderQuery = "provider";

        public IEnumerable<HularionRoute> Routes => routes;

        private List<HularionRoute> routes { get; set; } = new List<HularionRoute>();

        //private IParameterizedProvider<HularionRoute, IRouteProvider> routeFromProvider;

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="baseRoutePath">The starting path for each route.</param>
        /// <param name="routesProvider"></param>
        /// <param name="providerProvider"></param>
        /// <param name="hasProvider"></param>
        public MetaRouter(string baseRoutePath,
            IParameterizedProvider<HularionRoute, IRouteProvider> routeFromProvider,
            IProvider<HularionRoute[]> routesProvider, 
            IParameterizedProvider<string, IRouteProvider> providerProvider,
            IParameterizedProvider<string, bool> hasProvider)
        {
            RegisterRoutes(baseRoutePath, routeFromProvider, routesProvider, providerProvider, hasProvider);
        }

        public MetaRouter(HularionRouter router)
        {
            RegisterRoutes(router.BaseRoutePath, router.RouteToProvider, router.RoutesProvider, router.ProviderProvider, router.HasProvider);
        }

        private void RegisterRoutes(string baseRoutePath,
            IParameterizedProvider<HularionRoute, IRouteProvider> routeFromProvider,
            IProvider<HularionRoute[]> routesProvider,
            IParameterizedProvider<string, IRouteProvider> providerProvider,
            IParameterizedProvider<string, bool> hasProvider)
        {

            HularionRoute route;
            route = new HularionRoute<MetaRouteRequest, MetaRouteResponse>()
            {
                Name = "Meta Route",
                Usage = "Provides information about available routes.",
                Route = String.Format(@"{0}/meta/routes", baseRoutePath),
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<MetaRouteRequest>, RoutedResponse<MetaRouteResponse>>(request =>
                {
                    var response = request.CreateResponse<MetaRouteResponse>();
                    response.Detail = new MetaRouteResponse();
                    if (request.Variables.ContainsKey(routeProviderQuery))
                    {
                        var provider = request.Variables[routeProviderQuery].ToLower();
                        if (hasProvider.Provide(provider))
                        {
                            var routeProvider = providerProvider.Provide(provider);
                            foreach (var registeredRoute in routeProvider.Routes)
                            {
                                response.Detail.Routes.Add(new RouteInfo()
                                {
                                    Name = registeredRoute.Name,
                                    Route = registeredRoute.Route,
                                    Usage = registeredRoute.Usage,
                                    Provider = provider
                                });
                            }
                        }
                        else
                        {
                            response.Messages.Add(new RoutedResponseMessage(type: RoutedResponseMessageType.Warn, message: string.Format("The indicated provider, '{0}', was not found.", provider)));
                        }
                    }
                    else
                    {
                        var routes = routesProvider.Provide();
                        foreach (var registeredRoute in routes)
                        {
                            var provider = routeFromProvider.Provide(registeredRoute);
                            response.Detail.Routes.Add(new RouteInfo()
                            {
                                Name = registeredRoute.Name,
                                Route = registeredRoute.Route,
                                Usage = registeredRoute.Usage,
                                Provider = provider == null ? null : provider.Key
                            });
                        }
                    }
                    return response;
                })
            };

            routes.Add(route);
        }
    }
}
