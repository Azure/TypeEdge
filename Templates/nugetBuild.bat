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
copy TypeEdgeModule.nuspec build\TypeEdgeModule
copy TypeEdgeProxy.nuspec build\TypeEdgeProxy

dotnet build TypeEdgeEmulator
dotnet build TypeEdgeApplication
dotnet build TypeEdgeModule
dotnet build TypeEdgeProxy

dotnet new --install TypeEdgeModule
dotnet new --install TypeEdgeApplication
dotnet new --install TypeEdgeProxy
dotnet new --install TypeEdgeEmulator


dotnet clean TypeEdgeModule
dotnet clean TypeEdgeApplication
dotnet clean TypeEdgeProxy
dotnet clean TypeEdgeEmulator

xcopy TypeEdgeEmulator build\TypeEdgeEmulator\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeProxy build\TypeEdgeProxy\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeModule build\TypeEdgeModule\content /s /e /EXCLUDE:list-of-excluded-files.txt
xcopy TypeEdgeApplication build\TypeEdgeApplication\content /s /e /EXCLUDE:list-of-excluded-files.txt

nuget.exe pack build\TypeEdgeEmulator
nuget.exe pack build\TypeEdgeProxy
nuget.exe pack build\TypeEdgeModule
nuget.exe pack build\TypeEdgeApplication

move /Y *.nupkg ..\Example\build 