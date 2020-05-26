namespace HeatKeeper.Reporter.Sdk
{
    public class Sensor
    {
        public Sensor(string model, int protocolId, Mapping[] mappings)
        {
            Model = model;
            ProtocolId = protocolId;
            Mappings = mappings;
        }

        public string Model { get; }

        public int ProtocolId { get; }
        public Mapping[] Mappings { get; }
    }

    public static class Sensors
    {
        public static Sensor Acurite606TX => new Sensor("Acurite-606TX", 55, new Mapping[] { new Mapping("temperature_C", MeasurementType.Temperature, d => d), new Mapping("battery_ok", MeasurementType.BatteryLevel, (v) => v * 100) });
        public static Sensor AcuriteTower => new Sensor("Acurite-Tower", 40, new Mapping[] { new Mapping("temperature_C", MeasurementType.Temperature, d => d), new Mapping("humidity", MeasurementType.Humidity, d => d), new Mapping("battery_ok", MeasurementType.BatteryLevel, (v) => v * 100) });
        public static Sensor FineOffsetWH2 => new Sensor("Fineoffset-WH2", 18, new Mapping[] { new Mapping("temperature_C", MeasurementType.Temperature, d => d), new Mapping("humidity", MeasurementType.Humidity, d => d) });
    }
}