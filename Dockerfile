FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /source
COPY . .
RUN dotnet restore "SurveyNest.Api/SurveyNest.Api.csproj" --disable-parallel
RUN dotnet publish "SurveyNest.Api/SurveyNest.Api.csproj" -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /publish ./
EXPOSE 5000
ENTRYPOINT ["dotnet", "SurveyNest.Api.dll"]