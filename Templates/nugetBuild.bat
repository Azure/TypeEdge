rd /s /q build
mkdir build
cd build
mkdir TypeEdgeEmulator
cd TypeEdgeEmulator
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
copy TypeEdgeModule.nuspec build\TypeEdgeModule
copy TypeEdgeProxy.nuspec build\TypeEdgeProxy

dotnet build TypeEdgeEmulator
dotnet build TypeEdgeModule
dotnet build TypeEdgeProxy

dotnet new --install TypeEdgeModule
dotnet new --install TypeEdgeProxy
dotnet new --install TypeEdgeEmulator

xcopy TypeEdgeEmulator build\TypeEdgeEmulator\content /s /e
xcopy TypeEdgeProxy build\TypeEdgeProxy\content /s /e
xcopy TypeEdgeModule build\TypeEdgeModule\content /s /e

nuget.exe pack build\TypeEdgeEmulator
nuget.exe pack build\TypeEdgeProxy
nuget.exe pack build\TypeEdgeModule

move /Y *.nupkg ..\Example\build 