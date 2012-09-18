using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using Newtonsoft.Json;

    public class VcapResponse : EntityBase
    {
        [JsonProperty(PropertyName = "code")]
        public int Code
        {
            get; set;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description
        {
            get; set;
        }
    }
}