FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ./MetricsDemo.csproj .
RUN ls
RUN dotnet restore "MetricsDemo.csproj"
COPY . .
RUN dotnet build "MetricsDemo.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MetricsDemo.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MetricsDemo.dll"]

# docker build -t metricsdemo3 .
# docker tag metricsdemo3 localhost:5000/sansys/metricsdemo3:5
# docker push localhost:5000/sansys/metricsdemo3:5