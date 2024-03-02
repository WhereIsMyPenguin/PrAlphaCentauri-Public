using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PacketDotNet;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using SharpPcap;
using System.Net;

namespace HammerHead
{
    public class PacketHolder
    {
        public DateTime CpDate {  get; set; }
        public IPAddress CpFrom { get; set; }
        public ushort CpFromPort { get; set; }
        public IPAddress CpTo { get; set; }
        public ushort CpToPort { get; set; }
        public ProtocolType CpProtocol { get; set; }
        public string CpAppLayerProtocol { get; set; }
        public int CpSize { get; set; }
        public string CpData { get; set; }

    }
    public class PacketStore
    {
        private static readonly PacketStore instance = new PacketStore();
        private Queue<PacketHolder> pckQueue;
        private ObservableCollection<PacketHolder> pckList;

        private PacketStore()
        {
            pckQueue = new Queue<PacketHolder>();
            pckList = new ObservableCollection<PacketHolder>();
        }
        public ObservableCollection<PacketHolder> StoredPacket
        { get { return pckList; } }
        public void StorePacket(Queue<PacketHolder> list)
        {
            while(list.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    pckList.Insert(0, list.Dequeue());
                    if (MainWindow.Instance.TabScreen.Source.ToString() == "ListTab.xaml")
                    {
                        ListTab.Instance.updateView();
                    }
                    else if (MainWindow.Instance.TabScreen.Source.ToString() == "GraphTab.xaml")
                    {
                        //GraphTab.Instance.updateView();
                    }
                    else if (MainWindow.Instance.TabScreen.Source.ToString() == "OverallTab.xaml")
                    {
                        //ViewModelOverall.Instance.UpdateOverall();
                    }
                }));
            }
        }
        public void ClearPacket()
        { pckList.Clear(); }
        public static PacketStore Instance
        { get { return instance; } }
    }
}
