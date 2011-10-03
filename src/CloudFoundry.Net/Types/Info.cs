namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class Info : Message
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "build")]
        public string Build { get; set; }

        [JsonProperty(PropertyName = "support")]
        public string Support { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "limits")]
        public InfoLimits Limits { get; set; }

        [JsonProperty(PropertyName = "useage")]
        public InfoUsage Usage { get; set; }

        [JsonProperty(PropertyName = "framework")]
        public Framework[] Frameworks { get; set; }
    }

    public class InfoLimits : EntityBase
    {
        [JsonProperty(PropertyName = "memory")]
        public uint Memory { get; private set; }

        [JsonProperty(PropertyName = "app_uris")]
        public uint AppURIs { get; private set; }

        [JsonProperty(PropertyName = "services")]
        public uint Services { get; private set; }

        [JsonProperty(PropertyName = "apps")]
        public uint Apps { get; private set; }
    }

    public class InfoUsage : EntityBase
    {
        [JsonProperty(PropertyName = "memory")]
        public uint Memory { get; set; }

        [JsonProperty(PropertyName = "apps")]
        public uint Apps { get; set; }

        [JsonProperty(PropertyName = "services")]
        public uint Services { get; set; }
    }
}

#if BB999E9DB0AA47D09282F5651D349CAF
{
  "name": "vcap",
  "build": 2222,
  "support": "http://support.cloudfoundry.com",
  "version": "0.999",
  "description": "VMware's Cloud Application Platform",
  "user": "luke.bakken@tier3.com",
  "limits": {
    "memory": 32768,
    "app_uris": 16,
    "services": 32,
    "apps": 200
  },
  "usage": {
    "memory": 64,
    "apps": 1,
    "services": 1
  },
  "frameworks": {
    "aspdotnet": {
      "name": "aspdotnet",
      "runtimes": [
        {
          "name": "aspdotnet40",
          "version": "4.0.30319.1",
          "description": "ASP.NET 4.0 (4.0.30319.1)"
        }
      ],
      "appservers": [

      ],
      "detection": [
        {
          "web.config": true
        }
      ]
    },
    "django": {
      "name": "django",
      "runtimes": [
        {
          "name": "python26",
          "version": "2.6.5",
          "description": "Python 2.6.5"
        }
      ],
      "appservers": [

      ],
      "detection": [
        {
          "*.py": "."
        }
      ]
    },
    "java_web": {
      "name": "java_web",
      "runtimes": [
        {
          "name": "java",
          "version": "1.6",
          "description": "Java 6"
        }
      ],
      "appservers": [
        {
          "name": "tomcat",
          "description": "Tomcat"
        }
      ],
      "detection": [
        {
          "*.war": true
        }
      ]
    },
    "sinatra": {
      "name": "sinatra",
      "runtimes": [
        {
          "name": "ruby18",
          "version": "1.8.7",
          "description": "Ruby 1.8.7"
        },
        {
          "name": "ruby19",
          "version": "1.9.2p180",
          "description": "Ruby 1.9.2"
        }
      ],
      "appservers": [
        {
          "name": "thin",
          "description": "Thin"
        }
      ],
      "detection": [
        {
          "*.rb": "require 'sinatra'|require \"sinatra\""
        },
        {
          "config/environment.rb": false
        }
      ]
    },
    "php": {
      "name": "php",
      "runtimes": [
        {
          "name": "php",
          "version": "5.3",
          "description": "PHP 5"
        }
      ],
      "appservers": [
        {
          "name": "apache",
          "description": "Apache"
        }
      ],
      "detection": [
        {
          "*.php": true
        }
      ]
    },
    "grails": {
      "name": "grails",
      "runtimes": [
        {
          "name": "java",
          "version": "1.6",
          "description": "Java 6"
        }
      ],
      "appservers": [
        {
          "name": "tomcat",
          "description": "Tomcat"
        }
      ],
      "detection": [
        {
          "*.war": true
        }
      ]
    },
    "lift": {
      "name": "lift",
      "runtimes": [
        {
          "name": "java",
          "version": "1.6",
          "description": "Java 6"
        }
      ],
      "appservers": [
        {
          "name": "tomcat",
          "description": "Tomcat"
        }
      ],
      "detection": [
        {
          "*.war": true
        }
      ]
    },
    "spring": {
      "name": "spring",
      "runtimes": [
        {
          "name": "java",
          "version": "1.6",
          "description": "Java 6"
        }
      ],
      "appservers": [
        {
          "name": "tomcat",
          "description": "Tomcat"
        }
      ],
      "detection": [
        {
          "*.war": true
        }
      ]
    },
    "rails3": {
      "name": "rails3",
      "runtimes": [
        {
          "name": "ruby18",
          "version": "1.8.7",
          "description": "Ruby 1.8.7"
        },
        {
          "name": "ruby19",
          "version": "1.9.2p180",
          "description": "Ruby 1.9.2"
        }
      ],
      "appservers": [
        {
          "name": "thin",
          "description": "Thin"
        }
      ],
      "detection": [
        {
          "config/application.rb": true
        },
        {
          "config/environment.rb": true
        }
      ]
    },
    "wsgi": {
      "name": "wsgi",
      "runtimes": [
        {
          "name": "python26",
          "version": "2.6.5",
          "description": "Python 2.6.5"
        }
      ],
      "appservers": [

      ],
      "detection": [
        {
          "*.py": "."
        }
      ]
    },
    "otp_rebar": {
      "name": "otp_rebar",
      "runtimes": [
        {
          "name": "erlangR14B02",
          "version": "R14B02",
          "description": "Erlang R14B02"
        }
      ],
      "appservers": [

      ],
      "detection": [
        {
          "releases/*/*.rel": "."
        }
      ]
    },
    "node": {
      "name": "node",
      "runtimes": [
        {
          "name": "node",
          "version": "0.4.5",
          "description": "Node.js"
        }
      ],
      "appservers": [

      ],
      "detection": [
        {
          "*.js": "."
        }
      ]
    }
  }
}
#endif