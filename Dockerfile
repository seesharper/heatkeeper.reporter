FROM mcr.microsoft.com/dotnet/sdk:8.0

ENV TZ Europe/Oslo

WORKDIR /tmp/
RUN dotnet tool install dotnet-script --tool-path /usr/bin
# Execute a dummy script at build time to warm the nuget cache
RUN echo "Console.WriteLine(42);" > dummy.csx
RUN dotnet script dummy.csx
RUN dotnet script --info
RUN apt-get update
RUN apt-get install -y git libtool libusb-1.0-0-dev librtlsdr-dev rtl-sdr build-essential autoconf cmake pkg-config
RUN git clone https://github.com/merbanan/rtl_433.git

# Copy the build file
COPY build_rtl433.sh .
RUN chmod +x build_rtl433.sh
RUN ./build_rtl433.sh

WORKDIR /

WORKDIR /
RUN curl -L https://github.com/seesharper/HANReader/releases/latest/download/hanreader-linux-arm --output /usr/local/bin/hanreader-linux-arm
RUN chmod +x /usr/local/bin/hanreader-linux-arm
RUN ls -al
ENTRYPOINT [ "dotnet", "script" ]