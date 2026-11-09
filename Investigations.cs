namespace DiskLens;
public record InvestigationPreset(string Id,string Label,string Icon);
public record SummaryCard(string Label,string Value,string Detail="");
public record ChangeSummary(long Added,long Grown,long Shrunk,long Removed,long Count);
public partial class InvestigationStore {
 public static List<Entry> Files(Store store,ScanInfo scan,string mode,string query,long minimum,long maximum,int page,CancellationToken ct){
  var matches=Rules.Matcher(query);using var db=new Db(store.PathFor(scan));using var cancellation=ct.Register(db.Interrupt);
  long now=DateTime.UtcNow.Ticks;long min=Math.Max(0,minimum);if(mode is "Recent large files" or "Old large files")min=Math.Max(min,1048576);
  string where=$"size>={min} AND size<={(maximum>0?maximum:long.MaxValue)} AND {SearchCandidates.Where(query)}";
  if(mode=="Recent large files")where+=$" AND modified>={now-TimeSpan.FromDays(30).Ticks}";
  if(mode=="Old large files")where+=$" AND modified>0 AND modified<{now-TimeSpan.FromDays(180).Ticks}";
  var result=new List<Entry>();int skipped=0;
  try{foreach(var row in db.Query($"SELECT path,size,modified FROM files WHERE {where} ORDER BY size DESC")){
   ct.ThrowIfCancellationRequested();var path=row[0];var modified=long.Parse(row[2]);if(query.Length>0&&!matches(path))continue;
   if(mode=="Installer files"&&!Rules.Installer(path))continue;
   if(mode=="Archives and images"&&(Rules.Installer(path)||Rules.Category(path) is not ("Archives" or "Images")))continue;
   if(mode=="Developer clutter"&&!Rules.Developer(path))continue;
   if(skipped++<page*500)continue;
   string reason=mode switch{"Recent large files"=>$"Modified {new DateTime(modified,DateTimeKind.Utc).ToLocalTime():g}; at least 1 MB and changed in the last 30 days","Old large files"=>$"Not modified for {(now-modified)/TimeSpan.TicksPerDay:N0} days (older than 180 days)","Installer files"=>System.IO.Path.GetExtension(path).Equals(".msi",StringComparison.OrdinalIgnoreCase)?"Windows Installer package":Rules.Installer(path)?"Installer package or setup-style executable":"", "Developer clutter"=>path.Contains("node_modules",StringComparison.OrdinalIgnoreCase)?"Inside a node_modules dependency folder":path.Contains("cache",StringComparison.OrdinalIgnoreCase)?"Path contains a cache folder":"Build, binary or temporary output path", "Archives and images"=>Rules.Category(path)=="Images"?"Image file":"Archive candidate",_=>Rules.Explain(path,modified)};
   result.Add(new Entry(path,long.Parse(row[1]),modified,"file",reason+". Review before recycling; this does not prove the file is unused."));if(result.Count==500)break;
  }return result;}catch{ct.ThrowIfCancellationRequested();throw;}
 }
 public static (List<Entry> Rows,ChangeSummary Summary) Changes(Store store,ScanInfo current,ScanInfo previous,CancellationToken ct){
  using var db=new Db(store.PathFor(current));using var cancellation=ct.Register(db.Interrupt);db.Exec($"ATTACH DATABASE {Db.Q(store.PathFor(previous))} AS old;");
  const string union="SELECT n.path,n.size,n.modified,CASE WHEN o.path IS NULL THEN 'Added' WHEN n.size>o.size THEN 'Grown' ELSE 'Shrunk' END change,n.size-COALESCE(o.size,0) delta FROM files n LEFT JOIN old.files o ON n.path=o.path WHERE o.path IS NULL OR n.size!=o.size UNION ALL SELECT o.path,0,o.modified,'Removed',-o.size FROM old.files o LEFT JOIN files n ON n.path=o.path WHERE n.path IS NULL";
  try{var totals=db.Query("SELECT COALESCE(SUM(CASE WHEN change='Added' THEN delta ELSE 0 END),0),COALESCE(SUM(CASE WHEN change='Grown' THEN delta ELSE 0 END),0),COALESCE(SUM(CASE WHEN change='Shrunk' THEN -delta ELSE 0 END),0),COALESCE(SUM(CASE WHEN change='Removed' THEN -delta ELSE 0 END),0),COUNT(*) FROM ("+union+")").First();
  var rows=db.Query("SELECT * FROM ("+union+") ORDER BY ABS(delta) DESC LIMIT 1000").Select(r=>new Entry(r[0],long.Parse(r[1]),long.Parse(r[2]),r[3]=="Removed"?"removed":"change",r[3]=="Removed"?"Absent from the current scan; may be removed, inaccessible or excluded":r[3]+" since the previous completed scan",long.Parse(r[4]),r[3])).ToList();return(rows,new ChangeSummary(long.Parse(totals[0]),long.Parse(totals[1]),long.Parse(totals[2]),long.Parse(totals[3]),long.Parse(totals[4])));}catch{ct.ThrowIfCancellationRequested();throw;}
 }
}
