@echo off

if "%~1"=="" (
 echo Keine Version angegeben
 goto ende
)

set VERSION=%~1

cd output

cd release
if not exist ".\ObjectMapper.NET.%VERSION%.nupkg" (
 cd ..\..
 dotnet pack ".NET Standard 2.x\Source\ObjectMapper .NET Standard.csproj" -c Release
 cd output
 cd release
 if not exist ".\ObjectMapper.NET.%VERSION%.nupkg" (
  echo Fehlendes Paket: output\release\ObjectMapper.NET.%VERSION%.nupkg
  goto ende
 )
)
if not exist ".\ObjectMapper.NET_net472.%VERSION%.nupkg" (
 cd ..\..
 dotnet pack ".NET Framework\Source\ObjectMapper .NET Framework 4.x.csproj" -c Release
 cd output
 cd release
 if not exist ".\ObjectMapper.NET_net472.%VERSION%.nupkg" (
  echo Fehlendes Paket: output\release\ObjectMapper.NET_net472.%VERSION%.nupkg
  goto ende
 )
)
NupkgMerge.exe -p .\ObjectMapper.NET.%VERSION%.nupkg -s .\ObjectMapper.NET_net472.%VERSION%.nupkg -o ..\ObjectMapper.NET.%VERSION%.nupkg
del ObjectMapper.NET.%VERSION%.nupkg
del ObjectMapper.NET_net472.%VERSION%.nupkg
cd..

cd release.oracle
if not exist ".\ObjectMapper.NET_Oracle.%VERSION%.nupkg" (
 cd ..\..
 dotnet pack ".NET Standard 2.x\Source.Oracle\ObjectMapper .NET Oracle Standard.csproj" -c Release
 cd output
 cd release.oracle
 if not exist ".\ObjectMapper.NET_Oracle.%VERSION%.nupkg" (
  echo Fehlendes Paket: output\release.oracle\ObjectMapper.NET_Oracle.%VERSION%.nupkg
  goto ende
 )
)
if not exist ".\ObjectMapper.NET_Oracle_net472.%VERSION%.nupkg" (
 cd ..\..
 dotnet pack ".NET Framework\Source.Oracle\ObjectMapper .NET Oracle Framework 4.x.csproj" -c Release
 cd output
 cd release.oracle
 if not exist ".\ObjectMapper.NET_Oracle_net472.%VERSION%.nupkg" (
  echo Fehlendes Paket: output\release.oracle\ObjectMapper.NET_Oracle_net472.%VERSION%.nupkg
  goto ende
 )
)
NupkgMerge.exe -p .\ObjectMapper.NET_Oracle.%VERSION%.nupkg -s .\ObjectMapper.NET_Oracle_net472.%VERSION%.nupkg -o ..\ObjectMapper.NET_Oracle.%VERSION%.nupkg
del ObjectMapper.NET_Oracle.%VERSION%.nupkg
del ObjectMapper.NET_Oracle_net472.%VERSION%.nupkg
cd..

cd release.postgres
if not exist ".\ObjectMapper.NET_Postgres.%VERSION%.nupkg" (
 cd ..\..
 dotnet pack ".NET Standard 2.x\Source.Postgres\ObjectMapper .NET Postgres Standard.csproj" -c Release
 cd output
 cd release.postgres
 if not exist ".\ObjectMapper.NET_Postgres.%VERSION%.nupkg" (
  echo Fehlendes Paket: output\release.postgres\ObjectMapper.NET_Postgres.%VERSION%.nupkg
  goto ende
 )
)
if not exist ".\ObjectMapper.NET_Postgres_net472.%VERSION%.nupkg" (
 cd ..\..
 dotnet pack ".NET Framework\Source.Postgres\ObjectMapper .NET Postgres Framework 4.x.csproj" -c Release
 cd output
 cd release.postgres
 if not exist ".\ObjectMapper.NET_Postgres_net472.%VERSION%.nupkg" (
  echo Fehlendes Paket: output\release.postgres\ObjectMapper.NET_Postgres_net472.%VERSION%.nupkg
  goto ende
 )
)
NupkgMerge.exe -p .\ObjectMapper.NET_Postgres.%VERSION%.nupkg -s .\ObjectMapper.NET_Postgres_net472.%VERSION%.nupkg -o ..\ObjectMapper.NET_Postgres.%VERSION%.nupkg
del ObjectMapper.NET_Postgres.%VERSION%.nupkg
del ObjectMapper.NET_Postgres_net472.%VERSION%.nupkg
cd..

:ende
