param([string]$Configuration='Release',[string]$ReleaseDirectory='release')
$ErrorActionPreference='Stop'
$project=Join-Path $PSScriptRoot 'DiskLens.Wpf.csproj'
foreach($flavor in @('framework-dependent','self-contained')) {
 $out=Join-Path $PSScriptRoot "$ReleaseDirectory\$flavor"
 $contained=if($flavor -eq 'self-contained'){'true'}else{'false'}
 dotnet publish $project -c $Configuration -r win-x64 --self-contained $contained -p:DebugType=None -p:DebugSymbols=false -o $out
 if($LASTEXITCODE -ne 0){throw 'Publish failed'}
 Compress-Archive -Path (Join-Path $out '*') -DestinationPath (Join-Path $PSScriptRoot "$ReleaseDirectory\DiskLens-Wpf-win-x64-$flavor.zip") -Force
}
$sizes=foreach($flavor in @('framework-dependent','self-contained')){
 $folder=Join-Path $PSScriptRoot "$ReleaseDirectory\$flavor"
 $files=@(Get-ChildItem -LiteralPath $folder -Recurse -File)
 $zip=Get-Item -LiteralPath (Join-Path $PSScriptRoot "$ReleaseDirectory\DiskLens-Wpf-win-x64-$flavor.zip")
 [pscustomobject]@{Flavor=$flavor;FolderBytes=($files | Measure-Object Length -Sum).Sum;ZipBytes=$zip.Length;Files=$files.Count}
}
$sizes | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot "$ReleaseDirectory\package-sizes.json")
$sizes | Format-Table -AutoSize

