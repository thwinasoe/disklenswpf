namespace DiskLens;
public partial class MainVm {
 bool syncingHistory;
 // A refresh must not clear the selected scan through the DataGrid's two-way binding.
 void SyncSavedHistory(ScanInfo[] saved){
  if(History.SequenceEqual(saved))return;
  syncingHistory=true;
  try{History.Clear();foreach(var item in saved)History.Add(item);}
  finally{syncingHistory=false;}
  Notify(nameof(SelectedScan));
 }
}
