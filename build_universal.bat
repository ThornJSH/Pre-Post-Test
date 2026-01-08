@echo off
set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

echo Stopping previous instances...
taskkill /F /IM Pre-Post-Analysis-Universal.exe 2>nul
taskkill /F /IM Pre-Post-Analysis-Universal-v2.exe 2>nul
taskkill /F /IM Pre-Post-Analysis-Universal-Final.exe 2>nul

echo Compiling C# Application (Universal Final)...
"%CSC_PATH%" /target:winexe /out:"Pre-Post-Analysis.exe" /reference:System.dll,System.Drawing.dll,System.Windows.Forms.dll PairedAnalysis_Universal.cs

if %errorlevel% neq 0 (
    echo Compilation Failed!
    exit /b %errorlevel%
)

echo Compilation Success!
