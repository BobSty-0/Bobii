FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Bobii.csproj", "./"]
RUN dotnet restore "Bobii.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Bobii.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bobii.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bobii.dll"]
