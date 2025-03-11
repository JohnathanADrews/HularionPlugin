using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Identifier;
using HularionPlugin.KeyGenerators.Request;
using HularionPlugin.KeyGenerators.Response;
using HularionPlugin.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionPlugin.KeyGenerators
{
    public class KeyGeneratorRouteProvider : IRouteProvider
    {
        public string Name => "KeyGenerator";

        public string Key => "Hularion.KeyGenerator";

        public string Purpose => "Provides unique keys.";

        public IEnumerable<HularionRoute> Routes => routes;

        private List<HularionRoute> routes { get; set; } = new List<HularionRoute>();

        private string baseRoute = "hularion/key/generator/";

        public KeyGeneratorRouteProvider()
        {
            HularionRoute route;
            route = new HularionRoute<ObjectKeyRequest, ObjectKeyResponse>()
            {
                Name = "Object Key Route",
                Usage = "Provides an object key or the specified number of object keys.",
                Route = String.Format(@"{0}objectkeys", baseRoute),
                Method = "GetObjectKeys",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<ObjectKeyRequest>, RoutedResponse<ObjectKeyResponse>>(request =>
                {
                    var response = request.CreateResponse<ObjectKeyResponse>();
                    if(request.Detail.Count <= 0)
                    {
                        response.Detail.Keys.Add(ObjectKey.CreateUniqueTag());
                    }
                    else
                    {
                        for(var i = 0; i < request.Detail.Count; i++)
                        {
                            response.Detail.Keys.Add(ObjectKey.CreateUniqueTag());
                        }
                    }
                    return response;
                })
            };
            routes.Add(route);

            route = new HularionRoute<GuidRequest, GuidResponse>()
            {
                Name = "Guid Provider Route",
                Usage = "Provides a GUID or the specified number of GUIDs.",
                Route = String.Format(@"{0}guids", baseRoute),
                Method = "GetGuids",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<GuidRequest>, RoutedResponse<GuidResponse>>(request =>
                {
                    var response = request.CreateResponse<GuidResponse>();
                    if(request.Detail.Count <= 0)
                    {
                        response.Detail.Guids.Add(Guid.NewGuid().ToString());
                    }
                    else
                    {
                        for(var i = 0; i < request.Detail.Count; i++)
                        {
                            response.Detail.Guids.Add(Guid.NewGuid().ToString());
                        }
                    }
                    return response;
                })
            };
            routes.Add(route);

        }


    }
}
