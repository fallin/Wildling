using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json.Linq;
using Wildling.Core;
using Wildling.Server.Extensions;

namespace Wildling.Server
{
    [RoutePrefix("wildling/api")]
    public class WildlingController : ApiController
    {
        readonly Node _node;

        public WildlingController(Node node)
        {
            _node = node;
        }

        [Route("{key}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(HttpRequestMessage req, string key)
        {
            try
            {
                int n = GetN(req) ?? 3;

                JArray value = await _node.GetAsync(key, n);
                return Ok(value);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound();
            }
        }

        [Route("{key}")]
        [HttpPut]
        public async Task<OkResult> Put(HttpRequestMessage req, string key, [FromBody] JObject value, string context = null)
        {
            int n = GetN(req) ?? 3;

            await _node.PutAsync(key, value, n);
            return Ok();
        }

        int? GetN(HttpRequestMessage request)
        {
            int? n = null;
            string headerValue = request.GetHeader("X-Wildling-N");
            if (headerValue != null)
            {
                int parsedValue;
                if (int.TryParse(headerValue, out parsedValue))
                {
                    n = parsedValue;
                }
            }
            return n;
        }
    }
}