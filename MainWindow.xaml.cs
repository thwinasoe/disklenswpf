using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Globalization;
namespace DiskLens;
public partial class MainWindow:Window {
 readonly MainVm vm;public MainWindow(){InitializeComponent();Title="DiskLens • WPF "+typeof(MainWindow).Assembly.GetName().Version!.ToString(3);vm=new MainVm();DataContext=vm;ApplyTheme(vm.Preferences.Light);Closing+=(s,e)=>{if(vm.Busy){e.Cancel=true;vm.Stop.Execute(null);vm.Status="Stopping operation. Close again once it has stopped.";}};}
 void SearchQuery_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter&&vm.Search.CanExecute(null)){vm.Search.Execute(null);e.Handled=true;}}
 void Files_DoubleClick(object sender,MouseButtonEventArgs e)=>FileRowActions.Open((System.Windows.Controls.DataGrid)sender,e,vm); void Files_SelectionChanged(object sender,System.Windows.Controls.SelectionChangedEventArgs e){if(DataContext is MainVm model&&sender is System.Windows.Controls.DataGrid grid){model.SelectedFiles=grid.SelectedItems.Cast<Entry>().ToArray();CommandManager.InvalidateRequerySuggested();}}void Theme_Click(object sender,RoutedEventArgs e){var light=!vm.Preferences.Light;vm.SetTheme(light);ApplyTheme(light);}
 public void ApplyTheme(bool light){Resources["BrowseHover"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(light?"#EAF0F2":"#182125"));Resources["BrowseSelection"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(light?"#D7EBE6":"#203D38"));Resources["BrowseBar"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(light?"#168575":"#37B6A5"));Resources["ButtonHover"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(light?"#E1ECEF":"#29383F"));Resources["ButtonPressed"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(light?"#CDDDE2":"#34474F"));Resources["Bg"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(light?"#F3F5F6":"#101416"));Resources["Panel"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(light?"#FFFFFF":"#1D2327"));Resources["Text"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(light?"#192025":"#EDF2F4"));}
}
public class Treemap:FrameworkElement {
 public static readonly DependencyProperty SelectedEntryProperty=DependencyProperty.Register(nameof(SelectedEntry),typeof(Entry),typeof(Treemap),new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.AffectsRender|FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
 public Entry? SelectedEntry{get=>(Entry?)GetValue(SelectedEntryProperty);set=>SetValue(SelectedEntryProperty,value);}
 public static readonly DependencyProperty ItemsProperty=DependencyProperty.Register(nameof(Items),typeof(IEnumerable<Entry>),typeof(Treemap),new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.AffectsRender,Changed));
 public IEnumerable<Entry>? Items{get=>(IEnumerable<Entry>?)GetValue(ItemsProperty);set=>SetValue(ItemsProperty,value);}public static readonly DependencyProperty DrillProperty=DependencyProperty.Register(nameof(Drill),typeof(ICommand),typeof(Treemap));public ICommand? Drill{get=>(ICommand?)GetValue(DrillProperty);set=>SetValue(DrillProperty,value);}
 static void Changed(DependencyObject d,DependencyPropertyChangedEventArgs e){if(e.OldValue is INotifyCollectionChanged old)old.CollectionChanged-=((Treemap)d).OnChanged;if(e.NewValue is INotifyCollectionChanged n)n.CollectionChanged+=((Treemap)d).OnChanged;}void OnChanged(object? s,NotifyCollectionChangedEventArgs e)=>InvalidateVisual();
 readonly List<(Rect Rect,Entry Entry)> cells=[];Dictionary<string,string> folderColors=[];
 string TileColor(Entry e)=>e.Kind=="folder"&&folderColors.TryGetValue(e.Path,out var color)?color:MapPalette.FileColor(e); protected override void OnRender(DrawingContext dc){base.OnRender(dc);cells.Clear();var list=Items?.Where(e=>e.Size>0).ToArray()??[];folderColors=MapPalette.FolderColors(list);if(list.Length==0){dc.DrawText(new FormattedText("Scan a location to explore disk usage",CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),18,Brushes.Gray,VisualTreeHelper.GetDpi(this).PixelsPerDip),new Point(20,30));return;}Layout(dc,list,0,list.Length,new Rect(0,0,ActualWidth,ActualHeight));}
 void Layout(DrawingContext dc,Entry[] a,int start,int count,Rect r){if(count==1){var e=a[start];var rect=new Rect(r.X+2,r.Y+2,Math.Max(0,r.Width-4),Math.Max(0,r.Height-4));cells.Add((rect,e));var brush=new SolidColorBrush((Color)ColorConverter.ConvertFromString(TileColor(e)));dc.DrawRoundedRectangle(brush,null,rect,4,4);if(SelectedEntry?.Path==e.Path)dc.DrawRoundedRectangle(null,new Pen(Brushes.White,2),rect,4,4);if(rect.Width>60&&rect.Height>36){var text=new FormattedText(e.Name+"\n"+e.SizeText,CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),12,Brushes.White,VisualTreeHelper.GetDpi(this).PixelsPerDip){MaxTextWidth=Math.Max(1,rect.Width-12),MaxTextHeight=Math.Max(1,rect.Height-10),Trimming=TextTrimming.CharacterEllipsis};dc.DrawText(text,new Point(rect.X+6,rect.Y+5));}return;}double total=0;for(int i=start;i<start+count;i++)total+=a[i].Size;double sum=0;int split=1;for(int i=start;i<start+count-1;i++){sum+=a[i].Size;split=i-start+1;if(sum>=total/2)break;}var ratio=sum/total;if(r.Width>=r.Height){var w=r.Width*ratio;Layout(dc,a,start,split,new Rect(r.X,r.Y,w,r.Height));Layout(dc,a,start+split,count-split,new Rect(r.X+w,r.Y,r.Width-w,r.Height));}else{var h=r.Height*ratio;Layout(dc,a,start,split,new Rect(r.X,r.Y,r.Width,h));Layout(dc,a,start+split,count-split,new Rect(r.X,r.Y+h,r.Width,r.Height-h));}}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);var p=e.GetPosition(this);ToolTip=cells.FirstOrDefault(c=>c.Rect.Contains(p)).Entry?.Path;}
 protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e){base.OnMouseLeftButtonDown(e);var hit=cells.FirstOrDefault(c=>c.Rect.Contains(e.GetPosition(this))).Entry;SetCurrentValue(SelectedEntryProperty,hit);if(hit!=null&&e.ClickCount==2&&Drill?.CanExecute(hit)==true)Drill.Execute(hit);}
}








