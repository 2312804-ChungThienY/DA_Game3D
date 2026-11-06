# Local test helper: activate only this project's already-running Unity window.
# Keeps actual OS focus during the opt-in keyboard test; does not change game guards.
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class C2FocusedWindow {
 [StructLayout(LayoutKind.Sequential)] public struct Rect {public int left,top,right,bottom;}
 [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
 [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId();
 [DllImport("user32.dll")] static extern bool AttachThreadInput(uint a,uint b,bool on);
 [DllImport("user32.dll")] static extern bool BringWindowToTop(IntPtr h);
 [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] static extern IntPtr SetFocus(IntPtr h);
 [DllImport("user32.dll")] static extern bool ShowWindowAsync(IntPtr h,int cmd);
 [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h,out Rect rect);
 [DllImport("user32.dll")] static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] static extern void mouse_event(uint flags,uint x,uint y,uint data,UIntPtr extra);
 public static bool IsForeground(uint pid){uint actual;GetWindowThreadProcessId(GetForegroundWindow(),out actual);return actual==pid;}
 public static bool Activate(IntPtr h,uint pid){uint unused,current=GetCurrentThreadId(),fg=GetWindowThreadProcessId(GetForegroundWindow(),out unused),target=GetWindowThreadProcessId(h,out unused);
  bool a=current!=fg&&AttachThreadInput(current,fg,true),b=current!=target&&AttachThreadInput(current,target,true);
  try{BringWindowToTop(h);SetForegroundWindow(h);return IsForeground(pid);}
  finally{if(b)AttachThreadInput(current,target,false);if(a)AttachThreadInput(current,fg,false);}
 }
 public static bool ClickGame(uint pid){if(!IsForeground(pid))return false;Rect r;var h=GetForegroundWindow();if(!GetWindowRect(h,out r)||r.right-r.left<500||r.bottom-r.top<400)return false;
  SetCursorPos(r.left+(r.right-r.left)*50/100,r.top+(r.bottom-r.top)*55/100);mouse_event(2,0,0,0,UIntPtr.Zero);mouse_event(4,0,0,0,UIntPtr.Zero);return true;
 }
}
'@
$testRunStarted=[DateTime]::UtcNow
$unityEditorForTest=Get-Process -Id 17968 -ErrorAction Stop
$unityEditorForTest.Refresh()
if($unityEditorForTest.MainWindowHandle -eq 0){throw 'Unity main window unavailable'}
Set-Content -LiteralPath 'Library/SonDoongC2Traversal.request.json' -Value '{"action":"test"}' -Encoding UTF8
$deadlineForTest=$testRunStarted.AddSeconds(170)
$clickedGameForTest=$false
$unityWindowActivatorForTest=New-Object -ComObject WScript.Shell
while([DateTime]::UtcNow -lt $deadlineForTest){
 if(Test-Path -LiteralPath 'docs/reports/C2_Traversal_PlayMode_2026-11-05.json'){
  if((Get-Item -LiteralPath 'docs/reports/C2_Traversal_PlayMode_2026-11-05.json').LastWriteTimeUtc -gt $testRunStarted){'Traversal report finished';break}
 }
 if(Test-Path -LiteralPath 'Logs/C2Traversal-progress.json'){
  if((Get-Item -LiteralPath 'Logs/C2Traversal-progress.json').LastWriteTimeUtc -gt $testRunStarted){
   if(-not [C2FocusedWindow]::IsForeground([uint32]$unityEditorForTest.Id)){$null=$unityWindowActivatorForTest.AppActivate($unityEditorForTest.Id);if(-not [C2FocusedWindow]::IsForeground([uint32]$unityEditorForTest.Id)){$null=[C2FocusedWindow]::Activate($unityEditorForTest.MainWindowHandle,[uint32]$unityEditorForTest.Id)}}
   if(-not $clickedGameForTest){$clickedGameForTest=[C2FocusedWindow]::ClickGame([uint32]$unityEditorForTest.Id);if($clickedGameForTest){'Game View focus click: True'}}
  }
 }
 Start-Sleep -Milliseconds 500
}
