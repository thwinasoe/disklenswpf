using System.IO;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace DiskLens;
public record Entry(string Path,long Size,long Modified,string Kind,string Detail="",long Delta=0,string Change=""){public string Name=>System.IO.Path.GetFileName(Path.TrimEnd('\\'));public string SizeText=>Format(Size);public string DifferenceText=>Change.Length==0?"":(Delta>0?"+":Delta<0?"−":"")+Format(Math.Abs(Delta));public string IconName=>Kind=="folder"?"Folder":Rules.Category(Path) switch {"Video"=>"Film","Images"=>"FileImage","Archives"=>"FileArchive","Code"=>"FileCode2","Documents"=>"FileText","Applications"=>"Box",_=>"File"};public static string Format(long n)=>n<1024?$"{n:N0} B":n<1048576?$"{n/1024d:N1} KB":n<1073741824?$"{n/1048576d:N1} MB":$"{n/1073741824d:N2} GB";}
public record ScanInfo(string Id,string Root,string Date,long Bytes,long Files,bool Safe,string VolumeSerial="",string VolumeLabel="",long Issues=0);
public static class Rules {
 public static bool Developer(string path)=>Regex.IsMatch(path,@"node_modules|cache|temp|\.bin",RegexOptions.IgnoreCase);
 public static string Category(string path){var ext=System.IO.Path.GetExtension(path).ToLowerInvariant();return ext switch {".mp4" or ".mov" or ".mkv" or ".avi" or ".webm"=>"Video", ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".tiff" or ".raw"=>"Images", ".zip" or ".7z" or ".rar" or ".iso" or ".gz" or ".tar" or ".dmg" or ".pkg"=>"Archives", ".js" or ".jsx" or ".ts" or ".tsx" or ".cs" or ".java" or ".py" or ".cache"=>"Code", ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt" or ".pst"=>"Documents", ".exe" or ".dll" or ".msi" or ".msix" or ".appx" or ".appxbundle" or ".vhdx" or ".db" or ".bin"=>"Applications",_=>"Other"};}

 public static bool Match(string path,string patterns)=>Matcher(patterns)(path);
 public static Func<string,bool> Matcher(string patterns){var matchers=patterns.Split(';',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries).Select(p=>{if(!p.Contains('*')&&!p.Contains('?'))return (Func<string,bool>)(path=>path.Contains(p,StringComparison.OrdinalIgnoreCase));var regex=new Regex("^"+Regex.Escape(p).Replace("\\*",".*").Replace("\\?",".")+"$",RegexOptions.IgnoreCase|RegexOptions.NonBacktracking);return path=>regex.IsMatch(path)||regex.IsMatch(System.IO.Path.GetFileName(path));}).ToArray();return matchers.Length==0?(_=>false):(path=>{foreach(var match in matchers)if(match(path))return true;return false;});}
 public static bool Installer(string p)=>new[]{".msi",".msix",".appx",".appxbundle",".dmg",".pkg"}.Contains(System.IO.Path.GetExtension(p).ToLowerInvariant())||(p.EndsWith(".exe",StringComparison.OrdinalIgnoreCase)&&Regex.IsMatch(System.IO.Path.GetFileName(p).Replace('_',' ').Replace('-',' ').Replace('.',' '),@"\b(setup|install|installer|bootstrapper|runtime|redist|redistributable|sdk|driver|update|launcher)\b",RegexOptions.IgnoreCase));
 public static string Explain(string p,long modified){var r=new List<string>();if(Installer(p))r.Add("Installer package or setup-style executable");if(new[]{".zip",".7z",".rar",".iso",".gz",".tar"}.Contains(System.IO.Path.GetExtension(p).ToLowerInvariant()))r.Add("Archive: verify extracted content first");if(p.Contains("\\Downloads\\",StringComparison.OrdinalIgnoreCase))r.Add("Downloads location");if(Regex.IsMatch(p,@"\\(node_modules|bin|obj|\.cache|cache|temp)\\",RegexOptions.IgnoreCase))r.Add("Dependency, build output, cache or temporary path");if(modified>0&&DateTime.UtcNow.Ticks-modified>TimeSpan.FromDays(365).Ticks)r.Add("Not modified for over one year");return string.Join("; ",r)+ (r.Count>0?". Review before recycling; no rule proves a file is unused.":"");}
}
public sealed class Scanner(int timeoutMilliseconds=10000,Func<Process>? workerFactory=null) {
 public async Task<ScanInfo> Run(string root,string dbPath,bool safe,string exclusions,IProgress<string> progress,CancellationToken ct){
  var volume=await NativeWorkers.Probe(root,ct);safe|=volume?.Type=="Removable";
  using var db=new Db(dbPath);using var cancellation=ct.Register(db.Interrupt);
  db.Exec("CREATE TABLE files(path TEXT PRIMARY KEY COLLATE NOCASE,parent TEXT COLLATE NOCASE,size INTEGER,modified INTEGER,extension TEXT); CREATE TABLE folders(path TEXT PRIMARY KEY COLLATE NOCASE,parent TEXT COLLATE NOCASE,size INTEGER DEFAULT 0,done INTEGER DEFAULT 0); CREATE INDEX pending_folders ON folders(done); CREATE TABLE issues(path TEXT,detail TEXT); CREATE TABLE meta(root TEXT,date TEXT,safe INTEGER); BEGIN;");
  using var insertFile=db.Prepare("INSERT OR IGNORE INTO files VALUES(?1,?2,?3,?4,?5)");
  using var insertFolder=db.Prepare("INSERT OR IGNORE INTO folders(path,parent) VALUES(?1,?2)");
  using var insertIssue=db.Prepare("INSERT INTO issues VALUES(?1,?2)");
  using var finishFolder=db.Prepare("UPDATE folders SET done=1,size=?1 WHERE path=?2");
  using var aggregateFolder=db.Prepare("UPDATE folders SET size=size+(SELECT size FROM folders WHERE path=?1) WHERE path=?2");
  insertFolder.Text(1,root);insertFolder.Text(2,"");insertFolder.Execute();
  var excluded=Rules.Matcher(exclusions);var ownHome=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DiskLens.Wpf");
  Process? worker=null;long files=0,bytes=0,writes=0;var clock=Stopwatch.StartNew();var last=TimeSpan.Zero;
  Process Start()=>workerFactory!=null?workerFactory():NativeWorkers.Start("--worker");
  void Kill(){if(worker==null)return;try{if(!worker.HasExited)worker.Kill(true);}catch{}worker.Dispose();worker=null;}
  void Issue(string path,string detail){insertIssue.Text(1,path);insertIssue.Text(2,detail);insertIssue.Execute();}
  void Report(string path){if(clock.Elapsed-last<TimeSpan.FromMilliseconds(180))return;last=clock.Elapsed;var estimate=volume!=null&&root.Equals(volume.Root,StringComparison.OrdinalIgnoreCase)&&volume.Used>0?$" • estimated {Math.Min(99,100d*bytes/volume.Used):N1}% • remaining {Entry.Format(Math.Max(0,volume.Used-bytes))} • ETA {(bytes>0?TimeSpan.FromSeconds(Math.Min(864000,Math.Max(0,(volume.Used-bytes)/Math.Max(1,bytes/clock.Elapsed.TotalSeconds)))).ToString(@"hh\:mm\:ss"):"unknown")}":"";progress.Report($"Scanning{estimate} • {files:N0} files • {Entry.Format(bytes)} • {files/Math.Max(1,clock.Elapsed.TotalSeconds):N0} files/s\n{path}");}
  try{
   while(true){ct.ThrowIfCancellationRequested();var dir=db.Query("SELECT path FROM folders WHERE done=0 LIMIT 1").FirstOrDefault()?[0];if(dir==null)break;Report(dir);worker??=Start();await worker.StandardInput.WriteLineAsync(JsonSerializer.Serialize(dir));long directBytes=0;
    while(true){ct.ThrowIfCancellationRequested();ScanEntry[]? batch;
     try{using var deadline=CancellationTokenSource.CreateLinkedTokenSource(ct);deadline.CancelAfter(timeoutMilliseconds);batch=await ScanWire.Read(worker.StandardOutput.BaseStream,deadline.Token);}
     catch(OperationCanceledException) when(!ct.IsCancellationRequested){if(dir.Equals(root,StringComparison.OrdinalIgnoreCase))throw new IOException("Scan root did not respond within the filesystem timeout");Issue(dir,$"Directory or metadata operation exceeded {timeoutMilliseconds/1000d:N1} seconds; worker terminated");Kill();break;}
     if(batch==null)break;
     foreach(var e in batch){ct.ThrowIfCancellationRequested();if(excluded(e.Path)||e.Path.StartsWith(dbPath,StringComparison.OrdinalIgnoreCase)||e.Path.Equals(ownHome,StringComparison.OrdinalIgnoreCase))continue;
      if(e.Kind==1){insertFolder.Text(1,e.Path);insertFolder.Text(2,dir);insertFolder.Execute();}
      else if(e.Kind==2){if(e.Path.Equals(root,StringComparison.OrdinalIgnoreCase))throw new IOException(e.Detail);Issue(e.Path,e.Detail);}
      else{insertFile.Text(1,e.Path);insertFile.Text(2,dir);insertFile.Number(3,e.Size);insertFile.Number(4,e.Modified);insertFile.Text(5,System.IO.Path.GetExtension(e.Path).ToLowerInvariant());if(insertFile.Execute()>0){files++;bytes+=e.Size;directBytes+=e.Size;if(safe&&files%128==0)await Task.Delay(5,ct);}}
      if(++writes%8192==0)db.Exec("COMMIT; BEGIN;");Report(e.Path);
     }
    }
    finishFolder.Number(1,directBytes);finishFolder.Text(2,dir);finishFolder.Execute();
   }
   progress.Report("Finalizing • committing index and building query indexes");ct.ThrowIfCancellationRequested();db.Exec("COMMIT; CREATE INDEX by_parent ON files(parent); CREATE INDEX folder_parent ON folders(parent); CREATE INDEX by_size ON files(size); CREATE INDEX by_extension ON files(extension);");
   progress.Report("Finalizing • aggregating folder sizes");db.Exec("BEGIN;");long aggregate=0;
   foreach(var row in db.Query("SELECT path,parent FROM folders ORDER BY LENGTH(path) DESC")){ct.ThrowIfCancellationRequested();if(row[1].Length>0){aggregateFolder.Text(1,row[0]);aggregateFolder.Text(2,row[1]);aggregateFolder.Execute();}if(++aggregate%1000==0)progress.Report($"Finalizing • {aggregate:N0} folders aggregated");}
   db.Exec("COMMIT;");ct.ThrowIfCancellationRequested();var date=DateTime.UtcNow.ToString("O");db.Exec($"INSERT INTO meta VALUES({Db.Q(root)},{Db.Q(date)},{(safe?1:0)});");return new ScanInfo(System.IO.Path.GetFileNameWithoutExtension(dbPath),root,date,bytes,files,safe,volume?.Serial??"",volume?.Label??"",long.Parse(db.Query("SELECT COUNT(*) FROM issues").First()[0]));
  }catch{ct.ThrowIfCancellationRequested();throw;}finally{Kill();}
 }
}
