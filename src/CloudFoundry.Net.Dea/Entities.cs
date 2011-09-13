namespace CloudFoundry.Net.Dea.Entities
{

    /*
    [DataContract]
    public class Limits
    {
        [DataMember]
        public int mem;

        [DataMember]
        public int disk;

        [DataMember]
        public int fds;
    }    
     */

    /*
    [DataContract]
    public class DiscoverMessage
    {
        [DataMember]
        public int droplet;
        [DataMember]
        public string name;
        [DataMember]
        public string runtime;
        [DataMember]
        public string sha;
        [DataMember]
        public Limits limits;
    }
     */

    /*
    [DataContract]
    public class Stats
    {
        [DataMember]
        public string name;
        [DataMember]
        public string host;
        [DataMember]
        public int port;
        [DataMember]
        public double uptime;
        [DataMember]
        public string[] uris;
        [DataMember]
        public int mem_quota;
        [DataMember]
        public int disk_quota;
        [DataMember]
        public int fds_quota;
        [DataMember]
        public int cores;
        [DataMember]
        public int usage;
    }
     */

    /*
    [DataContract]
    public class Status : Hello
    {
        [DataMember]
        public int max_memory;
        [DataMember]
        public int reserved_memory;
        [DataMember]
        public int used_memory;
        [DataMember]
        public int num_clients;
        [DataMember]
        public string state;
    }
     */

    /*
    [DataContract]
    public class VcapComponentDiscover
    {
        [DataMember]
        public string type;

        [DataMember]
        public int index;

        [DataMember]
        public string uuid;

        [DataMember]
        public string host;

        [DataMember]
        public string credentials;

        [DataMember]
        public string start;
    }
     */

    /*
    [DataContract]
    public class Droplet
    {
        [DataMember]
        public uint droplet;

        [DataMember]
        public string name;

        [DataMember]
        public string[] uris;

        [DataMember]
        public string runtime;

        [DataMember]
        public string framework;

        [DataMember]
        public string sha1;

        [DataMember]
        public string executableFile;

        [DataMember]
        public string executableUri;

        [DataMember]
        public string version;

        [DataMember]
        public string[] services;

        [DataMember]
        public Limits limits;

        [DataMember]
        public string[] env;

        [DataMember]
        public string[] users;

        [DataMember]
        public string index;
    }
     */

    /*
    [DataContract]
    public class Instance
    {
        [DataMember]
        public uint droplet_id;
        [DataMember]
        public string instance_id;
        [DataMember]
        public string instance_index;
        [DataMember]
        public string name;
        [DataMember]
        public string dir;
        [DataMember]
        public string[] uris;
        [DataMember]
        public string[] users;
        [DataMember]
        public string version;
        [DataMember]
        public int mem_quota;
        [DataMember]
        public int disk_quota;
        [DataMember]
        public int fds_quota;
        [DataMember]
        public string state;
        [DataMember]
        public string runtime;
        [DataMember]
        public string framework;
        [DataMember]
        public string start;
        [DataMember]
        public int state_timestamp;
        [DataMember]
        public string log_id;
        [DataMember]
        public ushort port;
        [DataMember]
        public string staged;
        [DataMember]
        public string exit_reason;
        [DataMember]
        public string sha1;
        [DataMember]
        public string host;
        
    }
     */

    /*
    [DataContract]
    public class Heartbeat
    {
        [DataMember]
        public int droplet;
        [DataMember]
        public string version;
        [DataMember]
        public string instance;
        [DataMember]
        public string index;
        [DataMember]
        public string state;
        [DataMember]
        public int state_timestamp;
    }
     */

    /*
    [DataContract]
    public class Tag
    {
        [DataMember]
        public string framework;
        [DataMember]
        public string runtime;
    }
     */

    /*
    [DataContract]
    public class RouterRegister
    {
        [DataMember]
        public string dea;

        [DataMember]
        public string host;

        [DataMember]
        public int port;

        [DataMember]
        public string[] uris;

        [DataMember]
        public Tag tags;
    }
*/

    /*
    [DataContract]
    public class Hello
    {
        [DataMember]
        public string id;

        [DataMember]
        public string ip;

        [DataMember]
        public int port;

        [DataMember]
        public double version;
    }
     */

    /*
    [DataContract]
    public class FindDroplet
    {
        [DataMember]
        public int droplet;
        [DataMember]
        public int[] indices;
        [DataMember]
        public string[] states;
        [DataMember]
        public string version;
    }
     */

    /*
    [DataContract]
    public class FindDropletResponse
    {
        [DataMember]
        public string dea;
        [DataMember]
        public string version;
        [DataMember]
        public uint droplet;
        [DataMember]
        public string instance;
        [DataMember]
        public string index;
        [DataMember]
        public string state;
        [DataMember]
        public int state_timestamp;
        [DataMember]
        public string file_uri;
        [DataMember]
        public string credentials;
        [DataMember]
        public string staged;
        [DataMember]
        public Stats stats;
    }
     */

    /*
    [DataContract]
    public class InstanceEntry
    {
        [DataMember]
        public string instance_id;
        [DataMember]
        public Instance instance;
    }
     */

    /*
    [DataContract]
    public class DropletEntry
    {
        [DataMember]
        public uint droplet;
        [DataMember]
        public InstanceEntry[] instances;
    }
     */

    /*
    [DataContract]
    public class Snapshot
    {
        [DataMember]
        public DropletEntry[] entries;
    }
     */
}