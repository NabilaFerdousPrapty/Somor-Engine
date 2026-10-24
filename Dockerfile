FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/WarSim.Web -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# The app listens on the PORT environment variable (Render sets its own; this is the default for local Docker).
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WarSim.Web.dll"]