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
                var measurement = new Measurement(id, mapping.MeasurementType, RetentionPolicy.None, mapping.ConvertFunction(value), DateTime.UtcNow);
                result.Add(measurement);
            }

            return result.ToArray();
        }
    }


    public class HANMeasurementsFactory
    {
        public Measurement[] CreateMeasurements(JsonElement rootElement)
        {
            var timeStamp = rootElement.GetProperty("TimeStamp").GetDateTime();
            var payLoadElement = rootElement.GetProperty("Payload");
            var rootPayloadElement = payLoadElement.GetProperty("Value");
            var arrayLength = rootPayloadElement.GetArrayLength();


            if (arrayLength == 13)
            {
                var meterId = rootPayloadElement[1].GetProperty("Value").GetString();
                var activePower = rootPayloadElement[3].GetProperty("Value").GetDouble();
                var currentPhase1 = rootPayloadElement[7].GetProperty("Value").GetDouble();
                var currentPhase2 = rootPayloadElement[8].GetProperty("Value").GetDouble();
                var currentPhase3 = rootPayloadElement[9].GetProperty("Value").GetDouble();
                var voltageBetweenPhase1AndPhase2 = rootPayloadElement[10].GetProperty("Value").GetDouble();
                var voltageBetweenPhase2AndPhase3 = rootPayloadElement[12].GetProperty("Value").GetDouble();


                var activePowerMeasurement = new Measurement(meterId, MeasurementType.ActivePowerImport, RetentionPolicy.Hour, activePower, timeStamp);
                var currentPhase1Measurement = new Measurement(meterId, MeasurementType.CurrentPhase1, RetentionPolicy.Hour, currentPhase1, timeStamp);
                var currentPhase2Measurement = new Measurement(meterId, MeasurementType.CurrentPhase2, RetentionPolicy.Hour, currentPhase2, timeStamp);
                var currentPhase3Measurement = new Measurement(meterId, MeasurementType.CurrentPhase3, RetentionPolicy.Hour, currentPhase3, timeStamp);

                var voltageBetweenPhase1AndPhase2Measurement = new Measurement(meterId, MeasurementType.VoltageBetweenPhase1AndPhase2, RetentionPolicy.Hour, voltageBetweenPhase1AndPhase2, timeStamp);
                var voltageBetweenPhase2AndPhase3Measurement = new Measurement(meterId, MeasurementType.VoltageBetweenPhase2AndPhase3, RetentionPolicy.Hour, voltageBetweenPhase2AndPhase3, timeStamp);

                return new Measurement[] { activePowerMeasurement, currentPhase1Measurement, currentPhase2Measurement, currentPhase3Measurement, voltageBetweenPhase1AndPhase2Measurement, voltageBetweenPhase2AndPhase3Measurement };
            }

            return Array.Empty<Measurement>();
        }
    }
}