using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace ProcessExec
{
    public partial class Form1 : Form
    {
        RemoteProcessControl process;
        private Thread threadProcess;
        private delegate void SetTextCallback(string message);

        private void Listen(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);

            //txt_recive.Text = lastMsg; //Erro devido a multi-threads. Problema resolvido aqui:
            //https://stackoverflow.com/questions/14226921/threading-copied-error-settextcallback
            //http://www.macoratti.net/14/07/c_gptwf2.htm
            if (this.txt_recive.InvokeRequired)
            {
                if (e.Data == null) { return; }
                try
                {
                    SetTextCallback x = new SetTextCallback((texto) => { /*txt_recive.Text = texto;*/txt_recive.AppendText(texto); txt_recive.AppendText(Environment.NewLine); });
                    this.Invoke(x, new object[] { e.Data });
                }
                catch
                {

                }
            }
        }
        public Form1()
        {
            //process = new RemoteProcessControl(@"node.exe", @"C:\Users\willyan\Documents\willyan\NodeJSWebDir\local-api\dist\index.js", Listen);
            process = new RemoteProcessControl(@"node.exe", @"C:\mro\local-api\dist\index.js", Listen);
            InitializeComponent();
        }
        private void btn_test_Click(object sender, EventArgs e)
        {
            threadProcess = new Thread(new ThreadStart(process.Start));
            threadProcess.Priority = ThreadPriority.Normal;
            threadProcess.Start();
        }
        private void btn_stop_Click(object sender, EventArgs e)
        {
            process.Kill();
        }
        private void btn_exit_Click(object sender, EventArgs e)
        {
            process.Stop();
        }
        private void btn_send_Click(object sender, EventArgs e)
        {
            process.Write(txt_send.Text);
        }
        private void btn_clear_Click(object sender, EventArgs e)
        {
            txt_recive.Text = "";
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                threadProcess = new Thread(new ThreadStart(process.Start));
                threadProcess.Priority = ThreadPriority.Normal;
                threadProcess.Start();
            }
            catch(Exception ex)
            {
                txt_recive.Text = ex.Message;
            }
        }
        private void Form1_Leave(object sender, EventArgs e)
        {
            process.Stop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //process.Stop();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            process.Stop();
        }
    }

    public class RemoteProcessControl
    {
        //https://gist.github.com/elerch/5628117
        private string cmd = "";
        private string arg = "";
        Process proc;
        private DataReceivedEventHandler cb;

        public RemoteProcessControl(string cmd, string arg, DataReceivedEventHandler cb)
        {
            this.cmd = cmd;
            this.arg = arg;
            this.cb = cb;
        }

        private void Init()
        {
            proc = new System.Diagnostics.Process();
            proc.StartInfo.Verb = "runas";

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.Arguments = arg;
        }

        static void Listen(object sender, System.Diagnostics.DataReceivedEventArgs e)

        {
            Console.WriteLine("Hello: " + e.Data);
        }

        public void Write(string data)
        {
            try
            {
                proc.StandardInput.WriteLine(data);
            }
            catch(Exception ex)
            {
                //throw ex;
            }
        }

        public void Start()
        {
            if(proc != null){ return; }
            Init();
            proc.Start();
            proc.BeginOutputReadLine();
            //proc.OutputDataReceived += Listen;
            proc.OutputDataReceived += cb;
            proc.WaitForExit();

        }

        public void Stop()
        {
            try
            {
                Write("STOP_CMD");
                proc = null;
            }
            catch(Exception ex)
            {
                //throw ex;
            }
        }

        public void Kill()
        {
            try
            {
                proc.Kill();
                proc = null;
            }
            catch (Exception ex)
            {
                //throw ex;
            }

        }

    }

}