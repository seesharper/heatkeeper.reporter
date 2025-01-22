using System;

namespace HeatKeeper.Reporter.Sdk
{
    public class Sensor
    {
        public Sensor(string model, int protocolId, Mapping[] mappings, string idPrefix)
        {
            Model = model;
            ProtocolId = protocolId;
            Mappings = mappings;
            Prefix = idPrefix;
        }

        public string Model { get; }

        public int ProtocolId { get; }
        public Mapping[] Mappings { get; }
        public string Prefix { get; }
    }

    public static class Sensors
    {
        public static Sensor Acurite606TX(string prefix) => new Sensor("Acurite-606TX", 55, new Mapping[] { new Mapping("temperature_C", MeasurementType.Temperature, d => d), new Mapping("battery_ok", MeasurementType.BatteryLevel, (v) => v * 100) }, prefix);
        public static Sensor AcuriteTower(string prefix) => new Sensor("Acurite-Tower", 40, new Mapping[] { new Mapping("temperature_C", MeasurementType.Temperature, d => d), new Mapping("humidity", MeasurementType.Humidity, d => d), new Mapping("battery_ok", MeasurementType.BatteryLevel, (v) => v * 100) }, prefix);
        public static Sensor FineOffsetWH2(string prefix) => new Sensor("Fineoffset-WH2", 18, new Mapping[] { new Mapping("temperature_C", MeasurementType.Temperature, d => d), new Mapping("humidity", MeasurementType.Humidity, d => d) }, prefix);
        public static Sensor AmbientWeatherF007TH(string prefix) => new Sensor("Ambientweather-F007TH", 20, new Mapping[] { new Mapping("temperature_F", MeasurementType.Temperature, d => Math.Round((d - 32) / 1.8, 1)), new Mapping("humidity", MeasurementType.Humidity, d => d), new Mapping("battery_ok", MeasurementType.BatteryLevel, (v) => v * 100) }, prefix);
    }
}