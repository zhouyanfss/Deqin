using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace ZD.Client
{
    public partial class MainForm : Form
    {
        private LeftTree leftTree = new LeftTree();
        private CustomerSeach csearch = new CustomerSeach();
        private CustomerInfo cinfo = new CustomerInfo();
        public MainForm()
        {
            InitializeComponent();
            leftTree.Show(this.dockPanel, DockState.DockLeft);
            csearch.Show(this.dockPanel);
            cinfo.Show(this.dockPanel);
        }
    }
}
