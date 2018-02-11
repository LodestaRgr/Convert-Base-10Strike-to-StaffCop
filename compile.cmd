@echo off
SET FWPATH=%windir%\Microsoft.Net\Framework\v3.5
%FWPATH%\csc.exe /out:Convert_Base_10Strike_to_StaffCop.exe /t:exe /recurse:"Convert Base 10Strike to StaffCop\*.cs"