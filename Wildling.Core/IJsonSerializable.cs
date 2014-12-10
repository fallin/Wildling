using System;
using Newtonsoft.Json.Linq;

namespace Wildling.Core
{
    interface IJsonSerializable
    {
        JToken ToJson();
    }
}