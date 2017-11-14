"%~dp1..\tools\nuget\nuget.exe" restore LibProtection.sln

set vs2017dir=%programfiles(x86)%\Microsoft Visual Studio\2017
set msbuild="%vs2017dir%\Community\MSBuild\15.0\Bin\MSBuild.exe"
if not exist %msbuild% (
    set msbuild="%vs2017dir%\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
)
if not exist %msbuild% (
    set msbuild="%vs2017dir%\Professional\MSBuild\15.0\Bin\MSBuild.exe"
)

%msbuild% LibProtection.sln /t:Rebuild /p:Configuration=Release