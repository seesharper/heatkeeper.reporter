using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MQTTnet;
using ResourceReader;
using Xunit;
namespace HeatKeeper.Reporter.Sdk.Tests
{
    public class MapperTests
    {
        [Fact]
        public void ShouldMapAcurite606TX()
        {
            var sensorData = new ResourceBuilder().AddAssembly(typeof(MapperTests).Assembly).Build<ISensorData>();
            var document = JsonDocument.Parse(sensorData.Acurite606TX);
            var measurements = new MeasurementFactory().CreateMeasurements(document.RootElement, Sensors.Acurite606TX("A"));
            measurements.Should().Contain(m => m.Value == 22.2);
        }

        [Fact]
        public void ShouldMapAcuriteTower()
        {
            var sensorData = new ResourceBuilder().AddAssembly(typeof(MapperTests).Assembly).Build<ISensorData>();
            var document = JsonDocument.Parse(sensorData.AcuriteTower);
            var measurements = new MeasurementFactory().CreateMeasurements(document.RootElement, Sensors.AcuriteTower("AT"));
            measurements.Should().Contain(m => m.Value == 4.8 && m.MeasurementType == MeasurementType.Temperature);
            measurements.Should().Contain(m => m.Value == 75.0 && m.MeasurementType == MeasurementType.Humidity);
        }

        [Fact]
        public void ShouldMapFineoffsetWH2()
        {
            var sensorData = new ResourceBuilder().AddAssembly(typeof(MapperTests).Assembly).Build<ISensorData>();
            var document = JsonDocument.Parse(sensorData.FineoffsetWH2);
            var measurements = new MeasurementFactory().CreateMeasurements(document.RootElement, Sensors.FineOffsetWH2("FO"));
            measurements.Should().Contain(m => m.Value == 20.1 && m.MeasurementType == MeasurementType.Temperature);
            measurements.Should().Contain(m => m.Value == 40.0 && m.MeasurementType == MeasurementType.Humidity);
        }

        [Fact]
        public void ShouldMap_Kaifka_MA304H3E_List2()
        {
            var sensorData = new ResourceBuilder().AddAssembly(typeof(MapperTests).Assembly).Build<ISensorData>();
            var document = JsonDocument.Parse(sensorData.Kaifka_MA304H3E_List2);
            var measurements = new KaifaMeasurementsFactory().CreateMeasurements(document.RootElement);
            measurements.Length.Should().Be(6);
        }

        [Fact]
        public void ShouldMap_Kaifka_MA304H3E_List3()
        {
            var sensorData = new ResourceBuilder().AddAssembly(typeof(MapperTests).Assembly).Build<ISensorData>();
            var document = JsonDocument.Parse(sensorData.Kaifka_MA304H3E_List3);
            var measurements = new KaifaMeasurementsFactory().CreateMeasurements(document.RootElement);
            measurements.Length.Should().Be(7);
        }

        [Fact]
        public async Task ShouldMapShellyPlusNotifyFullStatus()
        {
            var sensorData = new ResourceBuilder().AddAssembly(typeof(MapperTests).Assembly).Build<ISensorData>();
            var document = JsonDocument.Parse(sensorData.ShellyPlusHT_FullStatus);
            MqttApplicationMessage message = new()
            {
                Payload = Encoding.UTF8.GetBytes(sensorData.ShellyPlusHT_FullStatus),
                Topic = "shelly/plus-ht/home/kitchen/events/rpc"
            };

            var measurements = await MqttSensors.ShellyPlusHT().HandleMessage(message);
            measurements.Should().Contain(m => m.Value == 24.4 && m.MeasurementType == MeasurementType.Temperature && m.SensorId == "shelly/plus-ht/home/kitchen");
            measurements.Should().Contain(m => m.Value == 31.3 && m.MeasurementType == MeasurementType.Humidity && m.SensorId == "shelly/plus-ht/home/kitchen");
            measurements.Should().Contain(m => m.Value == 100 && m.MeasurementType == MeasurementType.BatteryLevel && m.SensorId == "shelly/plus-ht/home/kitchen");
        }
    }
}
