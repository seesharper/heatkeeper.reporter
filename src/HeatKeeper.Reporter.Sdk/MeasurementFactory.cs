using System;
using System.Collections.Generic;
using System.Linq;
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
                var measurement = new Measurement($"{sensor.Prefix}{id}", mapping.MeasurementType, RetentionPolicy.Day, mapping.ConvertFunction(value), DateTime.UtcNow);
                result.Add(measurement);
            }

            return result.ToArray();
        }
    }


    public class KaifaMeasurementsFactory
    {

        public Measurement[] CreateMeasurements(JsonElement element)
        {
            var measurements = new List<Measurement>();
            var numberOfFrames = element.GetArrayLength();

            for (int i = 0; i < numberOfFrames; i++)
            {
                measurements.AddRange(CreateMeasurementsFromFrame(element[i]));
            }


            return measurements.ToArray();
        }



        private Measurement[] CreateMeasurementsFromFrame(JsonElement rootElement)
        {
            var timeStamp = rootElement.GetProperty("Timestamp").GetDateTime().ToUniversalTime();
            var payLoadElement = rootElement.GetProperty("Payload");
            var rootPayloadElement = payLoadElement.GetProperty("Value");
            var arrayLength = rootPayloadElement.GetArrayLength();

            // List 2
            if (arrayLength == 13)
            {
                return ParseList2(timeStamp, rootPayloadElement);
            }

            // List 3
            if (arrayLength == 18)
            {
                var list2Measurements = ParseList2(timeStamp, rootPayloadElement);
                var list3Measurements = ParseList3(timeStamp, rootPayloadElement);
                return list2Measurements.Concat(list3Measurements).ToArray();
            }



            return Array.Empty<Measurement>();
        }

        private static Measurement[] ParseList2(DateTime timeStamp, JsonElement rootPayloadElement)
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

        private static Measurement[] ParseList3(DateTime timeStamp, JsonElement rootPayloadElement)
        {
            var meterId = rootPayloadElement[1].GetProperty("Value").GetString();
            var cumulativePowerImport = rootPayloadElement[14].GetProperty("Value").GetDouble();
            return new Measurement[] { new Measurement(meterId, MeasurementType.CumulativePowerImport, RetentionPolicy.None, cumulativePowerImport, timeStamp) };
        }
    }
}