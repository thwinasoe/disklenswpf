using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace DiskLens;
public static class FileRowActions {
 public static void Open(DataGrid grid,MouseButtonEventArgs args,MainVm vm){var node=args.OriginalSource as DependencyObject;while(node!=null&&node is not DataGridRow){node=node is ContentElement content?ContentOperations.GetParent(content)??(content as FrameworkContentElement)?.Parent:VisualTreeHelper.GetParent(node);}if(node is DataGridRow {Item:Entry entry}&&entry.Kind is "file" or "folder" or "change"){vm.Selected=entry;if(vm.Reveal.CanExecute(null)){vm.Reveal.Execute(null);args.Handled=true;}}}
}

