namespace DiskLens;
public static class SnapshotLocationTests {
 public static void Run(string root,Store original,ScanInfo current,ScanInfo previous,Action<bool,string> check){
  var normal=System.IO.Path.Combine(root,"normal-app-data");var redirected=System.IO.Path.Combine(root,"redirected-app-data");
  Directory.CreateDirectory(normal);Directory.CreateDirectory(redirected);
  void Copy(ScanInfo scan,string destination){
   File.Copy(original.PathFor(scan),System.IO.Path.Combine(destination,scan.Id+".db"));
   File.Copy(System.IO.Path.ChangeExtension(original.PathFor(scan),".json"),System.IO.Path.Combine(destination,scan.Id+".json"));
  }
  Copy(current,normal);Copy(previous,redirected);
  var split=new Store(normal,[redirected]);var history=split.History();
  check(history.Count==2&&ComparisonSelection.Previous(current,history)?.Id==previous.Id,"normal startup discovers baseline in redirected app-data");
  check(split.PathFor(previous)==System.IO.Path.Combine(redirected,previous.Id+".db"),"comparison resolves earlier index in its original folder");
  var changes=InvestigationStore.Changes(split,current,previous,CancellationToken.None);
  check(changes.Rows.Count>0&&changes.Summary.Count>0,"comparison returns results across normal and redirected stores");
  Copy(previous,normal);
  check(split.History().Count==2&&split.PathFor(previous)==System.IO.Path.Combine(normal,previous.Id+".db"),"overlapping stores deduplicate snapshots and prefer normal store");
  split.DeleteSnapshot(previous);
  check(split.History().Count==1&&File.Exists(split.PathFor(current))&&!File.Exists(System.IO.Path.Combine(redirected,previous.Id+".json")),"explicit snapshot removal removes duplicate metadata without touching current scan");
 }
}
