FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS build
WORKDIR /src
COPY backend/Directory.Build.props backend/Directory.Build.props
COPY backend/src/SalesDashboard.DataAccess/ backend/src/SalesDashboard.DataAccess/
COPY backend/src/SalesDashboard.Application/ backend/src/SalesDashboard.Application/
COPY backend/src/SalesDashboard.Api/ backend/src/SalesDashboard.Api/
RUN dotnet restore backend/src/SalesDashboard.Api --locked-mode
RUN dotnet publish backend/src/SalesDashboard.Api -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SalesDashboard.Api.dll"]

