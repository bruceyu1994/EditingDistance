using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditingDistance
{
    public partial class MainForm : Form
    {
        public const int DELETE = 3;
        public const int INSERT = 2;
        public const int SUBSTITUTE = 1;
        public const int NOOPERATION = 0;
        private int[,] operation, dist;  //操作，编辑距离
        private string input_a = "", input_b = "";
        private int len_a, len_b;   //字符串a,b的长度

        private int offsetX = 10;    //起点相对X坐标的偏移值
        private int offsetY = 50;     //起点相对Y坐标的偏移值
        private int width = 30;   //每一格的宽度
        private int height = 30;  //每一格的高度

        private Font font = new Font("宋体", 10f, FontStyle.Regular);   //字体，字体大小，样式
        private SolidBrush brush = new SolidBrush(Color.Black);   //创建画刷，黑色

        private Thread t1 = null;  //运行算法的线程
        private volatile bool pause = false;

        public MainForm()
        {
            InitializeComponent();

            Graphics g = pictureBox_show.CreateGraphics();
            g.Clear(Color.White);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //textBox3.Clear();
                //只要有一个文本框不为空就进行操作
                if (textBox1.TextLength > 0 || textBox2.TextLength > 0)
                {
                    //将控件中的字符串分别保存到input_a和input_b中
                    input_a = textBox1.Text;
                    input_b = textBox2.Text;
                    //保存两个字符串长度
                    len_a = input_a.Length;
                    len_b = input_b.Length;

                    //线程下运行算法
                    if (t1 != null && t1.IsAlive)
                        t1.Abort();
                    t1 = new Thread(new ThreadStart(ThreadMethod));
                    t1.Start();

                }
            }
        }

        private void ThreadMethod()
        {
            //执行算法
            editDistance(input_a, input_b);

            //展示算法过程
            showAlgorithm();
        }


        /// <summary>
        /// 动态规划算法求编辑距离
        /// 计算a->b的编辑距离 
        /// dis[i][j]表示长度为i的字符串变为长度为j的字符串需要的编辑距离 
        /// operation[i][j]表示变换过程中对应的操作 
        /// 0:相同，无需操作; 1:字符替换; 2:插入; 3:删除 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void editDistance(string a, string b)
        {
            dist = new int[len_a + 1, len_b + 1];
            operation = new int[len_a + 1, len_b + 1];

            //初始化编辑距离和操作
            dist[0, 0] = 0;
            operation[0, 0] = 0;

            for (int i = 1; i <= len_a; i++)
            {
                dist[i, 0] = i;
                operation[i, 0] = DELETE;   //删除操作
            }

            for (int j = 1; j <= len_b; j++)
            {
                dist[0, j] = j;
                operation[0, j] = INSERT;   //插入操作
            }

            for (int i = 1; i <= len_a; i++)
            {
                for (int j = 1; j <= len_b; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                    int delete = dist[i - 1, j] + 1;         //删除操作
                    int insert = dist[i, j - 1] + 1;         //插入操作
                    int substitute = dist[i - 1, j - 1] + cost;  //替换操作

                    dist[i, j] = Math.Min(Math.Min(insert, substitute), delete);

                    if (dist[i, j] == delete)   //删除
                    {
                        operation[i, j] = DELETE;
                    }
                    else if (dist[i, j] == substitute) //替换
                    {
                        if (cost == 1)   //不相同，则替换
                            operation[i, j] = SUBSTITUTE;
                        else  //相同，则不做任何操作
                            operation[i, j] = NOOPERATION;
                    }
                    else if (dist[i, j] == insert) //插入
                    {
                        operation[i, j] = INSERT;
                    }
                }
            }
        }


        //public void backtrace(int[,] operation, string a, string b)
        //{
        //    string operation_str = "";        //操作说明保存变量
        //    int insertion = 0, deletion = 0, substitution = 0;
        //    int i, j;
        //    for (i = len_a, j = len_b; i >= 0 && j > 0; )
        //    {
        //        switch (operation[i, j])
        //        {
        //            case NOOPERATION:
        //                operation_str = "pos " + i + " No Operation \r\n" + operation_str;
        //                //textBox3.AppendText("pos " + i + " No Operation\n");
        //                i--;
        //                j--;
        //                continue;
        //            case SUBSTITUTE:
        //                operation_str = "pos " + i + "： " + a[i - 1] + " substitute for " + b[j - 1] + "\r\n" + operation_str;
        //                //textBox3.AppendText("pos " + i + "： " + a[i - 1] + " substitute for " + b[j - 1] + "\n");
        //                i--;
        //                j--;
        //                substitution++;
        //                continue;
        //            case INSERT:
        //                operation_str = "pos " + i + "： insert " + b[j - 1] + "\r\n" + operation_str;
        //                //textBox3.AppendText("pos " + i + "： insert " + b[j - 1] + "\n");
        //                j--;
        //                insertion++;
        //                continue;
        //            case DELETE:
        //                operation_str = "pos " + i + " delete " + a[i - 1] + "\r\n" + operation_str;
        //                //textBox3.AppendText("pos " + i + " delete " + a[i - 1] + "\n");
        //                i--;
        //                deletion++;
        //                continue;
        //        }

        //        if (i == 0)
        //            break;

        //    }
        //    //textBox3.AppendText(operation_str);
        //    //textBox3.AppendText("\ninsert:" + insertion + ",delete:" + deletion + ",substitute:" +
        //    //substitution + "\n");
        //}

        /// <summary>
        /// 通过回溯法，构造编辑距离的最优解，并将结果打印到界面
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="g"></param>
        public void backtrace(int[,] operation, string a, string b, Graphics g)
        {
            string operation_str = "";        //操作说明保存变量
            int insertion = 0, deletion = 0, substitution = 0;  //
            int i, j, k;
            k = Math.Max(len_a, len_b);
            for (i = len_a, j = len_b; i > 0 || j > 0; )
            {
                //改变对应操作方格位置的颜色
                Rectangle rect = new Rectangle(new Point(offsetX + (j + 1) * width, offsetY + height * (i + 1)), new Size(width, height));
                g.FillRectangle(Brushes.Yellow, rect); //填充
                g.Flush(); // 强制执行所有挂起的图形操作并立即返回而不等待操作完成。

                PointF point = new PointF(offsetX + (j + 1) * width, offsetY + height * (i + 1));
                SolidBrush brush2 = new SolidBrush(Color.Red);   //创建画刷
                g.DrawString(dist[i, j] + "", font, brush2, point);

                switch (operation[i, j])
                {
                    case NOOPERATION:
                        operation_str = "pos " + i + " No Operation \r\n";
                        //textBox3.AppendText("pos " + i + " No Operation\n");
                        i--;
                        j--;
                        break;
                    case SUBSTITUTE:
                        operation_str = "pos " + i + "： " + a[i - 1] + " substitute for " + b[j - 1] + "\r\n";
                        //textBox3.AppendText("pos " + i + "： " + a[i - 1] + " substitute for " + b[j - 1] + "\n");
                        i--;
                        j--;
                        substitution++;
                        break;
                    case INSERT:
                        operation_str = "pos " + i + "： insert " + b[j - 1] + "\r\n";
                        //textBox3.AppendText("pos " + i + "： insert " + b[j - 1] + "\n");
                        j--;  //插入操作则左移
                        insertion++;
                        break;
                    case DELETE:
                        operation_str = "pos " + i + " delete " + a[i - 1] + "\r\n";
                        //textBox3.AppendText("pos " + i + " delete " + a[i - 1] + "\n");
                        i--;  //删除操作则上移
                        deletion++;
                        break;
                }

                PointF point2 = new PointF(offsetX + (len_b + 5) * width, offsetY + (k--) * height);
                g.DrawString(operation_str, font, brush, point2);

                Thread.Sleep(1000);  //线程暂停1s

            }

            PointF p = new PointF(offsetX + (len_b + 5) * width, offsetY + k * height);
            g.DrawString("插入操作：" + insertion + "次；" + "删除操作：" + deletion + "次；" + "替换操作：" + substitution + "次；", 
                font, brush, p);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        //算法动态展示
        public void showAlgorithm()
        {
            //调用pictureBox_show控件的CreateGraphics方法
            Graphics g = pictureBox_show.CreateGraphics();
            g.Clear(Color.White);
            int rowLine_num = len_a + 3;  //行的线数
            int colLine_num = len_b + 3;  //列的线数
            int row_width = (colLine_num - 1) * width;//总表宽
            int col_height = (rowLine_num - 1) * height;  //总表高

            //画行线
            for (int i = 0; i < rowLine_num; i++)
            {
                g.DrawLine(new Pen(Color.Blue, 2), offsetX, offsetY + i * height, row_width + offsetX, offsetY + i * height);
                Thread.Sleep(200);
            }

            //画列线
            for (int j = 0; j < colLine_num; j++)
            {
                g.DrawLine(new Pen(Color.Blue, 2), offsetX + j * width, offsetY, offsetX + j * width, offsetY + col_height);
                Thread.Sleep(200);
            }

            //演示算法过程

            //画字符串a
            for (int i = 2; i < rowLine_num - 1; i++)
            {
                PointF point = new PointF(offsetX, offsetY + i * height);
                g.DrawString(input_a[i - 2] + "", font, brush, point);
                Thread.Sleep(200);
            }

            //画字符串b
            for (int j = 2; j < colLine_num - 1; j++)
            {
                PointF point = new PointF(offsetX + j * width, offsetY);
                g.DrawString(input_b[j - 2] + "", font, brush, point);
                Thread.Sleep(200);
            }

            //初始化第第0列和第0行的编辑距离
            for (int i = 1; i < rowLine_num - 1; i++)
            {
                PointF point = new PointF(offsetX + width, offsetY + i * height);
                g.DrawString((i - 1) + "", font, brush, point);
                Thread.Sleep(200);
            }

            for (int j = 1; j < colLine_num - 1; j++)
            {
                PointF point = new PointF(offsetX + j * width, offsetY + height);
                g.DrawString((j - 1) + "", font, brush, point);
                Thread.Sleep(200);
            }

            //画出余下编辑距离
            for (int i = 1; i <= len_a; i++)
            {
                for (int j = 1; j <= len_b; j++)
                {
                    //textBox3.AppendText(dist[i, j] + " ");
                    PointF point = new PointF(offsetX + (j + 1) * width, offsetY + height * (i + 1));
                    g.DrawString(dist[i, j] + "", font, brush, point);
                    Thread.Sleep(1000);
                }
                //textBox3.AppendText("\n");
            }


            //回溯，构造最优解
            backtrace(operation, input_a, input_b, g);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            textBox2_KeyDown(sender, e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        public RectangleF point { get; set; }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (t1 != null && t1.IsAlive)  //关闭线程
                t1.Abort();
        }

    }
}
