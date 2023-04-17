## Usage

Update Raspberry PI OS

```
sudo apt-get update && sudo apt-get upgrade
```

Install Docker

```
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
```

Add the Pi user to the docker group

´´´
sudo usermod -aG docker Pi

```

Reboot

```

sudo reboot -f

```

verify docker
```

docker run hello-world

```



https://phoenixnap.com/kb/docker-on-raspberry-pi

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

If the Raspberry won't boot for any reason, make sure to check out this link

https://raspberrypi.stackexchange.com/questions/40854/kernel-panic-not-syncing-vfs-unable-to-mount-root-fs-on-unknown-block179-6
```

I did this from ubuntu. Please unmount before running command

```
sudo fsck /dev/sdc1
sudo fsck /dev/sdc2
```

```
new ReporterHost()
	.WithHeatKeeperUrl("http://sdfsdfds", "dsfsdfkljsdfkjsdfskdj")
	.AddReporter(new RTL433Reporter().AddSensor)

```

## Configure Shelly PlusHT 

Environment variables

```shell
export SHELLY=192.168.10.162
export MQTT_PASSWORD="overintermoduluasjonsforvregning"
export MQTT_PORT=1883
export MQTT_SERVER=139.162.230.128
export MQTT_USER="heatkeeper"
```







