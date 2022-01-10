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
using System.Windows.Threading;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Win32;


namespace SchedulerSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private static bool Running = false;
        private static List<List<int>> InputDataList = new List<List<int>>();

        private static List<string> IDList = new List<string>();

        private System.Data.DataTable InputDataTable = new System.Data.DataTable();
        private System.Data.DataTable ProcDataTable = new System.Data.DataTable(); //status bar part, /////////////////100%///////////////// 17*/ 100% 17*/ 28*space 0% 28*space
        private static int BurstCount = 0;
        private static int WaitingTime = 0;
        private static double AvTT = 0;
        private static List<int> prevBT = new List<int>();
        //private static System.Threading.Timer timer;
        DispatcherTimer dt = new DispatcherTimer();

        private static List<List<int>> instructions = new List<List<int>>();

        public MainWindow()
        {
            InitializeComponent();
            InputDataTable.Columns.Add(new System.Data.DataColumn("ID", typeof(string)));
            InputDataTable.Columns.Add(new System.Data.DataColumn("ArrivalTime", typeof(int)));
            InputDataTable.Columns.Add(new System.Data.DataColumn("BurstTime", typeof(int)));
            InputDataTable.Columns.Add(new System.Data.DataColumn("Priority", typeof(int)));
            Input.ItemsSource = InputDataTable.DefaultView;

            ProcDataTable.Columns.Add(new System.Data.DataColumn("ID", typeof(string)));
            ProcDataTable.Columns.Add(new System.Data.DataColumn("Status", typeof(string)));
            ProcDataTable.Columns.Add(new System.Data.DataColumn("RBurstTime", typeof(int)));
            ProcDataTable.Columns.Add(new System.Data.DataColumn("WaitingTime", typeof(int)));
            Processing.ItemsSource = ProcDataTable.DefaultView;


        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dt.Interval = TimeSpan.FromMilliseconds(100);
            dt.Tick += dtTicker;
            dt.Start();



        }

        public class process
        {
            public String ID;
            public int ArrivalTime;
            public int BurstTime;
            public int priority;
            public int RBurstTime;
            public int WaitingTime;

        }





        private void dtTicker(object sender, EventArgs e)
        {
            if (instructions.Count == 0)
            {
                Queue.Clear();
                Running = false;
                //MessageBox.Show("Run completed");
            }
            if (Running && new List<string>() { "FCFS", "Priority", "SJF" }.Contains(AlgoName.Text.ToString()))
            {

                BurstCount++;
                WaitingTime++;
                ProcDataTable.Rows[instructions[0][3]]["Status"] = (100 * BurstCount / instructions[0][1]).ToString() + "%" + new String('/', 35 * BurstCount / instructions[0][1]);
                ProcDataTable.Rows[instructions[0][3]]["RBurstTime"] = instructions[0][1] - BurstCount;
                try { ProcDataTable.Rows[instructions[1][3]]["WaitingTime"] = WaitingTime; } catch { }

                AWT.Text = ((double)WaitingTime / (double)IDList.Count).ToString();
                TET.Text = WaitingTime.ToString();
                ATT.Text = AvTT.ToString();


                if (BurstCount == instructions[0][1])
                {
                    Queue.Clear();
                    for (int i = 0; i < instructions.Count; i++)
                    {
                        Queue.Text += IDList[instructions[i][3]] + ", ";
                    }

                    AvTT += (double)BurstCount / (double)IDList.Count;
                    BurstCount = 0;
                    instructions.RemoveAt(0);


                }

            }
            else if (Running)
            {
                prevBT[instructions[0][0]]++;
                BurstCount++;
                WaitingTime++;
                ProcDataTable.Rows[instructions[0][0]]["Status"] = (100 * prevBT[instructions[0][0]] / InputDataList[instructions[0][0]][1]).ToString() + "%" + new String('/', 35 * prevBT[instructions[0][0]] / InputDataList[instructions[0][0]][1]);
                ProcDataTable.Rows[instructions[0][0]]["RBurstTime"] = InputDataList[instructions[0][0]][1] - prevBT[instructions[0][0]];
                try { ProcDataTable.Rows[instructions[1][0]]["WaitingTime"] = WaitingTime; } catch { }



                AWT.Text = ((double)WaitingTime / (double)IDList.Count).ToString();
                TET.Text = WaitingTime.ToString();
                ATT.Text = AvTT.ToString();

                if (BurstCount == instructions[0][1])
                {
                    Queue.Clear();
                    for (int i = 0; i < instructions.Count; i++)
                    {

                        Queue.Text += IDList[instructions[i][0]] + ", ";
                    }

                    AvTT += (double)BurstCount / (double)IDList.Count;
                    BurstCount = 0;
                    instructions.RemoveAt(0);



                }
            }

        }


        private void Browse(object sender, RoutedEventArgs e)
        {
            if (Running)
                return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                Path.Text = openFileDialog.FileName;
        }

        private void Import(object sender, RoutedEventArgs e)
        {

            if (Running)
                return;
            try
            {
                Excel.Application xlApp = new Excel.Application(); //open excel
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(@Path.Text);
                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                Excel.Range xlRange = xlWorksheet.UsedRange;


                //IDList.Contain("");
                int i = 1;
                while (xlRange.Cells[i, 1].Value2 != null)
                {
                    if (!IDList.Contains(xlRange.Cells[i, 1].Value2.ToString()))
                    {
                        try
                        {
                            if (xlRange.Cells[i, 4].Value2 == null)
                            {
                                InputDataList.Add(new List<int>() { (int)xlRange.Cells[i, 2].Value2, (int)xlRange.Cells[i, 3].Value2, 0, IDList.Count });
                                InputDataTable.Rows.Add(xlRange.Cells[i, 1].Value2.ToString(), (int)xlRange.Cells[i, 2].Value2, (int)xlRange.Cells[i, 3].Value2, 0);
                                ProcDataTable.Rows.Add(xlRange.Cells[i, 1].Value2.ToString(), "0%", (int)xlRange.Cells[i, 3].Value2, 0);
                                IDList.Add(xlRange.Cells[i, 1].Value2.ToString());
                            }
                            else
                            {
                                InputDataList.Add(new List<int>() { (int)xlRange.Cells[i, 2].Value2, (int)xlRange.Cells[i, 3].Value2, (int)xlRange.Cells[i, 4].Value2, IDList.Count });
                                InputDataTable.Rows.Add(xlRange.Cells[i, 1].Value2.ToString(), (int)xlRange.Cells[i, 2].Value2, (int)xlRange.Cells[i, 3].Value2, (int)xlRange.Cells[i, 4].Value2);
                                ProcDataTable.Rows.Add(xlRange.Cells[i, 1].Value2.ToString(), "0%", (int)xlRange.Cells[i, 3].Value2, 0);
                                IDList.Add(xlRange.Cells[i, 1].Value2.ToString());
                            }
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }


                    }
                    i++;

                }
                xlWorkbook.Close(0);
                xlApp.Quit(); // close excel
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }



        }

        private void MImport(object sender, RoutedEventArgs e)
        {
            if (Running)
                return;
            if (!IDList.Contains(MID.Text.ToString()))
            {
                try
                {
                    if (MP.Text == "")
                    {
                        InputDataList.Add(new List<int>() { Int32.Parse(MAT.Text), Int32.Parse(MBT.Text), 0, IDList.Count });
                        InputDataTable.Rows.Add(MID.Text, Int32.Parse(MAT.Text), Int32.Parse(MBT.Text), 0);
                        ProcDataTable.Rows.Add(MID.Text, "0%", Int32.Parse(MBT.Text), 0);
                    }
                    else
                    {
                        InputDataList.Add(new List<int>() { Int32.Parse(MAT.Text), Int32.Parse(MBT.Text), Int32.Parse(MP.Text), IDList.Count });
                        InputDataTable.Rows.Add(MID.Text, Int32.Parse(MAT.Text), Int32.Parse(MBT.Text), Int32.Parse(MP.Text));


                        ProcDataTable.Rows.Add(MID.Text, "0%", Int32.Parse(MBT.Text), 0);
                    }

                    IDList.Add(MID.Text);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }


            }
            else
            {
                MessageBox.Show("ID already entered");
            }



        }



        private void Random(object sender, RoutedEventArgs e)
        {
            if (Running)
                return;
            var rand = new Random();
            int r = rand.Next(1, 20);
            int rAT = rand.Next(1, IDList.Count + 11);
            for (int i = 0; i < IDList.Count + 1; i++)
            {
                if (!IDList.Contains("r" + i.ToString()))
                {
                    InputDataList.Add(new List<int>() { rAT, r, 0, IDList.Count });
                    InputDataTable.Rows.Add("r" + i.ToString(), rAT, r, 0);


                    ProcDataTable.Rows.Add("r" + i, "0%", r, 0);
                    IDList.Add("r" + i.ToString());
                    break;
                }
            }
        }

        private void Delete(object sender, RoutedEventArgs e)
        {

            if (Running)
                return;
            for (int i = 0; i < IDList.Count; i++)
            {
                if (IDList[i] == DelID.Text)
                {
                    InputDataTable.Rows[i].Delete();
                    IDList.RemoveAt(i);
                    InputDataList.RemoveAt(i);
                    ProcDataTable.Rows[i].Delete();
                    return;

                }
            }
            MessageBox.Show("No such ID");

        }

        private void Simulate(object sender, RoutedEventArgs e)
        {
            if (Running)
            {
                return;
            }
            RS();
            AvTT = 0;
            WaitingTime = 0;
            List<List<int>> temp = new List<List<int>>();
            for (int i = 0; i < InputDataList.Count; i++) temp.Add(new List<int>(InputDataList[i]));
            switch (AlgoName.Text)
            {
                case "FCFS":
                    instructions = FCFS(temp);
                    Running = true;
                    break;
                case "SJF":
                    instructions = SJF(temp);
                    Running = true;
                    break;
                case "Priority":
                    instructions = Priority(temp);
                    Running = true;
                    break;
                case "Round-Robin":

                    instructions = RoundRobin(new List<List<int>>(temp));

                    prevBT = new List<int>();
                    for (int i = 0; i < IDList.Count; i++) prevBT.Add(0);
                    Running = true;
                    break;
                case "SRT":

                    instructions = SRT(temp);
                    prevBT = new List<int>();
                    for (int i = 0; i < IDList.Count; i++) prevBT.Add(0);
                    Running = true;
                    break;
                default:
                    MessageBox.Show("No selected algorithm");
                    break;
            }


        }


        private void Pause(object sender, RoutedEventArgs e)
        {
            Running = false;
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            Running = true;
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            RS();
        }

        private void RS()
        {
            prevBT = new List<int>();
            for (int i = 0; i < IDList.Count; i++) prevBT.Add(0);
            WaitingTime = 0;
            AvTT = 0;
            Queue.Clear();
            AWT.Text = "";
            TET.Text = "";
            ATT.Text = "";
            Running = false;
            instructions = new List<List<int>>();
            for (int i = 0; i < IDList.Count; i++)
            {
                ProcDataTable.Rows[i]["Status"] = "0%";
                ProcDataTable.Rows[i]["RBurstTime"] = InputDataList[i][1];
                ProcDataTable.Rows[i]["WaitingTime"] = 0;
            }
        }

        private List<List<int>> FCFS(List<List<int>> l) //First come first serve
        {

            if (l.Count == 1)
            {
                return l;
            }

            List<List<int>> left = new List<List<int>>();
            List<List<int>> right = new List<List<int>>();

            for (int i = 0; i < l.Count / 2; i++)  //Dividing the unsorted list
            {
                left.Add(l[i]);
            }
            for (int i = l.Count / 2; i < l.Count; i++)  //Dividing the unsorted list
            {
                right.Add(l[i]);
            }

            List<List<int>> sortedLeft = FCFS(left);
            List<List<int>> sortedRight = FCFS(right);
            List<List<int>> sorted = new List<List<int>>();

            for (int i = 0; i < l.Count; i++)
            {
                if (sortedRight.Count == 0 || (sortedLeft.Count != 0 && sortedLeft[0][0] < sortedRight[0][0]))
                {
                    sorted.Add(sortedLeft[0]);
                    sortedLeft.RemoveAt(0);
                }
                else if (sortedLeft.Count == 0 || (sortedRight.Count != 0 && sortedRight[0][0] < sortedLeft[0][0]))
                {
                    sorted.Add(sortedRight[0]);
                    sortedRight.RemoveAt(0);
                }
                else
                {
                    if (sortedLeft[0][1] < sortedRight[0][1])
                    {
                        sorted.Add(sortedLeft[0]);
                        sortedLeft.RemoveAt(0);
                    }
                    else
                    {
                        sorted.Add(sortedRight[0]);
                        sortedRight.RemoveAt(0);
                    }
                }
            }


            return sorted;
        }

        private List<List<int>> SJF(List<List<int>> l) //Shorted Job First
        {

            if (l.Count == 1)
            {
                return l;
            }

            List<List<int>> left = new List<List<int>>();
            List<List<int>> right = new List<List<int>>();

            for (int i = 0; i < l.Count / 2; i++)  //Dividing the unsorted list
            {
                left.Add(l[i]);
            }
            for (int i = l.Count / 2; i < l.Count; i++)  //Dividing the unsorted list
            {
                right.Add(l[i]);
            }

            List<List<int>> sortedLeft = SJF(left);
            List<List<int>> sortedRight = SJF(right);
            List<List<int>> sorted = new List<List<int>>();

            for (int i = 0; i < l.Count; i++)
            {
                if (sortedRight.Count == 0 || (sortedLeft.Count != 0 && sortedLeft[0][1] < sortedRight[0][1]))
                {
                    sorted.Add(sortedLeft[0]);
                    sortedLeft.RemoveAt(0);
                }
                else if (sortedLeft.Count == 0 || (sortedRight.Count != 0 && sortedRight[0][1] < sortedLeft[0][1]))
                {
                    sorted.Add(sortedRight[0]);
                    sortedRight.RemoveAt(0);
                }
                else
                {
                    if (sortedLeft[0][0] < sortedRight[0][0])
                    {
                        sorted.Add(sortedLeft[0]);
                        sortedLeft.RemoveAt(0);
                    }
                    else
                    {
                        sorted.Add(sortedRight[0]);
                        sortedRight.RemoveAt(0);
                    }
                }
            }
            return sorted;
        }

        private List<List<int>> Priority(List<List<int>> l) //Shorted Job First
        {

            if (l.Count == 1)
            {
                return l;
            }

            List<List<int>> left = new List<List<int>>();
            List<List<int>> right = new List<List<int>>();

            for (int i = 0; i < l.Count / 2; i++)  //Dividing the unsorted list
            {
                left.Add(l[i]);
            }
            for (int i = l.Count / 2; i < l.Count; i++)  //Dividing the unsorted list
            {
                right.Add(l[i]);
            }

            List<List<int>> sortedLeft = Priority(left);
            List<List<int>> sortedRight = Priority(right);
            List<List<int>> sorted = new List<List<int>>();

            for (int i = 0; i < l.Count; i++)
            {
                if (sortedRight.Count == 0 || (sortedLeft.Count != 0 && sortedLeft[0][2] < sortedRight[0][2]))
                {
                    sorted.Add(sortedLeft[0]);
                    sortedLeft.RemoveAt(0);
                }
                else if (sortedLeft.Count == 0 || (sortedRight.Count != 0 && sortedRight[0][2] < sortedLeft[0][2]))
                {
                    sorted.Add(sortedRight[0]);
                    sortedRight.RemoveAt(0);
                }
                else
                {
                    if (sortedLeft[0][0] < sortedRight[0][0])
                    {
                        sorted.Add(sortedLeft[0]);
                        sortedLeft.RemoveAt(0);
                    }
                    else if (sortedLeft[0][0] > sortedRight[0][0])
                    {
                        sorted.Add(sortedRight[0]);
                        sortedRight.RemoveAt(0);
                    }
                    else
                    {
                        if (sortedLeft[0][1] < sortedRight[0][1])
                        {
                            sorted.Add(sortedLeft[0]);
                            sortedLeft.RemoveAt(0);
                        }
                        else if (sortedLeft[0][1] > sortedRight[0][1])
                        {
                            sorted.Add(sortedRight[0]);
                            sortedRight.RemoveAt(0);
                        }
                    }
                }
            }
            return sorted;
        }

        private List<List<int>> RoundRobin(List<List<int>> l)
        {
            int TBT = 0;
            int time = 0;
            bool fa = true;
            List<List<int>> sorted = FCFS(l);//arivaltime 0, burst time 1, id is 3
            List<List<int>> results = new List<List<int>>();
            for (int i = 0; i < IDList.Count; i++)
                TBT += sorted[i][1];

            for (int i = 0; i < TBT; i += 1)
            {
                fa = true;
                time = 0;
                for (int j = 0; j < sorted.Count; j++)
                {

                    if (time <= i)
                    {
                        if (sorted[j][1] / 5 != 0)
                        {
                            results.Add(new List<int>() { sorted[j][3], 5 });
                            sorted[j][1] -= 5;
                            if (fa)
                                i += 4;
                            else
                                i += 5;
                            //List<List<int>> ittt = InputDataList;
                        }
                        else if (sorted[j][1] % 5 != 0)
                        {
                            results.Add(new List<int>() { sorted[j][3], sorted[j][1] % 5 });
                            sorted[j][1] = 0;
                            if (fa)
                                i += (sorted[j][1] % 5) - 1;
                            else
                                i += (sorted[j][1] % 5);

                            //List<List<int>> ittt = InputDataList;
                        }

                    }
                    else
                    {
                        break;
                    }

                    time += sorted[j][0];
                }



            }

            return results;
        }
        private List<List<int>> SRT(List<List<int>> l)
        {

            int time;
            int TBT = 0;
            int min;
            int minID = 0;
            List<List<int>> sorted = FCFS(l);//arivaltime 0, burst time 1, id is 3
            List<List<int>> results = new List<List<int>>();
            for (int i = 0; i < IDList.Count; i++)
                TBT += sorted[i][1];
            for (int i = 0; i < TBT + 1; i += 1)
            {
                time = 0;
                min = -1;
                minID = 0;
                for (int j = 0; j < sorted.Count; j++)
                {
                    if (time <= i)
                    {
                        if ((sorted[j][1] < min && sorted[j][1] > 0) || (min == -1 && sorted[j][1] > 0))
                        {
                            minID = j;
                            min = sorted[j][1];
                        }
                    }
                    else
                    {
                        break;
                    }


                    time += sorted[j][0];
                }
                if (min > 0)
                {
                    if (sorted[minID][1] != 0)
                    {

                        results.Add(new List<int>() { sorted[minID][3], 1 });
                        sorted[minID][1] -= 1;

                    }
                }

            }
            return results;
        }



    }
}
