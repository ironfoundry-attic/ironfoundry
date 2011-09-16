namespace CloudFoundry.Net.Types.Test
{
    using System;
    using Xunit;

    public class JsonSerializationTests
    {
        private Random r = new Random();

        [Fact]
        public void Can_Serialize_Snapshot()
        {
            var snapshot = new Snapshot
            {
                Entries = new[]
                {
                    new DropletEntry
                    {
                        DropletID = (uint)r.Next(), Instances = new[]
                        {
                            new InstanceEntry
                            {
                                InstanceID = Guid.NewGuid(),
                                Instance = new Instance(new Droplet
                                {
                                    ID = (uint)r.Next(),
                                })
                            }
                        }
                    },
                    new DropletEntry
                    {
                        DropletID = (uint)r.Next(), Instances = new[]
                        {
                            new InstanceEntry
                            {
                                InstanceID = Guid.NewGuid(),
                                Instance = new Instance(new Droplet
                                {
                                    ID = (uint)r.Next(),
                                })
                            }
                        }
                    },
                },
            };

            string json = snapshot.ToJson();

            Assert.False(String.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void Can_Serialize_RouterRegister()
        {
            Guid dea = Guid.NewGuid();

            string host = randomString();

            ushort port = (ushort)r.Next(ushort.MinValue, ushort.MaxValue);

            string[] uris = new[] { randomString(), randomString () };

            string framework = randomString();

            string runtime = randomString();

            var tag = new Tag
            {
                Framework = framework, Runtime = runtime,
            };

            var rr = new RouterRegister
            {
                Dea = dea, Host = host, Port = port, Uris = uris, Tag = tag
            };

            string json = rr.ToJson();

            Assert.False(String.IsNullOrWhiteSpace(json));
        }

        private static string randomString()
        {
            return Guid.NewGuid().ToString();
        }
    }
}