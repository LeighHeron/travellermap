﻿using Maps.Pages;
using Maps.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Maps
{
    public abstract class HandlerBase : ITypeAccepter
    {
        protected void ParseOptions(HttpContext context, ref MapOptions options, ref Stylesheet.Style style)
        {
            ParseOptions(context.Request, Defaults(context), ref options, ref style);
        }
        public static void ParseOptions(HttpRequest request, IDictionary<string, Object> queryDefaults, ref MapOptions options, ref Stylesheet.Style style)
        {
            options = (MapOptions)GetIntOption(request, "options", queryDefaults, (int)options);

#if LEGACY_STYLES
            // Handle deprecated/legacy options bits for selecting style
            style =
                (options & MapOptions.StyleMaskDeprecated) == MapOptions.PrintStyleDeprecated ? Stylesheet.Style.Atlas :
                (options & MapOptions.StyleMaskDeprecated) == MapOptions.CandyStyleDeprecated ? Stylesheet.Style.Candy :
                Stylesheet.Style.Poster;
#endif // LEGACY_STYLES

            if (HasOption(request, "style", queryDefaults))
            {
                switch (GetStringOption(request, "style", queryDefaults).ToLowerInvariant())
                {
                    case "poster": style = Stylesheet.Style.Poster; break;
                    case "atlas": style = Stylesheet.Style.Atlas; break;
                    case "print": style = Stylesheet.Style.Print; break;
                    case "candy": style = Stylesheet.Style.Candy; break;
                }
            }
        }

        private static System.Web.Routing.RouteValueDictionary Defaults(HttpContext context)
        {
            return (context.Items["RouteData"] as System.Web.Routing.RouteData).Values;
        }

        protected bool HasOption(HttpContext context, string name)
        {
            return HasOption(context.Request, name, Defaults(context));
        }
        public static bool HasOption(HttpRequest request, string name, IDictionary<string, Object> queryDefaults)
        {
           return request[name] != null || (queryDefaults != null && queryDefaults.ContainsKey(name));
        }

        protected string GetStringOption(HttpContext context, string name, string defaultValue = null)
        {
            return GetStringOption(context.Request, name, Defaults(context), defaultValue);
        }
        public static string GetStringOption(HttpRequest request, string name, IDictionary<string, Object> queryDefaults, string defaultValue = null)
        {
            if (request[name] != null)
                return request[name];
            if (queryDefaults != null && queryDefaults.ContainsKey(name))
                return queryDefaults[name].ToString();
            return defaultValue;
        }

        protected int GetIntOption(HttpContext context, string name, int defaultValue)
        {
            return GetIntOption(context.Request, name, Defaults(context), defaultValue);
        }
        public static int GetIntOption(HttpRequest request, string name, IDictionary<string, Object> queryDefaults, int defaultValue)
        {
            int temp;
            if (Int32.TryParse(GetStringOption(request, name, queryDefaults), NumberStyles.Integer, CultureInfo.InvariantCulture, out temp))
            {
                return temp;
            }
            return defaultValue;
        }

        protected double GetDoubleOption(HttpContext context, string name, double defaultValue)
        {
            return GetDoubleOption(context.Request, name, Defaults(context), defaultValue);
        }
        public static double GetDoubleOption(HttpRequest request, string name, IDictionary<string, Object> queryDefaults, double defaultValue)
        {
            double temp;
            if (Double.TryParse(GetStringOption(request, name, queryDefaults), NumberStyles.Float, CultureInfo.InvariantCulture, out temp))
            {
                return temp;
            }
            return defaultValue;
        }

        protected bool GetBoolOption(HttpContext context, string name, bool defaultValue)
        {
            return GetBoolOption(context.Request, name, Defaults(context), defaultValue);
        }
        public static bool GetBoolOption(HttpRequest request, string name, IDictionary<string, Object> queryDefaults, bool defaultValue)
        {
            int temp;
            if (Int32.TryParse(GetStringOption(request, name, queryDefaults), NumberStyles.Integer, CultureInfo.InvariantCulture, out temp))
            {
                return temp != 0;
            }
            return defaultValue;
        }

        public static void SendError(HttpResponse response, int code, string description, string message)
        {
            response.StatusCode = code;
            response.StatusDescription = description;
            response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;
            response.Output.WriteLine(message);
        }

        public abstract string DefaultContentType { get; }

        public bool Accepts(HttpContext context, string mediaType)
        {
            return AcceptTypes(context).Contains(mediaType);
        }

        public IEnumerable<string> AcceptTypes(HttpContext context)
        {
            IDictionary<string, Object> queryDefaults = null;
            if (context.Items.Contains("RouteData"))
                queryDefaults = (context.Items["RouteData"] as System.Web.Routing.RouteData).Values;

            if (context.Request["accept"] != null)
            {
                yield return context.Request["accept"];
            }
            if (queryDefaults != null && queryDefaults.ContainsKey("accept"))
            {
                yield return queryDefaults["accept"].ToString();
            }
            if (context.Request.AcceptTypes != null)
            {
                foreach (var type in context.Request.AcceptTypes)
                {
                    yield return type;
                }
            }

            yield return DefaultContentType;
        }
    }
}
