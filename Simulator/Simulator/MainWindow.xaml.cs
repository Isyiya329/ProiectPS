using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Win32.SafeHandles;

namespace Simulator
{
    public partial class MainWindow : Window
    {
        private const int STD_OUTPUT_HANDLE = -11;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        private bool[] beltsOn = new bool[5]; // index 1-4 pentru benzi

        public MainWindow()
        {
            InitializeComponent();

            AllocConsole();
            RedirectConsoleOutput();

            Console.WriteLine("Simulare pornita...");

            // Butoane
            S0.Click += (s, e) => StopAll();
            S1.Click += (s, e) => StartBelt(1);
            S2.Click += (s, e) => StartBelt(2);
            S3.Click += (s, e) => StartBelt(3);
            S4.Click += (s, e) => StartBelt(4);
            S5.Click += (s, e) => StopEntryBelts();

            // Comutatoare
            S6.Checked += (s, e) => ToggleChanged();
            S6.Unchecked += (s, e) => ToggleChanged();
            S7.Checked += (s, e) => ToggleChanged();
            S7.Unchecked += (s, e) => ToggleChanged();
            S8.Checked += (s, e) => ToggleChanged();
            S8.Unchecked += (s, e) => ToggleChanged();
        }

        private void RedirectConsoleOutput()
        {
            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeHandle = new SafeFileHandle(stdHandle, true);
            FileStream fs = new FileStream(safeHandle, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);
        }

        private void WriteLine(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
        }

        private void StartBelt(int id)
        {
            // LOGICA FUNCTIONALA 1 & 2
            if (id == 1 || id == 2)
            {
                int other = (id == 1) ? 2 : 1;
                if (beltsOn[other])
                {
                    WriteLine($"Nu poti porni banda {id}: banda {other} este deja pornita.");
                    return;
                }

                if (!IsEntryAllowed(id))
                {
                    WriteLine($"Banda {id} NU poate porni: pozitie clapeta sau iesire necorespunzatoare.");
                    return;
                }
            }

            beltsOn[id] = true;
            WriteLine($"Banda {id} PORNITA.");
        }

        private void StopAll()
        {
            for (int i = 1; i <= 4; i++)
                beltsOn[i] = false;

            WriteLine("S0 apasat - Toate benzile OPRITE.");
        }

        private void StopEntryBelts()
        {
            beltsOn[1] = false;
            beltsOn[2] = false;
            WriteLine("S5 apasat - Benzile 1 si 2 OPRITE.");
        }

        private void ToggleChanged()
        {
            int active = 0;
            if (S6.IsChecked == true) active++;
            if (S7.IsChecked == true) active++;
            if (S8.IsChecked == true) active++;

            if (active >= 2)
            {
                WriteLine("ALERTA: Clapete multiple active - sistem oprit 5 secunde.");
                System.Threading.Thread.Sleep(5000);
            }
            else
            {
                WriteLine($"Stare clapete: S6={S6.IsChecked}, S7={S7.IsChecked}, S8={S8.IsChecked}");
            }
        }

        private bool IsEntryAllowed(int entryId)
        {
            // S6 = intrare 1 -> banda 3
            // S8 = intrare 2 -> banda 4
            // S7 = clapeta pe mijloc -> ambele iesiri

            if (entryId == 1)
            {
                if (S6.IsChecked == true || S7.IsChecked == true)
                    return beltsOn[3] || beltsOn[4];
            }

            if (entryId == 2)
            {
                if (S8.IsChecked == true || S7.IsChecked == true)
                    return beltsOn[3] || beltsOn[4];
            }

            return false;
        }
    }
}
