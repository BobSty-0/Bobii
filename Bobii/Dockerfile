#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Bobii/Bobii.csproj", "Bobii/"]
RUN dotnet restore "Bobii/Bobii.csproj"
COPY . .
WORKDIR "/src/Bobii"
RUN dotnet publish "Bobii.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bobii.dll"]