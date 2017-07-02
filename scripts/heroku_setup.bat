@ECHO OFF

WHERE ruby /Q

IF %ERRORLEVEL%==1 (
  ECHO RUBYВ™ОјНsВ≈ВЂВ№ВєВс
  EXIT 1
)

ruby %~dp0heroku_setup.rb %*
