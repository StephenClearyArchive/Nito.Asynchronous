@echo off
%SystemRoot%\Microsoft.NET\Framework\v3.5\msbuild.exe /t:BuildRelease /nologo /p:Version=2.0 /fl1 /flp1:v=diag
pause