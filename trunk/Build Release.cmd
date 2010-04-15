@echo off
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /t:BuildRelease /nologo /p:Version=1.4 /fl1 /flp1:v=diag
pause