namespace IronFoundry.Misc.Utilities
{
    using System;
    using System.Timers;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Properties;

    public class ActionTimer : IDisposable
    {
        private readonly ILog log;
        private readonly CallbackTimer callbackTimer;

        /// <summary>
        /// Schedules an immediate one-time callback.
        /// </summary>
        public ActionTimer(ILog log, TimeSpan interval, Action callback)
            : this(log, interval, callback, true, true) { }

        public ActionTimer(ILog log, TimeSpan interval, Action callback, bool oneTime, bool startImmediately)
        {
            this.log = log;
            
            if (oneTime)
            {
                this.callbackTimer = ScheduleOneTimeAction(interval, callback);
            }
            else
            {
                this.callbackTimer = SchedulePeriodicAction(interval, callback);
            }

            if (startImmediately)
            {
                this.callbackTimer.SetEnabled();
            }
        }

        public void Start()
        {
            if (null != callbackTimer)
            {
                this.callbackTimer.SetEnabled();
            }
        }

        public void Stop()
        {
            if (null != callbackTimer)
            {
                this.callbackTimer.SetDisabled();
            }
        }

        public void Dispose()
        {
            if (null != callbackTimer && false == callbackTimer.GetDisposed())
            {
                callbackTimer.SetDisabled();
                callbackTimer.Dispose();
            }
        }

        private CallbackTimer ScheduleOneTimeAction(TimeSpan interval, Action callback)
        {
            var ct = new CallbackTimer(interval, callback) { AutoReset = false };
            ct.Elapsed += oneTimeTimerElapsed;
            return ct;
        }

        private void oneTimeTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Action timerAction = null;
            try
            {
                var timer = (CallbackTimer)sender;
                timerAction = timer.TimerAction;
                timer.Enabled = false;
                timer.Elapsed -= oneTimeTimerElapsed;
                timer.Close();
                timer.Dispose();
            }
            catch { }

            try
            {
                if (null != timerAction)
                {
                    timerAction();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, Resources.ActionTimer_UnhandledException_Message);
            }
        }

        private CallbackTimer SchedulePeriodicAction(TimeSpan interval, Action callback)
        {
            var ct = new CallbackTimer(interval, callback) { AutoReset = false };
            ct.Disposed += periodicTimerDisposed;
            ct.Elapsed += periodicTimerElapsed;
            return ct;
        }

        private void periodicTimerDisposed(object sender, EventArgs e)
        {
            var ct = (CallbackTimer)sender;
            ct.Disposed -= periodicTimerDisposed;
            ct.Elapsed -= periodicTimerElapsed;
            ct.SetDisabled();
            ct.SetDisposed();
        }

        private void periodicTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var actionTimer = (CallbackTimer)sender;
            Action timerAction = actionTimer.TimerAction;
            try
            {
                timerAction();
            }
            catch (Exception ex)
            {
                log.Error(ex, Resources.ActionTimer_UnhandledException_Message); 
            }

            actionTimer.SetEnabled();
        }

        private class CallbackTimer : Timer
        {
            private readonly object disposedLock = new object();
            private bool isDisposed = false;

            private readonly Action timerAction;

            public CallbackTimer(TimeSpan interval, Action timerAction)
                : base(Math.Max(interval.TotalMilliseconds, 1))
            {
                this.timerAction = timerAction;
            }

            public Action TimerAction { get { return timerAction; } }

            public void SetEnabled()
            {
                lock (disposedLock)
                {
                    if (false == isDisposed)
                    {
                        this.Enabled = true;
                    }
                }
            }

            public void SetDisabled()
            {
                lock (disposedLock)
                {
                    if (false == isDisposed)
                    {
                        this.Enabled = false;
                    }
                }
            }

            public void SetDisposed()
            {
                lock (disposedLock)
                {
                    isDisposed = true;
                }
            }

            public bool GetDisposed()
            {
                lock (disposedLock)
                {
                    return isDisposed;
                }
            }
        }
    }
}