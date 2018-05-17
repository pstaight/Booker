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

namespace Booker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private int CurrentShowDisplayed = 0;
        private AddTicket ticketWindow;
        private MoveTicket MoveWindow;

        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory(FilePusher.folder);
            FilePusher.init();
            timer.Tick += Timer_Tick;
            FilePusher.TotMessage = LTotalTicks;
            LoadDay(DateTime.Today);
        }

        private void LoadDay(DateTime value)
        {
            FilePusher.ReadDay(value);
            lbSched.ItemsSource = FilePusher.shows;
            LTotalTicks.Content = "Today's Total: "+FilePusher.TotalShows.ToString();
            LoadShow(0);
            if (value.Date.Equals(DateTime.Today))
            {
                Lwarn.Visibility = Visibility.Hidden;
                if (FilePusher.HasMoreShows)
                {
                    timer.Interval = FilePusher.NextShowTime - DateTime.Now;
                    timer.Start();
                }
                else
                {
                    if (timer.IsEnabled)
                    {
                        timer.Stop();
                    }
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
            CurrentShowDisplayed = s;
            lShowtime.Content = "Tickets For " + FilePusher.shows[s].ShowTime;
            icTicket.ItemsSource = FilePusher.shows[s].Tickets;
        }

        //Q: How will this work if the schedule crosses Midnight?
        //A: Don't worry about that. It's outside the scope of this project.
        void Timer_Tick(object sender, EventArgs e)
        {
            if (FilePusher.shows[0].DTShowTime.Date == DateTime.Today)
            {
                FilePusher.UpLastColor();
            }
            if (FilePusher.HasMoreShows)
            {
                timer.Interval = FilePusher.NextShowTime - DateTime.Now;
            }
            else
            {
                timer.Stop();
            }
        }

        private void DpShowDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = (sender as DatePicker).SelectedDate;
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

        private void LbSched_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var i = (sender as ListBox).SelectedIndex;
            if (ticketWindow != null)
            {
                ticketWindow.Close();
            }
            if (MoveWindow != null)
            {
                MoveWindow.Close();
            }
            if (i >= 0)
            {
                LoadShow(i);
            }
            else
            {
                lShowtime.Content = "No Show Selected";
                icTicket.ItemsSource = null;
            }
        }

        private void bAddTicket_Click(object sender, RoutedEventArgs e)
        {
            if(ticketWindow != null)
            {
                ticketWindow.Close();
            }
            if (FilePusher.shows[CurrentShowDisplayed].Seats < 1)
            {
                MessageBox.Show(FilePusher.shows[CurrentShowDisplayed].ShowTime+" show time is fully booked", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ticketWindow = new AddTicket(CurrentShowDisplayed);
                ticketWindow.Show();
            }
        }

        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            if (MoveWindow != null)
            {
                MoveWindow.Close();
            }
            MoveWindow = new MoveTicket(CurrentShowDisplayed, (sender as Button).Tag as int?);
            MoveWindow.Show();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            FilePusher.RemoveTicket(CurrentShowDisplayed, (sender as Button).Tag as int?);
            LTotalTicks.Content = "Today's Total: " + FilePusher.TotalShows.ToString();
        }
    }

    public class TicketItem : IComparable<TicketItem>
    {
        public DateTime ShowTime;
        public DateTime Created;
        public int ID { get; set; }
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
