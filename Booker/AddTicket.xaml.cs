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
    /// Interaction logic for AddTicket.xaml
    /// </summary>
    public partial class AddTicket : Window
    {
        private List<TicketMenuItem> MenuList = new List<TicketMenuItem>();
        private SchedItem SchedItemShow;
        public AddTicket(SchedItem item)
        {
            InitializeComponent();
            SchedItemShow = item;
            LAddTime.Content = item.ShowTime+" TICKET";
            int seats = item.Seats;
            LNumTicketsAvalible.Content = "Number of Tickets (Max "+seats.ToString()+"):";
            for (int i = 0; i < seats; ++i) MenuList.Add(new TicketMenuItem() { MenuOption = (i+1).ToString()+" Ticket"+(i==0?"":"s")});
            CMBTicketAvalible.ItemsSource = MenuList;
            CMBTicketAvalible.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var clean = new System.Text.RegularExpressions.Regex(@"[^\d]");
            var phone = clean.Replace(TBPhone.Text, "");
            if (phone.Length != 10 && phone.Length != 7) phone = "";
            var t = new TicketItem() { SaleType = RBFree.IsChecked ?? false ? 'F' : RBDiscount.IsChecked ?? false ? 'D' : 'P', NumTickets = CMBTicketAvalible.SelectedIndex + 1, BuyerName = TBName.Text, Phone = phone };
            SchedItemShow.Tickets.Add(t);
            FilePusher.AddTicket(t,SchedItemShow.DTShowTime);
            SchedItemShow.UpSeats();
            Close();
            if (SchedItemShow.DTShowTime>=DateTime.Now && SchedItemShow.DTShowTime < DateTime.Now.AddMinutes(45))
            {
                FilePusher.Push();
            }
        }
    }

    internal class TicketMenuItem
    {
        public string MenuOption { get; set; }
    }
}
