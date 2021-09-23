using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System.IO;

namespace ExplorerView
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var watcher = new FileSystemWatcher(Directory.GetCurrentDirectory())
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };
            tree.Items = new AvaloniaList<TreeViewItem>
            {
                new DirectoryTreeItem(new DirectoryInfo(Directory.GetCurrentDirectory()), watcher)
            };
        }
    }
}
