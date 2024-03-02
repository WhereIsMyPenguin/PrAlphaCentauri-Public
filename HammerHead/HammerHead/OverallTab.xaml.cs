using LiveChartsCore.Kernel.Sketches;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using System.Collections.Specialized;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.Defaults;
using System.ComponentModel;
using System.Runtime.Intrinsics.Arm;

namespace HammerHead
{
    /// <summary>
    /// Interaction logic for OverallTab.xaml
    /// </summary>
    public partial class OverallTab : UserControl
    {

        public OverallTab()
        {
            InitializeComponent();
        }

        private void ME(object sender, EventArgs e)
        {
            Panel.SetZIndex(G1, 2);
            Panel.SetZIndex(G2, 2);
            Panel.SetZIndex(G1s, 1);
            Panel.SetZIndex(G2s, 1);
        }
        private void ML(object sender, EventArgs e)
        {
            Panel.SetZIndex(G1, 1);
            Panel.SetZIndex(G2, 1);
            Panel.SetZIndex(G1s, 2);
            Panel.SetZIndex(G2s, 2);
        }

        private void Swap(object sender, RoutedEventArgs e)
        {
            if(Swapper.IsChecked == true)
            {
                Panel.SetZIndex(G1, 1);
                Panel.SetZIndex(G2, 1);
                Panel.SetZIndex(G1s, 0);
                Panel.SetZIndex(G2s, 0);
                G1.Margin = new Thickness(100);
                G2.Margin = new Thickness(100);
                G1s.Margin = new Thickness(0);
                G2s.Margin = new Thickness(0);
            }
            else
            {
                Panel.SetZIndex(G1, 0);
                Panel.SetZIndex(G2, 0);
                Panel.SetZIndex(G1s, 1);
                Panel.SetZIndex(G2s, 1);
                G1.Margin = new Thickness(0);
                G2.Margin = new Thickness(0);
                G1s.Margin = new Thickness(100);
                G2s.Margin = new Thickness(100);
            }
        }
    }
}
