Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

$readmePath = ./README.md
$publishPath = ./Publish/ToUpload

./p5rpc.flowutils/Publish.ps1 -ReadmePath $readmePath -PublishOutputDir $publishPath

Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./p5rpc.flowutils.customsavedata/Publish.ps1 -ReadmePath $readmePath -PublishOutputDir $publishPath