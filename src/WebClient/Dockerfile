FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS build
ARG BUILD_CONFIGURATION=Release
ARG READY_TO_RUN=false
ARG TARGETARCH
WORKDIR /src
# Razor files participate in the SDK's implicit static-asset package references.
COPY . .
RUN dotnet restore "WebClient/WebClient.csproj" --locked-mode -a "$TARGETARCH" -p:PublishReadyToRun="$READY_TO_RUN"
RUN dotnet publish "WebClient/WebClient.csproj" --no-restore -c "$BUILD_CONFIGURATION" \
    -a "$TARGETARCH" --no-self-contained -p:PublishReadyToRun="$READY_TO_RUN" -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12 AS final
WORKDIR /app
RUN mkdir -p /app/App_Data && chown app:app /app/App_Data
USER app
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebClient.dll"]
