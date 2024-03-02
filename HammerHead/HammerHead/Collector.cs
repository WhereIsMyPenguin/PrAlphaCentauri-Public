using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Controls;
using System.Net.NetworkInformation;
using System.Windows;
using System.Security.AccessControl;
using System.Drawing;
using static HammerHead.Collector;
using Microsoft.Win32;

namespace HammerHead
{
    class Collector
    {
        private static readonly Collector instance = new Collector();
        private List<RawCapture> QueuedPacket = new List<RawCapture>();
        private List<RawCapture> Save = new List<RawCapture>();
        private static ICaptureDevice device;
        private object QueueLock = new object();
        private static System.Threading.Thread backgroundThread;
        private static bool backgroundThreadStop;
        private static PacketArrivalEventHandler arrivedEventHandler;
        private static CaptureStoppedEventHandler captureStoppedEventHandler;
        private int packetCount;
        public string IPAddress { get; set; } = string.Empty;
        private ICaptureStatistics captureStats;
        public static Collector Instance
        {
            get { return instance; }
        }
        private Queue<PacketHolder> packetStrings;

        public void StartCollecting(CancellationToken Token, int DeviceID)
        {
            while(!Token.IsCancellationRequested)
            {
                packetCount = 0;
                device = CaptureDeviceList.Instance[DeviceID];
                packetStrings = new Queue<PacketHolder>();
                Save = new List<RawCapture>();
                backgroundThreadStop = false;
                backgroundThread = new Thread(Threader);
                backgroundThread.IsBackground = true;
                backgroundThread.Start();
                arrivedEventHandler = new PacketArrivalEventHandler(OnPacketArrive);
                captureStoppedEventHandler = new CaptureStoppedEventHandler(StopCapture);

                device.OnPacketArrival += arrivedEventHandler;
                device.OnCaptureStopped += captureStoppedEventHandler;

                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x.Description == device.Description);
                string IPaddress = "";
                if(ni != null)
                {
                    var ipaddr = ni.GetIPProperties().UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    if (ipaddr != null)
                    { IPaddress = ipaddr.Address.ToString(); IPAddress = IPaddress; }
                    else
                    {
                        ShutIt();
                        MessageBox.Show("???", "IP アドレスが見つからない??", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                }
                else
                {
                    ShutIt();
                    MessageBox.Show("???","IP アドレスが見つからない??",MessageBoxButton.OK,MessageBoxImage.Error);
                    break;
                }
                Application.Current.Dispatcher.Invoke(new Action(() =>
                    MainWindow.Instance.CaptureIP.Text = IPaddress
                ));
                int readTimeoutMilliseconds = 1000;
                device.Open(DeviceModes.Promiscuous, readTimeoutMilliseconds);
                string Filter = "host " + IPaddress + " and not ip6[6] & 0x0F != 0";
                device.Filter = Filter;
                device.Open();
                device.Capture();
            }
        }

        public void OnStatisticsChanged (object sender, string e)
        {
            if(MainWindow.Instance != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                    MainWindow.Instance.StatusText.Text = e
                ));
            }
        }

