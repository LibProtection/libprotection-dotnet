@echo off
"%ProgramFiles(x86)%\IIS Express\iisexpress" /path:%~dp0 /systray:false /clr:v4.0 /port:8080 /trace:error