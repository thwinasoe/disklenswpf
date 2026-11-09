global using System.IO;
using System.Windows;
namespace DiskLens;
public static class Program {
 [STAThread] public static int Main(string[] args){
 if(args.Contains("--hash-worker"))return Duplicates.HashWorker();
 if(args.Contains("--volume-worker"))return NativeWorkers.VolumeWorker();
 if(args.Contains("--test-partial-worker"))return ScanReliabilityTests.PartialWorker();
 if(args.Contains("--worker"))return Worker.Run();
 if(args.Length==3&&args[0]=="--benchmark")return Benchmark.Run(args).GetAwaiter().GetResult();
 if(args.Length==2&&args[0]=="--verify-saved-comparison")return ComparisonSelection.VerifySaved(args[1]).GetAwaiter().GetResult();
 if(args.Length==3&&args[0]=="--benchmark-duplicates")return DuplicateBenchmark.Run(args[1],args[2]).GetAwaiter().GetResult();
 if(args.Length==2&&args[0]=="--verify-saved-search")return SavedSearchCheck.Run(args[1]);
 if(args.Contains("--self-test"))return SelfTest.Run().GetAwaiter().GetResult();
 var test=args.Contains("--ui-test");if(test){Store.DefaultHome=System.IO.Path.Combine(AppContext.BaseDirectory,"ui-test-data");Directory.CreateDirectory(Store.DefaultHome);File.Delete(System.IO.Path.Combine(AppContext.BaseDirectory,"ui-test-failure.txt"));File.Delete(System.IO.Path.Combine(Store.DefaultHome,"errors.log"));}
 try{var app=new Application();app.DispatcherUnhandledException+=(s,e)=>{try{var folder=Store.DefaultHome??System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DiskLens.Wpf");Directory.CreateDirectory(folder);File.AppendAllText(System.IO.Path.Combine(folder,"errors.log"),e.Exception+"\n");}catch{}if(!test)MessageBox.Show(e.Exception.Message,"DiskLens error");e.Handled=true;};
 var window=new MainWindow();if(test)UiTest.Attach(window,app);if(args.Contains("--show-duplicates"))window.Loaded+=(_,_)=>{var vm=(MainVm)window.DataContext;vm.SelectedInvestigation=vm.InvestigationPresets[6];vm.ActiveTab=2;};if(args.Contains("--show-changes"))window.Loaded+=(_,_)=>((MainVm)window.DataContext).ActiveTab=2;if(args.Length==2&&args[0]=="--verify-live-comparison")LiveComparisonCheck.Attach(window,app,args[1]);app.Run(window);return test&&File.Exists(System.IO.Path.Combine(AppContext.BaseDirectory,"ui-test-failure.txt"))?1:0;
 }catch(Exception e){if(test)File.WriteAllText(System.IO.Path.Combine(AppContext.BaseDirectory,"ui-test-failure.txt"),e.ToString());else MessageBox.Show(e.Message,"DiskLens startup failed",MessageBoxButton.OK,MessageBoxImage.Error);return 1;}}
}








