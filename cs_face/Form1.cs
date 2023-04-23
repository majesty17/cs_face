using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using HtmlAgilityPack;

namespace cs_face
{
    public partial class Form1 : Form
    {
        private IntPtr hi_ptr = IntPtr.Zero;
        private IntPtr last_for_ptr = IntPtr.Zero;
        private IntPtr myself_ptr = Process.GetCurrentProcess().MainWindowHandle;


        private int currentPage = 0;
        private int currentItemCount = 0;

        public Form1()
        {
            InitializeComponent();
        }

        //
        private void Form1_Load(object sender, EventArgs e)
        {
            //启动时找hi的句柄
            Process[] ps = Process.GetProcessesByName("infoflow");
            if (ps.Length > 0) {
                //有多个同名进程的时候，找到第一个窗口标题不为空的
                int hi_pid = -1;
                for (int i = 0; i < ps.Length; i++)
                {
                    if (ps[i].MainWindowTitle != null && ps[i].MainWindowTitle != "")
                    {
                        hi_pid = ps[i].Id;
                        break;
                    }
                }

                if (hi_pid==-1)
                {
                    MessageBox.Show("未找到正确的hi窗口!!");
                    this.Close();
                }

                hi_ptr = WinAPI.GetMainWindowHandle(hi_pid);


                if ((int)hi_ptr != 0)
                {
                    //开始定时任务
                    timer1.Start();
                }
                else {
                    MessageBox.Show("未检测到hi的窗口~");
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("未检测到hi的进程~");
                this.Close();
            }
            //动态调整grid
        }



        //search
        private void button_search_Click(object sender, EventArgs e)
        {
            currentPage = 1;//置当前页为1
            string keyword = textBox1.Text.Trim();
            if (keyword.Length == 0)
                return;

            PictureBox[] pics =null;
            try
            {
                pics = get_pics(keyword,currentPage);
            }
            catch (Exception ex) {
                MessageBox.Show("图片获取失败||没有搜到！");
                return;
            }

            tableLayoutPanel1.Controls.Clear();

            for (int i = 0; i < pics.Length; i++) {

                tableLayoutPanel1.Controls.Add(pics[i]);
                pics[i].Height = pics[i].Width;
                
            }
            currentItemCount = pics.Length;

 
            return;


        }


        //窗口自动吸附
        private void timer1_Tick(object sender, EventArgs e)
        {
            WinAPI.RECT rec = new WinAPI.RECT();
            if (WinAPI.GetWindowRect(this.hi_ptr, ref rec) == false)
                return;

            //还原窗口
            this.WindowState = FormWindowState.Normal;


            Console.WriteLine(rec.Right + "," + rec.Top);

            int right = rec.Right;
            int top = rec.Top;

            this.Left = right + 2;
            this.Top = top;

            checkFore();
        }



        //下载图片
        private PictureBox[] get_pics(string keyword,int page)
        {
            WebClient web = new WebClient();
            //string url_k = "http://md.itlun.cn/plus/search.php?kwtype=1&searchtype=titlekeyword&q=" + UrlEncode(keyword) + "&PageNo="+page;
            string url_k = "https://www.pkdoutu.com/search?keyword=" + keyword;
            string content = web.DownloadString(url_k);


            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(content);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectSingleNode("//div[@class='random_picture']").SelectNodes("//img[@referrerpolicy='no-referrer']");


            int show_count = Math.Min(nodes.Count, 30);
            PictureBox[] ret = new PictureBox[show_count];
            for (int i = 0; i < show_count; i++)
            {
                string name = nodes[i].InnerText;
                //string url = nodes[i].SelectSingleNode("/img").InnerHtml;//GetAttributeValue("src", "/");
                string url = nodes[i].GetAttributeValue("data-original", "/"); //   .InnerHtml.Split(new char[] { '"' })[1].Replace("//", "http://");

                ret[i] = new PictureBox();
                ret[i].ImageLocation = url;
                ret[i].Tag = name;
                ret[i].SizeMode = PictureBoxSizeMode.Zoom;
                ret[i].Dock = DockStyle.Fill;
                ret[i].Margin = ret[i].Padding = new Padding(0);
                ret[i].MouseHover += new EventHandler(Pic_MouseHover);
                ret[i].MouseDoubleClick += new MouseEventHandler(Pic_MouseDoubleClick);

            }

            return ret;

        }



        //pic双击
        void Pic_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            string url = pic.ImageLocation.Replace("-lp","");

            Clipboard.SetImage(pic.Image);
            WinAPI.SetForegroundWindow(hi_ptr);
            WinAPI.paste();
        }

        //pic悬停
        void Pic_MouseHover(object sender, EventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            string name = (string)pic.Tag;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(pic, name);

        }

        //encode
        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.Default.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16).ToUpper());
            }

            return (sb.ToString());
        }



        private void checkFore() {
            IntPtr now_for_ptr=WinAPI.GetForegroundWindow();
            //如果hi被激活并且上一次不是hi，也不是自己：则把自己激活，同时再次把hi激活
            if (now_for_ptr == hi_ptr && hi_ptr != last_for_ptr && last_for_ptr != myself_ptr)
            {
                this.Activate();
                WinAPI.SetForegroundWindow(hi_ptr);
            }
            last_for_ptr = now_for_ptr;
        }



        //last
        private void button_left_Click(object sender, EventArgs e)
        {

        }


        //next
        private void button_right_Click(object sender, EventArgs e)
        {
            if (currentItemCount < 20) {
                MessageBox.Show("没有下一页了！");
                return;
            }

            currentPage = currentPage + 1;
            string keyword = textBox1.Text.Trim();
            if (keyword.Length == 0)
                return;

            PictureBox[] pics = null;
            try
            {
                pics = get_pics(keyword,currentPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("图片获取失败||没有搜到！");
                return;
            }

            tableLayoutPanel1.Controls.Clear();

            for (int i = 0; i < pics.Length; i++)
            {

                tableLayoutPanel1.Controls.Add(pics[i]);
                pics[i].Height = pics[i].Width;

            }
            currentItemCount = pics.Length;
        }

        //
    }
}
