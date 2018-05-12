FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

COPY *.sln                      ./
COPY src/ChatApp/*.csproj       ./src/ChatApp/
COPY test/ChatApp.Test/*.csproj ./test/ChatApp.Test/
RUN dotnet restore

COPY src/ChatApp/.       ./src/ChatApp/
COPY test/ChatApp.Test/. ./test/ChatApp.Test/
RUN dotnet build

FROM node:8 AS static
WORKDIR /src
COPY src/ChatApp/package.json ./
RUN npm install

FROM build AS sdk
WORKDIR /app
ENTRYPOINT ["/bin/bash", "-c"]

FROM static AS wwwroot
COPY src/ChatApp/ ./
RUN npm run make

FROM build AS publish
WORKDIR /app/src/ChatApp
COPY --from=wwwroot /src/wwwroot/js  ./wwwroot/js
COPY --from=wwwroot /src/wwwroot/css ./wwwroot/css
COPY --from=wwwroot /src/wwwroot/lib ./wwwroot/lib
RUN rm -rf ./wwwroot/src
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY --from=publish /app/src/ChatApp/out ./
ENTRYPOINT ["dotnet", "ChatApp.dll"]
