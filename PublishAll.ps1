Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./p5rpc.flowutils/Publish.ps1 -ReadmePath ./README.md -PublishOutputDir ./Publish/ToUpload/
Pop-Location
./p5rpc.flowutils.customsavedata/Publish.ps1 -ReadmePath ./README.md -PublishOutputDir ./Publish/ToUpload/