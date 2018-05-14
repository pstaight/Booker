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
        private TicketItem item;
        private MainWindow mw;
        private List<ShowOptionItem> SOI;
        private DateTime dt;
        private bool NoShow = false;
        public MoveTicket(TicketItem _item, MainWindow _mw)
        {
            InitializeComponent();
            item = _item;
            mw = _mw;
            TBLNumTickets.Text = item.NumTickets + " Ticket" + (item.NumTickets == 1 ? "" : "s");
            TBLBuyerName.Text = item.BuyerName;
            SetShows(item.ShowTime);
        }

        private void DpMoveDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker dt = sender as DatePicker;
            DateTime? date = dt.SelectedDate;
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
            dt = _dt;
            var shows = FilePusher.ReadShows(dt);
            SOI = new List<ShowOptionItem>();
            foreach (var s in shows)
            {
                if (s.DTShowTime > DateTime.Now && s.Seats >= item.NumTickets) SOI.Add(new ShowOptionItem() { ShowOption = s.ShowTime });
            }
            NoShow = false;
            if (SOI.Count == 0)
            {
                SOI.Add(new ShowOptionItem() { ShowOption = "No Available Shows" });
                NoShow = true;
            }
            dpMoveDate.DisplayDateStart = DateTime.Today;
            CMBShowsAvalible.ItemsSource = SOI;
            CMBShowsAvalible.SelectedIndex = 0;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (NoShow)
            {
                MessageBox.Show("No suitable Show Times for " + dt.ToString("yyyy-MM-dd"), "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string t = SOI[CMBShowsAvalible.SelectedIndex].ShowOption;
            DateTime d;
            if(dpMoveDate.SelectedDate == null)
            {
                d=item.ShowTime.Date.AddHours(int.Parse(t.Substring(0, 2)) + (t.Substring(5, 3) == " PM" ? 12 : 0)).AddMinutes(int.Parse(t.Substring(3, 2)));
            }
            else
            {
                d = dpMoveDate.SelectedDate.Value.AddHours(int.Parse(t.Substring(0, 2)) + (t.Substring(5, 3) == " PM" ? 12 : 0)).AddMinutes(int.Parse(t.Substring(3, 2)));
            }
            var newItem = new TicketItem() { ShowTime = d,NumTickets=item.NumTickets,SaleType=item.SaleType,BuyerName=item.BuyerName,Phone=item.Phone};
            mw.MoveT(item, newItem);
            this.Close();
        }
    }
    public class ShowOptionItem
    {
        public string ShowOption { get; set; }
    }
}
