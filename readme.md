## Usage

We need to map the RealTek USB dongle device to the container.

```
lsusb
```

Look for your device

```
Bus 001 Device 005: RealTek something
```

This device needs to be mounted in the container using `--device`

```
docker run --device /dev/bus/usb/001/005 -v /home/pi/reporter/reporter.csx:/reporter.csx --restart always heatkeeper.reporter:latest /reporter.csx
```

We also need the `reporter.csx` file that is used to set up our reporter.

It typically looks like something like this

```
string apiKey = "Your reporter API key";


new Reporter()
        .WithHeatKeeperEndpoint("http://yourheatkeeperserver", apiKey)
        .WithPublishInterval(new TimeSpan(0, 0, 30))
        .AddSensor(Sensors.Acurite606TX)
        .AddSensor(Sensors.AcuriteTower)
        .Start();
```
