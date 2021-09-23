using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ExplorerView
{
    public class DirectoryTreeItem : TreeViewItem, IStyleable
    {
        public readonly DirectoryInfo DirectoryInfo;
        private readonly AvaloniaList<TreeViewItem> _items = new();
        private readonly FileSystemWatcher _watcher;

        private bool IsAdd;//サブフォルダを作成済みかどうか

        public DirectoryTreeItem(DirectoryInfo info, FileSystemWatcher watcher)
        {
            DirectoryInfo = info;
            Header = info.Name;
            Items = _items;
            _watcher = watcher;

            //イベント、ツリー展開時
            //サブフォルダを追加
            this.GetObservable(IsExpandedProperty).Subscribe(_ =>
            {
                if (IsAdd) return;//追加済みなら何もしない
                AddSubDirectory();
            });

            _watcher.Renamed += Watcher_Renamed;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Created += Watcher_Created;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var parent = Path.GetDirectoryName(e.FullPath);

                if (parent == DirectoryInfo.FullName)
                {
                    if (Directory.Exists(e.FullPath))
                    {
                        var di = new DirectoryInfo(e.FullPath);
                        var last=_items.LastOrDefault(i => i is DirectoryTreeItem);
                        if (last != null)
                        {
                            var index = _items.IndexOf(last);
                            _items.Insert(index, new DirectoryTreeItem(di, _watcher));
                        }
                        else
                        {
                            _items.Add(new DirectoryTreeItem(di, _watcher));
                        }
                    }
                    else
                    {
                        _items.Add(new TreeViewItem
                        {
                            Header = Path.GetFileName(e.Name),
                        });
                    }
                }
            });
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var parent = Path.GetDirectoryName(e.FullPath);
                var filename = Path.GetFileName(e.Name);

                if (parent == DirectoryInfo.FullName)
                {
                    var item = _items.FirstOrDefault(i => i.Header is string str && str == filename);
                    if (item != null)
                    {
                        _items.Remove(item);
                    }
                }
            });
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var parent = Path.GetDirectoryName(e.FullPath);
                var oldFilename = Path.GetFileName(e.OldName);
                var newFilename = Path.GetFileName(e.Name);

                if (parent == DirectoryInfo.FullName)
                {
                    var item = _items.FirstOrDefault(i => i.Header is string str && str == oldFilename);
                    if (item != null)
                    {
                        item.Header = newFilename;
                    }
                }
            });
        }

        Type IStyleable.StyleKey => typeof(TreeViewItem);

        //サブフォルダツリー追加
        public void AddSubDirectory()
        {
            //すべてのサブフォルダを追加
            foreach (var item in DirectoryInfo.GetDirectories().Where(i => !i.Attributes.HasAnyFlag(FileAttributes.Hidden | FileAttributes.System)))
            {
                _items.Add(new DirectoryTreeItem(item, _watcher));
            }

            // 全てのファイル追加
            foreach (var item in DirectoryInfo.GetFiles().Where(i => !i.Attributes.HasAnyFlag(FileAttributes.Hidden | FileAttributes.System)))
            {
                _items.Add(new TreeViewItem
                {
                    Header = item.Name,
                });
            }

            IsAdd = true;//サブフォルダ作成済みフラグ
        }

        public override string ToString()
        {
            return DirectoryInfo.FullName;
        }
    }
}
