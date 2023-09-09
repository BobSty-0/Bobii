FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /Bobii/src
COPY ["/Bobii/Bobii.csproj", "./"]
RUN dotnet restore "Bobii.csproj"
COPY . .
WORKDIR "/Bobii/src/."
RUN dotnet build "Bobii.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bobii.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bobii.dll"]
