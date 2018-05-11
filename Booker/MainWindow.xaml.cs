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

namespace Booker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>schedItem
    public partial class MainWindow : Window
    {
        private const string folder = @"C:\Users\Public\Booker\";
        private List<SchedItem> shows = new List<SchedItem>();
        public MainWindow()
        {
            InitializeComponent();
            System.IO.Directory.CreateDirectory(folder);
            LoadDay(DateTime.Today);
            LoadShow(0);
            var tickets = new List<TicketItem>();
            tickets.Add(new TicketItem() { NumTickets = 5, SaleType = 'P', Phone = "(619) 361-1024", BuyerName = "Patrick Staight" });
            tickets.Add(new TicketItem() { NumTickets = 2, SaleType = 'F', Phone = "(619) 555-1024", BuyerName = "John Smith" });
            tickets.Add(new TicketItem() { NumTickets = 2, SaleType = 'P', Phone = "", BuyerName = "Mary Sue" });
            tickets.Add(new TicketItem() { NumTickets = 1, SaleType = 'D', Phone = "(619) 361-1024", BuyerName = "Starlord" });
            tickets.Add(new TicketItem() { NumTickets = 2, SaleType = 'P', Phone = "", BuyerName = "Peater Parker" });
            icTicket.ItemsSource = tickets;
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
                if (date.Value.Equals(DateTime.Today))
                {
                    Lwarn.Visibility = Visibility.Hidden;
                }
                else
                {
                    Lwarn.Visibility = Visibility.Visible;
                }
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
                shows.Add(new SchedItem() { ShowTime = "09:00", Seats = 12, Color = "gray" });
                shows.Add(new SchedItem() { ShowTime = "09:15", Seats = 12, Color = "gray" });
                shows.Add(new SchedItem() { ShowTime = "09:30", Seats = 12, Color = "gray" });
                shows.Add(new SchedItem() { ShowTime = "09:45", Seats = 12, Color = "gray" });
                for (var i = 10; i < 17; ++i)
                {
                    shows.Add(new SchedItem() { ShowTime = i + ":00", Seats = 12, Color = i < 11 ? "gray" : "" });
                    shows.Add(new SchedItem() { ShowTime = i + ":15", Seats = 12, Color = i < 11 ? "gray" : "" });
                    shows.Add(new SchedItem() { ShowTime = i + ":30", Seats = 12, Color = i < 11 ? "gray" : "" });
                    shows.Add(new SchedItem() { ShowTime = i + ":45", Seats = 12, Color = i < 11 ? "gray" : "" });
                }
            }
            else
            {
                if (System.IO.File.Exists(folder + "default.csv"))
                {
                    char[] d = { 'U', 'M', 'T', 'W', 'H', 'S' };
                    char mark = d[(int)DateTime.Now.DayOfWeek];
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
                                string st = (s.Hour <= 12 ? s.Hour.ToString("00") : (s.Hour - 12).ToString("00")) + ":" + s.Minute.ToString("00") + (s.Hour < 12 ? " AM" : " PM");
                                string c = s < DateTime.Now ? "gray" : "";
                                shows.Add(new SchedItem() { ShowTime = st, Seats = seats, Color = c, Tickets = new List<TicketItem>() });
                                s = s.AddMinutes(length);
                            }
                        }
                    shows.Sort();
                }
                else
                {
                    string[] lines = { "M,0900,1700,12,15", "T,0900,1700,12,15", "W,0900,1700,12,15", "H,0900,1700,12,15", "F,0900,1700,12,15", "S,0900,1700,12,15", "U,0900,1700,12,15" };
                    System.IO.File.WriteAllLines(folder + "default.csv", lines);
                    for (int i = 9; i < 17; ++i)
                    {
                        for (var j = 0; j < 60; j += 15)
                        {
                            string st = (i <= 12 ? i.ToString("00") : (i - 12).ToString("00")) + ":" + j.ToString("00") + (i < 12 ? " AM" : " PM");
                            string c = DateTime.Now.Hour == i ? DateTime.Now.Minute > j ? "gray" : "" : DateTime.Now.Hour > i ? "gray" : "";
                            shows.Add(new SchedItem() { ShowTime = st, Seats = 12, Color = c, Tickets = new List<TicketItem>() });
                        }
                    }
                }
            }
            lbSched.ItemsSource = shows;
        }

        private void LoadShow(int s)
        {
            if (shows[s] == null) return;
            lShowtime.Content = "Tickets For " + shows[s].ShowTime;
        }

        private void LbSched_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var i = (sender as ListBox).SelectedIndex;
            if (i >= 0) LoadShow(i);
        }
    }

    public class TicketItem
    {
        public int NumTickets { get; set; }
        public char SaleType { get; set; }
        public string Phone { get; set; }
        public string BuyerName { get; set; }
    }

    public class SchedItem : IComparable<SchedItem>
    {
        public string ShowTime { get; set; }
        public int Seats { get; set; }
        public string Color { get; set; }
        public List<TicketItem> Tickets;
        public int CompareTo(SchedItem b)
        {
            if (b == null) return 1;
            int H1 = int.Parse(ShowTime.Substring(0, 2));
            if (H1 == 12) H1 += ShowTime.Substring(5, 3).Equals(" PM") ? 0 : -12;
            else H1 += ShowTime.Substring(5, 3).Equals(" PM") ? 12 : 0;
            int H2 = int.Parse(b.ShowTime.Substring(0, 2));
            if (H2 == 12) H2 += b.ShowTime.Substring(5, 3).Equals(" PM") ? 0 : -12;
            else H2 += b.ShowTime.Substring(5, 3).Equals(" PM") ? 12 : 0;
            if (H1 == H2)
            {
                return int.Parse(ShowTime.Substring(3, 2)) - int.Parse(b.ShowTime.Substring(3, 2));
            }
            else
            {
                return H1 - H2;
            }
        }
    }
}
