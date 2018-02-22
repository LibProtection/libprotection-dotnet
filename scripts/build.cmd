dotnet restore ..\sources\LibProtection.Injections
dotnet build -c Release ..\sources\LibProtection.Injections

dotnet restore ..\sources\LibProtection.Injections.Tests
dotnet build -c Release ..\sources\LibProtection.Injections.Tests

..\tools\nuget\nuget.exe restore ..\sources\LibProtection.sln

set vs2017dir=%programfiles(x86)%\Microsoft Visual Studio\2017
set msbuild="%vs2017dir%\Community\MSBuild\15.0\Bin\MSBuild.exe"
if not exist %msbuild% (
   set msbuild="%vs2017dir%\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
)
if not exist %msbuild% (
     set msbuild="%vs2017dir%\Professional\MSBuild\15.0\Bin\MSBuild.exe"
)

%msbuild% ..\sources\LibProtection.TestSite\LibProtection.TestSite.csproj /t:Build /p:Configuration=Release /p:Platform=AnyCPU
