using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace WindowsFormsApplication3
{
    public partial class Form2 : Form
    {
        String openPath;
        public Form2(String value)
        {
            openPath = value;
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true; //获取或设置一个值，该值指示BackgroundWorker能否报告进度更新
            backgroundWorker1.WorkerSupportsCancellation = true;//当此属性为 true 时，可以调用 CancelAsync 方法中断后台操作
            backgroundWorker1.RunWorkerAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //这里，就是后台进程开始工作时，调用工作函数的地方。你可以把你现有的处理函数写在这儿。
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            translate();
        }
        //这里就是通过响应消息，来处理界面的显示工作
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //ReportProgress 会调用到这里，此处可以进行自定义报告方式
            progressBar1.Value = e.ProgressPercentage;
            label1.Text = e.ProgressPercentage + "%";
        }
        //这里是后台工作完成后的消息处理，可以在这里进行后续的处理工作。
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            this.Text = "翻译完成";
        }
        public void translate()
        {
            int num = 0,i = 0;
            String newString = "";
            String str = "";
            String str1 = "";
            String str2 = "";
            string savePath = openPath.Insert(openPath.LastIndexOf('.'),"_1");
            Form1 form1 = new Form1();
            Regex rx = new Regex(@"\d+:\d{2}:\d{2},\d{3}\s-->\s\d+:\d{2}:\d{2},\d{3}");
            try
            {
                StreamReader sr = new StreamReader(openPath, Encoding.GetEncoding("utf-8"));     //源字幕文件位置
                StreamReader ss = new StreamReader(openPath, Encoding.GetEncoding("utf-8"));     //源字幕文件位置
                while (ss.ReadLine() != null)
                {
                    num++;
                }
                ss.Close();
                while ((str = sr.ReadLine()) != null)
                {
                    ++i;
                    newString += str + "\r\n";
                    Match found = rx.Match(str);
                    if (found.Success)
                    {
                        str1 = sr.ReadLine();
                        if (!(str2 = sr.ReadLine()).Equals(""))
                        {
                            newString += form1.googleTranslate(str1 + " " + str2, "en", "zh-CN") + "\r\n" + str1 + " " + str2 + "\r\n";
                        }
                        else {
                            newString += form1.googleTranslate(str1, "en", "zh-CN") + "\r\n" + str1 + "\r\n\r\n";
                        }
                        i += 2;
                    }
                    backgroundWorker1.ReportProgress(i * 100 / num);//报告完成进度
                    Thread.Sleep(0);//后台线程交出时间片
                }
                sr.Close();
                StreamWriter sw = new StreamWriter(savePath, true, Encoding.GetEncoding("utf-8"));
                sw.Write(newString);
                sw.Close();
            }
            catch { }
            
        }

    }
}
