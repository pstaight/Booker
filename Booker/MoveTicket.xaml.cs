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
using System.Windows.Shapes;

namespace Booker
{
    /// <summary>
    /// Interaction logic for MoveTicket.xaml
    /// </summary>
    public partial class MoveTicket : Window
    {
        private List<ShowOptionItem> SOI;
        private TicketItem item;
        private int ShowIndex;
        private int? TicketIndex;
        private bool NoShow = false;
        public MoveTicket(int _ShowIndex, int? _TicketIndex)
        {
            ShowIndex = _ShowIndex;
            TicketIndex = _TicketIndex;
            InitializeComponent();
            item = FilePusher.shows[ShowIndex].Tickets[TicketIndex ?? 0];
            TBLNumTickets.Text = item.NumTickets + " Ticket" + (item.NumTickets == 1 ? "" : "s");
            TBLBuyerName.Text = item.BuyerName;
            SetShows(item.ShowTime);
        }

        private void DpMoveDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = (sender as DatePicker).SelectedDate;
            if (date.Equals(item.ShowTime.Date))
            {
                TBLwarn2.Visibility = Visibility.Hidden;
            }
            else
            {
                TBLwarn2.Visibility = Visibility.Visible;
            }
            SetShows(date?? item.ShowTime);
        }
        private void SetShows(DateTime _dt)
        {
            //Check if FilePusher.shows is the same day as _dt
            SOI = FilePusher.MakeShowOptions(_dt, FilePusher.shows[ShowIndex].Tickets[TicketIndex ?? 0].NumTickets);
            NoShow = false;
            if (SOI.Count == 0)
            {
                SOI.Add(new ShowOptionItem() { ShowOption = "No Available Shows" });
                NoShow = true;
            }
            CMBShowsAvalible.ItemsSource = SOI;
            CMBShowsAvalible.SelectedIndex = 0;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (NoShow)
            {
                MessageBox.Show("No suitable Show Times", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            item.ShowTime = SOI[CMBShowsAvalible.SelectedIndex].DTShowTime;
            FilePusher.RemoveTicket(ShowIndex, TicketIndex);
            FilePusher.AddTicket(item);
            FilePusher.TotMessage.Content = "Today's Total: " + FilePusher.TotalShows.ToString();
            this.Close();
        }
    }
    public class ShowOptionItem
    {
        public string ShowOption { get; set; }
        public DateTime DTShowTime;
    }
}
