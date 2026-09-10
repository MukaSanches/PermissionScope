@echo off
"%~dp0..\.toolchain\gh\bin\gh.exe" auth login --hostname github.com --git-protocol https --web
if errorlevel 1 (
  echo GitHub authorization was not completed.
) else (
  echo GitHub authorization completed. You can return to the conversation.
)
pause
