using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Renci.SshNet;

namespace Booker
{
    public static class FilePusher
    {
        public const string folder = @"C:\Users\Public\Booker\";
        public static List<SchedItem> shows;
        public static DispatcherTimer SFTPtimer = new DispatcherTimer(new TimeSpan(2,0,0), DispatcherPriority.Background,Push, Dispatcher.CurrentDispatcher);
        public static bool HasMoreShows => shows.Any(x => x.DTShowTime >= DateTime.Now);
        public static DateTime NextShowTime => shows.First(x => x.DTShowTime >= DateTime.Now).DTShowTime;
        public static int TotalShows => shows.Sum(x => x.Tickets.Sum(y => y.NumTickets));
        public static System.Windows.Controls.Label TotMessage;
        public static void ReadDay(DateTime value)
        {

            shows = new List<SchedItem>();
            string fileName = folder + "sched" + value.ToString("yyyyMMdd") + "-000.csv";
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
                    foreach (var l in lines)
                    {
                        if (l[0] == mark)
                        {
                            var dat = l.Split(',');
                            if (!int.TryParse(dat[1], out int start)) continue;
                            if (!int.TryParse(dat[2], out int end)) continue;
                            if (!int.TryParse(dat[3], out int seats)) continue;
                            if (!int.TryParse(dat[4], out int length)) continue;
                            var s = value.Date.AddHours(start / 100).AddMinutes(start % 100);
                            var e = value.Date.AddHours(end / 100).AddMinutes(end % 100);
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
                    }
                    shows.Sort();
                }
                else
                {
                    string[] lines = { "M,0900,1700,12,15", "T,0900,1700,12,15", "W,0900,1700,12,15", "H,0900,1700,12,15", "F,0900,1700,12,15", "S,0900,1700,12,15", "U,0900,1700,12,15" };
                    File.WriteAllLines(folder + "default.csv", lines);
                    var s = value.Date.AddHours(9);
                    var e = value.Date.AddHours(17);
                    while (s < e)
                    {
                        shows.Add(new SchedItem() { DTShowTime = s, TotalSeats = 12, Tickets = new ObservableCollection<TicketItem>() });
                        s = s.AddMinutes(15);
                    }
                }
            }
            fileName = folder + "ticket" + value.ToString("yyyyMMdd") + "-000.csv";
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
                        sh.Tickets.Add(new TicketItem() { ShowTime = sh.DTShowTime, NumTickets = int.Parse(dat[1]), SaleType = dat[2][0], BuyerName = b, Phone = dat[4] });
                    }
                }
                foreach(var sh in shows)
                {
                    for(int i = 0; i < sh.Tickets.Count; ++i)
                    {
                        sh.Tickets[i].ID = i;
                    }    
                }
            }
        }

        public static List<ShowOptionItem> MakeShowOptions(DateTime dt, int NumTickets)
        {
            var ShowOp = new List<ShowOptionItem>();
            if (dt.Date == shows[0].DTShowTime.Date)
            {
                foreach (var s in shows)
                {
                    if (s.Seats >= NumTickets)
                    {
                        ShowOp.Add(new ShowOptionItem() { DTShowTime = s.DTShowTime, ShowOption = s.ShowTime });
                    }
                }
            }
            else
            {
                string fileName = folder + "sched" + dt.ToString("yyyyMMdd") + "-000.csv";
                var tcounts = new Dictionary<DateTime, int>();
                if (File.Exists(fileName))
                {
                    string[] lines = File.ReadAllLines(fileName);
                    foreach (var l in lines)
                    {
                        var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                        if (d.Date == dt.Date)
                        {
                            tcounts.Add(d, int.Parse(l.Split(',')[1]));
                        }
                    }
                }
                else
                {
                    if (File.Exists(folder + "default.csv"))
                    {
                        char[] d = { 'U', 'M', 'T', 'W', 'H', 'F', 'S' };
                        char mark = d[(int)dt.DayOfWeek];
                        string[] lines = File.ReadAllLines(folder + "default.csv");
                        foreach (var l in lines)
                        {
                            if (l[0] == mark)
                            {
                                var dat = l.Split(',');
                                if (!int.TryParse(dat[1], out int start)) continue;
                                if (!int.TryParse(dat[2], out int end)) continue;
                                if (!int.TryParse(dat[3], out int seats)) continue;
                                if (!int.TryParse(dat[4], out int length)) continue;
                                var s = dt.Date.AddHours(start / 100).AddMinutes(start % 100);
                                var e = dt.Date.AddHours(end / 100).AddMinutes(end % 100);
                                while (s < e)
                                {
                                    if (tcounts.ContainsKey(s))
                                    {
                                        if (tcounts[s] > seats)
                                        {
                                            tcounts[s] = seats;
                                        }
                                    }
                                    else
                                    {
                                        tcounts.Add(s, seats);
                                    }
                                    var sh = shows.Find(x => x.DTShowTime.Equals(s));
                                    s = s.AddMinutes(length);
                                }
                            }
                        }
                    }
                    else
                    {
                        var s = dt.Date.AddHours(9);
                        var e = dt.Date.AddHours(17);
                        while (s < e)
                        {
                            tcounts.Add(s, 12);
                            s = s.AddMinutes(15);
                        }
                    }
                }
                fileName = folder + "ticket" + dt.ToString("yyyyMMdd") + "-000.csv";
                if (File.Exists(fileName))
                {
                    string[] lines = File.ReadAllLines(fileName);
                    foreach (var l in lines)
                    {
                        var d = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                        if (tcounts.ContainsKey(d))
                        {
                            tcounts[d] -= int.Parse(l.Split(',')[1]);
                        }
                    }
                }
                foreach (var item in tcounts)
                {
                    if (item.Value >= NumTickets)
                    {
                        int i = item.Key.Hour;
                        ShowOp.Add(new ShowOptionItem() { DTShowTime = item.Key, ShowOption = (i <= 12 ? i.ToString("00") : (i - 12).ToString("00")) + ":" + item.Key.Minute.ToString("00") + (i < 12 ? " AM" : " PM") });
                    }
                }
            }
            return ShowOp;
        }

        public static void UpLastColor()
        {
            shows.FindLast(x => x.DTShowTime <= DateTime.Now).UpColor();
        }

        /*public static void MakeSched()
        {
            string schedfilename = folder + "sched" + shows[0].DTShowTime.ToString("yyyyMMdd") + "-000.csv";
            if (!File.Exists(schedfilename))
            {
                string[] lines = new string[shows.Count];
                int i = 0;
                foreach (var s in shows)
                {
                    lines[i] = s.DTShowTime.ToString("yyyy-MM-dd HHmm") + "," + s.TotalSeats.ToString();
                    ++i;
                }
                File.WriteAllLines(schedfilename, lines);
            }
        }*/

        public static void RemoveTicket(int ShowIndex,int? TicketIndex)
        {
            //var sp = (sender as Button).Parent as StackPanel;
            //var ti = new TicketItem() { NumTickets = int.Parse((sp.Children[2] as TextBlock).Text), SaleType = (sp.Children[3] as TextBlock).Text[0], Phone = (sp.Children[4] as TextBlock).Text, BuyerName = (sp.Children[5] as TextBlock).Text };
            //var item = shows[CurrentShowDisplayed].Tickets.First(x => x.NumTickets == ti.NumTickets && x.SaleType == ti.SaleType && x.Phone.Equals(ti.Phone) && x.BuyerName.Equals(ti.BuyerName));
            //shows[CurrentShowDisplayed].Tickets.Remove(item);
            //shows[CurrentShowDisplayed].UpSeats();
            //FilePusher.RemoveTicket(item, shows[CurrentShowDisplayed].DTShowTime);
            //read in the ticket file, remove this record, sort it, write it back out, Remove this record from Tickets
            var item = shows[ShowIndex].Tickets[TicketIndex ?? 0];
            var itemTime = item.ShowTime;
            shows[ShowIndex].Tickets.Remove(item);
            for (int i = 0; i < shows[ShowIndex].Tickets.Count; ++i)
            {
                shows[ShowIndex].Tickets[i].ID = i;
            }
            shows[ShowIndex].UpSeats();
            var lines = File.ReadAllLines(folder + "ticket" + itemTime.ToString("yyyyMMdd") + "-000.csv");
            var ticks = new List<TicketItem>();
            foreach (var l in lines)
            {
                var dat = l.Split(',');
                var st = new DateTime(int.Parse(l.Substring(0, 4)), int.Parse(l.Substring(5, 2)), int.Parse(l.Substring(8, 2)), int.Parse(l.Substring(11, 2)), int.Parse(l.Substring(13, 2)), 0);
                string b = dat[3];
                if (b.Length > 1 && (b[b.Length - 1] == '"' || b[b.Length - 1] == '\xFFFD') && (b[0] == '"' || b[0] == '\xFFFD')) b = b.Substring(1, b.Length - 2);
                var t = new TicketItem() { ShowTime = st, NumTickets = int.Parse(dat[1]), SaleType = dat[2][0], BuyerName = b, Phone = dat[4] };
                if (!(item.ShowTime.Equals(t.ShowTime) && item.NumTickets == t.NumTickets && item.SaleType == t.SaleType && item.BuyerName.Equals(t.BuyerName) && item.Phone.Equals(t.Phone)))
                {
                    ticks.Add(t);
                }
            }
            ticks.Sort();
            var outlines = new List<string>();
            foreach (var t in ticks)
            {
                outlines.Add(t.ShowTime.ToString("yyyy-MM-dd HHmm") + "," + t.NumTickets.ToString() + "," + t.SaleType + ",\"" + t.BuyerName + "\"," + t.Phone);
            }
            File.WriteAllLines(folder + "ticket" + itemTime.ToString("yyyyMMdd") + "-000.csv", outlines);
        }

        public static void AddTicket(TicketItem t)
        {
            var s = shows.Find(x => x.DTShowTime == t.ShowTime);
            if (s != null)
            {
                t.ID = s.Tickets.Count;
                s.Tickets.Add(t);
                s.UpSeats();
            }
            File.AppendAllLines(folder + "ticket" + t.ShowTime.ToString("yyyyMMdd") + "-000.csv", new List<string>
            {
                t.ShowTime.ToString("yyyy-MM-dd HHmm") + "," + t.NumTickets.ToString() + "," + t.SaleType + ",\"" + t.BuyerName + "\"," + t.Phone + ","+t.Created.ToString("yyyy-MM-dd HHmmss")
            });
        }

        //Should we always push today's file or should we push the currently open day?
        public static void Push(object sender, EventArgs e)
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
        /*
        public void MoveT(TicketItem Old, TicketItem NewItem)
        {
            shows[CurrentShowDisplayed].Tickets.Remove(Old);
            shows[CurrentShowDisplayed].UpSeats();
            FilePusher.RemoveTicket(Old, shows[CurrentShowDisplayed].DTShowTime);
            FilePusher.AddTicket(NewItem, NewItem.ShowTime);
            if (shows[CurrentShowDisplayed].DTShowTime.Date == NewItem.ShowTime.Date)
            {
                shows.Find(x => x.DTShowTime == NewItem.ShowTime).Tickets.Add(NewItem);
            }
        }
        */
    }
}
