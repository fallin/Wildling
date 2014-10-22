using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Wildling.Server.Extensions
{
    static class HttpRequestMessageExtensions
    {
        public static string GetHeader(this HttpRequestMessage request, string name)
        {
            string headerValue = null;
            IEnumerable<string> values;
            if (request.Headers.TryGetValues(name, out values))
            {
                headerValue = values.FirstOrDefault();
            }

            return headerValue;
        }
    }
}