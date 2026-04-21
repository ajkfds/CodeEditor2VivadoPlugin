using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace pluginVivado.Views
{
    public partial class SimPanel : UserControl
    {
        public SimPanel()
        {
            InitializeComponent();

            ListBox0.ItemsSource = listItems;
        }

        private ObservableCollection<ListBoxItem> listItems = new ObservableCollection<ListBoxItem>();

        public void LineReceived(string lineString, Color? color)
        {
            appendLog(lineString, color);
        }

        private void appendLog(string lineString, Color? color)
        {
            Dispatcher.UIThread.Post(
                    new Action(() =>
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = lineString;
                        textBlock.FontSize = 10;
                        textBlock.Height = 11;
                        textBlock.MinHeight = 11;
                        if (color != null)
                        {
                            textBlock.Foreground = new SolidColorBrush((Color)color);
                        }
                        textBlock.Margin = new Avalonia.Thickness(0, 0, 0, 0);
                        lock (listItems)
                        {
                            ListBoxItem item = new ListBoxItem();
                            item.Content = textBlock;
                            listItems.Add(item);
                            if (listItems.Count > 1000)
                            {
                                ListBoxItem? removeItem = listItems[0] as ListBoxItem;
                                if (removeItem == null) return;
                                listItems.Remove(removeItem);
                            }
                            listItems.Last().IsSelected = true;
                            ListBox0.ScrollIntoView(listItems.Last());
                        }
                        ListBox0.InvalidateVisual();
                    })
                );
        }

    }
}
