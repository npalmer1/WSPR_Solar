using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WSPR_Solar
{
    internal class MessageClass
    {

        public async void TCMessageBox(string text, string caption, int delay, MessageForm mForm)
        {
           
                runTCMessageBox(text, caption, delay, mForm);
          
        }
        public async Task runTCMessageBox(string text, string caption, int delay, MessageForm mForm)
        {
            //maximum text length 55           
            mForm.Text = caption;
            mForm.yesno = false;
            mForm.message = text;
            mForm.delay = delay;
            mForm.StartPosition = FormStartPosition.CenterParent;
            mForm.ShowDialog();
            mForm.BringToFront();
           
        }
        public async void TMessageBox(string text, string caption, int delay)
        {
          
                runTMessageBox(text, caption, delay);
          
        }
        public async Task runTMessageBox(string text, string caption, int delay)
        {
            //maximum text length 55
            MessageForm mForm = new MessageForm();
            mForm.Text = caption;
            mForm.yesno = false;
            mForm.message = text;
            mForm.delay = delay;
            mForm.StartPosition = FormStartPosition.CenterParent;
            mForm.ShowDialog();
        }

        public void OKMessageBox(string text, string caption)
        {

            //maximum text length 55
            MessageForm mForm = new MessageForm();
            mForm.Text = caption;
            mForm.yesno = false;
            mForm.message = text;
            mForm.delay = 0;
            mForm.StartPosition = FormStartPosition.CenterParent;
            mForm.ShowDialog();
            while (!mForm.reply)
            {

            }
            mForm.Dispose();
        }
        public DialogResult ynMessageBox(string text, string caption)
        {
            DialogResult res;
            //maximum text length 55
            MessageForm mForm = new MessageForm();
            mForm.Text = caption;
            mForm.yesno = true;
            mForm.message = text;
            mForm.delay = 0;
            mForm.StartPosition = FormStartPosition.CenterParent;
            mForm.ShowDialog();
            while (!mForm.reply)
            {

            }
            if (mForm.YES)
            {
                res = DialogResult.Yes;
            }
            else
            {
                res = DialogResult.No;

            }
            mForm.Dispose();
            return res;

        }

        public int editMessageBox(string text, string caption)
        {

            //maximum text length 45
            MessageForm mForm = new MessageForm();
            mForm.Text = caption;
            mForm.yesno = false;
            mForm.message = text;
            mForm.delay = 0;
            mForm.editbuttons = true;
            mForm.StartPosition = FormStartPosition.CenterParent;
            mForm.ShowDialog();
            int R = 0;
            while (!mForm.reply)
            {

            }
            if (mForm.button == 1)
            {
                R = 1; //add
            }
            else if (mForm.button == 2)
            {
                R = 2; //edit

            }
            else if (mForm.button == 3)
            {
                R = 3; //delete
            }
            else
            {
                R = 0;
            }
            mForm.Dispose();
            return R;


        }

        public async Task<bool> IsUrlReachable(string url)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsFtpReachable(string ftpUrl)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using var response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
