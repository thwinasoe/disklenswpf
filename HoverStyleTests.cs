using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
namespace DiskLens;
public static class HoverStyleTests {
 public static async Task Verify(MainWindow window,Application app){
  var button=Find<Button>(window,b=>b.Content?.ToString()?.Contains("Parent folder")==true)??throw new Exception("Parent button missing");
  var key=(DependencyPropertyKey?)typeof(UIElement).GetField("IsMouseOverPropertyKey",BindingFlags.Static|BindingFlags.NonPublic)?.GetValue(null)??throw new Exception("Hover state key missing");
  var original=((MainVm)window.DataContext).Preferences.Light;
  try{foreach(var light in new[]{false,true}){
   window.ApplyTheme(light);button.SetValue(key,true);await app.Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);
   var border=(Border)button.Template.FindName("ButtonSurface",button);
   var text=Find<TextBlock>(button,_=>true)??throw new Exception("Button label missing");
   var foreground=((SolidColorBrush)text.Foreground).Color;var background=((SolidColorBrush)border.Background).Color;
   if(background!=((SolidColorBrush)window.Resources["ButtonHover"]).Color)throw new Exception("Theme hover background not applied");
   if(Contrast(foreground,background)<4.5)throw new Exception("Button hover contrast below 4.5:1");
   UiTest.Render(window,light?"ui-button-hover-light.png":"ui-button-hover-dark.png");button.SetValue(key,false);
  }}finally{button.SetValue(key,false);window.ApplyTheme(original);}
 }
 static double Contrast(Color a,Color b){double L(Color c){double C(byte value){var n=value/255d;return n<=.04045?n/12.92:Math.Pow((n+.055)/1.055,2.4);}return .2126*C(c.R)+.7152*C(c.G)+.0722*C(c.B);}var x=L(a);var y=L(b);return (Math.Max(x,y)+.05)/(Math.Min(x,y)+.05);}
 static T? Find<T>(DependencyObject root,Func<T,bool> predicate)where T:DependencyObject{if(root is T item&&predicate(item))return item;for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var found=Find(VisualTreeHelper.GetChild(root,i),predicate);if(found!=null)return found;}return null;}
}
