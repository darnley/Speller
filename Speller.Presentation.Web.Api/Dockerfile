FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Speller.Presentation.Web.Api/Speller.Presentation.Web.Api.csproj", "Speller.Presentation.Web.Api/"]
COPY ["Speller.SpellingBox/Speller.SpellingBox.csproj", "Speller.SpellingBox/"]
RUN dotnet restore "Speller.Presentation.Web.Api/Speller.Presentation.Web.Api.csproj"
COPY . .
WORKDIR "/src/Speller.Presentation.Web.Api"
RUN dotnet build "Speller.Presentation.Web.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Speller.Presentation.Web.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Speller.Presentation.Web.Api.dll"]