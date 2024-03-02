using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using PacketDotNet;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using System.Net;
using System.IO.Pipes;
using System.Text.RegularExpressions;


namespace HammerHead
{
    /// <summary>
    /// Interaction logic for ListTab.xaml
    /// </summary>
    public partial class ListTab : UserControl
    {
        public static ListTab Instance { get { return instance; } }
        private static ListTab instance = new ListTab();
        ObservableCollection<PacketHolder> Items = new ObservableCollection<PacketHolder>();
        public ListTab()
        {
            InitializeComponent();
            DataContext = this;
            Items = PacketStore.Instance.StoredPacket;
            dataGridView.ItemsSource = Items;
        }
        public void BindData() { Items = PacketStore.Instance.StoredPacket; dataGridView.Items.Refresh(); }
        public void UnBindData() { Items = null; dataGridView.Items.Refresh(); }
        public void updateView()
        {

            dataGridView.Items.Refresh();
        }

        private void dataGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dataGridView.SelectedItem != null)
            {
                var selectedItem = dataGridView.SelectedItem as PacketHolder;
                if(selectedItem != null)
                {
                    Textbox1.Text = selectedItem.CpData;
                }
            }
        }

        private void Filter(object sender, RoutedEventArgs e)
        {
            Sieve();

            dataGridView.Items.Refresh();
        }
        private void Sieve()
        {
            try
            {
                int selectedProtocol = Fprotocol.SelectedIndex;
                IPAddress iPaddress = null;
                ushort port = 0;
                int selectedDestination = Fwhere.SelectedIndex;
                dataGridView.Items.Filter = (e) =>
                {
                    PacketHolder packetHolder = e as PacketHolder;
                    bool TCP = packetHolder.CpProtocol == ProtocolType.Tcp;
                    bool UDP = packetHolder.CpProtocol == ProtocolType.Udp;

                    bool IP = IPAddress.TryParse(Fipadd.Text.ToString(), out iPaddress);
                    bool From = packetHolder.CpFrom == iPaddress;
                    bool To = packetHolder.CpTo == iPaddress;
                    bool Port = ushort.TryParse(Fport.Text.ToString(), out port);
                    switch (selectedProtocol)
                    {
                        case 0:
                            {
                                switch(selectedDestination)
                                {
                                    case 0:
                                        {
                                            if (IP && Port) return (packetHolder.CpFrom?.ToString() == Fipadd.Text && packetHolder.CpFromPort.ToString() == Fport.Text) || (packetHolder.CpTo?.ToString() == Fipadd.Text && packetHolder.CpToPort.ToString() == Fport.Text);
                                            else if (IP && !Port) return (packetHolder.CpFrom?.ToString() == Fipadd.Text || packetHolder.CpTo?.ToString() == Fipadd.Text);
                                            else if (!IP && Port) return packetHolder.CpFromPort.ToString() == Fport.Text || packetHolder.CpToPort.ToString() == Fport.Text;
                                            else  return TCP || UDP;
                                        }
                                    case 1:
                                        {
                                            if (IP && Port) return packetHolder.CpFrom?.ToString() == Fipadd.Text && packetHolder.CpFromPort.ToString() == Fport.Text;
                                            else if (IP && !Port) return packetHolder.CpFrom?.ToString() == Fipadd.Text;
                                            else if (!IP && Port) return packetHolder.CpFromPort.ToString() == Fport.Text;
                                            else return TCP || UDP;
                                        }
                                    case 2:
                                        {
                                            if (IP && Port) return packetHolder.CpTo.ToString() == Fipadd.Text && packetHolder.CpToPort.ToString() == Fport.Text;
                                            else if (IP && !Port) return packetHolder.CpTo.ToString() == Fipadd.Text;
                                            else if (!IP && Port) return packetHolder.CpToPort.ToString() == Fport.Text;
                                            else return TCP || UDP;
                                        }
                                }
                                break;
                            }
                        case 1:
                            {
                                switch (selectedDestination)
                                {
                                    case 0:
                                        {
                                            if (IP && Port) return TCP && (packetHolder.CpFrom?.ToString() == Fipadd.Text && packetHolder.CpFromPort.ToString() == Fport.Text) || (packetHolder.CpTo?.ToString() == Fipadd.Text && packetHolder.CpToPort.ToString() == Fport.Text);
                                            else if (IP && !Port) return TCP && (packetHolder.CpFrom?.ToString() == Fipadd.Text || packetHolder.CpTo?.ToString() == Fipadd.Text);
                                            else if (!IP && Port) return TCP && (packetHolder.CpFromPort.ToString() == Fport.Text || packetHolder.CpToPort.ToString() == Fport.Text);
                                            else return TCP;
                                        }
                                    case 1:
                                        {
                                            if (IP && Port) return TCP && (packetHolder.CpFrom?.ToString() == Fipadd.Text && packetHolder.CpFromPort.ToString() == Fport.Text);
                                            else if (IP && !Port) return TCP && packetHolder.CpFrom?.ToString() == Fipadd.Text;
                                            else if (!IP && Port) return TCP && packetHolder.CpFromPort.ToString() == Fport.Text;
                                            else return TCP;
                                        }
                                    case 2:
                                        {
                                            if (IP && Port) return TCP && (packetHolder.CpTo.ToString() == Fipadd.Text && packetHolder.CpToPort.ToString() == Fport.Text);
                                            else if (IP && !Port) return TCP && packetHolder.CpTo.ToString() == Fipadd.Text;
                                            else if (!IP && Port) return TCP && packetHolder.CpToPort.ToString() == Fport.Text;
                                            else return TCP;
                                        }
                                }
                                break;
                            }
                        case 2:
                            {
                                switch (selectedDestination)
                                {
                                    case 0:
                                        {
                                            if (IP && Port) return UDP && (packetHolder.CpFrom?.ToString() == Fipadd.Text && packetHolder.CpFromPort.ToString() == Fport.Text) || (packetHolder.CpTo?.ToString() == Fipadd.Text && packetHolder.CpToPort.ToString() == Fport.Text);
                                            else if (IP && !Port) return UDP && (packetHolder.CpFrom?.ToString() == Fipadd.Text || packetHolder.CpTo?.ToString() == Fipadd.Text);
                                            else if (!IP && Port) return UDP && (packetHolder.CpFromPort.ToString() == Fport.Text || packetHolder.CpToPort.ToString() == Fport.Text);
                                            else return UDP;
                                        }
                                    case 1:
                                        {
                                            if (IP && Port) return UDP && (packetHolder.CpFrom?.ToString() == Fipadd.Text && packetHolder.CpFromPort.ToString() == Fport.Text);
                                            else if (IP && !Port) return UDP && packetHolder.CpFrom?.ToString() == Fipadd.Text;
                                            else if (!IP && Port) return UDP && packetHolder.CpFromPort.ToString() == Fport.Text;
                                            else return UDP;
                                        }
                                    case 2:
                                        {
                                            if (IP && Port) return UDP && (packetHolder.CpTo.ToString() == Fipadd.Text && packetHolder.CpToPort.ToString() == Fport.Text);
                                            else if (IP && !Port) return UDP && packetHolder.CpTo.ToString() == Fipadd.Text;
                                            else if (!IP && Port) return UDP && packetHolder.CpToPort.ToString() == Fport.Text;
                                            else return UDP;
                                        }
                                }
                                break;
                            }
                    }
                    return false;
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Fipadd_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
