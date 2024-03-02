using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;

namespace HammerHead
{
    public partial class ViewModelOverall : ObservableObject
    {
        private ObservableCollection<PacketHolder> Items = new ObservableCollection<PacketHolder>();
        public ObservableCollection<ISeries> BarSeries { get; set; }
        public ObservableCollection<ISeries> PieSeriesF { get; set; }
        public ObservableCollection<ISeries> PieSeriesFs { get; set; }
        public ObservableCollection<ISeries> PieSeriesT { get; set; }
        public ObservableCollection<ISeries> PieSeriesTs { get; set; }
        private ObservableCollection<double> _tcp = new();
        private ObservableCollection<double> _udp = new();
        private PieSeries<double> _pie = new();


        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        private Axis Xa { get; set; }
        private Axis Ya { get; set; }
        public ViewModelOverall()
        {
            _tcp = new() { 0 }; _udp = new() { 0 };
           BarSeries = new ObservableCollection<ISeries>
           { 
                new StackedRowSeries<double>
                {
                    Values = _tcp,
                    Name = "TCP",
                },
                new StackedRowSeries<double>
                {
                    Values = _udp,
                    Name = "UDP",
                }
           };
            PieSeriesF = new ObservableCollection<ISeries>();
            PieSeriesFs = new ObservableCollection<ISeries>();
            PieSeriesT = new ObservableCollection<ISeries>();
            PieSeriesTs = new ObservableCollection<ISeries>();

            Ya = new Axis
            {
                UnitWidth = -1,
                IsVisible = false,
            };
            Xa = new Axis
            {
                MinLimit = 0,
                TextSize = 12,
            };
            YAxes = new Axis[] { Ya };
            XAxes = new Axis[] { Xa };


            _ = UpdateOverall();
        }
        private object Sync { get; } = new object();

        private bool IsReading { get; set; } = true;

        public async Task UpdateOverall()
        {
            while (IsReading)
            {
                await Task.Delay(100);
                lock (Sync)
                {
                    Items = PacketStore.Instance.StoredPacket;
                    var sItems = Items.Where(x=>x.CpProtocol != PacketDotNet.ProtocolType.IPv6HopByHopOptions).ToList();
                    int tcp = Items.Where(x => x.CpProtocol == PacketDotNet.ProtocolType.Tcp).Count();
                    int udp = Items.Where(x => x.CpProtocol == PacketDotNet.ProtocolType.Udp).Count();
                    if (_tcp.Count() > 0) _tcp.RemoveAt(0);
                    if (_udp.Count() > 0) _udp.RemoveAt(0);
                    _tcp.Add(tcp); _udp.Add(udp);
                    Xa.MaxLimit = tcp + udp;
                    string ip = Collector.Instance.IPAddress;
                    var groupedIPFrom = sItems.GroupBy(x => x.CpFrom).ToList();
                    var groupedIPTo = sItems.GroupBy(x => x.CpTo).ToList();
                    foreach (var i in groupedIPFrom)
                    {
                        string IPad = i.Key.ToString();
                        int count = i.Count();
                        int Size = i.Sum(x => x.CpSize);
                        if(IPad != ip && !PieSeriesF.Any(x => x.Name == IPad))
                            PieSeriesF.Add(new PieSeries<double> { Values = new double[] { count }, Name = IPad, MaxRadialColumnWidth = 40, HoverPushout = 20 });
                        else if (PieSeriesF.Any(x => x.Name == IPad))
                            { var AA = PieSeriesF.First(x => x.Name == IPad); AA.Values = new double[] { count }; }
                        if (IPad != ip && !PieSeriesFs.Any(x => x.Name == IPad))
                            PieSeriesFs.Add(new PieSeries<double> { Values = new double[] { Size }, Name = IPad, MaxRadialColumnWidth = 60, HoverPushout = 10, ToolTipLabelFormatter = (chartPoint) => $"{chartPoint.AsDataLabel} Byte" });
                        else if (PieSeriesFs.Any(x => x.Name == IPad))
                            { var AA = PieSeriesFs.First(x => x.Name == IPad); AA.Values = new double[] { Size }; }
                    }
                    foreach (var i in groupedIPTo)
                    {
                        string IPad = i.Key.ToString();
                        int count = i.Count(); int Size = i.Sum(x => x.CpSize);
                        if (IPad != ip && !PieSeriesT.Any(x => x.Name == IPad))
                            PieSeriesT.Add(new PieSeries<double> { Values = new double[] { count }, Name = IPad, MaxRadialColumnWidth = 40, HoverPushout = 20 });
                        else if (PieSeriesT.Any(x => x.Name == IPad))
                        { var AA = PieSeriesT.First(x => x.Name == IPad); AA.Values = new double[] { count }; }
                        if (IPad != ip && !PieSeriesTs.Any(x => x.Name == IPad))
                            PieSeriesTs.Add(new PieSeries<double> { Values = new double[] { Size }, Name = IPad, MaxRadialColumnWidth = 60, HoverPushout = 10, ToolTipLabelFormatter = (chartPoint) => $"{chartPoint.AsDataLabel} Byte" });
                        else if (PieSeriesTs.Any(x => x.Name == IPad))
                        { var AA = PieSeriesTs.First(x => x.Name == IPad); AA.Values = new double[] { Size }; }
                    }
                }
                await Task.Delay(100);
            }
        }
    }
}
