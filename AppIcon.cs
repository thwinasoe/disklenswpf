using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
namespace DiskLens;
public class AppIcon : FrameworkElement {
 public static readonly DependencyProperty KindProperty=DependencyProperty.Register(nameof(Kind),typeof(string),typeof(AppIcon),new FrameworkPropertyMetadata("File",FrameworkPropertyMetadataOptions.AffectsRender));
 public string Kind{get=>(string)GetValue(KindProperty);set=>SetValue(KindProperty,value);}
 public static readonly DependencyProperty ForegroundProperty=DependencyProperty.Register(nameof(Foreground),typeof(Brush),typeof(AppIcon),new FrameworkPropertyMetadata(Brushes.Gray,FrameworkPropertyMetadataOptions.AffectsRender));
 public Brush Foreground{get=>(Brush)GetValue(ForegroundProperty);set=>SetValue(ForegroundProperty,value);}
 static readonly JsonDocument shapes=Load();
 static JsonDocument Load(){using var stream=Application.GetResourceStream(new Uri("/DiskLens;component/Assets/lucide-icons.json",UriKind.Relative))!.Stream;return JsonDocument.Parse(stream);}
 public AppIcon(){Width=18;Height=18;SnapsToDevicePixels=true;}
 public static void ValidateShapes(){foreach(var icon in shapes.RootElement.EnumerateObject())foreach(var shape in icon.Value.EnumerateArray())if(shape[0].GetString()=="path")Geometry.Parse(shape[1].GetProperty("d").GetString()!);}
 protected override void OnRender(DrawingContext dc){base.OnRender(dc);dc.PushTransform(new ScaleTransform(ActualWidth/24,ActualHeight/24));if(Kind=="Brand"){double[] opacity=[1,.5,.72,.3];for(int i=0;i<4;i++){var brush=Foreground.Clone();brush.Opacity=opacity[i];dc.DrawRoundedRectangle(brush,null,new Rect(i%2*12,i/2*12,10,10),2,2);}dc.Pop();return;}if(!shapes.RootElement.TryGetProperty(Kind??"File",out var nodes))nodes=shapes.RootElement.GetProperty("File");var pen=new Pen(Foreground,2){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};foreach(var node in nodes.EnumerateArray()){var attr=node[1];double N(string key,double fallback=0)=>attr.TryGetProperty(key,out var value)?value.ValueKind==JsonValueKind.Number?value.GetDouble():double.Parse(value.GetString()!,CultureInfo.InvariantCulture):fallback;switch(node[0].GetString()){
 case "path":dc.DrawGeometry(null,pen,Geometry.Parse(attr.GetProperty("d").GetString()!));break;
 case "line":dc.DrawLine(pen,new Point(N("x1"),N("y1")),new Point(N("x2"),N("y2")));break;
 case "circle":dc.DrawEllipse(null,pen,new Point(N("cx"),N("cy")),N("r"),N("r"));break;
 case "ellipse":dc.DrawEllipse(null,pen,new Point(N("cx"),N("cy")),N("rx"),N("ry"));break;
 case "rect":dc.DrawRoundedRectangle(null,pen,new Rect(N("x"),N("y"),N("width"),N("height")),N("rx"),N("ry",N("rx")));break;
 case "polyline":case "polygon":var values=attr.GetProperty("points").GetString()!.Split([' ', ','],StringSplitOptions.RemoveEmptyEntries).Select(s=>double.Parse(s,CultureInfo.InvariantCulture)).ToArray();var geometry=new StreamGeometry();using(var context=geometry.Open()){context.BeginFigure(new Point(values[0],values[1]),false,node[0].GetString()=="polygon");for(int i=2;i<values.Length;i+=2)context.LineTo(new Point(values[i],values[i+1]),true,false);}dc.DrawGeometry(null,pen,geometry);break;
 }}dc.Pop();}
}
public class ButtonIconConverter : IValueConverter {
 public object Convert(object value,Type targetType,object parameter,CultureInfo culture){var text=value?.ToString()??"";return text switch{"Quick check"=>"Hash","Full check"=>"Hash","Map"=>"LayoutGrid","Search"=>"Search","Investigate"=>"ScanSearch","Insights / History"=>"BarChart3","Drives"=>"HardDrive","Settings"=>"Settings",_=>text.Contains("Stop")?"CircleStop":text.Contains("Recycle")||text.Contains("Forget")?"Trash2":text.Contains("duplicate",StringComparison.OrdinalIgnoreCase)?"Hash":text.Contains("health",StringComparison.OrdinalIgnoreCase)?"ShieldAlert":text.Contains("changed",StringComparison.OrdinalIgnoreCase)||text.Contains("last scan",StringComparison.OrdinalIgnoreCase)?"History":text.Contains("Refresh")?"RefreshCw":text.Contains("Scan")||text.Contains("Rescan")?"ScanSearch":text.Contains("Browse")||text.Contains("folder")?"FolderOpen":text.Contains("Export")||text is "CSV" or "JSON" or "HTML"?"Download":text.Contains("Previous")||text.Contains("Parent")?"ArrowLeft":text=="Next"?"ChevronRight":text.Contains("type")?"BarChart3":text.Contains("Light")?"Sun":"Info"};}
 public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture)=>Binding.DoNothing;
}

