using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
namespace DiskLens;
public class HistoryChart : FrameworkElement {
 public static readonly DependencyProperty PointsProperty=DependencyProperty.Register(nameof(Points),typeof(IEnumerable<ScanInfo>),typeof(HistoryChart),new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.AffectsRender,Changed));
 public IEnumerable<ScanInfo>? Points {get=>(IEnumerable<ScanInfo>?)GetValue(PointsProperty);set=>SetValue(PointsProperty,value);}
 public static readonly DependencyProperty RootProperty=DependencyProperty.Register(nameof(Root),typeof(string),typeof(HistoryChart),new FrameworkPropertyMetadata("",FrameworkPropertyMetadataOptions.AffectsRender));
 public string Root {get=>(string)GetValue(RootProperty);set=>SetValue(RootProperty,value);}
 public static readonly DependencyProperty PeriodProperty=DependencyProperty.Register(nameof(Period),typeof(string),typeof(HistoryChart),new FrameworkPropertyMetadata("Scan",FrameworkPropertyMetadataOptions.AffectsRender));
 public string Period {get=>(string)GetValue(PeriodProperty);set=>SetValue(PeriodProperty,value);}
 static void Changed(DependencyObject d,DependencyPropertyChangedEventArgs e){if(e.OldValue is INotifyCollectionChanged old)old.CollectionChanged-=((HistoryChart)d).Change;if(e.NewValue is INotifyCollectionChanged n)n.CollectionChanged+=((HistoryChart)d).Change;}
 void Change(object? s,NotifyCollectionChangedEventArgs e)=>InvalidateVisual();
 public static ScanInfo[] Measurements(IEnumerable<ScanInfo> source,string? root,string period){var scans=source.Where(s=>s.Root.Equals(root,StringComparison.OrdinalIgnoreCase)).OrderBy(s=>s.Date);if(period=="Scan")return scans.ToArray();return scans.GroupBy(s=>{var date=DateTime.Parse(s.Date).ToLocalTime().Date;return period=="Month"?new DateTime(date.Year,date.Month,1):period=="Week"?date.AddDays(-((int)date.DayOfWeek+6)%7):date;}).Select(g=>g.Last()).ToArray();}
 protected override void OnRender(DrawingContext dc){
  var scans=Measurements(Points??[],Root,Period);var ink=TryFindResource("Text") as Brush??Brushes.Gray;var accent=new SolidColorBrush(Color.FromRgb(55,198,181));
  void Text(string text,double x,double y,double size=12){var t=new FormattedText(text,CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),size,ink,VisualTreeHelper.GetDpi(this).PixelsPerDip){MaxTextWidth=Math.Max(1,ActualWidth-x-8),Trimming=TextTrimming.CharacterEllipsis};dc.DrawText(t,new Point(x,y));}
  if(scans.Length==0){Text("No completed scan selected",12,25,16);return;}
  double left=85,top=36,bottom=Math.Max(top+12,ActualHeight-42),right=Math.Max(left+20,ActualWidth-20),height=bottom-top,max=Math.Max(1,scans.Max(s=>s.Bytes))*1.15;
  for(int i=0;i<=3;i++){var y=bottom-height*i/3;dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(45,128,128,128)),1),new Point(left,y),new Point(right,y));Text(Entry.Format((long)(max*i/3)),4,y-7);}
  if(scans.Length==1){var s=scans[0];var x=left+(right-left)/2;var width=Math.Min(100,(right-left)*.35);var barHeight=Math.Max(3,height*s.Bytes/max);dc.DrawRoundedRectangle(accent,null,new Rect(x-width/2,bottom-barHeight,width,barHeight),4,4);Text(Entry.Format(s.Bytes),Math.Max(left,x-width/2),Math.Max(top-16,bottom-barHeight-22),14);Text(DateTime.Parse(s.Date).ToLocalTime().ToString("g"),Math.Max(left,x-80),bottom+8);Text(Period=="Scan"?"One completed scan — storage breakdowns below are available now.":$"One recorded {Period.ToLowerInvariant()} period — choose Scan to see every completed scan.",left,8);return;}
  Text($"{scans.Length:N0} recorded measurements • {Period.ToLowerInvariant()} view",left,8);var first=DateTime.Parse(scans[0].Date);var last=DateTime.Parse(scans[^1].Date);double range=Math.Max(.001,(last-first).TotalSeconds);Point? previous=null;foreach(var s in scans){var p=new Point(left+(DateTime.Parse(s.Date)-first).TotalSeconds/range*(right-left),bottom-height*s.Bytes/max);if(previous.HasValue)dc.DrawLine(new Pen(accent,2.5),previous.Value,p);dc.DrawEllipse(accent,null,p,5,5);previous=p;}Text(first.ToLocalTime().ToString("g"),left,bottom+8);Text(last.ToLocalTime().ToString("g"),Math.Max(left,right-150),bottom+8);
 }
}
public class BreakdownChart : FrameworkElement {
 public static readonly DependencyProperty ItemsProperty=DependencyProperty.Register(nameof(Items),typeof(IEnumerable<Entry>),typeof(BreakdownChart),new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.AffectsRender,Changed));
 public IEnumerable<Entry>? Items {get=>(IEnumerable<Entry>?)GetValue(ItemsProperty);set=>SetValue(ItemsProperty,value);}
 static void Changed(DependencyObject d,DependencyPropertyChangedEventArgs e){if(e.OldValue is INotifyCollectionChanged old)old.CollectionChanged-=((BreakdownChart)d).Change;if(e.NewValue is INotifyCollectionChanged n)n.CollectionChanged+=((BreakdownChart)d).Change;}
 void Change(object? sender,NotifyCollectionChangedEventArgs e)=>InvalidateVisual();
 protected override void OnRender(DrawingContext dc){var rows=(Items??[]).Where(e=>e.Size>0).OrderByDescending(e=>e.Size).Take(8).ToArray();var ink=TryFindResource("Text") as Brush??Brushes.Gray;void Text(string text,double x,double y,double width){dc.DrawText(new FormattedText(text,CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),12,ink,VisualTreeHelper.GetDpi(this).PixelsPerDip){MaxTextWidth=Math.Max(1,width),Trimming=TextTrimming.CharacterEllipsis},new Point(x,y));}if(rows.Length==0){Text("No indexed data for this snapshot",8,20,ActualWidth-16);return;}var max=(double)rows.Max(e=>e.Size);var labelWidth=Math.Min(140,ActualWidth*.3);var valueWidth=90d;var barWidth=Math.Max(1,ActualWidth-labelWidth-valueWidth-20);double rowHeight=Math.Min(30,ActualHeight/rows.Length);string[] colors=["#37C6B5","#4FA9CF","#A486DF","#E6A650","#DF6C83","#638DE1","#79868E","#3FB987"];for(int i=0;i<rows.Length;i++){var y=i*rowHeight+5;Text(rows[i].Name,4,y,labelWidth-8);var brush=new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i%colors.Length]));dc.DrawRoundedRectangle(brush,null,new Rect(labelWidth,y,Math.Max(2,barWidth*rows[i].Size/max),Math.Max(3,rowHeight-10)),3,3);Text(rows[i].SizeText,labelWidth+barWidth+8,y,valueWidth);}}
}
