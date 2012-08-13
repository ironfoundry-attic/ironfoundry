namespace IronFoundry.Bosh.Messages
{
    using System;
    using IronFoundry.Bosh.Properties;
    using IronFoundry.Nats.Client;
    using Newtonsoft.Json;

    public class Heartbeat : NatsMessage
    {
        private readonly string publishSubject;

        public Heartbeat(string agentID)
        {
            if (agentID.IsNullOrWhiteSpace()) // TODO code contracts?
            {
                throw new ArgumentException(Resources.Heartbeat_RequiresAgentID_Message, "agentID");
            }
            this.publishSubject = String.Format("hm.agent.heartbeat.{0}", agentID);
        }

        [JsonProperty("job")]
        public string Job { get; set; }
        [JsonProperty("index")]
        public ushort Index { get; set; } // TODO comes from state
        [JsonProperty("job_state")]
        public string JobState { get; set; } // TODO one job per Agent
        [JsonProperty("vitals")]
        public Vitals Vitals { get; set; }
        [JsonProperty("ntp")]
        public NtpStat Ntp { get; set; }

        public override string PublishSubject
        {
            get { return publishSubject; }
        }

        public override bool CanPublishWithSubject(string subject)
        {
            return subject.Equals(publishSubject);
        }
    }

    public class Vitals
    {
        [JsonProperty("load")]
        public float[] Load { get; set; }
        [JsonProperty("cpu")]
        public CpuStat Cpu { get; set; }
        [JsonProperty("mem")]
        public UsageStat Mem { get; set; }
        [JsonProperty("swap")]
        public UsageStat Swap { get; set; }
        [JsonProperty("disk")]
        public DiskStat Disk { get; set; }
    }

    public class CpuStat
    {
        [JsonProperty("user")]
        public float User { get; set; }
        [JsonProperty("sys")]
        public float Sys { get; set; }
        [JsonProperty("wait")]
        public float Wait { get; set; }
    }

    public class UsageStat
    {
        [JsonProperty("percent")]
        public float Percent { get; set; }
        [JsonProperty("kb")]
        public uint KiloBytes { get; set; }
    }

    public class DiskStat
    {
        [JsonProperty("system")]
        public Percentage System { get; set; }
        [JsonProperty("ephemeral")]
        public Percentage Ephemeral { get; set; }
        [JsonProperty("persistent")]
        public Percentage Persistent { get; set; }
    }

    public class Percentage
    {
        public Percentage() { }

        public Percentage(ushort percent)
        {
            this.Percent = percent;
        }

        [JsonProperty("percent")]
        public ushort Percent { get; set; }
    }

    public class NtpStat
    {
        [JsonProperty("offset")]
        public float Offset { get; set; }
        [JsonProperty("timestamp")] // TODO converter?
        public string Timestamp { get; set; }
    }
}

/*
    # Heartbeat payload example:
    # {
    #   "job": "cloud_controller",
    #   "index": 3,
    #   "job_state":"running",
    #   "vitals": {
    #     "load": ["0.09","0.04","0.01"],
    #     "cpu": {"user":"0.0","sys":"0.0","wait":"0.4"},
    #     "mem": {"percent":"3.5","kb":"145996"},
    #     "swap": {"percent":"0.0","kb":"0"},
    #     "disk": {
    #       "system": {"percent" => "82"},
    #       "ephemeral": {"percent" => "5"},
    #       "persistent": {"percent" => "94"}
    #     },
    #   "ntp": {
    #       "offset": "-0.06423",
    #       "timestamp": "14 Oct 11:13:19"
    #   }
    # }

    def heartbeat_payload
      job_state = Bosh::Agent::Monit.service_group_state
      monit_vitals = Bosh::Agent::Monit.get_vitals

      # TODO(?): move DiskUtil out of Message namespace
      disk_usage = Bosh::Agent::Message::DiskUtil.get_usage

      job_name = @state["job"] ? @state["job"]["name"] : nil
      index = @state["index"]

      vitals = monit_vitals.merge("disk" => disk_usage)

      Yajl::Encoder.encode("job" => job_name,
                           "index" => index,
                           "job_state" => job_state,
                           "vitals" => vitals,
                           "ntp" => Bosh::Agent::NTP.offset)
*/