using System;
using Janitor;

namespace HeatKeeper.Reporter.Sdk;


public class TimeSpanSchedule : ISchedule
{
    private readonly TimeSpan waitTime;

    public TimeSpanSchedule(TimeSpan waitTime)
    {
        this.waitTime = waitTime;
    }

    public DateTime? GetNext(DateTime utcNow)
    {
        return utcNow.Add(waitTime);
    }
}