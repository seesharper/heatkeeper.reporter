#! /usr/bin/env dotnet-script

#r "nuget:HeatKeeper.Reporter.Sdk, 0.1.1"

using HeatKeeper.Reporter.Sdk;

string apiKey = "";
string heatKeeperUrl = "";
string hanReaderSerialPort = "";

await new ReporterHost()
        .WithHeatKeeperEndpoint(heatKeeperUrl, apiKey)
        .AddReporter(new RTL433Reporter().AddSensor(Sensors.Acurite606TX).AddSensor(Sensors.AcuriteTower).AddSensor(Sensors.FineOffsetWH2).WithPublishIntervall(new TimeSpan(0, 10, 0)))
        .AddReporter(new HANReporter().WithSerialPort(hanReaderSerialPort).WithPublishIntervall(new TimeSpan(0, 0, 10)))
        .Start();
