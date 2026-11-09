using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
namespace DiskLens;
public partial class BrowseView:UserControl {
 MainVm? model;
 public BrowseView(){InitializeComponent();DataContextChanged+=(_,_)=>{if(model!=null)model.PropertyChanged-=Changed;model=DataContext as MainVm;if(model!=null)model.PropertyChanged+=Changed;UpdateView();};}
 void Changed(object? sender,PropertyChangedEventArgs e){if(e.PropertyName==nameof(MainVm.BrowseView))UpdateView();}
 void UpdateView(){var ranked=model?.BrowseView=="Ranked sizes";KindColumn.Visibility=ranked?Visibility.Collapsed:Visibility.Visible;BrowseGrid.Columns[2].DisplayIndex=ranked?1:2;BrowseGrid.Columns[2].Width=new DataGridLength(ranked?3:1,DataGridLengthUnitType.Star);if(ranked){var view=CollectionViewSource.GetDefaultView(BrowseGrid.ItemsSource);if(view!=null){view.SortDescriptions.Clear();view.SortDescriptions.Add(new SortDescription(nameof(Entry.Size),ListSortDirection.Descending));}}}
 void Rows_DoubleClick(object sender,MouseButtonEventArgs e){if(ItemsControl.ContainerFromElement(BrowseGrid,e.OriginalSource as DependencyObject) is DataGridRow row)Open(row.Item as Entry);}
 void Rows_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter){Open(BrowseGrid.SelectedItem as Entry);e.Handled=true;}}
 void Open(Entry? entry){if(entry!=null&&model?.Drill.CanExecute(entry)==true)model.Drill.Execute(entry);}
}
public sealed class RelativeSizeConverter:IMultiValueConverter {
 public object Convert(object[] values,Type targetType,object parameter,CultureInfo culture){if(values[0] is not long size||values[1] is not IEnumerable<Entry> entries)return 0d;var max=entries.Select(e=>e.Size).DefaultIfEmpty(0).Max();return max>0?100d*size/max:0d;}
 public object[] ConvertBack(object value,Type[] targetTypes,object parameter,CultureInfo culture)=>throw new NotSupportedException();
}
