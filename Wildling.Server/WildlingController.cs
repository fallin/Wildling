using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                Siblings siblings = await _node.GetAsync(key);

                // Only return the values (strip-off the clocks)
                List<JToken> values = siblings.Select(s => s.Value).ToList();

                HttpResponseMessage res = req.CreateResponse(values);
                VersionVector context = _node.Kernel.Join(siblings);
                res.Headers.Add("X-Context", context.ToContextString());

                return ResponseMessage(res);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound();
            }
        }

        [Route("{key}")]
        [HttpPut]
        public async Task<OkResult> Put(HttpRequestMessage req, string key, [FromBody] JToken value)
        {
            VersionVector context = GetContext(req);

            await _node.PutAsync(key, value, context);
            return Ok();
        }

        [Route("replica/{key}")]
        [HttpGet]
        public IHttpActionResult GetReplica(HttpRequestMessage req, string key)
        {
            try
            {
                Siblings siblings = _node.GetReplicaAsync(key);
                return Ok(siblings);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound();
            }
        }

        [Route("replica/{key}")]
        [HttpPut]
        public IHttpActionResult PutReplica(HttpRequestMessage req, string key, [FromBody] Siblings siblings)
        {
            _node.PutReplicaAsync(key, siblings);
            return Ok();
        }

        VersionVector GetContext(HttpRequestMessage request)
        {
            VersionVector vv = null;
            string headerValue = request.GetHeader("X-Context");
            if (!string.IsNullOrEmpty(headerValue))
            {
                vv = VersionVector.FromContextString(headerValue);
            }
            return vv;
        }
    }
}