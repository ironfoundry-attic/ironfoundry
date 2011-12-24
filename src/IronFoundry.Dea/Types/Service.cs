namespace IronFoundry.Dea.Types
{
    using System;
    using Newtonsoft.Json;

    public class Service : EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public string[] Tags { get; set; }

        [JsonProperty(PropertyName = "plan")]
        public string Plan { get; set; }

        [JsonProperty(PropertyName = "plan_option")]
        public string PlanOption { get; set; }

        [JsonProperty(PropertyName = "credentials")]
        public Credentials Credentials { get; set; }

        [JsonIgnore]
        public bool IsMSSqlServer
        {
            get { return false == Vendor.IsNullOrWhiteSpace() && "mssql" == Vendor; } // TODO OK for now
        }
    }
}
/*
NB: Documentation for a vcap message with service info
{
  "services":[
    {
      "name":"mysql-cf",
      "type":"database",
      "label":"mysql-5.1",
      "vendor":"mysql",
      "version":"5.1",
      "tags":[
        "mysql",
        "mysql-5.1",
        "relational"
      ],
      "plan":"free",
      "plan_option":null,
      "credentials":{
        "name":"d9ccf6c9b1c384c1182eb3b5d075c48b8",
        "hostname":"127.0.0.1",
        "host":"127.0.0.1",
        "port":3306,
        "user":"ujOkmwbBtdffF",
        "username":"ujOkmwbBtdffF",
        "password":"pCNJsjwoV25BV"
      }
    }
  ],
}
*/