        private void Threader()
        {
            while (!backgroundThreadStop)
            {
                bool shouldSleep = true;
                lock (QueueLock)
                {
                    if (QueuedPacket.Count != 0)
                    {
                        shouldSleep = false;
                    }
                }
                if (shouldSleep) { Thread.Sleep(250); }
                else
                {
                    List<RawCapture> captures;
                    lock (QueueLock)
                    {
                        captures = QueuedPacket;
                        QueuedPacket = new List<RawCapture>();
                    }
                    foreach(var capture in captures) 
                    {
                        Packet pack = Packet.ParsePacket(capture.LinkLayerType, capture.Data);
                        var Holder = new PacketHolder();
                        if (pack is EthernetPacket ethernetPacket && ethernetPacket.PayloadPacket is IPPacket ipPacket)
                        {
                            DateTime currentDateTime = DateTime.Now;
                            DateTime dateTimeWithoutMilliseconds = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second);

                            if (ipPacket is PacketDotNet.IPv6Packet)
                            {
                                return;
                            }
                            if (ipPacket.PayloadPacket is TcpPacket tcpPacket)
                            {
                                Holder.CpFrom = ipPacket.SourceAddress;
                                Holder.CpFromPort = tcpPacket.SourcePort;
                                Holder.CpTo = ipPacket.DestinationAddress;
                                Holder.CpToPort = tcpPacket.DestinationPort;
                                Holder.CpProtocol = ipPacket.Protocol;
                                Holder.CpAppLayerProtocol = AppLayerConvertTCP(tcpPacket.SourcePort, tcpPacket.DestinationPort);
                                Holder.CpDate = dateTimeWithoutMilliseconds;
                                Holder.CpSize = capture.Data.Length;
                                string AppLayer = EncodeTCP(tcpPacket.SourcePort, tcpPacket.DestinationPort, tcpPacket.PayloadData);
                                Holder.CpData = pack.ToString(StringOutputType.VerboseColored) + "\n" + AppLayer;
                            }
                            else if(ipPacket.PayloadPacket is UdpPacket udpPacket)
                            {
                                Holder.CpFrom = ipPacket.SourceAddress;
                                Holder.CpFromPort = udpPacket.SourcePort;
                                Holder.CpTo = ipPacket.DestinationAddress;
                                Holder.CpToPort = udpPacket.DestinationPort;
                                Holder.CpProtocol = ipPacket.Protocol;
                                Holder.CpDate = dateTimeWithoutMilliseconds;
                                Holder.CpSize = capture.Data.Length;
                                Holder.CpData = pack.ToString(StringOutputType.VerboseColored);
                            }
                        }
                        if(Holder != null)
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>

                            packetStrings.Enqueue(Holder)
                            ));
                        }
                        packetCount++;
                    }
                    string n = string.Format("パケット : {0}, パケットロスト : {1}, NICパケットロスト : {2}",
                                 captureStats.ReceivedPackets,
                                 captureStats.DroppedPackets,
                                 captureStats.InterfaceDroppedPackets);
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                           {
                               PacketStore.Instance.StorePacket(packetStrings);
                           MainWindow.Instance.StatusText.Text = n; 
                    }));
                }
            }
        }

        private string AppLayerConvertTCP(ushort SourcePort,ushort DestinationPort)
        {
            if (Enum.IsDefined(typeof(AppLayerProtocolTCP), SourcePort.ToString()))
                return Enum.GetName(typeof(AppLayerProtocolTCP), SourcePort);
            else
                return Enum.GetName(typeof(AppLayerProtocolTCP), DestinationPort);
        }
        private string EncodeTCP(ushort SourcePort,ushort DestinationPort, byte[] data)
        {
            if (SourcePort == 443 || DestinationPort == 443)
            {
                if(data.Length >= 5 && data[0] == 0x16) //handshake
                {
                    int length = (data[3] << 8) + data[4];
                    if(data.Length >= 5 + length)
                    {
                        string handshakeType = "";
                        //int handshake = data[5];
                        int handshake = data[data[0] == 22 ? 5 : 0];
                        int versionMajor = data[9];
                        int versionMinor = data[10];
                        switch(handshake)
                        {
                            case 0x01: handshakeType = "ClientHello";
                                break;
                            case 0x02: handshakeType = "ServerHello";
                                break;
                            case 0x0b: handshakeType = "Certificate";
                                break;
                            default: handshakeType = "Unknown";
                                break;
                        }
                        return string.Format("TLS version : {0}.{1}\nHandShake Type : {2}", versionMajor, versionMinor, handshakeType);
                    }
                }
                return null;
            }
            else
                return null;
            
        }
        private void OnPacketArrive(object sender, PacketCapture e)
        {
            captureStats = e.Device.Statistics;

            Save.Add(e.GetPacket());
            lock (QueueLock)
            {
                QueuedPacket.Add(e.GetPacket());
            }
           
        }

        public void StopCapture(object sender, CaptureStoppedEventStatus status)
        {
            if(status != CaptureStoppedEventStatus.CompletedWithoutError)
            {
                MessageBox.Show("キャプチャ停止中にエラーが発生しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public bool ShutIt()
        {
            if(device != null)
            {
                device.StopCapture();
                device.Close();
                device.OnPacketArrival -= arrivedEventHandler;
                device.OnCaptureStopped -= captureStoppedEventHandler;
                device = null;
                backgroundThreadStop = true;
                backgroundThread.Join();
                SaveIt();
            }
            return true;
        }

        public void SaveIt()
        {
            if(Save.Count > 0)
            {
                var savedialog = new SaveFileDialog
                {
                    Filter = "PCAP Files (*.pcap)|*.pcap",
                    Title = "保存しますか？"
                };
                if (savedialog.ShowDialog() == true)
                {
                    var dumpFile = savedialog.FileName;
                    using (var Dumper = new CaptureFileWriterDevice(dumpFile, System.IO.FileMode.Create))
                    {
                        Dumper.Open();
                        foreach (var packet in Save)
                        {
                            Dumper.Write(packet);
                        }
                        Dumper.Close();
                    };
                }
            }
        }
    }
}
