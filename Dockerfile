FROM mcr.microsoft.com/dotnet/core/sdk:3.1

WORKDIR /tmp/
RUN dotnet tool install dotnet-script --tool-path /usr/bin
# Execute a dummy script at build time to warm the nuget cache
RUN echo "Console.WriteLine(42);" > dummy.csx
RUN dotnet script dummy.csx

RUN apt-get update
RUN apt-get install -y git libtool libusb-1.0-0-dev librtlsdr-dev rtl-sdr build-essential autoconf cmake pkg-config
RUN git clone https://github.com/merbanan/rtl_433.git

# Copy the build file
COPY build_rtl433.sh .
RUN chmod +x build_rtl433.sh
RUN ./build_rtl433.sh

ENTRYPOINT [ "dotnet", "script" ]