using System.Text.Json;
namespace DiskLens;
public static class ComparisonDiagnostics {
 // Keep the report outside History's *.json metadata inventory.
 public static void Write(Store store,ScanInfo current,ScanInfo[] saved,ScanInfo? previous){
  try {File.WriteAllText(Path.Combine(store.Home,"comparison-diagnostics.log"),JsonSerializer.Serialize(new {
   CapturedUtc=DateTime.UtcNow,Version=typeof(MainVm).Assembly.GetName().Version?.ToString(),
   store.Home,store.HistoryHomes,Current=current,Previous=previous,
   Snapshots=saved.Select(s=>new {Scan=s,Database=store.PathFor(s),Compatible=ComparisonSelection.Compatible(current,s)}),
   Metadata=store.HistoryHomes.Where(Directory.Exists).SelectMany(location=>Directory.EnumerateFiles(location,"*.json")).Where(p=>Path.GetFileName(p)!="settings.json").Select(p=>new {Path=p,Read=Read(p)})
  },new JsonSerializerOptions{WriteIndented=true}));}catch { /* Diagnostics must not prevent comparison. */ }
 }
 static string Read(string path){try{return File.ReadAllText(path);}catch(Exception e){return e.GetType().Name+": "+e.Message;}}
}
