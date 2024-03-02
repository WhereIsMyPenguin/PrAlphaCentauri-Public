using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;

namespace HammerHead
{
    /// <summary>
    /// Interaction logic for GraphTab.xaml
    /// </summary>
    public partial class GraphTab : UserControl
    {
        public GraphTab()
        {
            InitializeComponent();
        }

        private void EE(object sender, RoutedEventArgs e)
        {
            Total.IsChecked = true;
            TCP.IsChecked = true;
            UDP.IsChecked = true;
        }
    }
}
