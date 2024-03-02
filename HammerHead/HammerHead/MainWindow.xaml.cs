using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using SharpPcap;
using PacketDotNet;
using System.Windows.Threading;


namespace HammerHead
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Threading.Thread CaptureThread;
        CancellationTokenSource cts = new CancellationTokenSource();

        public event EventHandler<string> StatisticsChanged;

        public static MainWindow Instance { get; private set; }
        
        public bool IsCapturing = false;

        private SelectNI selectni;
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            var Contt = Collector.Instance;
            InitializeStaticsHandler();
        }
        private void ScreenDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void InitializeStaticsHandler()
        {
            Collector collectorInstance = Collector.Instance;
            StatisticsChanged += collectorInstance.OnStatisticsChanged;

            OnStaticsChanged("Ready");
        }
        protected virtual void OnStaticsChanged(string text)
        {
            StatisticsChanged?.Invoke(this, text);
        }
        private void HomeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
        private void CloseThis(object sender, RoutedEventArgs e)
        {
            Collector.Instance.ShutIt();
            cts.Cancel();
            Close();
            
        }
        private void BL(object sender, EventArgs e)
        {
            BubblePop.Visibility = Visibility.Collapsed;
            BubblePop.IsOpen = false;
            PcapPop.Visibility = Visibility.Collapsed;
            PcapPop.IsOpen = false; 
        }
        private void LBE(object sender, MouseEventArgs e)
        {
            BubblePop.PlacementTarget = ListButton;
            //BubblePop.Placement = PlacementMode.Right;
            BubblePop.IsOpen = true;
            Header.BubbleText.Text = "リスト";
        }
        
        private void GBE(object sender, MouseEventArgs e)
        {
            BubblePop.PlacementTarget = GraphButton;
            //BubblePop.Placement = PlacementMode.Right;
            BubblePop.IsOpen = true;
            Header.BubbleText.Text = "グラフ";
        }
        private void OBE(object sender, MouseEventArgs e)
        {
            BubblePop.PlacementTarget = OverallButton;
            //BubblePop.Placement = PlacementMode.Right;
            BubblePop.IsOpen = true;
            Header.BubbleText.Text = "全体";
        }

        private void SBE(object sender, MouseEventArgs e)
        {
            BubblePop.PlacementTarget = SettingsButton;
            //BubblePop.Placement = PlacementMode.Right;
            BubblePop.IsOpen = true;
            Header.BubbleText.Text = "保存";
        }
        private void EBE(object sender, MouseEventArgs e)
        {
            BubblePop.PlacementTarget = ExitButton;
            //BubblePop.Placement = PlacementMode.Right;
            BubblePop.IsOpen = true;
            Header.BubbleText.Text = "終了";
        }
        private void pcapBE(object sender, EventArgs e)
        {
            PcapPop.PlacementTarget = StartCaptureButton;
            //PcapPop.Placement = PlacementMode.Right;
            PcapPop.IsOpen = true;
            if (!IsCapturing)
            { HeaderSP.BubbleTitle.Text = "キャプチャ開始"; HeaderSP.BubbleDescription.Text = "観測したいネットワークを選択してパケットをキャプチャします"; }
            else
            { HeaderSP.BubbleTitle.Text = "保存＆停止"; HeaderSP.BubbleDescription.Text = "観測を停止します。保存できますがしなくても大丈夫です。"; }
        }

        private void Launch(object sender, EventArgs e)
        {
            if(!IsCapturing)
            {
                selectni = new SelectNI();
                selectni.OnDeviceSelected += new SelectNI.OnDeviceSelectedDelegate(StartCollector);
                selectni.Height = 350;
                selectni.Width = 600;
                selectni.ShowDialog();
                PacketStore.Instance.ClearPacket();
            }
            else
            {
                cts.Cancel();
                Collector.Instance.ShutIt();
                ListTab.Instance.UnBindData();
                CaptureButton.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                HeaderSP.BubbleTitle.Text = "キャプチャ開始";
                HeaderSP.BubbleDescription.Text = "観測したいネットワークを選択してパケットをキャプチャします";
                IsCapturing = false;
            }
        }
        void StartCollector(int deviceID)
        {
            cts = new CancellationTokenSource();
            Task Fire = Task.Run(() => Collector.Instance.StartCollecting(cts.Token,deviceID));
            CaptureButton.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Stop;
            HeaderSP.BubbleTitle.Text = "停止";
            ListTab.Instance.BindData();
            IsCapturing = true;

            selectni.Close();
        }
        private void CallList(object sender, EventArgs e)
        {
            TabScreen.Navigate(new Uri("ListTab.xaml", UriKind.Relative));
        }

        private void CallGraph(object sender, EventArgs e)
        {
            TabScreen.Navigate(new Uri("GraphTab.xaml", UriKind.Relative));
        }
        private void CallOverall(object sender, EventArgs e)
        {
            TabScreen.Navigate(new Uri("OverallTab.xaml", UriKind.Relative));
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            Collector.Instance.SaveIt();
        }
    }
}
