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
        private DateTime SchedItemShow;
        public AddTicket(int ShowIndex)
        {
            InitializeComponent();
            SchedItemShow = FilePusher.shows[ShowIndex].DTShowTime;
            LAddTime.Content = FilePusher.shows[ShowIndex].ShowTime+ " TICKET";
            int seats = FilePusher.shows[ShowIndex].Seats;
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
            var t = new TicketItem() { ShowTime = SchedItemShow, SaleType = RBFree.IsChecked ?? false ? 'F' : RBDiscount.IsChecked ?? false ? 'D' : 'P', NumTickets = CMBTicketAvalible.SelectedIndex + 1, BuyerName = TBName.Text, Phone = phone, Created=DateTime.Now };
            FilePusher.AddTicket(t);
            Close();
            FilePusher.TotMessage.Content = "Today's Total: " + FilePusher.TotalShows.ToString();
            if (SchedItemShow>=DateTime.Now && SchedItemShow < DateTime.Now.AddMinutes(45))
            {
                FilePusher.Push(null,null);
                if (FilePusher.SFTPtimer.IsEnabled)
                {
                    FilePusher.SFTPtimer.Stop();
                }
                FilePusher.SFTPtimer.Start();
            }
        }
    }

    internal class TicketMenuItem
    {
        public string MenuOption { get; set; }
    }
}
