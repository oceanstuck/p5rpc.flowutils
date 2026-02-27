# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/p5rpc.flowutils/*" -Force -Recurse
dotnet publish "./p5rpc.flowutils.csproj" -c Release -o "$env:RELOADEDIIMODS/p5rpc.flowutils" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location