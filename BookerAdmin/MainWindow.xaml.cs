using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BookerAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string folder = @"C:\Users\Public\Booker\";
        public ObservableCollection<RuleItem> R;
        private int LastID;
        public MainWindow()
        {
            InitializeComponent();
            ReadSecrets();
            ReadDefault();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            char[] d = { 'M', 'T', 'W', 'H', 'F', 'S', 'U' };
            char mark = d[CBDay.SelectedIndex];
            var clean = new System.Text.RegularExpressions.Regex(@"[^\d]");
            var start = clean.Replace(TBStart.Text, "");
            if (start.Length != 4) start = "0900";
            var end = clean.Replace(TBEnd.Text, "");
            if (end.Length != 4) end = "1700";
            var seats = clean.Replace(TBSeats.Text, "");
            if (seats == "") seats = "12";
            var len = clean.Replace(TBLength.Text, "");
            if (len == "") len = "15";
            R.Add(new RuleItem() { ID = LastID++, Rule = mark + "," + start + "," + end + "," + seats + "," + len });
        }

        private void ReadSecrets()
        {
            //string sid = "", token = "";
            if (File.Exists(folder + "admin.txt"))
            {
                int i;
                var lines = File.ReadAllLines(folder + "admin.txt");
                foreach (var l in lines)
                {
                    switch (l.Substring(0, i = l.IndexOf(':')))
                    {
                        case "SMS TEXT": TBMsg.Text = l.Substring(i + 1); break;
                        case "SMS SID": TBSid.Text = l.Substring(i + 1); break;
                        case "SMS TOKEN": TBToken.Text = l.Substring(i + 1); break;
                        case "SMS FROM": TBFrom.Text = l.Substring(i + 1); break;
                        case "SFTP SERVER": TBServer.Text = l.Substring(i + 1); break;
                        case "SFTP USER": TBUser.Text = l.Substring(i + 1); break;
                        case "SFTP PASS": TBPass.Text = l.Substring(i + 1); break;
                    }
                }
            }
        }

        private void ReadDefault()
        {
            if (File.Exists(folder + "default.csv"))
            {
                R = new ObservableCollection<RuleItem>();
                var lines = File.ReadAllLines(folder + "default.csv");
                for (LastID = 0; LastID < lines.Length; ++LastID)
                {
                    R.Add(new RuleItem() { ID = LastID, Rule = lines[LastID] });
                }
            }
            else
            {
                R = new ObservableCollection<RuleItem>() {
                    new RuleItem(){ID=0,Rule="M,0900,1700,12,15" },
                    new RuleItem(){ID=1,Rule="T,0900,1700,12,15" },
                    new RuleItem(){ID=2,Rule="W,0900,1700,12,15" },
                    new RuleItem(){ID=3,Rule="H,0900,1700,12,15" },
                    new RuleItem(){ID=4,Rule="F,0900,1700,12,15" },
                    new RuleItem(){ID=5,Rule="S,0900,1700,12,15" },
                    new RuleItem(){ID=6,Rule="U,0900,1700,12,15" } };
                LastID = 7;
            }
            ICRules.ItemsSource = R;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int tag = ((sender as Button).Tag as int?) ?? -1;
            if (tag == -1) return;
            R.Remove(R.First(x => x.ID == tag));
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = { "SMS TEXT:"+ TBMsg.Text,"SMS SID:"+ TBSid.Text,"SMS TOKEN:"+ TBToken.Text, "SMS FROM:"+TBFrom.Text, "SFTP SERVER:"+TBServer.Text, "SFTP USER:"+TBUser.Text, "SFTP PASS:"+TBPass.Text };
            File.WriteAllLines(folder + "admin.txt", lines);
            var l2 = new string[R.Count];
            int i = 0;
            foreach(var ri in R)
            {
                l2[i++] = ri.Rule;
            }
            File.WriteAllLines(folder + "default.csv", l2);
            Close();
        }

        private void BtnReport_Click(object sender, RoutedEventArgs e)
        {
            var a = new Report();
            a.Show();
        }
    }
    public class RuleItem
    {
        public int ID { get; set; }
        public string Rule { get; set; }
    }
}
