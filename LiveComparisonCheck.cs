using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
namespace DiskLens;
public static class LiveComparisonCheck {
 static IEnumerable<T> Find<T>(DependencyObject root) where T:DependencyObject {for(int i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);i++){var child=System.Windows.Media.VisualTreeHelper.GetChild(root,i);if(child is T match)yield return match;foreach(var item in Find<T>(child))yield return item;}}
 static async Task Wait(MainVm vm){var deadline=DateTime.UtcNow.AddMinutes(2);while(vm.IsInvestigationLoading&&DateTime.UtcNow<deadline)await Task.Delay(100);}
 public static void Attach(MainWindow window,Application app,string output){window.Loaded+=async(_,_)=>{try{var vm=(MainVm)window.DataContext;var store=new Store();var before=store.History().Select(s=>new{s.Id,s.Root,s.Date,s.VolumeSerial,Path=store.PathFor(s),Exists=File.Exists(store.PathFor(s)),Compatible=vm.SelectedScan!=null&&ComparisonSelection.Compatible(vm.SelectedScan,s)}).ToArray();vm.PropertyChanged+=(_,e)=>{if(e.PropertyName==nameof(MainVm.InvestigationStatus)&&vm.InvestigationStatus=="Comparing completed indexes…")app.Dispatcher.BeginInvoke((Action)(async()=>{await Task.Delay(1000);if(vm.IsInvestigationLoading)UiTest.Render(window,"ui-comparison-loading.png");}));};await Task.Delay(3000);var tabs=Find<TabControl>(window).First();tabs.SelectedIndex=2;await Wait(vm);var automatic=vm.InvestigationStatus;vm.RefreshInvestigation.Execute(null);await Wait(vm);await app.Dispatcher.InvokeAsync(()=>{},System.Windows.Threading.DispatcherPriority.ApplicationIdle);UiTest.Render(window,"ui-actual-C-comparison.png");File.WriteAllText(output,JsonSerializer.Serialize(new{store.Home,Selected=vm.SelectedScan,History=before,Automatic=automatic,vm.InvestigationStatus,vm.ComparisonContext,Rows=vm.InvestigationRows.Count},new JsonSerializerOptions{WriteIndented=true}));}catch(Exception ex){File.WriteAllText(output,ex.ToString());}finally{window.Close();app.Shutdown();}};}
}





