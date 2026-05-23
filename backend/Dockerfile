FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Api/TechTalentManagement.Api.csproj", "src/Api/"]
COPY ["src/Application/TechTalentManagement.Application.csproj", "src/Application/"]
COPY ["src/Domain/TechTalentManagement.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/TechTalentManagement.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/Api/TechTalentManagement.Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet publish "TechTalentManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TechTalentManagement.Api.dll"]
