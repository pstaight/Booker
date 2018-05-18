using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace BookerAdmin
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class Report : Window
    {
        public List<ReportItem> items;
        public ReportItem T = new ReportItem();
        public Report()
        {
            InitializeComponent();
            LoadDay(DateTime.Today);
        }

        private void LoadDay(DateTime value)
        {
            T.Avalible = 0;
            T.Sold = 0;
            T.Full = 0;
            T.Discount = 0;
            T.Free = 0;
            T.SMS = 0;
            items = new List<ReportItem>();
            if(File.Exists(MainWindow.folder + "sched" + value.ToString("yyyyMMdd") + "-000.csv"))
            {
                var lines = File.ReadAllLines(MainWindow.folder + "sched" + value.ToString("yyyyMMdd") + "-000.csv");
                foreach (var l in lines)
                {
                    var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                    if (d.Date == value.Date)
                    {
                        var dat = l.Split(',');
                        int i = int.Parse(dat[1]);
                        T.Avalible += i;
                        items.Add(new ReportItem() { ShowTime = d, Avalible = i,Sold=0,Full=0,Discount=0,Free=0,SMS=0 });
                    }
                }
                if (File.Exists(MainWindow.folder + "ticket" + value.ToString("yyyyMMdd") + "-000.csv"))
                {
                    lines = File.ReadAllLines(MainWindow.folder + "ticket" + value.ToString("yyyyMMdd") + "-000.csv");
                    foreach (var l in lines)
                    {
                        var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                        if (d.Date == value.Date)
                        {
                            var dat = l.Split(',');
                            var s = items.Find(x => x.ShowTime == d);
                            var a = int.Parse(dat[1]);
                            s.Sold += a;
                            T.Sold += a;
                            switch (dat[2])
                            {
                                case "P": s.Full += a; T.Full += a; break;
                                case "D": s.Discount += a; T.Discount += a; break;
                                case "F": s.Free += a; T.Free += a; break;
                            }
                            if (dat[4] != "")
                            {
                                s.SMS++;
                                T.SMS++;
                            }
                        }
                    }
                }
            }
            ICRepot.ItemsSource = items;
            TBAva.Text = T.Avalible.ToString();
            TBSold.Text = T.Sold.ToString();
            TBFull.Text = T.Full.ToString();
            TBDis.Text = T.Discount.ToString();
            TBFree.Text = T.Free.ToString();
            TBSms.Text = T.SMS.ToString();
        }

        private void DPReport_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDay((sender as DatePicker).SelectedDate??DateTime.Today);
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                string[] lines = { "Avalible,Sold,Full,Discount,Free,SMS", T.Avalible.ToString()+","+T.Sold.ToString()+","+T.Full.ToString()+","+T.Discount.ToString()+","+T.Free.ToString()+","+T.SMS.ToString() };
                File.WriteAllLines(saveFileDialog.FileName, lines );
            }                
        }
    }
    public class ReportItem
    {
        public DateTime ShowTime { get; set; }
        public int Avalible { get; set; }
        public int Sold { get; set; }
        public int Full { get; set; }
        public int Discount { get; set; }
        public int Free { get; set; }
        public int SMS { get; set; }
    }
}
