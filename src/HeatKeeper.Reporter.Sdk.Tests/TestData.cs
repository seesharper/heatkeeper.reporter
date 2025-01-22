namespace HeatKeeper.Reporter.Sdk.Tests
{
    public interface ISensorData
    {
        string Acurite606TX { get; }

        string AcuriteTower { get; }

        string FineoffsetWH2 { get; }

        string Kaifka_MA304H3E_List1 { get; }

        string Kaifka_MA304H3E_List2 { get; }

        string Kaifka_MA304H3E_List3 { get; }

        string ShellyPlusHT_Temperature { get; }

        string ShellyPlusHT_Humidity { get; }

        string ShellyPlusHT_DevicePower { get; }

        string ShellyPlusHT_FullStatus { get; }

        string Tasmota_DS18B20 { get; }

        string AmbientWeather_F007TH { get; }
    }
}