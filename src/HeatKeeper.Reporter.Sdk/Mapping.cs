using System;

namespace HeatKeeper.Reporter.Sdk
{
    public class Mapping
    {
        public Mapping(string propertyName, MeasurementType measurementType, Func<double, double> convertFunction)
        {
            PropertyName = propertyName;
            MeasurementType = measurementType;
            convertFunction ??= d => d;
            ConvertFunction = convertFunction;
        }

        public string PropertyName { get; }

        public MeasurementType MeasurementType { get; }

        public Func<double, double> ConvertFunction { get; }
    }
}
