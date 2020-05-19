using System;
using System.Collections.Generic;
using System.Text.Json;

namespace HeatKeeper.Reporter.Sdk
{
    public class MeasurementFactory
    {
        public Measurement[] CreateMeasurements(JsonElement payload, Sensor sensor)
        {
            var result = new List<Measurement>();

            var id = payload.GetProperty("id").GetRawText();

            foreach (var mapping in sensor.Mappings)
            {
                double value = payload.GetProperty(mapping.PropertyName).GetDouble();
                var measurement = new Measurement(id, mapping.MeasurementType, mapping.ConvertFunction(value), DateTime.UtcNow);
                result.Add(measurement);
            }

            return result.ToArray();
        }
    }
}