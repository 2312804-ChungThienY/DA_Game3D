param([switch]$TraceNativeLeaks)
$c2Root=(Get-Location).Path
$c2Exe=Join-Path $c2Root 'Builds/Chapter02/Windows/SonDoong.exe'
if(-not (Test-Path -LiteralPath $c2Exe)){throw 'Build the C2 development player first'}
$c2SaveRoot=[IO.Path]::GetFullPath((Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Doan_Game3D'))
$c2Files=@('save_v1.json','save_v1.json.bak','save_v1.json.tmp')|ForEach-Object {Join-Path $c2SaveRoot $_}
$c2Recovery=Join-Path $c2Root ('Logs/C2WindowsRecovery/'+[DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss'))
New-Item -ItemType Directory -Force -Path $c2Recovery|Out-Null
$c2Original=@()
foreach($c2File in $c2Files){
 if(-not ([IO.Path]::GetFullPath($c2File).StartsWith($c2SaveRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase))){throw 'Unexpected save target'}
 $c2Exists=Test-Path -LiteralPath $c2File
 $c2Original+=@{path=$c2File;existed=$c2Exists;hash=if($c2Exists){(Get-FileHash -LiteralPath $c2File -Algorithm SHA256).Hash}else{'absent'}}
 if($c2Exists){Copy-Item -LiteralPath $c2File -Destination (Join-Path $c2Recovery ([IO.Path]::GetFileName($c2File)))}
}
$c2Original|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $c2Recovery 'manifest.json') -Encoding UTF8
$c2Report=Join-Path $c2Root 'docs/reports/C2_Art_Windows_2026-11-06.json'
$c2PlayerLog=Join-Path $c2Root 'Logs/C2WindowsPlayer.log'
if($TraceNativeLeaks){$c2Report=Join-Path $c2Root 'docs/reports/C2_Art_WindowsDiagnostics_2026-11-06.json';$c2PlayerLog=Join-Path $c2Root 'Logs/C2WindowsPlayer-Leaks.log'}
$c2RunStarted=[DateTime]::UtcNow
$c2Process=$null
try{
 $c2Args=@('-c2-delivery-smoke','-c2-report',('"'+$c2Report+'"'),'-logFile',('"'+$c2PlayerLog+'"'),'-screen-width','1920','-screen-height','1080','-screen-fullscreen','0')
 if($TraceNativeLeaks){$c2Args+=@('-native-leak-detection','EnabledWithStackTrace')}
 $c2Process=Start-Process -FilePath $c2Exe -ArgumentList $c2Args -WindowStyle Hidden -PassThru
 $c2Deadline=$c2RunStarted.AddSeconds(240)
 while(-not $c2Process.HasExited -and [DateTime]::UtcNow -lt $c2Deadline){Start-Sleep -Milliseconds 500;$c2Process.Refresh()}
 if(-not $c2Process.HasExited){$c2Process.Kill();$c2Process.WaitForExit();throw 'Player smoke timeout; launched process stopped'}
 if(-not (Test-Path -LiteralPath $c2Report) -or (Get-Item -LiteralPath $c2Report).LastWriteTimeUtc -lt $c2RunStarted){throw 'No fresh player smoke report; inspect Logs/C2WindowsPlayer.log'}
 $c2Native=Get-Content -LiteralPath $c2Report -Raw -Encoding UTF8|ConvertFrom-Json
 @{exitCode=$c2Process.ExitCode;checks=$c2Native.checks.Count;failures=$c2Native.failures;errors=$c2Native.errors;gpu=$c2Native.gpu;quality=$c2Native.quality;width=$c2Native.width;height=$c2Native.height;timings=$c2Native.staticViewTimings}|ConvertTo-Json -Depth 6
}finally{
 $c2LauncherRestore=@()
 foreach($c2OriginalFile in $c2Original){
  $c2CurrentHash=if(Test-Path -LiteralPath $c2OriginalFile.path){(Get-FileHash -LiteralPath $c2OriginalFile.path -Algorithm SHA256).Hash}else{'absent'}
  if($c2CurrentHash -ne $c2OriginalFile.hash){
   if($c2OriginalFile.existed){Copy-Item -LiteralPath (Join-Path $c2Recovery ([IO.Path]::GetFileName($c2OriginalFile.path))) -Destination $c2OriginalFile.path -Force}
   elseif(Test-Path -LiteralPath $c2OriginalFile.path){Remove-Item -LiteralPath $c2OriginalFile.path -Force}
   $c2LauncherRestore+=@($c2OriginalFile.path)
  }
 }
 @{date=[DateTime]::UtcNow;backup=$c2Recovery;restoredByLauncher=$c2LauncherRestore;personalSaves=$c2Original}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $c2Root 'Logs/C2Windows-save-restoration.json') -Encoding UTF8
}
