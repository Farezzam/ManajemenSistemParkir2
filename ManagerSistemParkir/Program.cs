﻿using System;
using System.Windows.Forms;

namespace ManagerSistemParkir
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormUtama());
        }
    }
}

// untuk menjalankan form utama