# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY . ./
RUN dotnet restore RESTAPI_DynamoDB
RUN dotnet publish RESTAPI_DynamoDB -c Release -o out

# Serve Stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

ARG envVar=value1

ENTRYPOINT ["dotnet", "SongRepository.dll", "--environment=Development"]