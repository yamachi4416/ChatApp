@ECHO OFF

WHERE ruby /Q

IF %ERRORLEVEL%==1 (
  ECHO RUBYが実行できません
  EXIT 1
)

ruby %~dp0heroku_setup.rb %*
