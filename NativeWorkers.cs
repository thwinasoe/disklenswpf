using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
namespace DiskLens;
public record Volume(string Root,string Label,string Serial,string Type,long Used,long Total,bool Ready);
public static class NativeWorkers {
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool GetVolumeInformation(string root,StringBuilder name,int size,out uint serial,out uint max,out uint flags,StringBuilder fs,int fsSize);
 public static int VolumeWorker(){var root=JsonSerializer.Deserialize<string>(Console.ReadLine()!)!;try{var d=new DriveInfo(System.IO.Path.GetPathRoot(root)!);var name=new StringBuilder(260);var fs=new StringBuilder(260);var ok=GetVolumeInformation(d.Name,name,260,out var serial,out _,out _,fs,260);Console.WriteLine(JsonSerializer.Serialize(new Volume(d.Name,name.ToString(),ok?serial.ToString("X8"):"",d.DriveType.ToString(),d.TotalSize-d.AvailableFreeSpace,d.TotalSize,d.IsReady)));}catch{Console.WriteLine("null");}return 0;}
 public static Process Start(string mode)=>Process.Start(new ProcessStartInfo(Environment.ProcessPath!,mode){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true})!;
 public static async Task<Volume?> Probe(string root,CancellationToken ct){using var p=Start("--volume-worker");try{await p.StandardInput.WriteLineAsync(JsonSerializer.Serialize(root));var line=await p.StandardOutput.ReadLineAsync(ct).AsTask().WaitAsync(TimeSpan.FromSeconds(5),ct);return line==null?null:JsonSerializer.Deserialize<Volume>(line);}catch(TimeoutException){return null;}finally{try{if(!p.HasExited)p.Kill(true);}catch{}}}
 public static async Task<List<Entry>> Health(CancellationToken ct){const string script="""
$ErrorActionPreference='SilentlyContinue'
$rows=@()
Get-PhysicalDisk | ForEach-Object {
 $disk=$_
 $reliability=$disk | Get-StorageReliabilityCounter
 $rows += [pscustomobject]@{Path=$disk.FriendlyName;Size=[long]$disk.Size;Modified=0;Kind='health';Detail=('Physical drive: '+$disk.HealthStatus+'; '+($disk.OperationalStatus -join ', ')+'; temperature='+$reliability.Temperature+'; wear='+$reliability.Wear+'; uncorrected read errors='+$reliability.ReadErrorsUncorrected+'; uncorrected write errors='+$reliability.WriteErrorsUncorrected+'; power-on hours='+$reliability.PowerOnHours)}
}
Get-WinEvent -FilterHashtable @{LogName='System';ProviderName=@('disk','Ntfs','stornvme','storahci');StartTime=(Get-Date).AddDays(-14)} -MaxEvents 50 | ForEach-Object { $rows += [pscustomobject]@{Path=$_.ProviderName;Size=0;Modified=0;Kind='health';Detail=($_.TimeCreated.ToString('s')+'; Event '+$_.Id+'; '+$_.Message)} }
ConvertTo-Json -InputObject @($rows) -Compress
""";
 using var p=Process.Start(new ProcessStartInfo("powershell.exe"){ArgumentList={"-NoProfile","-NonInteractive","-Command",script},UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true})!;try{var text=await p.StandardOutput.ReadToEndAsync(ct).WaitAsync(TimeSpan.FromSeconds(12),ct);await p.WaitForExitAsync(ct).WaitAsync(TimeSpan.FromSeconds(2),ct);var rows=JsonSerializer.Deserialize<List<Entry>>(text)??[];if(rows.Count==0)rows.Add(new Entry("Windows storage signals",0,0,"health","No readable physical-drive status or recent matching events. This is not a health guarantee."));return rows;}catch(Exception e)when(e is TimeoutException or JsonException){return [new Entry("Windows storage signals",0,0,"health","Unavailable or timed out: "+e.Message)];}finally{try{if(!p.HasExited)p.Kill(true);}catch{}}}
}


