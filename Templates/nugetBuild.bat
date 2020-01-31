echo off
set version=%1
rd /s /q build
mkdir build
cd build
mkdir TypeEdgeEmulator
cd TypeEdgeEmulator
mkdir content
cd ..
mkdir TypeEdgeModuleVsCode
cd TypeEdgeModuleVsCode
mkdir content
cd ..
mkdir TypeEdgeApplication
cd TypeEdgeApplication
mkdir content
cd ..
mkdir TypeEdgeModule
cd TypeEdgeModule
mkdir content
cd ..
mkdir TypeEdgeServiceProxy
cd TypeEdgeServiceProxy
mkdir content
cd ..
cd ..
copy TypeEdgeEmulator.nuspec build\TypeEdgeEmulator
copy TypeEdgeModuleVsCode.nuspec build\TypeEdgeModuleVsCode
copy TypeEdgeApplication.nuspec build\TypeEdgeApplication
copy TypeEdgeModule.nuspec build\TypeEdgeModule
copy TypeEdgeServiceProxy.nuspec build\TypeEdgeServiceProxy

dotnet build TypeEdgeEmulator
dotnet build TypeEdgeModuleVsCode
dotnet build TypeEdgeApplication\TypeEdgeApplication.sln
dotnet build TypeEdgeModule
dotnet build TypeEdgeServiceProxy

dotnet nuget locals http-cache --clear
dotnet new --debug:reinit

dotnet new --install TypeEdgeModule
dotnet new --install TypeEdgeApplication
dotnet new --install TypeEdgeServiceProxy
dotnet new --install TypeEdgeEmulator
dotnet new --install TypeEdgeModuleVsCode

dotnet clean TypeEdgeModule
dotnet clean TypeEdgeApplication
dotnet clean TypeEdgeServiceProxy
dotnet clean TypeEdgeModuleVsCode
dotnet clean TypeEdgeEmulator

xcopy TypeEdgeModuleVsCode build\TypeEdgeModuleVsCode\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeEmulator build\TypeEdgeEmulator\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeServiceProxy build\TypeEdgeServiceProxy\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeModule build\TypeEdgeModule\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeApplication build\TypeEdgeApplication\content /s /e /EXCLUDE:list-of-excluded-files.txt

nuget.exe pack build\TypeEdgeEmulator -Version %version%
nuget.exe pack build\TypeEdgeModuleVsCode -Version %version%
nuget.exe pack build\TypeEdgeServiceProxy -Version %version%
nuget.exe pack build\TypeEdgeModule -Version %version%
nuget.exe pack build\TypeEdgeApplication -Version %version%

move /Y *.nupkg ..\..\TypeEdgeNuGets 

REM nuget.exe push ..\..\TypeEdgeNuGets\*%version%.nupkg -ApiKey VSTS

REM dotnet nuget locals http-cache --clear
REM dotnet new --debug:reinit

REM dotnet new -i TypeEdge.Application::*
REM dotnet new -i TypeEdge.ML::*
