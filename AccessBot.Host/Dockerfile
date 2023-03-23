FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AccessBot/AccessBot.csproj", "AccessBot/"]
RUN dotnet restore "AccessBot/AccessBot.csproj"
COPY . .
WORKDIR "/src/AccessBot"
RUN dotnet build "AccessBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccessBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccessBot.dll"]
