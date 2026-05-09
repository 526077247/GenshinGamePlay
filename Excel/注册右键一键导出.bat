@echo off
setlocal enabledelayedexpansion

:: 获取批处理所在目录的父文件夹名
for %%I in ("%~dp0..") do set "parent=%%~nxI"
if "%parent%"=="" set "parent=Excalibur"  :: fallback

set "menu_prefix=【%parent%】"

:: 删除文件夹背景右键菜单项
reg delete "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%导所有" /f 2>nul
reg delete "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%导配置" /f 2>nul
reg delete "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%导多语言" /f 2>nul
reg delete "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%导属性" /f 2>nul
reg delete "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%校验" /f 2>nul

:: 删除文件右键菜单项
reg delete "HKEY_CLASSES_ROOT\*\Shell\%menu_prefix%导该表" /f 2>nul

:: 添加新的菜单项
reg add "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%导所有\command" /d "%~dp0\win_startExportAll.bat" /f
reg add "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%导配置\command" /d "%~dp0\prot2cs.bat" /f
reg add "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%导多语言\command" /d "%~dp0\win_startI18NExport.bat" /f
reg add "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%导属性\command" /d "%~dp0\win_startAttrExport.bat" /f
reg add "HKEY_CLASSES_ROOT\Directory\Background\shell\%menu_prefix%校验\command" /d "%~dp0\策划校验表工具.bat" /f
reg add "HKEY_CLASSES_ROOT\*\Shell\%menu_prefix%导该表\command" /d "%~dp0\win_startExportTarget.bat %%1" /f

pause