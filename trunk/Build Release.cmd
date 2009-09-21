@echo off
%SystemRoot%\Microsoft.NET\Framework\v3.5\msbuild.exe /t:BuildRelease /nologo /p:Version=1.4 /fl1 /flp1:v=diag
pause