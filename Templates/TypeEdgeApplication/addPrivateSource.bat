@echo off
IF "%1"=="" GOTO HAVE_USERNAME
IF "%2"=="" GOTO HAVE_PASSWORD


if exist nuget.exe (
	echo nuget.exe found
) else (
	echo downloading nuget.exe from https://www.nuget.org/nuget.exe 

	Powershell.exe wget -outf nuget.exe https://nuget.org/nuget.exe
	if not exist .\nuget.exe (
		echo Error: nuget does not exist. 
		exit /b 1
	)	
)


echo Adding the private nuget packages feed "typeedge-feed"
nuget.exe sources Add -Name "typeedge-feed" -Source "https://msblox-03.pkgs.visualstudio.com/_packaging/typeedge-feed/nuget/v3/index.json" -StorePasswordInClearText -ConfigFile NuGet.Config -UserName "%1" -Password "%2"

if errorlevel 1  (
	echo Failed, trying to remove an existing record first ..
	echo Removing the private "typeedge-feed" ..
	nuget.exe sources Remove -Name "typeedge-feed" 
	echo Adding the private nuget packages feed "typeedge-feed"
	nuget.exe sources Add -Name "typeedge-feed" -Source "https://msblox-03.pkgs.visualstudio.com/_packaging/typeedge-feed/nuget/v3/index.json" -StorePasswordInClearText -ConfigFile NuGet.Config -UserName "%1" -Password "%2"
	if errorlevel 0 (
		echo Success!			
	)	
)

if exist nuget.exe (
	del nuget.exe /q
)

exit /b 0


:HAVE_USERNAME
echo Your Git credentials username is required
exit /b 1

:HAVE_PASSWORD
echo Your Git credentials password is required
exit /b 1

:exit
echo exiting..