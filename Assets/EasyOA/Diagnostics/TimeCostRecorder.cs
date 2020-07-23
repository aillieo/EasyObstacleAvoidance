using System;
using System.Diagnostics;

namespace AillieoUtils
{
    public class TimeCostRecorder
    {
        private const long kTicksPerMillisecond = 10000;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private int testTimes = 0;

        private long lastStart = 0;
        private long timeCostMax = 0;
        private long timeCostMin = long.MaxValue;

        public void Reset()
        {
            stopwatch.Reset();
            testTimes = 0;
            lastStart = 0;
            timeCostMax = 0;
            timeCostMin = long.MaxValue;
        }

        public void Start()
        {
            lastStart = Stopwatch.GetTimestamp();
            stopwatch.Start();
        }
        public void Stop()
        {
            stopwatch.Stop();
            long now = Stopwatch.GetTimestamp();
            long lastCost = (now - lastStart) / kTicksPerMillisecond;
            timeCostMax = Math.Max(lastCost, timeCostMax);
            timeCostMin = Math.Min(lastCost, timeCostMin);
            testTimes++;
        }

        public long GetTimeCostTotalMS()
        {
            if (testTimes == 0)
            {
                return 0;
            }
            return stopwatch.ElapsedMilliseconds;
        }

        public long GetTimeCostMaxMS()
        {
            if (testTimes == 0)
            {
                return 0;
            }
            return timeCostMax;
        }

        public long GetTimeCostMinMS()
        {
            if (testTimes == 0)
            {
                return 0;
            }
            return timeCostMin;
        }

        public long GetTimeCostAvgMS()
        {
            if(testTimes == 0)
            {
                return 0;
            }
            return stopwatch.ElapsedMilliseconds / testTimes;
        }

        public int GetTestTimes()
        {
            return testTimes;
        }
    }
}


