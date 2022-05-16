ARG registry=mcr.microsoft.com
ARG nuget=https://api.nuget.org/v3/index.json

FROM ${registry}/dotnet/sdk:6.0 AS build 

WORKDIR /app

COPY *.sln .
COPY Core/*.csproj ./Core/

RUN dotnet restore --runtime linux-musl-x64

COPY . .

#RUN dotnet build
#RUN dotnet tool install --global dotnet-ef && /root/.dotnet/tools/dotnet-ef migrations list --project ./Core && /root/.dotnet/tools/dotnet-ef --project ./Core database update
RUN dotnet publish Core --self-contained --runtime  linux-musl-x64 -c Release -o out

FROM ${registry}/dotnet/runtime-deps:6.0-alpine3.15


RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["./Core"]
