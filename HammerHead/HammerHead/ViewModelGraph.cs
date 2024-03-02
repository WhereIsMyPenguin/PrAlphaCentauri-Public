using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;

namespace HammerHead
{
    public partial class ViewModelGraph : ObservableObject
    {
        public ObservableCollection<ISeries> Series { get; set; }
        public ObservableCollection<ISeries> ScrollSeries { get; set; }
        private ObservableCollection<PacketHolder> Items = new ObservableCollection<PacketHolder>();
        private ObservableCollection<DateTimePoint> _pointAll = new();
        private ObservableCollection<DateTimePoint> _pointTcp = new();
        private ObservableCollection<DateTimePoint> _pointUdp = new();

        private readonly DateTimeAxis _dateAxis;

        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public Axis[] InvisibleX { get; set; }
        public Axis[] InvisibleY { get; set; }

        public object Sync { get; } = new object();
        public bool IsReading { get; set; } = true;
        private bool MouseDown = false;
        public RectangularSection[] Thumbs { get; set; }

        public ViewModelGraph()
        {
            var colorAll = new SKColor(133, 115, 219);
            var colorTcp = new SKColor(75, 169, 243);
            var colorUdp = new SKColor(159, 203, 107);
            Series = new ObservableCollection<ISeries>()
            {
                new LineSeries<DateTimePoint>
                {
                    Values = _pointAll,
                    Fill = new SolidColorPaint(colorAll.WithAlpha(40)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.4,
                    Name = "Total",
                },
                new LineSeries<DateTimePoint>
                {
                    Values = _pointTcp,
                    Fill = new SolidColorPaint(colorTcp.WithAlpha(40)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.4,
                    Name = "TCP",
                },
                new LineSeries<DateTimePoint>
                {
                    Values = _pointUdp,
                    Fill = new SolidColorPaint(colorUdp.WithAlpha(40)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.4,
                    Name = "UDP",
                }
            };

            ScrollSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<DateTimePoint>
                {
                    Values = _pointAll,
                    Fill = new SolidColorPaint(colorAll.WithAlpha(40)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.4,
                    Name = "Total",
                },
                new LineSeries<DateTimePoint>
                {
                    Values = _pointTcp,
                    Fill = new SolidColorPaint(colorTcp.WithAlpha(40)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.4,
                    Name = "TCP",
                },
                new LineSeries<DateTimePoint>
                {
                    Values = _pointUdp,
                    Fill = new SolidColorPaint(colorUdp.WithAlpha(40)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.4,
                    Name = "UDP",
                }
            };

            _dateAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter);

            XAxes = new Axis[] { _dateAxis };
            YAxes = new Axis[] { new Axis { MinLimit = 0 } };
            InvisibleX = new[] { new Axis { IsVisible = false } };
            InvisibleY = new[] { new Axis { IsVisible = false, MinLimit = 0 } };

            Thumbs = new[]
{
            new RectangularSection
            {
                Fill = new SolidColorPaint(new SKColor(255, 94, 94, 60))
            }
        };

            _ = UpdateGraph();
        }

        private async Task UpdateGraph()
        {
            while(IsReading)
            {
                await Task.Delay(1000);
                lock(Sync)
                {
                    //_point.Clear();
                    Items = PacketStore.Instance.StoredPacket;
                    var sItem = Items.Where(x => x.CpProtocol != PacketDotNet.ProtocolType.IPv6HopByHopOptions).ToList();
                    var Test = sItem.OrderBy(x => x.CpDate).GroupBy(x => x.CpDate).ToList();
                    foreach (var item in Test)
                    {
                        if (!_pointAll.Any(x => x.DateTime == item.Key))
                        {
                            int size = item.Sum(x => x.CpSize);
                            _pointAll.Add(new DateTimePoint(item.Key, size));
                        }
                        if (!_pointTcp.Any(x => x.DateTime == item.Key))
                        {
                            int size = item.Where(x => x.CpProtocol == PacketDotNet.ProtocolType.Tcp).Sum(x => x.CpSize);
                            _pointTcp.Add(new DateTimePoint(item.Key, size));
                        }
                        if (!_pointUdp.Any(x => x.DateTime == item.Key))
                        {
                            int size = item.Where(x => x.CpProtocol == PacketDotNet.ProtocolType.Udp).Sum(x => x.CpSize);
                            _pointUdp.Add(new DateTimePoint(item.Key, size));
                        }

                        //var test2 = item.GroupBy(x => x.CpFrom).ToList();
                        //foreach(var test in test2)
                        //{
                        //    int size = test.Sum(x => x.CpSize);
                        //}

                    }
                }
            }
        }

        private static string Formatter(DateTime date)
        {
            return $"{date.TimeOfDay}";
        }

        [RelayCommand]
        public void ToggleSeries0()
        {
            Series[0].IsVisible = !Series[0].IsVisible;
            ScrollSeries[0].IsVisible = !ScrollSeries[0].IsVisible;
        }

        [RelayCommand]
        public void ToggleSeries1()
        {
            Series[1].IsVisible = !Series[1].IsVisible;
            ScrollSeries[1].IsVisible = !ScrollSeries[1].IsVisible;
        }

        [RelayCommand]
        public void ToggleSeries2()
        {
            Series[2].IsVisible = !Series[2].IsVisible;
            ScrollSeries[2].IsVisible = !ScrollSeries[2].IsVisible;
        }

        [RelayCommand]
        public void ToggleSeries3()
        {
            if (Series[3] != null)
            {
                Series.Skip(3).ToList().ForEach(s => s.IsVisible = !s.IsVisible);
                ScrollSeries.Skip(3).ToList().ForEach(s => s.IsVisible = !s.IsVisible);
            }
        }

        [RelayCommand]
        public void ChartUpdated(ChartCommandArgs args)
        {
            var cartesianChart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;

            var x = cartesianChart.XAxes.First();
            var y = cartesianChart.YAxes.First();
            // update the scroll bar thumb when the chart is updated (zoom/pan)
            // this will let the user know the current visible range
            var thumb = Thumbs[0];

            thumb.Xi = x.MinLimit;
            thumb.Xj = x.MaxLimit;
            thumb.Yi = y.MinLimit;
            thumb.Yj = y.MaxLimit;
        }

        [RelayCommand]
        public void PointerDown(PointerCommandArgs args)
        {
            MouseDown = true;
        }

        [RelayCommand]
        public void PointerMove(PointerCommandArgs args)
        {
            if (!MouseDown) return;

            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            var positionInData = chart.ScalePixelsToData(args.PointerPosition);

            var thumb = Thumbs[0];
            var currentRange = thumb.Xj - thumb.Xi;

            // update the scroll bar thumb when the user is dragging the chart
            thumb.Xi = positionInData.X - currentRange / 2;
            thumb.Xj = positionInData.X + currentRange / 2;

            // update the chart visible range
            XAxes[0].MinLimit = thumb.Xi;
            XAxes[0].MaxLimit = thumb.Xj;
            YAxes[0].MinLimit = thumb.Yi;
            YAxes[0].MaxLimit = thumb.Yj;
        }

        [RelayCommand]
        public void PointerUp(PointerCommandArgs args)
        {
            MouseDown = false;
        }
    }
}
