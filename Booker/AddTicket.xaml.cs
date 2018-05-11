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
        public AddTicket()
        {
            InitializeComponent();
            LNumTicketsAvalible.Content = "Number of Tickets (Max 12):";
            for (int i = 0; i < 12; ++i) MenuList.Add(new TicketMenuItem() { MenuOption = i.ToString() + " Ticket" + (i == 0 ? "s" : "") });
            CMBTicketAvalible.ItemsSource = MenuList;
        }
        public AddTicket(SchedItem item)
        {
            InitializeComponent();
            LAddTime.Content = item.ShowTime+" TICKET";
            int seats = item.Seats;
            LNumTicketsAvalible.Content = "Number of Tickets (Max "+seats.ToString()+"):";
            for (int i = 0; i < seats; ++i) MenuList.Add(new TicketMenuItem() { MenuOption = (i+1).ToString()+" Ticket"+(i==0?"":"s")});
            CMBTicketAvalible.ItemsSource = MenuList;
            CMBTicketAvalible.SelectedIndex = 0;
        }
    }

    internal class TicketMenuItem
    {
        public string MenuOption { get; set; }
    }
}
