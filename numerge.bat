if %1s==s ( 
 echo Keine Version angegeben
 goto ende
)

NupkgMerge -p .\ObjectMapper.NET_net472.%1.nupkg -s .\ObjectMapper.NET_Standard20.%1.nupkg -o tmp.nupkg
NupkgMerge.exe -p .\tmp.nupkg -s .\ObjectMapper.NET_Standard21.%1.nupkg -o ObjectMapper.NET.%1.nupkg
del tmp.nupkg
del ObjectMapper.NET_net472.%1.nupkg
del ObjectMapper.NET_Standard20.%1.nupkg
del ObjectMapper.NET_Standard21.%1.nupkg
:ende