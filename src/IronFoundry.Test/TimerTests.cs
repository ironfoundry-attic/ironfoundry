namespace IronFoundry.Test
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using IronFoundry.Misc.Utilities;
    using Xunit;

    public class TimerTests
    {
        [Fact(Skip="MANUAL")]
        public void Test_One_Shot_Timer()
        {
            var timer = new ActionTimer(null, TimeSpan.FromMilliseconds(500), () => Debug.WriteLine("PING"));
        }

        [Fact(Skip="MANUAL")]
        public void Test_Recurring_Timer()
        {
            var timer = new ActionTimer(null, TimeSpan.FromSeconds(2), () => Debug.WriteLine("PING"), false, true);
            Thread.Sleep(TimeSpan.FromSeconds(30));
            timer.Dispose();
        }
    }
}
