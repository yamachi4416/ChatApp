@ECHO OFF

WHERE ruby /Q

IF %ERRORLEVEL%==1 (
  ECHO RUBY�����s�ł��܂���
  EXIT 1
)

ruby %~dp0heroku_setup.rb %*
