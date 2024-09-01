#region License
/*
MIT License

Copyright (c) 2024 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionPlugin.Route
{
    public class RouterProvider : IRouteProvider
    {
        public string Name { get; set; }

        public string Key { get; set; }

        public string Purpose { get; set; }

        public IEnumerable<HularionRoute> Routes => AnonymousRoutes;

        public List<HularionRoute> AnonymousRoutes = new List<HularionRoute>();


        public RouterProvider()
        {
        }

        public RouterProvider(IRouteProvider source)
        {
            AssignSource(source);
        }

        public RouterProvider(IRouteProvider source, string prefix)
        {
            AssignSource(source, prefix);
        }

        public void AddRouteProvider(IRouteProvider source, string prefix = null)
        {
            AssignSource(source, prefix);
        }

        private void AssignSource(IRouteProvider source, string prefix = null)
        {
            Name = source.Name;
            Key = source.Key;
            Purpose = source.Purpose;
            if(prefix != null)
            {
                Key = prefix;
            }
            if(prefix == null)
            {
                AnonymousRoutes = source.Routes.ToList();
            }
            else
            {
                foreach(var route in source.Routes)
                {
                    var clone = (HularionRoute)Activator.CreateInstance(route.GetType());
                    clone.Name = route.Name;
                    clone.Usage = route.Usage;
                    clone.IsOpen = route.IsOpen;
                    clone.Handler = route.Handler;
                    clone.Route = String.Format("{0}/{1}", prefix, route.Route);
                    AnonymousRoutes.Add(clone);
                }
            }
        }


    }
}
