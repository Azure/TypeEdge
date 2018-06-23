echo off
set version=%1
rd /s /q build
mkdir build
cd build
mkdir TypeEdgeEmulator
cd TypeEdgeEmulator
mkdir content
cd ..
mkdir TypeEdgeApplication
cd TypeEdgeApplication
mkdir content
cd ..
mkdir TypeEdgeML
cd TypeEdgeML
mkdir content
cd ..
mkdir TypeEdgeModule
cd TypeEdgeModule
mkdir content
cd ..
mkdir TypeEdgeProxy
cd TypeEdgeProxy
mkdir content
cd ..
cd ..
copy TypeEdgeEmulator.nuspec build\TypeEdgeEmulator
copy TypeEdgeApplication.nuspec build\TypeEdgeApplication
copy TypeEdgeML.nuspec build\TypeEdgeML
copy TypeEdgeModule.nuspec build\TypeEdgeModule
copy TypeEdgeProxy.nuspec build\TypeEdgeProxy

dotnet build TypeEdgeEmulator
dotnet build TypeEdgeApplication\TypeEdgeApplication.sln
dotnet build TypeEdgeML\TypeEdgeML.sln
dotnet build TypeEdgeModule
dotnet build TypeEdgeProxy

dotnet nuget locals http-cache --clear
dotnet new --debug:reinit

dotnet new --install TypeEdgeModule
dotnet new --install TypeEdgeApplication
dotnet new --install TypeEdgeML
dotnet new --install TypeEdgeProxy
dotnet new --install TypeEdgeEmulator

dotnet clean TypeEdgeModule
dotnet clean TypeEdgeApplication
dotnet clean TypeEdgeML
dotnet clean TypeEdgeProxy
dotnet clean TypeEdgeEmulator

xcopy TypeEdgeEmulator build\TypeEdgeEmulator\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeProxy build\TypeEdgeProxy\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeModule build\TypeEdgeModule\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeApplication build\TypeEdgeApplication\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeML build\TypeEdgeML\content /s /e /EXCLUDE:list-of-excluded-files.txt

nuget.exe pack build\TypeEdgeEmulator -Version %version%
nuget.exe pack build\TypeEdgeProxy -Version %version%
nuget.exe pack build\TypeEdgeModule -Version %version%
nuget.exe pack build\TypeEdgeApplication -Version %version%
nuget.exe pack build\TypeEdgeML -Version %version%

move /Y *.nupkg ..\..\TypeEdgeNuGets 

nuget.exe push ..\..\TypeEdgeNuGets\*%version%.nupkg -ApiKey VSTS

dotnet nuget locals http-cache --clear
dotnet new --debug:reinit

dotnet new -i TypeEdge.Application::*
dotnet new -i TypeEdge.ML::*
