FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/HelpingHands.Api/HelpingHands.Api.csproj src/HelpingHands.Api/
RUN dotnet restore src/HelpingHands.Api/HelpingHands.Api.csproj
COPY src/HelpingHands.Api/ src/HelpingHands.Api/
RUN dotnet publish src/HelpingHands.Api/HelpingHands.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "HelpingHands.Api.dll"]
