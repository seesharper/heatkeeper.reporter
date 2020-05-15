using System;
using System.Collections.Generic;
using System.Text.Json;

namespace HeatKeeper.Reporter.Sdk
{
    public class MeasurementFactory
    {
        public Measurement[] CreateMeasurements(JsonElement payload, Sensor sensor)
        {
            List<Measurement> result = new List<Measurement>();

            foreach (var mapping in sensor.Mappings)
            {
                var id = payload.GetProperty("id").GetRawText();
                var protocol = payload.GetProperty("protocol").GetInt32();
                double value = payload.GetProperty(mapping.PropertyName).GetDouble();
                var measurement = new Measurement(id, mapping.MeasurementType, mapping.ConvertFunction(value), DateTime.UtcNow);
                result.Add(measurement);
            }

            return result.ToArray();
        }
    }
}