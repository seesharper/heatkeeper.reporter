#! /usr/bin/env dotnet-script
#r "/Users/bernhardrichter/GitHub/heatkeeper.reporter/src/HeatKeeper.Reporter.Sdk/bin/Debug/net7.0/HeatKeeper.Reporter.Sdk.dll"
// #r "nuget:HeatKeeper.Reporter.Sdk, 0.2.0"

using HeatKeeper.Reporter.Sdk;

string apiKey = "";
string heatKeeperUrl = "";
string hanReaderSerialPort = "";

await new ReporterHost()
        .WithHeatKeeperEndpoint(heatKeeperUrl, apiKey)
        .AddReporter(new RTL433Reporter().AddSensor(Sensors.Acurite606TX("A")).AddSensor(Sensors.AcuriteTower).AddSensor(Sensors.FineOffsetWH2).WithPublishInterval(new TimeSpan(0, 10, 0)))
        .AddReporter(new HANReporter().WithSerialPort(hanReaderSerialPort).WithPublishInterval(new TimeSpan(0, 0, 10)))
        .Start();
