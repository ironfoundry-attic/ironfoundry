namespace IronFoundry.Bosh.Agent
{
    using System;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Utilities;
using IronFoundry.Nats.Client;

    public class HeartbeatProcessor : IDisposable
    {
        private const ushort MaxOutstandingHeartbeats = 2;

        private readonly ILog log;
        private readonly INatsClient natsClient;
        private readonly ActionTimer actionTimer;

        public HeartbeatProcessor(ILog log, INatsClient natsClient, TimeSpan heartbeatInterval)
        {
            this.log = log;
            this.natsClient = natsClient;
            this.actionTimer = new ActionTimer(log, heartbeatInterval, this.Beat, false);
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public void Dispose()
        {
            actionTimer.Dispose();
        }

        private void Beat()
        {
            /*
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