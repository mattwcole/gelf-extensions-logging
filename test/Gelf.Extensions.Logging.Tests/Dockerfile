FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /src
COPY ["test/Gelf.Extensions.Logging.Tests/Gelf.Extensions.Logging.Tests.csproj", "test/Gelf.Extensions.Logging.Tests/"]
COPY ["src/Gelf.Extensions.Logging/Gelf.Extensions.Logging.csproj", "src/Gelf.Extensions.Logging/"]
RUN dotnet restore "test/Gelf.Extensions.Logging.Tests/Gelf.Extensions.Logging.Tests.csproj"
COPY . .
WORKDIR "/src/test/Gelf.Extensions.Logging.Tests"
RUN dotnet build -c Release
ENTRYPOINT ["dotnet", "test", "-c", "Release"]
