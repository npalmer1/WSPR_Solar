using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WSPR_Solar
{
    public partial class MessageForm : Form
    {
        public int delay = 0;
        public string caption = "";
        public string message = "";
        public bool yesno = false;
        public bool YES = false;
        public bool reply = false;
        public bool editbuttons = false;
        public int button = 0;
        public bool allowcancel = false;
        public bool cancel = false;

        public MessageForm()
        {
            InitializeComponent();
        }

        private void MessageForm_Load(object sender, EventArgs e)
        {
            if (editbuttons)
            {
                OKbutton.Visible = false;
                Yesbutton.Visible = false;
                Nobutton.Visible = false;
                Editbutton.Visible = editbuttons;
                Addbutton.Visible = editbuttons;
                Delbutton.Visible = editbuttons;
            }
            else
            {
                OKbutton.Visible = !yesno;
                Yesbutton.Visible = yesno;
                Nobutton.Visible = yesno;
                Editbutton.Visible = false;
                Addbutton.Visible = false;
                Delbutton.Visible = false;
            }
           
            try
            {
                if (message != null)
                {
                    label1.Text = message;

                    if (message.Length > 50)
                    {
                        label1.Text = message.Substring(0, 50);
                    }

                    if (!yesno && delay > 0)
                    {
                        timer1.Interval = delay;
                        timer1.Enabled = true;
                        timer1.Start();
                    }
                    else
                    {
                        timer1.Enabled = false;
                    }
                }
                else
                {
                    label1.Text = "null";
                }

            }
            catch
            {
                MessageBox.Show("Error");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            reply = true;
            this.Hide();
        }

        private void Yesbutton_Click(object sender, EventArgs e)
        {
            YES = true;
            reply = true;
            this.Hide();

        }

        private void Nobutton_Click(object sender, EventArgs e)
        {
            YES = false;
            reply = true;
            this.Hide();

        }

        private void MessageForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            reply = true;
        }

        private void Addbutton_Click(object sender, EventArgs e)
        {
            button = 1; //add
            reply = true;
            this.Hide();
        }

        private void Editbutton_Click(object sender, EventArgs e)
        {
            button = 2; //edit
            reply = true;
            this.Hide();
        }

        private void Delbutton_Click(object sender, EventArgs e)
        {
            button = 3;
            reply = true;
            this.Hide();
        }

       
    }
}
