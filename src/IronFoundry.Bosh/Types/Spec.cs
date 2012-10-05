namespace IronFoundry.Bosh.Types
{
    using Newtonsoft.Json;

    public class Spec
    {
        [JsonProperty(PropertyName = "deployment")]
        public string Deployment { get; set; }

        [JsonProperty(PropertyName = "release")]
        public Release Release { get; set; }

        [JsonProperty(PropertyName = "job")]
        public Job Job { get; set; }

        [JsonProperty(PropertyName = "index")]
        public ushort Index { get; set; }
    }

    public class Release
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
    }

    public class Job
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "sha1")]
        public string SHA1 { get; set; }

        [JsonProperty(PropertyName = "blobstore_id")]
        public string BlobstoreID { get; set; }
    }
}

#if E3EF64C533CC6F
{
  "deployment": "wordpress",
  "release": {
    "name": "wordpress",
    "version": "1.2-dev"
  },
  "job": {
    "name": "nginx",
    "template": "nginx",
    "version": "0.1-dev",
    "sha1": "aa06a7bb78bbfe9193eb9485b3cba13b27db357a",
    "blobstore_id": "9f0b4508-9f87-4a5e-b324-68814aa7359d"
  },
  "index": 0,
  "networks": {
    "vlan10": {
      "ip": "172.21.10.125",
      "netmask": "255.255.255.0",
      "cloud_properties": {
        "name": "vlan10_172.21.10-vm"
      },
      "default": [
        "dns",
        "gateway"
      ],
      "dns": [
        "172.17.1.26",
        "172.17.1.27"
      ],
      "gateway": "172.21.10.1"
    }
  },
  "resource_pool": {
    "name": "infrastructure",
    "cloud_properties": {
      "cpu": 1,
      "disk": 8192,
      "ram": 4096
    },
    "stemcell": {
      "name": "bosh-stemcell",
      "version": "0.6.3"
    }
  },
  "packages": {
    "nginx": {
      "name": "nginx",
      "version": "0.1-dev.1",
      "sha1": "ed9f2de4c680e0536c052c2a56eb8d4464a03828",
      "blobstore_id": "ca67c02b-4f28-481b-9659-7ca2489fc081"
    }
  },
  "persistent_disk": 0,
  "configuration_hash": "de90d7e242b0f5be5fefc5bd1f0a1776e2b00d2b",
  "properties": {
    "wordpress": {
      "admin": "luke.bakken@tier3.com",
      "port": 8008,
      "servers": [
        "172.21.10.121"
      ],
      "servername": "wordpress.wfabric.com",
      "db": {
        "name": "wp",
        "user": "wordpress",
        "pass": "Pass@word1"
      },
      "auth_key": "0xdeadbeef",
      "secure_auth_key": "0xdeadbeef",
      "logged_in_key": "0xdeadbeef",
      "nonce_key": "0xdeadbeef",
      "auth_salt": "0xdeadbeef",
      "secure_auth_salt": "0xdeadbeef",
      "logged_in_salt": "0xdeadbeef",
      "nonce_salt": "0xdeadbeef"
    },
    "mysql": {
      "address": "172.21.10.120",
      "port": 3306,
      "password": "Pass@word1"
    },
    "nginx": {
      "workers": 1
    }
  }
}
#endif