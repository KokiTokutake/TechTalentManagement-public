FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Api/TeckTalentManagement.Api.csproj", "src/Api/"]
COPY ["src/Application/TeckTalentManagement.Application.csproj", "src/Application/"]
COPY ["src/Domain/TeckTalentManagement.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/TeckTalentManagement.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/Api/TeckTalentManagement.Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet publish "TeckTalentManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TeckTalentManagement.Api.dll"]
