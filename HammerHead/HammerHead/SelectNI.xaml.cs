using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HammerHead
{
    /// <summary>
    /// Interaction logic for SelectNI.xaml
    /// </summary>
    public partial class SelectNI : Window
    {
        public delegate void OnDeviceSelectedDelegate(int deviceId);
        public event OnDeviceSelectedDelegate OnDeviceSelected;
        public SelectNI()
        {
            InitializeComponent();
            GetDevices();
        }

        private void GetDevices()
        {
            foreach (var dev in CaptureDeviceList.Instance)
            {
                var str = String.Format("{0} {1}", dev.Name, dev.Description);
                DeviceList.Items.Add(str);
            }
        }


        private void DeviceSelected(object sender, RoutedEventArgs e)
        {
            if (OnDeviceSelected != null && DeviceList.SelectedItem != null)
            {
                OnDeviceSelected(DeviceList.SelectedIndex);
            }
        }

        private void DeviceSelected(object sender, MouseButtonEventArgs e)
        {
            if (OnDeviceSelected != null && DeviceList.SelectedItem != null)
            {
                OnDeviceSelected(DeviceList.SelectedIndex);
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
