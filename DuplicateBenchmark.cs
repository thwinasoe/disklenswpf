using System.Diagnostics;
using System.Text.Json;
namespace DiskLens;
public static class DuplicateBenchmark {
 public static async Task<int> Run(string fixture,string output){try{Directory.CreateDirectory(fixture);var payload=new byte[65536];new Random(42).NextBytes(payload);for(int i=0;i<1000;i++)File.WriteAllBytes(Path.Combine(fixture,$"copy-{i:D4}.bin"),payload);var store=new Store(Path.Combine(Path.GetTempPath(),"DiskLens-duplicate-bench-"+Guid.NewGuid().ToString("N")));try{var pending=Path.Combine(store.Home,"pending");var scan=await new Scanner().Run(fixture,pending,false,"",new Progress<string>(),CancellationToken.None);store.Commit(scan,pending,20);var measurements=new List<object>();for(int i=0;i<3;i++){var sw=Stopwatch.StartNew();var rows=await Duplicates.Find(store,scan,"",1,0,new Progress<string>(),CancellationToken.None);measurements.Add(new{Milliseconds=sw.Elapsed.TotalMilliseconds,Rows=rows.Count});}File.WriteAllText(output,JsonSerializer.Serialize(measurements,new JsonSerializerOptions{WriteIndented=true}));}finally{Directory.Delete(store.Home,true);}return 0;}catch(Exception ex){File.WriteAllText(output+".error.txt",ex.ToString());return 1;}}
}
