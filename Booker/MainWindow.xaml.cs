using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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

using System.Windows.Threading;
using Renci.SshNet;

namespace Booker
{
    public static class FilePusher
    {
        public const string folder = @"C:\Users\Public\Booker\";
        public static void RemoveTicket(TicketItem item,DateTime itemTime)
        {
            var lines  = File.ReadAllLines(folder + "ticket" + itemTime.ToString("yyyyMMdd") + "-000.csv");
            var ticks = new List<TicketItem>();
            foreach(var l in lines)
            {
                var dat = l.Split(',');
                var st = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                string b = dat[3];
                if (b.Length > 1 && (b[b.Length - 1] == '"' || b[b.Length - 1] == '\xFFFD') && (b[0] == '"' || b[0] == '\xFFFD')) b = b.Substring(1, b.Length - 2);
                var t = new TicketItem() { ShowTime = st, NumTickets = int.Parse(dat[1]), SaleType = dat[2][0], BuyerName = b, Phone = dat[4] };
                if (!(item.ShowTime.Equals(t.ShowTime) && item.NumTickets == t.NumTickets && item.SaleType == t.SaleType && item.BuyerName.Equals(t.BuyerName) && item.Phone.Equals(t.Phone))){
                    ticks.Add(t);
                }
            }
            ticks.Sort();
            var outlines = new List<string>();
            foreach(var t in ticks)
            {
                outlines.Add(t.ShowTime.ToString("yyyy-MM-dd HHmm") + "," + t.NumTickets.ToString() + "," + t.SaleType + ",\"" + t.BuyerName + "\"," + t.Phone);
            }
            File.WriteAllLines(folder + "ticket" + itemTime.ToString("yyyyMMdd") + "-000.csv", outlines);
        }

