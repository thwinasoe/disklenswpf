using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace DiskLens;
public partial class InvestigationView:UserControl {
 public InvestigationView()=>InitializeComponent();
 void LayoutChanged(object sender,SizeChangedEventArgs e){bool compact=ActualWidth<900;NavigationColumn.Width=new GridLength(compact?0:225);CompactRow.Height=compact?GridLength.Auto:new GridLength(0);SummaryStrip.Visibility=compact?Visibility.Collapsed:Visibility.Visible;Description.Visibility=compact?Visibility.Collapsed:Visibility.Visible;Navigation.Visibility=compact?Visibility.Collapsed:Visibility.Visible;CompactNavigation.Visibility=compact?Visibility.Visible:Visibility.Collapsed;}
 void Files_DoubleClick(object sender,MouseButtonEventArgs e){if(DataContext is MainVm model)FileRowActions.Open((DataGrid)sender,e,model);}
 void FilesChanged(object sender,SelectionChangedEventArgs e){if(DataContext is MainVm model&&sender is DataGrid grid){model.SelectedFiles=grid.SelectedItems.Cast<Entry>().ToArray();CommandManager.InvalidateRequerySuggested();}}
}

