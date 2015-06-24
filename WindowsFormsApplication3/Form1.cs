using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        private String fromLanguage = "";
        private String toLanguage = "";
        private String tts = "";
        private int box2index = 1;
        public Form1()
        {
            InitializeComponent();
            this.InitialCombobox(0);
        }

        private void 打开_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = "srt文件|*.srt";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                String fName = openFileDialog.FileName;
                new Thread((ThreadStart)delegate//新线程运行Form2
                {
                    Application.Run(new Form2(fName));
                }).Start();
            }
        }

        private void 退出_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void google_Click(object sender, EventArgs e)
        {
            baidu.Checked = false;
            google.Checked = true;
            this.InitialCombobox(0);
        }

        private void baidu_Click(object sender, EventArgs e)
        {
            google.Checked = false;
            baidu.Checked = true;
            this.InitialCombobox(1);
        }

        private void 关于_Click(object sender, EventArgs e)
        {
            MessageBox.Show("支持Google与Baidu翻译，可以对srt格式英文字幕进行翻译！", "关于");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String strToTranslate = richTextBox1.Text;
            if (google.Checked == true)
            {
                richTextBox2.Text = googleTranslate(strToTranslate, fromLanguage, toLanguage);
            }
            else
            {
                richTextBox2.Text = baiduTranslate(strToTranslate, fromLanguage, toLanguage);
            }
            box2index = this.comboBox2.SelectedIndex;//设置comboBox默认值
        }
        
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fromLanguage = this.comboBox1.SelectedValue.ToString();//设置将要翻译的语言
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            toLanguage = this.comboBox2.SelectedValue.ToString();//设置翻译后的语言
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            richTextBox1.Paste();
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            richTextBox1.Cut();
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            richTextBox2.Copy();
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            richTextBox2.Paste();
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            richTextBox2.Cut();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            new Thread((ThreadStart)delegate
            {
                this.fromS();
            }).Start();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            new Thread((ThreadStart)delegate
            {
                this.toS();
            }).Start();
        }
        public String baiduTranslate(String strToTranslate, String fromLanguage, String toLanguage)
        {
            String translatedStr = "";
            String encodedStr = HttpUtility.UrlEncode(strToTranslate);
            String baiduTransBaseUrl = "http://openapi.baidu.com/public/2.0/bmt/translate?client_id=YEBdxswo4u7Oe21Y2LYcfz9I&q=";
            String baiduTransUrl = baiduTransBaseUrl;
            baiduTransUrl += encodedStr;
            baiduTransUrl += "&from=" + fromLanguage;// source   language
            baiduTransUrl += "&to=" + toLanguage;  // to       language
            crifanLib cri = new crifanLib();        //使用crifanlib库
            try
            {
                String transRetHtml = cri.getUrlRespHtml(baiduTransUrl);
                String[] str = transRetHtml.Split(new String[] { "\"dst\"" }, StringSplitOptions.RemoveEmptyEntries);
                cri.extractSingleStr(":\"(.+?)\"}]}", str[1], out translatedStr);
                translatedStr = System.Text.RegularExpressions.Regex.Unescape(translatedStr);//解码escape形式的Unicode，即将反斜杠u加上数字的字符

                //获得fromLanguage语言
                int index = transRetHtml.IndexOf("\",\"to\"");
                String fl = transRetHtml.Substring(9,index-9);
                tts = "http://tts.baidu.com/text2audio?lan=" + fl;
                tts += "&ie=UTF-8&text=" + encodedStr;
            }
            
            catch { }
            return translatedStr;
        }

        public string googleTranslate(String strToTranslate, String fromLanguage, String toLanguage)
        {
            String translatedStr = "";
            String encodedStr = HttpUtility.UrlEncode(strToTranslate);
            String googleTransBaseUrl = "http://translate.google.cn/translate_a/single?client=t";
            String googleTransUrl = googleTransBaseUrl;
            googleTransUrl += "&sl=" + fromLanguage;// source   language
            googleTransUrl += "&tl=" + toLanguage;  // to       language
            googleTransUrl += "&hl=zh-CN&dt=bd&dt=ex&dt=ld&dt=md&dt=qc&dt=rw&dt=rm&dt=ss&dt=t&dt=at&ie=UTF-8&oe=UTF-8&q=";
            googleTransUrl += encodedStr;
            crifanLib cri = new crifanLib();        //使用crifanlib库
            try
            {
                String transRetHtml = cri.getUrlRespHtml(googleTransUrl);
                int index1 = transRetHtml.IndexOf("\",,,");
                String str1 = transRetHtml.Substring(0,index1);
                int index2 = str1.IndexOf(",[,,");
                if (index2 > 0)
                {
                    str1 = str1.Substring(3, index2 - 3);
                }
                else
                {
                    str1 = str1.Substring(3, str1.Length - 3);
                }
                String[] str = str1.Split(new String[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
                String ss = "";
                foreach (String s in str)
                {
                    if (cri.extractSingleStr("\"(.+?)\",\".+?\"", s, out ss))
                    {
                        translatedStr += ss;
                    }
                }
                
                String fl = "en";
                String str2 = transRetHtml.Substring(index1 - 7, 8);
                cri.extractSingleStr("\"(.+?)\"", str2, out fl);
                tts = "http://translate.google.cn/translate_tts?ie=UTF-8&tl=" + fl;
                tts += "&total=1&prev=input&q=";
            }
            catch { }
            return translatedStr;
        }

        private void fromS()//原文发音
        {
            this.label3.Enabled = false;
            if (google.Checked == true)
            {
                String encodedStr = HttpUtility.UrlEncode(this.richTextBox1.Text);
                String[] str = encodedStr.Split('.');
                foreach (String s in str)
                {
                    Task task = new Task(() => { new PlayMp3Url(tts + s); });
                    task.Start();
                    task.Wait();
                }
            }
            else
            {
                new PlayMp3Url(tts);
            }
            this.label3.Enabled = true;
        }
        private void toS()//译文发音
        {
            this.label4.Enabled = false;
            String encodedStr = HttpUtility.UrlEncode(richTextBox2.Text);
            String url = "";
            if (google.Checked == true)
            {
                url = "http://translate.google.cn/translate_tts?ie=UTF-8&tl=" + toLanguage;
                url += "&total=1&prev=input&q=";
                String[] str = encodedStr.Split('.');
                foreach (String s in str)
                {
                    Task task = new Task(() => { new PlayMp3Url(url + s); });
                    task.Start();
                    task.Wait();
                }
            }
            else
            {
                url = "http://tts.baidu.com/text2audio?lan=" + toLanguage;
                url += "&ie=UTF-8&text=" + encodedStr;
                new PlayMp3Url(url);
            }
            this.label4.Enabled = true;
        }
        private void InitialCombobox(int n)
        {
            
            switch (n)
            {
                case 0://谷歌

                    DataTable table1 = new DataTable();
                    table1.Columns.Add("Language");
                    table1.Columns.Add("Value");
                    //添加google翻译对应的代码
                    table1.Rows.Add(new object[] { "自动检测", "auto" });
                    table1.Rows.Add(new object[] { "English", "en" });
                    table1.Rows.Add(new object[] { "简体中文", "zh-CN" });
                    table1.Rows.Add(new object[] { "日语", "ja" });
                    table1.Rows.Add(new object[] { "韩语", "ko" });
                    table1.Rows.Add(new object[] { "法语", "fr" });
                    table1.Rows.Add(new object[] { "德语", "de" });
                    table1.Rows.Add(new object[] { "俄语", "ru" });
                    table1.Rows.Add(new object[] { "阿拉伯语", "ar" });
                    table1.Rows.Add(new object[] { "西班牙语", "es" });
                    DataTable table2 = table1.Copy();
                    table2.Rows.Remove(table2.Rows[0]);//删除自动检测
                    //绑定table表
                    this.comboBox1.DisplayMember = "Language";
                    this.comboBox1.ValueMember = "Value";
                    this.comboBox1.DataSource = table1;

                    this.comboBox2.DisplayMember = "Language";
                    this.comboBox2.ValueMember = "Value";
                    this.comboBox2.DataSource = table2;
                    this.comboBox2.SelectedIndex = box2index; //设置默认显示
                    break;

                case 1://百度
                    DataTable table3 = new DataTable();
                    table3.Columns.Add("Language");
                    table3.Columns.Add("Value");
                    //添加baidu翻译对应的代码
                    table3.Rows.Add(new object[] { "自动检测", "auto" });
                    table3.Rows.Add(new object[] { "English", "en" });
                    table3.Rows.Add(new object[] { "简体中文", "zh" });
                    table3.Rows.Add(new object[] { "日语", "jp" });
                    table3.Rows.Add(new object[] { "韩语", "kor" });
                    table3.Rows.Add(new object[] { "法语", "fra" });
                    table3.Rows.Add(new object[] { "德语", "de" });
                    table3.Rows.Add(new object[] { "俄语", "ru" });
                    table3.Rows.Add(new object[] { "阿拉伯语", "ara" });
                    table3.Rows.Add(new object[] { "西班牙语", "spa" });
                    DataTable table4 = table3.Copy();
                    table4.Rows.Remove(table4.Rows[0]);//删除自动检测
                    //绑定table表
                    this.comboBox1.DisplayMember = "Language";
                    this.comboBox1.ValueMember = "Value";
                    this.comboBox1.DataSource = table3;

                    this.comboBox2.DisplayMember = "Language";
                    this.comboBox2.ValueMember = "Value";
                    this.comboBox2.DataSource = table4;
                    this.comboBox2.SelectedIndex = box2index; //设置默认显示
                    break;
                /*
                case 2:
                    DataTable table5 = new DataTable();
                    table5.Columns.Add("Language");
                    table5.Columns.Add("Value");
                    //添加bing翻译对应的代码
                    table5.Rows.Add(new object[] { "自动检测", "" });
                    table5.Rows.Add(new object[] { "English", "en" });
                    table5.Rows.Add(new object[] { "简体中文", "zh-CHS" });
                    table5.Rows.Add(new object[] { "日语", "ja" });
                    table5.Rows.Add(new object[] { "韩语", "ko" });
                    table5.Rows.Add(new object[] { "法语", "fr" });
                    table5.Rows.Add(new object[] { "德语", "de" });
                    table5.Rows.Add(new object[] { "俄语", "ru" });
                    table5.Rows.Add(new object[] { "阿拉伯语", "ar" });
                    table5.Rows.Add(new object[] { "西班牙语", "es" });
                    DataTable table6 = table5.Copy();
                    table6.Rows.Remove(table6.Rows[0]);//删除自动检测
                    //绑定table表
                    this.comboBox1.DisplayMember = "Language";
                    this.comboBox1.ValueMember = "Value";
                    this.comboBox1.DataSource = table5;

                    this.comboBox2.DisplayMember = "Language";
                    this.comboBox2.ValueMember = "Value";
                    this.comboBox2.DataSource = table6;
                    this.comboBox2.SelectedIndex = 1; //设置默认显示“简体中文”
                    break;
                */
                default:
                    break;
            }
            
        }
        /*
        public String bingTranslate(String strToTranslate, String fromLanguage, String toLanguage)
        {
            String translatedStr = "";
            String transRetHtml = "";
            String encodedStr = HttpUtility.UrlEncode(strToTranslate);
            String bingTransBaseUrl = "http://api.microsofttranslator.com/v2/ajax.svc/TranslateArray2?appId=%22TB-kMleEc74CtS3O-DofOqx3IFo9TEXNaRaHsVc5Po4DTY66oG6IDGj4yS3v_S1kj%22&texts=";
            String bingTransUrl = bingTransBaseUrl;
            bingTransUrl += "%5B%22" + encodedStr + "%22%5D";
            bingTransUrl += "&from=" + fromLanguage;// source   language
            bingTransUrl += "&to=" + toLanguage;  // to       language
            crifanLib cri = new crifanLib();        //使用crifanlib库
            try
            {
                transRetHtml = cri.getUrlRespHtml(bingTransUrl);
                String[] str = transRetHtml.Split(new String[] { "TranslatedText" }, StringSplitOptions.RemoveEmptyEntries);
                //输入"晚上好"，则str[1]为 :"Good evening","
                cri.extractSingleStr(":\"(.+?)\",\"", str[1], out translatedStr);
            }
            catch { }
            return translatedStr;

        }
        */
    }
}
