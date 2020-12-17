#! /usr/bin/env dotnet-script

#r "nuget:HeatKeeper.Reporter.Sdk, 0.1.0"


using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using HeatKeeper.Reporter.Sdk;

string apiKey = "";


new Reporter()
        .WithHeatKeeperEndpoint("", apiKey)
        .WithPublishInterval(new TimeSpan(0, 0, 5))
        .AddSensor(Sensors.Acurite606TX)
        .AddSensor(Sensors.AcuriteTower)
        .AddSensor(Sensors.FineOffsetWH2)
        .Start();
