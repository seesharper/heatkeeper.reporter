using System;

namespace HeatKeeper.Reporter.Sdk
{
    public class Measurement
    {
        public Measurement(string sensorId, MeasurementType measurementType, RetentionPolicy retentionPolicy, double value, DateTime created)
        {
            SensorId = sensorId;
            MeasurementType = measurementType;
            RetentionPolicy = retentionPolicy;
            Value = value;
            Created = created;
        }

        public string SensorId { get; }
        public MeasurementType MeasurementType { get; }
        public RetentionPolicy RetentionPolicy { get; }
        public double Value { get; }
        public DateTime Created { get; }
    }

    public enum RetentionPolicy
    {
        None = 0,
        Hour = 1,
        Day = 2,
        Week = 3
    }
}