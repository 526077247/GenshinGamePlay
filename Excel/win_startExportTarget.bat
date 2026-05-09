@echo off
cd /d "%~dp0..\Bin"
dotnet ExcelExport.dll --AppType=ExporterTarget --Param="%~1"
pause