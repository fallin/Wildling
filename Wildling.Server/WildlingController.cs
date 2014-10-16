using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Wildling.Core;

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
        public async Task<IHttpActionResult> Get(string key)
        {
            try
            {
                object value = await _node.GetAsync(key);
                var responseMessage = value as HttpResponseMessage;
                if (responseMessage != null)
                {
                    return ResponseMessage(responseMessage);
                }
                else
                {
                    return Ok(value);
                }
            }
            catch (KeyNotFoundException e)
            {
                return NotFound();
            }
        }

        [Route("{key}")]
        [HttpPut]
        public async Task<OkResult> Put(string key, [FromBody] object value, string context = null)
        {
            await _node.PutAsync(key, value);
            return Ok();
        }
    }
}