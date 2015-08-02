using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZD.Serivce.Contract;

namespace ZD.Client
{
    public partial class SignIn : Form
    {
        public SignIn()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var client1 = new UserServiceClient();
            var response = client1.SignIn(new Serivce.Contract.UserSignInRequest
            {
                UserName = tbUserName.Text,
                Password = tbPassword.Text
            });

            if (response.IsSuccess)
            {
                lblMessage.Text = "登陆成功";

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
            {
                lblMessage.Text = response.ErrorMessage;
            }
        }

        private void SignIn_Load(object sender, EventArgs e)
        {

        }

    }
}
