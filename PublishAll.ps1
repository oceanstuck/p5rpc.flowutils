Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

.\p5rpc.flowutils\Publish.ps1 -ProjectPath .\p5rpc.flowutils\p5rpc.flowutils.csproj -ReadmePath README.md -ChangelogPath p5rpc.flowutils\CHANGELOG.MD
.\p5rpc.flowutils.customsavedata\Publish.ps1 -ProjectPath .\p5rpc.flowutils.customsavedata\p5rpc.flowutils.customsavedata.csproj -ReadmePath README.md -ChangelogPath p5rpc.flowutils\CHANGELOG.MD