﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZD.Client
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new SignIn());

            SignIn fl = new SignIn();
            fl.ShowDialog();
            if (fl.DialogResult == DialogResult.OK)
            {
                Application.Run(new MainForm());
            }
            else
            {
                return;
            }
        }
    }
}