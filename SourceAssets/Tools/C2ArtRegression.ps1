param([ValidateSet('Traversal','Bridge','Arena','Ending')][string]$Mode='Traversal')
$regressionDate=if($Mode -eq 'Traversal'){'2026-11-05'}elseif($Mode -eq 'Bridge'){'2026-11-05'}else{'2026-11-06'}
$regressionPrefix='C2'+$Mode
$regressionReport='docs/reports/C2_'+$Mode+'_PlayMode_'+$regressionDate+'.json'
$regressionSaved='Logs/C2ArtPrior/'+$Mode
New-Item -ItemType Directory -Force -Path $regressionSaved | Out-Null
$regressionOriginals=@($regressionReport)+@(Get-ChildItem -LiteralPath 'docs/images' -Filter ('c2-'+$Mode.ToLower()+'-*-'+$regressionDate+'.png') | ForEach-Object {$_.FullName})
$regressionManifest=@()
foreach($regressionFile in $regressionOriginals){
 if(Test-Path -LiteralPath $regressionFile){$regressionCopy=Join-Path $regressionSaved ([IO.Path]::GetFileName($regressionFile));Copy-Item -LiteralPath $regressionFile -Destination $regressionCopy -Force;$regressionManifest+=@{original=$regressionFile;copy=$regressionCopy}}
}
$regressionHelper=Join-Path $regressionSaved 'Focus.ps1'
$regressionText=[IO.File]::ReadAllText((Join-Path (Get-Location) 'SourceAssets/Tools/C2ArtFocusGameView.ps1'))
$regressionText=$regressionText.Replace('C2Art',$regressionPrefix).Replace('C2_Art','C2_'+$Mode).Replace('2026-11-06',$regressionDate)
[IO.File]::WriteAllText((Join-Path (Get-Location) $regressionHelper),$regressionText,[Text.UTF8Encoding]::new($true))
$regressionStarted=[DateTime]::UtcNow
try{
 & $regressionHelper -Action test
 if(-not (Test-Path -LiteralPath $regressionReport) -or (Get-Item -LiteralPath $regressionReport).LastWriteTimeUtc -le $regressionStarted){throw 'No fresh regression report'}
 $regressionResult=Get-Content -Raw -Encoding UTF8 -LiteralPath $regressionReport | ConvertFrom-Json
 if($regressionResult.PSObject.Properties.Name -contains 'screenshots'){
  $regressionNewImages=@()
  foreach($regressionImage in $regressionResult.screenshots){
   $regressionNewImage=$regressionImage.Replace('c2-'+$Mode.ToLower()+'-','c2-art-regression-'+$Mode.ToLower()+'-').Replace($regressionDate,'2026-11-06')
   Copy-Item -LiteralPath $regressionImage -Destination $regressionNewImage -Force;$regressionNewImages+=@($regressionNewImage)
  }
  $regressionResult.screenshots=$regressionNewImages
 }
 $regressionResult | Add-Member -NotePropertyName regressionRun -NotePropertyValue '2026-11-06; existing production probe rerun against C2 environment art; original historical artifacts restored' -Force
 $regressionResult | ConvertTo-Json -Depth 12 | Set-Content -Encoding UTF8 -LiteralPath ('docs/reports/C2_Art_'+$Mode+'_2026-11-06.json')
 @{mode=$Mode;checks=$regressionResult.checks.Count;failures=$regressionResult.failures;errors=$regressionResult.errors}|ConvertTo-Json -Depth 5
}finally{
 foreach($regressionFile in $regressionManifest){Copy-Item -LiteralPath $regressionFile.copy -Destination $regressionFile.original -Force}
}
