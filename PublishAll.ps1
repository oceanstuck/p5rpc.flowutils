Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./Publish.ps1 -ProjectPath "p5rpc.flowutils/p5rpc.flowutils.csproj" `
              -PackageName "p5rpc.flowutils" `
              -PublishOutputDir "Publish/ToUpload/Main" `
			  -ReadmePath "README.md" `
			  -ChangelogPath "p5rpc.flowutils/CHANGELOG.MD" `

./Publish.ps1 -ProjectPath "p5rpc.flowutils.customsavedata/p5rpc.flowutils.customsavedata.csproj" `
              -PackageName "p5rpc.flowutils.customsavedata" `
              -PublishOutputDir "Publish/ToUpload/CSD" `
			  -ReadmePath "README.md" `
			  -ChangelogPath "p5rpc.flowutils.customsavedata/CHANGELOG.MD" `

Pop-Location