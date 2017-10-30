if not exist "%TEMP%\nuget.exe" (
    cmd /c "bitsadmin /transfer nuget_job /download /priority high https://dist.nuget.org/win-x86-commandline/latest/nuget.exe %TEMP%\nuget.exe"
)

"%TEMP%\nuget" restore LibProtection.sln

set vs2017dir=%programfiles(x86)%\Microsoft Visual Studio\2017
set msbuild="%vs2017dir%\Community\MSBuild\15.0\Bin\MSBuild.exe"
if not exist %msbuild% (
    set msbuild="%vs2017dir%\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
)
if not exist %msbuild% (
    set msbuild="%vs2017dir%\Professional\MSBuild\15.0\Bin\MSBuild.exe"
)

%msbuild% LibProtection.sln /t:Rebuild /p:Configuration=Release