using System;

namespace HeatKeeper.Reporter.Sdk
{
    public class Measurement
    {
        public Measurement(string sensorId, MeasurementType measurementType, double value, DateTime dateTime)
        {
            SensorId = sensorId;
            MeasurementType = measurementType;
            Value = value;
            Created = dateTime;
        }

        public string SensorId { get; }
        public MeasurementType MeasurementType { get; }
        public double Value { get; }
        public DateTime Created { get; }
    }
}