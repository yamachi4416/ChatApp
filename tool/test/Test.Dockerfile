# vim: set ft=dockerfile :

FROM ubuntu

RUN apt-get update \
 && apt-get install -y language-pack-ja apt-transport-https curl sudo

RUN update-locale LC_ALL=ja_JP.UTF-8

RUN ( curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg ) \
 && mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg \
 && echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" \
 > /etc/apt/sources.list.d/dotnetdev.list

RUN apt-get update && apt-get install -y postgresql nodejs npm dotnet-sdk-2.0.2
RUN ln -s /usr/bin/nodejs /usr/local/bin/node

RUN service postgresql start \
 && sudo -u postgres psql -c 'CREATE USER "ChatApp" WITH PASSWORD '\''Password'\'' CREATEDB' \
 && sudo -u postgres psql -c 'GRANT CONNECT ON DATABASE postgres TO "ChatApp"'

WORKDIR /app

ADD . /app

RUN dotnet restore

ENTRYPOINT \
  set -eu ; \
  service postgresql start 1>/dev/null ; \
  dotnet build test/ChatApp.Test ; \
  dotnet test --no-restore --no-build -v m test/ChatApp.Test
