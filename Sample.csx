#! /usr/bin/env dotnet-script
#r "/Users/bernhardrichter/GitHub/heatkeeper.reporter/src/HeatKeeper.Reporter.Sdk/bin/Debug/net7.0/HeatKeeper.Reporter.Sdk.dll"
#r "nuget: MQTTnet.Extensions.ManagedClient, 4.1.4.563"

using HeatKeeper.Reporter.Sdk;

string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6ImFkbWluQG5vLm9yZyIsImdpdmVuX25hbWUiOiJGaXJzdCBuYW1lIiwiZmFtaWx5X25hbWUiOiJMYXN0IE5hbWUiLCJyb2xlIjpbInJlcG9ydGVyIiwicmVwb3J0ZXIiXSwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvc2lkIjoiMSIsIm5iZiI6MTYwODcyMTQ5MywiZXhwIjoxOTI0MjU0MjkzLCJpYXQiOjE2MDg3MjE0OTN9.Put7cyrXeE0zE6HxQLJx3ekU8bprsTH8IGjm7RRTTys";
string heatKeeperUrl = "http://139.162.230.128:5000/";
await new ReporterHost()
        .WithHeatKeeperEndpoint(heatKeeperUrl, apiKey)
        .AddReporter(new MqttReporter().AddSensor(MqttSensors.ShellyPlusHT()).WithMqttBrokerOptions("139.162.230.128", "heatkeeper", "overintermoduluasjonsforvregning").WithPublishInterval(new TimeSpan(0, 10, 0)))
        .Start();