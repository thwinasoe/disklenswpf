using System.Diagnostics;
using System.Text.Json;
namespace DiskLens;
public static class SavedSearchCheck {
 public static int Run(string output){var store=new Store();var scan=store.History().First(s=>s.Root=="C:\\");var sw=Stopwatch.StartNew();var rows=store.Search(scan,"*.docx",0,0,"All files");File.WriteAllText(output,JsonSerializer.Serialize(new{Rows=rows.Count,AllDocx=rows.All(e=>e.Path.EndsWith(".docx",StringComparison.OrdinalIgnoreCase)),Milliseconds=sw.Elapsed.TotalMilliseconds},new JsonSerializerOptions{WriteIndented=true}));return rows.Count>0&&rows.All(e=>e.Path.EndsWith(".docx",StringComparison.OrdinalIgnoreCase))?0:1;}
}
