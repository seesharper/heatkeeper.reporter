using System;
using System.Text.Json;
using FluentAssertions;
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
            var measurements = new MeasurementFactory().CreateMeasurements(document.RootElement, Sensors.Acurite606TX);
            measurements.Should().Contain(m => m.Value == 22.2);
        }

        [Fact]
        public void ShouldMapAcuriteTower()
        {
            var sensorData = new ResourceBuilder().AddAssembly(typeof(MapperTests).Assembly).Build<ISensorData>();
            var document = JsonDocument.Parse(sensorData.AcuriteTower);
            var measurements = new MeasurementFactory().CreateMeasurements(document.RootElement, Sensors.AcuriteTower);
            measurements.Should().Contain(m => m.Value == 4.8 && m.MeasurementType == MeasurementType.Temperature);
            measurements.Should().Contain(m => m.Value == 75.0 && m.MeasurementType == MeasurementType.Humidity);
        }

        [Fact]
        public void ShouldMapFineoffsetWH2()
        {
            var sensorData = new ResourceBuilder().AddAssembly(typeof(MapperTests).Assembly).Build<ISensorData>();
            var document = JsonDocument.Parse(sensorData.FineoffsetWH2);
            var measurements = new MeasurementFactory().CreateMeasurements(document.RootElement, Sensors.FineOffsetWH2);
            measurements.Should().Contain(m => m.Value == 20.1 && m.MeasurementType == MeasurementType.Temperature);
            measurements.Should().Contain(m => m.Value == 40.0 && m.MeasurementType == MeasurementType.Humidity);
        }
    }
}