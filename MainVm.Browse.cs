namespace DiskLens;
public partial class MainVm {
 public string[] BrowseViews {get;}=["Explorer table","Ranked sizes","Space map"];
 public string[] BrowseDensities {get;}=["Comfortable","Compact"];
 public string BrowseView {get=>BrowseViews.Contains(Preferences.BrowseView)?Preferences.BrowseView:BrowseViews[0];set{if(!BrowseViews.Contains(value)||value==BrowseView)return;Preferences=Preferences with{BrowseView=value};Save();Notify();}}
 public string BrowseDensity {get=>BrowseDensities.Contains(Preferences.BrowseDensity)?Preferences.BrowseDensity:BrowseDensities[0];set{if(!BrowseDensities.Contains(value)||value==BrowseDensity)return;Preferences=Preferences with{BrowseDensity=value};Save();Notify();}}
 Entry? selectedMapEntry;
 public Entry? SelectedMapEntry {get=>selectedMapEntry;set{selectedMapEntry=value;Notify();System.Windows.Input.CommandManager.InvalidateRequerySuggested();}}
 public System.Windows.Input.ICommand OpenMapFolder=>new Command(_=>Guard(()=>Actions.OpenFolder(SelectedMapEntry!.Path)),()=>!Busy&&SelectedMapEntry?.Kind is "folder" or "file");
}
