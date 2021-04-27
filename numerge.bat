if %1s==s ( 
 echo Keine Version angegeben
 goto ende
)

cd output

cd release
NupkgMerge -p .\ObjectMapper.NET.%1.nupkg -s .\ObjectMapper.NET_Standard20.%1.nupkg -o tmp.nupkg
NupkgMerge.exe -p .\tmp.nupkg -s .\ObjectMapper.NET_net472.%1.nupkg -o ..\ObjectMapper.NET.%1.nupkg
del tmp.nupkg
del ObjectMapper.NET.%1.nupkg
del ObjectMapper.NET_net472.%1.nupkg
del ObjectMapper.NET_Standard20.%1.nupkg
cd..

cd release.oracle
NupkgMerge -p .\ObjectMapper.NET_Oracle.%1.nupkg -s .\ObjectMapper.NET_Oracle_Standard20.%1.nupkg -o tmp.nupkg
NupkgMerge.exe -p .\tmp.nupkg -s .\ObjectMapper.NET_Oracle_net472.%1.nupkg -o ..\ObjectMapper.NET_Oracle.%1.nupkg
del tmp.nupkg
del ObjectMapper.NET_Oralce.%1.nupkg
del ObjectMapper.NET_Oralce_net472.%1.nupkg
del ObjectMapper.NET_Oralce_Standard20.%1.nupkg
cd..

cd release.postgres
NupkgMerge -p .\ObjectMapper.NET_Postgres.%1.nupkg -s .\ObjectMapper.NET_Postgres_Standard20.%1.nupkg -o tmp.nupkg
NupkgMerge.exe -p .\tmp.nupkg -s .\ObjectMapper.NET_Postgres_net472.%1.nupkg -o ..\ObjectMapper.NET_Postgres.%1.nupkg
del tmp.nupkg
del ObjectMapper.NET_Oralce.%1.nupkg
del ObjectMapper.NET_Oralce_net472.%1.nupkg
del ObjectMapper.NET_Oralce_Standard20.%1.nupkg
cd..

:ende