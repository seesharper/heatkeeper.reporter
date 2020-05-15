namespace HeatKeeper.Reporter.Sdk
{
    public class Sensor
    {
        public Sensor(string name, int protocolId, Mapping[] mappings)
        {
            Name = name;
            ProtocolId = protocolId;
            Mappings = mappings;
        }

        public string Name { get; }

        public int ProtocolId { get; }
        public Mapping[] Mappings { get; }
    }
}