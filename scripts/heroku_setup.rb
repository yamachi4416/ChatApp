#! /usr/bin/env ruby

require 'open3'

def info(*args)
  puts '-' * 50
  puts args.first
  puts '-' * 50
  puts args[1..-1]
  puts '-' * 50 if args.size > 1
end

def shellcmd(*args)
  Open3.capture3(*args).first.chomp
end

def heroku(*args)
  shellcmd 'heroku', *args
end

def config_get(var)
  heroku 'config:get', var
end

def config_set(var, val)
  heroku 'config:set', "#{var}=#{val}"
end

def connect_string
  database_regexp = /\A\w+:\/\/(?<User Id>[^:]+):(?<Password>[^@]+)@(?<Host>[^:]+):(?<Port>\d+)\/(?<Database>.+)\Z/
  database_url = heroku 'config:get', 'DATABASE_URL'
  matches = database_url.match database_regexp
  matches.names.map {|k| "#{k}=#{matches[k]}" }.select{ |v| !v.empty? }.join ';'
end

def postgres_add
  info = heroku 'addons:info', 'heroku-postgresql'

  if /postgresql/.match(info.split(/\r\n|\r|\n/).first)
    info 'アドオンにPostgresを追加済み'
    puts info
  else
    info 'アドオンにPostgresを追加'
    puts heroku 'addons:create', 'heroku-postgresql:hobby-dev'
  end
end

def database_configure
  info 'データベース接続の環境変数を設定'
  puts config_set 'CHATAPP_DATAPROVIDER', 'Postgres'
  puts config_set 'CHATAPP_CONNECTSTRING', "#{connect_string};Pooling=true;"
end

def stack_configure
  puts heroku 'stack:set', 'heroku-16'
end

def buildpack_configure
  info 'ビルドパックをクリア'
  puts heroku 'buildpacks:clear'
  info 'ビルドパックを設定'
  puts heroku 'buildpacks:set', 'https://github.com/jincod/dotnetcore-buildpack#v1.0.4'
  puts heroku 'buildpacks:add', '--index', '1', 'heroku/nodejs'
end

stack_configure
postgres_add
database_configure
buildpack_configure
