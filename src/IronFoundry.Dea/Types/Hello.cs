﻿using System;
using System.Net;
using IronFoundry.Dea.JsonConverters;
using Newtonsoft.Json;

namespace IronFoundry.Dea.Types
{
    public class Hello : Message
    {
        public Hello(Guid argID, IPAddress argAddress)
        {
            ID = argID;
            IPAddress = argAddress;
            Version = 0.99m;
        }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return "dea.start"; }
        }

        [JsonProperty(PropertyName = "id"), JsonConverter(typeof (VcapGuidConverter))]
        public Guid ID { get; private set; }

        [JsonProperty(PropertyName = "ip"), JsonConverter(typeof (IPAddressConverter))]
        public IPAddress IPAddress { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public decimal Version { get; private set; }

        public override bool CanPublishWithSubject(string subject)
        {
            return false == subject.IsNullOrWhiteSpace();
        }
    }
}