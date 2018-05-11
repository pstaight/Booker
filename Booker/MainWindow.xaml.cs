using System;
using System.Collections.Generic;
using System.ComponentModel;
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

using System.Windows.Threading;

namespace Booker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string folder = @"C:\Users\Public\Booker\";
        private List<SchedItem> shows = new List<SchedItem>();
        private DispatcherTimer timer = new DispatcherTimer();
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
            if (System.IO.File.Exists(fileName))
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);
                foreach (var l in lines)
                {
                    var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                    if (d.Date == value.Date)
                    {
                        var dat = l.Split(',');
                        shows.Add(new SchedItem() {DTShowTime=d,TotalSeats=int.Parse(dat[1]),Tickets=new List<TicketItem>() });
                    }
                }
                shows.Sort();
            }
            else
            {
                if (System.IO.File.Exists(folder + "default.csv"))
                {
                    char[] d = { 'U', 'M', 'T', 'W', 'H','F', 'S' };
                    char mark = d[(int)value.DayOfWeek];
                    string[] lines = System.IO.File.ReadAllLines(folder + "default.csv");
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
                                shows.Add(new SchedItem() { DTShowTime = s, TotalSeats = seats, Tickets = new List<TicketItem>() });
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
                    System.IO.File.WriteAllLines(folder + "default.csv", lines);
                    var s = DateTime.Today.AddHours(9);
                    var e = DateTime.Today.AddHours(17);
                    while (s < e)
                    {
                        shows.Add(new SchedItem() { DTShowTime = s, TotalSeats=12, Tickets = new List<TicketItem>() });
                        s = s.AddMinutes(15);
                    }
                }
            }
            fileName = folder + "ticket" + value.Year.ToString() + value.Month.ToString("00") + value.Day.ToString("00") + "-000.csv";
            if (System.IO.File.Exists(fileName))
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);
                foreach (var l in lines)
                {
                    var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                    if(d.Date == value.Date)
                    {
                        var dat = l.Split(',');
                        var sh = shows.Find(x => x.DTShowTime.Equals(d));
                        if(sh == null)
                        {
                            int i = d.Hour;
                            string st = (i <= 12 ? i.ToString("00") : (i - 12).ToString("00")) + ":" + d.Minute.ToString("00") + (i < 12 ? " AM" : " PM");
                            sh=new SchedItem() { DTShowTime=d, TotalSeats=0, Tickets = new List<TicketItem>() };
                            shows.Add(sh);
                        }
                        string b = dat[3];
                        if (b.Length >1 && (b[b.Length - 1] == '"' || b[b.Length - 1] == '\xFFFD') && (b[0] == '"' || b[0]=='\xFFFD')) b = b.Substring(1, b.Length - 2);
                        sh.Tickets.Add(new TicketItem() {NumTickets=int.Parse(dat[1]),SaleType=dat[2][0],BuyerName=b,Phone=dat[4] });
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

        private void LoadShow(int s)
        {
            if (shows.ElementAtOrDefault(s) == null)
            {
                lShowtime.Content = "No Show Time Selected";
                return;
            }
            lShowtime.Content = "Tickets For " + shows[s].ShowTime;
            icTicket.ItemsSource = shows[s].Tickets;
        }

        private void LbSched_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var i = (sender as ListBox).SelectedIndex;
            if (i >= 0) LoadShow(i);
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
    }

    public class TicketItem
    {
        public int NumTickets { get; set; }
        public char SaleType { get; set; }
        public string Phone { get; set; }
        public string BuyerName { get; set; }
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
        public List<TicketItem> Tickets;

        public event PropertyChangedEventHandler PropertyChanged;
        public void UpSeats() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Seats"));
        public void UpColor() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
        public int CompareTo(SchedItem b)
        {
            return DTShowTime.CompareTo(b.DTShowTime);
        }
    }
}
