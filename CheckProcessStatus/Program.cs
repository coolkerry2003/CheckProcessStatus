using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CheckProcessStatus
{
    class Program
    {
        static List<float> AvailableCPU = new List<float>();
        static List<float> AvailableRAM = new List<float>();

        protected static PerformanceCounter t_cpuCounter;
        protected static PerformanceCounter cpuCounter;
        protected static PerformanceCounter ramCounter;

        static int gap = 10;
        static int lastMinute = DateTime.Now.Minute-gap;
        static void Main(string[] args)
        {
            string name = "VIGOR_ClinicalMonitor";
            //string name = "3R_DriverMonitor";
            t_cpuCounter = new PerformanceCounter("Process", "% Processor Time", "_Total");
            cpuCounter = new PerformanceCounter("Process", "% Processor Time", name);
            ramCounter = new PerformanceCounter("Process", "Working Set", name);

            try
            {
                Message($">>{name} <系統監控中>...每{gap}分鐘進行監測");
                System.Timers.Timer t = new System.Timers.Timer(1000);
                t.Elapsed += new ElapsedEventHandler(TimerElapsed);
                t.Start();
            }
            catch (Exception e)
            {
                Message(e.Message);
            }
            while (true)
            {
                Console.ReadLine();
            }
        }
        public static void TimerElapsed(object source, ElapsedEventArgs e)
        {
            if(DateTime.Now.Minute % gap == 0 && lastMinute != DateTime.Now.Minute)
            {
                lastMinute = DateTime.Now.Minute;
                recordInfo();
            }
        }
        public static void recordInfo()
        {
            try
            {
                float tcpu = t_cpuCounter.NextValue();
                float cpu = cpuCounter.NextValue();
                float ram = ramCounter.NextValue();
                float rsCpu = ((cpu / (tcpu)) * 100);
                float rsRam = ram / 1024 / 1024;

                Message(string.Format("[{0}] > CPU Value: {1:000} %, ram value: {2:0000} MB", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), rsCpu, rsRam));
                if (Double.IsNaN(rsCpu) || Double.IsNaN(rsRam) || rsCpu > 100)
                {
                    Message("  >>數值異常,不列入計算");
                    return;
                }
                AvailableCPU.Add(rsCpu);
                AvailableRAM.Add(rsRam);

                if (DateTime.Now.Hour == 8 && DateTime.Now.Minute == 0)
                {
                    Message("=============================================================================");
                    float avgCpu = AvailableCPU.Sum() / AvailableCPU.Count;
                    float avgRam = AvailableRAM.Sum() / AvailableRAM.Count;
                    Message(string.Format("[{0}] > CPU Value: {1:000} %, ram value: {2:0000} MB (average)", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), avgCpu, avgRam));
                    Message("=============================================================================");

                    AvailableCPU.Clear();
                    AvailableRAM.Clear();
                }
            }
            catch (Exception ex)
            {
                Message(ex.Message);
            }
            finally
            {
                
            }
        }
        protected static void Message(string aContent)
        {
            string mUserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            DateTime mNow = DateTime.Now;
            string mFileName = string.Format("{0:000}{1:00}{2:00}.log", mNow.Year, mNow.Month, mNow.Day);
            string mDir = Path.Combine(mUserPath, "VIGOR");
            string mPath = Path.Combine(mDir, mFileName);

            //string mMessage = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), aContent);
            string[] mContent = { aContent };
            Directory.CreateDirectory(mDir);
            File.AppendAllLines(mPath, mContent);
            Console.WriteLine(aContent);
        }
    }
}