        public static void AddTicket(TicketItem t, DateTime dTShowTime)
        {
            List<string> lines = new List<string>
            {
                dTShowTime.ToString("yyyy-MM-dd HHmm") + "," + t.NumTickets.ToString() + "," + t.SaleType + ",\"" + t.BuyerName + "\"," + t.Phone
            };
            File.AppendAllLines(folder + "ticket" + dTShowTime.ToString("yyyyMMdd") + "-000.csv", lines);
        }
        public static List<SchedItem> ReadShows(DateTime value)
        {
            var shows = new List<SchedItem>();
            var fileName = folder + "sched" + value.Year.ToString() + value.Month.ToString("00") + value.Day.ToString("00") + "-000.csv";
            if (File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);
                foreach (var l in lines)
                {
                    var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                    if (d.Date == value.Date)
                    {
                        var dat = l.Split(',');
                        shows.Add(new SchedItem() { DTShowTime = d, TotalSeats = int.Parse(dat[1]), Tickets = new ObservableCollection<TicketItem>() });
                    }
                }
                shows.Sort();
            }
            else
            {
                if (File.Exists(folder + "default.csv"))
                {
                    char[] d = { 'U', 'M', 'T', 'W', 'H', 'F', 'S' };
                    char mark = d[(int)value.DayOfWeek];
                    string[] lines = File.ReadAllLines(folder + "default.csv");
                    foreach (var l in lines) if (l[0] == mark)
                        {
                            var dat = l.Split(',');
                            if (!int.TryParse(dat[1], out int start)) continue;
                            if (!int.TryParse(dat[2], out int end)) continue;
                            if (!int.TryParse(dat[3], out int seats)) continue;
                            if (!int.TryParse(dat[4], out int length)) continue;
                            var s = value.AddHours(start / 100).AddMinutes(start % 100);
                            var e = value.AddHours(end / 100).AddMinutes(end % 100);
                            while (s < e)
                            {
                                var sh = shows.Find(x => x.DTShowTime.Equals(s));
                                if (sh == null)
                                {
                                    shows.Add(new SchedItem() { DTShowTime = s, TotalSeats = seats, Tickets = new ObservableCollection<TicketItem>() });
                                }
                                else
                                {
                                    if (seats < sh.TotalSeats)
                                    {
                                        sh.TotalSeats = seats;
                                    }
                                }
                                s = s.AddMinutes(length);
                            }
                        }
                    shows.Sort();
                }
                else
                {
                    string[] lines = { "M,0900,1700,12,15", "T,0900,1700,12,15", "W,0900,1700,12,15", "H,0900,1700,12,15", "F,0900,1700,12,15", "S,0900,1700,12,15", "U,0900,1700,12,15" };
                    File.WriteAllLines(folder + "default.csv", lines);
                    var s = DateTime.Today.AddHours(9);
                    var e = DateTime.Today.AddHours(17);
                    while (s < e)
                    {
                        shows.Add(new SchedItem() { DTShowTime = s, TotalSeats = 12, Tickets = new ObservableCollection<TicketItem>() });
                        s = s.AddMinutes(15);
                    }
                }
            }
            fileName = folder + "ticket" + value.Year.ToString() + value.Month.ToString("00") + value.Day.ToString("00") + "-000.csv";
            if (File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);
                foreach (var l in lines)
                {
                    var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                    if (d.Date == value.Date)
                    {
                        var dat = l.Split(',');
                        var sh = shows.Find(x => x.DTShowTime.Equals(d));
                        if (sh == null)
                        {
                            sh = new SchedItem() { DTShowTime = d, TotalSeats = 0, Tickets = new ObservableCollection<TicketItem>() };
                            shows.Add(sh);
                        }
                        string b = dat[3];
                        if (b.Length > 1 && (b[b.Length - 1] == '"' || b[b.Length - 1] == '\xFFFD') && (b[0] == '"' || b[0] == '\xFFFD')) b = b.Substring(1, b.Length - 2);
                        sh.Tickets.Add(new TicketItem() { NumTickets = int.Parse(dat[1]), SaleType = dat[2][0], BuyerName = b, Phone = dat[4] });
                    }
                }
            }
            return shows;

        }
        public static void Push()
        {
            string uploadfn = "ticket" + DateTime.Today.ToString("yyyyMMdd") + "-000.csv";
            using (var client = new SftpClient("patricksapps.com", "client1", @"j8nVV.v{z""\3L#:K"))
            {
                client.Connect();
                using (var uplfileStream = File.OpenRead(@"C:\Users\Public\Booker\" + uploadfn))
                {
                    client.UploadFile(uplfileStream, uploadfn, true);
                }
                client.Disconnect();
            }
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string folder = @"C:\Users\Public\Booker\";
        private List<SchedItem> shows = new List<SchedItem>();
        private DispatcherTimer timer = new DispatcherTimer();
        private int CurrentShowDisplayed = 0;
        private AddTicket ticketWindow;
        private MoveTicket MoveWindow;

        public MainWindow()
        {
            InitializeComponent();
            System.IO.Directory.CreateDirectory(folder);
            timer.Tick += Timer_Tick;
            LoadDay(DateTime.Today);
        }

        private void DpShowDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker dt = sender as DatePicker;
            DateTime? date = dt.SelectedDate;
            if (ticketWindow != null)
            {
                ticketWindow.Close();
            }
            if (date == null)
            {
                LoadDay(DateTime.Today);
            }
            else
            {
                LoadDay(date.Value);
            }
            LoadShow(0);
        }

        private void LoadDay(DateTime value)
        {
            shows = new List<SchedItem>();
            var fileName = folder + "sched" + value.Year.ToString() + value.Month.ToString("00") + value.Day.ToString("00") + "-000.csv";
            if (File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);
                foreach (var l in lines)
                {
                    var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                    if (d.Date == value.Date)
                    {
                        var dat = l.Split(',');
                        shows.Add(new SchedItem() {DTShowTime=d,TotalSeats=int.Parse(dat[1]),Tickets=new ObservableCollection<TicketItem>() });
                    }
                }
                shows.Sort();
            }
            else
            {
                if (File.Exists(folder + "default.csv"))
                {
                    char[] d = { 'U', 'M', 'T', 'W', 'H','F', 'S' };
                    char mark = d[(int)value.DayOfWeek];
                    string[] lines = File.ReadAllLines(folder + "default.csv");
                    foreach (var l in lines) if (l[0] == mark)
                    {
                        var dat = l.Split(',');
                        if (!int.TryParse(dat[1], out int start)) continue;
                        if (!int.TryParse(dat[2], out int end)) continue;
                        if (!int.TryParse(dat[3], out int seats)) continue;
                        if (!int.TryParse(dat[4], out int length)) continue;
                        var s = value.AddHours(start / 100).AddMinutes(start % 100);
                        var e = value.AddHours(end / 100).AddMinutes(end % 100);
                        while (s < e)
                        {
                            var sh = shows.Find(x => x.DTShowTime.Equals(s));
                            if (sh == null)
                            {
                                shows.Add(new SchedItem() { DTShowTime = s, TotalSeats = seats, Tickets = new ObservableCollection<TicketItem>() });
                            }
                            else
                            {
                                if (seats < sh.TotalSeats)
                                {
                                    sh.TotalSeats = seats;
                                }
                            }
                            s = s.AddMinutes(length);
                        }
                    }
                    shows.Sort();
                }
                else
                {
                    string[] lines = { "M,0900,1700,12,15", "T,0900,1700,12,15", "W,0900,1700,12,15", "H,0900,1700,12,15", "F,0900,1700,12,15", "S,0900,1700,12,15", "U,0900,1700,12,15" };
                    File.WriteAllLines(folder + "default.csv", lines);
                    var s = DateTime.Today.AddHours(9);
                    var e = DateTime.Today.AddHours(17);
                    while (s < e)
                    {
                        shows.Add(new SchedItem() { DTShowTime = s, TotalSeats=12, Tickets = new ObservableCollection<TicketItem>() });
                        s = s.AddMinutes(15);
                    }
                }
            }
            fileName = folder + "ticket" + value.Year.ToString() + value.Month.ToString("00") + value.Day.ToString("00") + "-000.csv";
            if (File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);
                foreach (var l in lines)
                {
                    var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                    if(d.Date == value.Date)
                    {
                        var dat = l.Split(',');
                        var sh = shows.Find(x => x.DTShowTime.Equals(d));
                        if(sh == null)
                        {
                            sh=new SchedItem() { DTShowTime=d, TotalSeats=0, Tickets = new ObservableCollection<TicketItem>() };
                            shows.Add(sh);
                        }
                        string b = dat[3];
                        if (b.Length >1 && (b[b.Length - 1] == '"' || b[b.Length - 1] == '\xFFFD') && (b[0] == '"' || b[0]=='\xFFFD')) b = b.Substring(1, b.Length - 2);
                        sh.Tickets.Add(new TicketItem() {ShowTime=sh.DTShowTime,  NumTickets=int.Parse(dat[1]),SaleType=dat[2][0],BuyerName=b,Phone=dat[4] });
                    }
                }
            }
            lbSched.ItemsSource = shows;
            LoadShow(0);
            if (value.Date.Equals(DateTime.Today))
            {
                Lwarn.Visibility = Visibility.Hidden;
                int i=shows.FindIndex(x => x.DTShowTime > DateTime.Now);
                if(shows.ElementAtOrDefault(i) != null)
                {
                    timer.Interval = shows[i].DTShowTime - DateTime.Now;
                    timer.Start();
                }
            }
            else
            {
                Lwarn.Visibility = Visibility.Visible;
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
            }
        }

        private void MakeSched()
        {
            DateTime value = shows[CurrentShowDisplayed].DTShowTime;
            string schedfilename = folder + "sched" + value.Year.ToString("0000") + value.Month.ToString("00") + value.Day.ToString("00") + "-000.csv";
            if (!File.Exists(schedfilename))
            {
                string[] lines = new string[shows.Count];
                int i = 0;
                foreach(var s in shows)
                {
                    lines[i]=s.DTShowTime.ToString("yyyy-MM-dd HHmm")+","+s.TotalSeats.ToString("0");
                    ++i;
                }
                File.WriteAllLines(schedfilename, lines);
            }
        }

        private void LoadShow(int? s)
        {
            CurrentShowDisplayed = s?? CurrentShowDisplayed;
            if (shows.ElementAtOrDefault(CurrentShowDisplayed) == null)
            {
                lShowtime.Content = "No Show Time Selected";
                return;
            }
            lShowtime.Content = "Tickets For " + shows[CurrentShowDisplayed].ShowTime;
            icTicket.ItemsSource = shows[CurrentShowDisplayed].Tickets;
            shows[CurrentShowDisplayed].UpSeats();
        }

        private void LbSched_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var i = (sender as ListBox).SelectedIndex;
            if (i >= 0)
            {
                if (ticketWindow != null)
                {
                    ticketWindow.Close();
                }
                if(MoveWindow != null)
                {
                    MoveWindow.Close();
                }
                LoadShow(i);
            }
        }

        //Q: How will this work if the schedule crosses Midnight?
        //A: Don't worry about that. It's outside the scope of this project.
        void Timer_Tick(object sender, EventArgs e)
        {
            var i = shows.FindLastIndex(x => x.DTShowTime < DateTime.Now);
            if (shows.ElementAtOrDefault(i) == null)
            {
                timer.Stop();
            }
            else
            {
                shows[i].UpColor();
                if (shows.ElementAtOrDefault(i+1) == null)
                {
                    timer.Stop();
                }
                else
                {
                    timer.Interval = shows[i + 1].DTShowTime - DateTime.Now;
                }
            }
        }

        private void bAddTicket_Click(object sender, RoutedEventArgs e)
        {
            if(ticketWindow != null)
            {
                ticketWindow.Close();
            }
            if (shows[CurrentShowDisplayed].Seats < 1)
            {
                MessageBox.Show(shows[CurrentShowDisplayed].ShowTime+" show time is fully booked", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ticketWindow = new AddTicket(shows[CurrentShowDisplayed]);
                MakeSched();
                ticketWindow.Show();
            }
        }

        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            var sp = (sender as Button).Parent as StackPanel;
            var ti = new TicketItem() { NumTickets = int.Parse((sp.Children[2] as TextBlock).Text), SaleType = (sp.Children[3] as TextBlock).Text[0], Phone = (sp.Children[4] as TextBlock).Text, BuyerName = (sp.Children[5] as TextBlock).Text };
            var item = shows[CurrentShowDisplayed].Tickets.First(x => x.NumTickets == ti.NumTickets && x.SaleType == ti.SaleType && x.Phone.Equals(ti.Phone) && x.BuyerName.Equals(ti.BuyerName));
            if (MoveWindow != null)
            {
                MoveWindow.Close();
            }
            MoveWindow = new MoveTicket(item,this);
            MoveWindow.Show();
        }

        public void MoveT(TicketItem Old, TicketItem NewItem)
        {
            shows[CurrentShowDisplayed].Tickets.Remove(Old);
            shows[CurrentShowDisplayed].UpSeats();
            FilePusher.RemoveTicket(Old, shows[CurrentShowDisplayed].DTShowTime);
            FilePusher.AddTicket(NewItem, NewItem.ShowTime);
            if(shows[CurrentShowDisplayed].DTShowTime.Date == NewItem.ShowTime.Date)
            {
                shows.Find(x => x.DTShowTime == NewItem.ShowTime).Tickets.Add(NewItem);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var sp = (sender as Button).Parent as StackPanel;
            var ti = new TicketItem() { NumTickets = int.Parse((sp.Children[2] as TextBlock).Text), SaleType = (sp.Children[3] as TextBlock).Text[0], Phone = (sp.Children[4] as TextBlock).Text, BuyerName = (sp.Children[5] as TextBlock).Text };
            var item = shows[CurrentShowDisplayed].Tickets.First(x => x.NumTickets == ti.NumTickets && x.SaleType == ti.SaleType && x.Phone.Equals(ti.Phone) && x.BuyerName.Equals(ti.BuyerName));
            shows[CurrentShowDisplayed].Tickets.Remove(item);
            shows[CurrentShowDisplayed].UpSeats();
            FilePusher.RemoveTicket(item, shows[CurrentShowDisplayed].DTShowTime);
                //read in the ticket file, remove this record, sort it, write it back out, Remove this record from Tickets
        }
    }

    public class TicketItem : IComparable<TicketItem>
    {
        public DateTime ShowTime;
        public int NumTickets { get; set; }
        public char SaleType { get; set; }
        public string Phone { get; set; }
        public string BuyerName { get; set; }
        public int CompareTo(TicketItem b)
        {
            return ShowTime.CompareTo(b.ShowTime);
        }
    }

    public class SchedItem : INotifyPropertyChanged , IComparable<SchedItem>
    {
        public DateTime DTShowTime;
        public string ShowTime {
            get { int i=DTShowTime.Hour; return (i <= 12 ? i.ToString("00") : (i - 12).ToString("00")) + ":" + DTShowTime.Minute.ToString("00") + (i < 12 ? " AM" : " PM"); }
            }
        public int Seats { get { return TotalSeats - Tickets.Sum(x => x.NumTickets); } }
        public int TotalSeats;
        public string Color { get { return DTShowTime <= DateTime.Now ? "#aaaaaa" : ""; } set { UpColor(); } }
        public ObservableCollection<TicketItem> Tickets;

        public event PropertyChangedEventHandler PropertyChanged;
        public void UpSeats() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Seats"));
        public void UpColor() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
        public int CompareTo(SchedItem b)
        {
            return DTShowTime.CompareTo(b.DTShowTime);
        }
    }
}
