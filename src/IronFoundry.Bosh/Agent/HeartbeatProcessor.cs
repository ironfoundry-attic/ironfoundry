﻿namespace IronFoundry.Bosh.Agent
{
    using System;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Bosh.Messages;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Utilities;
    using IronFoundry.Nats.Client;

    public class HeartbeatProcessor : IDisposable
    {
        private const ushort MaxOutstandingHeartbeats = 2;

        private readonly ILog log;
        private readonly INatsClient natsClient;
        private readonly IBoshConfig config;
        private readonly ActionTimer actionTimer;

        public HeartbeatProcessor(ILog log, INatsClient natsClient, IBoshConfig config)
        {
            this.log = log;
            this.natsClient = natsClient;
            this.config = config;
            this.actionTimer = new ActionTimer(log, config.HeartbeatInterval, this.Beat, false, false);
        }

        public void Start()
        {
            actionTimer.Start();
        }

        public void Stop()
        {
            actionTimer.Stop();
        }

        public void Dispose()
        {
            actionTimer.Dispose();
        }

        private void Beat()
        {
            HeartbeatStateData hsb = config.HeartbeatStateData;

            var hb = new Heartbeat(config.AgentID)
                {
                    Job      = hsb.Job,
                    Index    = hsb.Index,
                    JobState = hsb.JobState,
                };

            hb.Vitals = new Vitals
            {
                Load = new[] { 0.00F, 0.00F, 0.00F },
                Cpu = new CpuStat { Sys = 0.00F, User = 0.00F, Wait = 0.00F },
                Mem = new UsageStat { Percent = 0.0F, KiloBytes = 1024 },
                Swap = new UsageStat { Percent = 0.0F, KiloBytes = 1024 },
                Disk = new DiskStat
                {
                    Ephemeral = new Percentage(0),
                    Persistent = new Percentage(0),
                    System = new Percentage(0),
                },
            };

            hb.Ntp = new NtpStat();

            natsClient.Publish(hb);

            /*
             *       @nats.publish("hm.agent.heartbeat.#{@agent_id}", heartbeat_payload) do
      raise HeartbeatError, "#{@pending} outstanding heartbeat(s)" if @pending > MAX_OUTSTANDING_HEARTBEATS
      Heartbeat.new.send_via_mbus do
        @pending -= 1
      end
      @pending += 1
    rescue => e
      Config.logger.warn("Error sending heartbeat: #{e}")
      Config.logger.warn(e.backtrace.join("\n"))
      raise e if @pending > MAX_OUTSTANDING_HEARTBEATS
             */
        }
    }
}