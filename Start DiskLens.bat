@echo off
if exist "%~dp0release\v1.0.18\self-contained\DiskLens.exe" (
 start "DiskLens WPF" "%~dp0release\v1.0.18\self-contained\DiskLens.exe"
) else if exist "%~dp0release\self-contained\DiskLens.exe" (
 start "DiskLens WPF" "%~dp0release\self-contained\DiskLens.exe"
) else (
 start "DiskLens WPF" "%~dp0release\framework-dependent\DiskLens.exe"
)





