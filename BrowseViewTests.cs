using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Text.Json;
namespace DiskLens;
public static class BrowseViewTests {
 static T? Find<T>(DependencyObject parent)where T:DependencyObject{for(int i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);i++){var child=System.Windows.Media.VisualTreeHelper.GetChild(parent,i);if(child is T match)return match;if(Find<T>(child) is T nested)return nested;}return null;}
 public static async Task Verify(MainWindow window,Application app,MainVm vm){
  var original=vm.Preferences;var folder=vm.MapFolder;var rows=vm.MapRows.ToArray();var selected=rows.First();vm.SelectedMapEntry=selected;
  try{foreach(var view in vm.BrowseViews){vm.BrowseView=view;await app.Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);if(vm.MapFolder!=folder||!vm.MapRows.SequenceEqual(rows)||vm.SelectedMapEntry!=selected)throw new Exception("Browse view switch changed folder, data or selection");UiTest.Render(window,"ui-browse-"+view.Replace(' ','-')+".png");}
   foreach(var view in new[]{"Ranked sizes","Explorer table"}){
    vm.BrowseView=view;await app.Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);
    var button=(Button)Find<BrowseView>(window)!.FindName("OpenBrowseFolderButton");string? opened=null,revealed=null;
    Actions.OpenFolderTestOverride=p=>opened=p;Actions.RevealTestOverride=p=>revealed=p;
    vm.SelectedMapEntry=rows.First(e=>e.Kind=="folder");if(!button.Command.CanExecute(null))throw new Exception("Open folder disabled for folder in "+view);button.Command.Execute(null);
    if(opened!=vm.SelectedMapEntry.Path||vm.MapFolder!=folder)throw new Exception("Open folder failed or navigated DiskLens in "+view);
    vm.SelectedMapEntry=rows.First(e=>e.Kind=="file");button.Command.Execute(null);if(revealed!=vm.SelectedMapEntry.Path)throw new Exception("Open folder did not reveal selected file in "+view);
    vm.SelectedMapEntry=new Entry("Other indexed items",1,0,"summary");if(button.Command.CanExecute(null))throw new Exception("Open folder enabled for aggregate row");
    vm.SelectedMapEntry=null;if(button.Command.CanExecute(null))throw new Exception("Open folder enabled without selection");
    vm.SelectedMapEntry=selected;UiTest.Render(window,"ui-browse-open-folder-"+view.Replace(' ','-')+".png");
   }
   vm.BrowseView="Explorer table";vm.BrowseDensity="Compact";window.ApplyTheme(true);window.Width=640;window.Height=480;await app.Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);UiTest.Render(window,"ui-browse-light-compact.png");
   var settings=JsonSerializer.Deserialize<Settings>(File.ReadAllText(System.IO.Path.Combine(Store.DefaultHome!,"settings.json")))!;if(settings.BrowseView!="Explorer table"||settings.BrowseDensity!="Compact")throw new Exception("Browse preferences not persisted");
   var legacy=JsonSerializer.Deserialize<Settings>("{\"Light\":true}")!;if(legacy.BrowseView!="Explorer table"||legacy.BrowseDensity!="Comfortable")throw new Exception("Legacy settings defaults failed");
   var relative=new RelativeSizeConverter();if((double)relative.Convert([50L,new[]{new Entry("large",100,0,"folder"),new Entry("small",50,0,"file")},2],typeof(double),null!,System.Globalization.CultureInfo.InvariantCulture)!=50)throw new Exception("Relative bars not proportional");
  }finally{Actions.OpenFolderTestOverride=null;Actions.RevealTestOverride=null;vm.SelectedMapEntry=selected;vm.BrowseView=original.BrowseView;vm.BrowseDensity=original.BrowseDensity;window.ApplyTheme(original.Light);window.Width=1200;window.Height=800;}
 }
}
