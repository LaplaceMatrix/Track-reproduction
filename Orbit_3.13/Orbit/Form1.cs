using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Runtime.InteropServices;

namespace Orbit
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);//系统dll导入ini写函数
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);//系统dll导入ini读函数
        string PortData = System.AppDomain.CurrentDomain.BaseDirectory + "data.ini";//ini文件名
        StringBuilder temp = new StringBuilder(255);//存储读出ini内容变量
        string CurrentBaudRate;
        string CurrentPortName;

        string Path1 = System.AppDomain.CurrentDomain.BaseDirectory + @"Orbit";//路径
        string Path2 = System.AppDomain.CurrentDomain.BaseDirectory + @"Track";//赛道
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;       //关多线程检测
        }

 /*****************************************************************************************///窗口
        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.BackgroundImage = Properties.Resources.BlueTooth2;
            GetPrivateProfileString("PortData", "BaudRate", "115200", temp, 256, PortData);//读取ini值，默认是COM1
            comboBox2.Text = temp.ToString();//初始化
            GetPrivateProfileString("PortData", "PortName", "COM1", temp, 256, PortData);//读取ini值，默认是COM1
            comboBox1.Items.Add(temp);

            if (Directory.Exists(Path1)) { ;}
            else { DirectoryInfo Di = Directory.CreateDirectory(Path1); }
            if (Directory.Exists(Path2)) { ;}
            else { DirectoryInfo Di = Directory.CreateDirectory(Path2); }
            OX_Ever = BackX + Back_XMax / 2;
            OY_Ever = BackY + Back_YMax / 2;
            OX = OX_Ever;
            OY = OY_Ever;
            TrackChange_X.Add(0);
            TrackChange_Y.Add(0);
            OrbitChange_X1.Add(0);
            OrbitChange_Y1.Add(0);
            OrbitChange_X2.Add(0);
            OrbitChange_Y2.Add(0);
            Back_XMax=this.Size .Width -BackX-35;
            Back_YMax = this.Size.Height - BackY - 60;
            ReadOrbit_OX = BackX + Back_XMax / 2;
            ReadOrbit_OY = BackY + Back_YMax / 2;
            ReadTrack_OX = BackX + Back_XMax / 2;
            ReadTrack_OY = BackY + Back_YMax / 2;

            //
            ChangeX.RemoveRange(0, ChangeX.Count);//清除轨迹
            ChangeY.RemoveRange(0, ChangeX.Count);
            OrbitX.RemoveRange(0, OrbitX.Count);
            OrbitY.RemoveRange(0, OrbitY.Count);

            ReadOrbit_X.RemoveRange(0, ReadOrbit_X.Count);//清除读取赛道
            ReadOrbit_Y.RemoveRange(0, ReadOrbit_Y.Count);
            OrbitChange_X1.RemoveRange(0, OrbitChange_X1.Count);
            OrbitChange_Y1.RemoveRange(0, OrbitChange_Y1.Count);
            OrbitChange_X2.RemoveRange(0, OrbitChange_X2.Count);
            OrbitChange_Y2.RemoveRange(0, OrbitChange_Y2.Count);
            ReadOrbit_X1.RemoveRange(0, ReadOrbit_X1.Count);
            ReadOrbit_Y1.RemoveRange(0, ReadOrbit_Y1.Count);
            ReadOrbit_X2.RemoveRange(0, ReadOrbit_X1.Count);
            ReadOrbit_Y2.RemoveRange(0, ReadOrbit_Y1.Count);

            TrackChange_X.RemoveRange(0, TrackChange_X.Count);//清除赛道
            TrackChange_Y.RemoveRange(0, TrackChange_Y.Count);
            Track_X.RemoveRange(0, Track_X.Count);
            Track_Y.RemoveRange(0, Track_Y.Count);
            //重新初始化起点及变形大小
            OX = OX_Ever;
            OY = OY_Ever;
            ReadOrbit_OX = BackX + Back_XMax / 2;
            ReadOrbit_OY = BackY + Back_YMax / 2;
            ReadTrack_OX = BackX + Back_XMax / 2;
            ReadTrack_OY = BackY + Back_YMax / 2;
            Orbit_Change_New = 1;
            Orbit_Change_Last = 1;
            ReadOrbit_Count = 0;
            ReadTrack_Count = 0;
            ReadOrbit_Change1 = 1;
            ReadOrbit_Change2 = 1;
            ReadTrack_Change = 1;
            ReadOrbit_CHANGE = 1;
            ReadTrack_Flag = false;
            ReadOrbit_Flag = false;

            ////////时间点相关变量
            How_Long_Orbit = 0;
            How_Long_Read1 = 0;
            How_Long_Read2 = 0;
            Long_Max = 0;
            PointX_Read1 = 0;
            PointY_Read1 = 0;
            PointX_Read2 = 0;
            PointY_Read2 = 0;

            PointX_Orbit = 0;
            PointY_Orbit = 0;
            Point_Time = 0;
        }
        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            WritePrivateProfileString("PortData", "BaudRate", CurrentBaudRate, PortData);//窗口关闭，保存
            WritePrivateProfileString("PortData", "PortName", CurrentPortName, PortData);//窗口关闭，保存
        }
        private void Shake()
        {
            for (int i = 0; i < 10; i++)
            {
                this.Location = new Point(this.Location.X + 10, this.Location.Y);
                System.Threading.Thread.Sleep(15);
                this.Location = new Point(this.Location.X - 10, this.Location.Y);
                System.Threading.Thread.Sleep(15);
            }
        }

/*****************************************************************************************///串口
        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)                                     //检查串口是否关闭
            {
                serialPort1.Close();
                button2.Text = "打开串口";
                pictureBox1.BackgroundImage = Properties.Resources.BlueTooth2;
            }
            comboBox1.Items.Clear();                                    //清空ComboBox内容
            SearchAndAddSerialToComboBox();
            textBox2.Text = "";
            textBox2.AppendText("串口扫描完毕!");
        }
        private void SearchAndAddSerialToComboBox()
        {                                                               //将可用端口号添加到ComboBox
            int ii = 0; string str;
            foreach (string s in SerialPort.GetPortNames())
            {
                if (ii >= 1)
                {
                    str = s.Substring(0, s.Length );
                    comboBox1.Items.Add(str);
                }

                ii++;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Close();
                }
                catch { }
                button2.Text = "打开串口";
                textBox2.Text = "";
                textBox2.AppendText("串口已关闭!");
                pictureBox1.BackgroundImage = Properties.Resources.BlueTooth2;
            }
            else
            {
                try
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.Open();

                    CurrentBaudRate = comboBox2.Text;
                    CurrentPortName = comboBox1.Text;

                    button2.Text = "关闭串口";
                    textBox2.Text = "";
                    textBox2.AppendText("串口打开成功!");
                    pictureBox1.BackgroundImage = Properties.Resources.BlueTooth;
                }
                catch
                {
                    textBox2.Text = "";
                    textBox2.AppendText("串口打开失败!");
                    serialPort1.Close();
                }
            }
        }
        
    /*{
            Back_XMax = this.Size.Width - BackX - 35;
            Back_YMax = this.Size.Height - BackY - 60;
            OX_Ever = BackX + Back_XMax / 2;
            OY_Ever = BackY + Back_YMax / 2;
            ReadOrbit_OX = BackX + Back_XMax / 2;
            ReadOrbit_OY = BackY + Back_YMax / 2;
            ReadTrack_OX = BackX + Back_XMax / 2;
            ReadTrack_OY = BackY + Back_YMax / 2;
            Invalidate();
        }
/****************************************************************************************************///图形放大部分


 /*****************************************************************************************///快捷键
        private bool KeyShift = false;
        private bool View_Flag1 = false;
        private bool View_Flag2 = false;
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift)//shift功能键按下
                KeyShift = true;//标志位置位
            switch (e.KeyCode)
            {
                case Keys.V: View_Flag1 = true; break;
                case Keys.C: View_Flag2 = true; break;
                default: ; break;
            }
        }
        private void Form_KeyUp(object sender, KeyEventArgs e)
        {
            if (KeyShift)
            {
                if (View_Flag1)
                {
                    View_Flag1 = false;
                    textBox2.Visible = true;
                    tabControl1.Visible = true;
                }
                if (View_Flag2)
                {
                    View_Flag2 = false;
                    textBox2.Visible = false;
                    tabControl1.Visible = false;
                }
            }
            else
            {
                View_Flag1 = false;
                View_Flag2 = false;
            }
            KeyShift = false;
        }




/****************************************************************************************************///总体画轨迹赛道等
        private Pen Kuang = new Pen(Color.Aqua);            //外框
        private Int16 Wide = 10;

        private double Orbit_Change_New ;                //缩放系数
        private double Orbit_Change_Last ;
        private bool Break_Flag = false;
        private void Form_Paint(object sender, PaintEventArgs e)
        {
            //填充画图区
            e.Graphics.FillRectangle(Brushes.Black, new RectangleF(BackX, BackY, Back_XMax, Back_YMax));
            //画框
            e.Graphics.DrawLine(Kuang, (BackX - Wide), (BackY - Wide), (BackX - Wide) + (Back_XMax + 2 * Wide), (BackY - Wide));
            e.Graphics.DrawLine(Kuang, (BackX - Wide), (BackY - Wide), (BackX - Wide), (BackY - Wide) + (Back_YMax + 2 * Wide));
            e.Graphics.DrawLine(Kuang, (BackX - Wide), (BackY - Wide) + (Back_YMax + 2 * Wide), (BackX - Wide) + (Back_XMax + 2 * Wide), (BackY - Wide) + (Back_YMax + 2 * Wide));
            e.Graphics.DrawLine(Kuang, (BackX - Wide) + (Back_XMax + 2 * Wide), (BackY - Wide), (BackX - Wide) + (Back_XMax + 2 * Wide), (BackY - Wide) + (Back_YMax + 2 * Wide));
            ShiBo_Paint(sender, e);
            bool Inval_Again = false;
            if (Double_Invalidate)
            {
                Inval_Again = true;

                for (int i = 0; i < 5; i++)
                {
                    Break_Flag = false;
                    Compare_Point();
                    if (ReadTrack_Flag) Track_Paint_Ready();
                    if (ReadOrbit_Flag) ReadOrbit_Paint_Ready();
                    if (Orbit_Received) Orbit_Paint_Ready();
                    Compare_Change();
                    Orbit_Change_Last = Orbit_Change_New;
                    if (Break_Flag) break;
                }
                Double_Invalidate = false;
            }
            else
            {
                Compare_Point();
                if (ReadTrack_Flag) Track_Paint_Ready();
                if (ReadOrbit_Flag) ReadOrbit_Paint_Ready();
                if (Orbit_Received) Orbit_Paint_Ready();
                Compare_Change();
            }
            Compare_Change();
            All_Paint(sender, e);
            if (ReadOrbit_Flag) Point_Paint(sender, e);
            if (Inval_Again)
            {
                Invalidate();
            }
        }
        private Int16 double_inval_count = 0;
        private void All_Paint(object sender, PaintEventArgs e)
        {
            /////////////////画赛道
            for (int i = 0; i < Track_X.Count - 1; i++)
            {
                e.Graphics.DrawLine(Track_Pen, OX + (int)(Track_X[i] / Orbit_Change_New), OY + (int)(Track_Y[i] / Orbit_Change_New), OX + (int)(Track_X[i + 1] / Orbit_Change_New), OY + (int)(Track_Y[i + 1] / Orbit_Change_New));
            }
            /////////////////画历史轨迹
            for (int i = 0; i < ReadOrbit_X1.Count - 1; i++)
            {
                e.Graphics.DrawLine(Orbit_Pen1, OX + (int)(ReadOrbit_X1[i] / Orbit_Change_New), OY + (int)(ReadOrbit_Y1[i] / Orbit_Change_New), OX + (int)(ReadOrbit_X1[i + 1] / Orbit_Change_New), OY + (int)(ReadOrbit_Y1[i + 1] / Orbit_Change_New));
            }
            if (ReadOrbit_Count > 1)
            {
                for (int i = 0; i < ReadOrbit_X2.Count - 1; i++)
                {
                    e.Graphics.DrawLine(Orbit_Pen2, OX + (int)(ReadOrbit_X2[i] / Orbit_Change_New), OY + (int)(ReadOrbit_Y2[i] / Orbit_Change_New), OX + (int)(ReadOrbit_X2[i + 1] / Orbit_Change_New), OY + (int)(ReadOrbit_Y2[i + 1] / Orbit_Change_New));
                }
            }
            ////////////////////////画轨迹
            for (int i = 0; i < OrbitX.Count - 1; i++)
            {
                e.Graphics.DrawLine(Orbit_Color, OX + (int)(OrbitX[i] / Orbit_Change_New), OY + (int)(OrbitY[i] / Orbit_Change_New), OX + (int)(OrbitX[i + 1] / Orbit_Change_New), OY + (int)(OrbitY[i + 1] / Orbit_Change_New));
            }

            ///////////////////////
            ///////////////////////传递缩放系数
            Orbit_Change_Last = Orbit_Change_New;
        }

/****************************************************************************************************///画点

        private int PointX_Read1 = 0;
        private int PointY_Read1 = 0;
        private int PointX_Read2 = 0;
        private int PointY_Read2 = 0;

        private int PointX_Orbit = 0;
        private int PointY_Orbit = 0;

        private void Point_Paint(object sender, PaintEventArgs e)
        {
            bool Read1_Point = false;
            bool Read2_Point = false;
            bool Orbit_Point = false;

            if (ReadOrbit_Count == 1)
            {
                if (Point_Time > Long_Max)
                {
                    Shake();
                    textBox2.Text = "";
                    textBox2.AppendText("超出最大时间点！");
                }
                else
                {
                    if (Point_Time < How_Long_Read1)
                    {
                        PointX_Read1 = (int)(ReadOrbit_X1[Point_Time] / Orbit_Change_New);
                        PointY_Read1 = (int)(ReadOrbit_Y1[Point_Time] / Orbit_Change_New);
                        Read1_Point = true;
                    }
                }
            }
            if (ReadOrbit_Count > 1)
            {
                if (Point_Time > Long_Max)
                {
                    Shake();
                    textBox2.Text = "";
                    textBox2.AppendText("超出最大时间点！");

                }
                else
                {
                    if (Point_Time < How_Long_Read2)
                    {
                        PointX_Read2 = (int)(ReadOrbit_X2[Point_Time] / Orbit_Change_New);
                        PointY_Read2 = (int)(ReadOrbit_Y2[Point_Time] / Orbit_Change_New);
                        Read2_Point = true;
                    }

                    if (Point_Time < How_Long_Read1)
                    {
                        PointX_Read1 = (int)(ReadOrbit_X1[Point_Time] / Orbit_Change_New);
                        PointY_Read1 = (int)(ReadOrbit_Y1[Point_Time] / Orbit_Change_New);
                        Read1_Point = true;
                    }
                }
            }
            if (Orbit_Received)
            {

                Orbit_Received = false;//处理完链表后清除标志位

                if (Point_Time > Long_Max)
                {
                    Shake();
                    textBox2.Text = "";
                    textBox2.AppendText("超出最大时间点！");
                }
                else 
                {
                    if (Point_Time < How_Long_Orbit)
                    {
                        PointX_Orbit = (int)(OrbitX[Point_Time] / Orbit_Change_New);
                        PointY_Orbit = (int)(OrbitY[Point_Time] / Orbit_Change_New);
                        Orbit_Point = true;
                    }
                }
            }
            /////////////////////填充各个时间点坐标
            //e.Graphics.FillEllipse(Brushes.Green, OX + PointX_Read1, OY + PointY_Read1, 10, 10);
            SolidBrush brush1 = new SolidBrush(Color.Red );
            SolidBrush brush2 = new SolidBrush(Color.Aqua );
            SolidBrush brush3 = new SolidBrush(Color.Fuchsia);
            if (Read1_Point) e.Graphics.FillEllipse(brush1, OX + PointX_Read1 - 3, OY + PointY_Read1 - 3, 6, 6);
            if (Read2_Point) e.Graphics.FillEllipse(brush2, OX + PointX_Read2 - 3, OY + PointY_Read2 - 3, 6, 6);
            if (Orbit_Point) e.Graphics.FillEllipse(brush2, OX + PointX_Orbit - 3, OY + PointY_Orbit - 3, 6, 6);
        }



/****************************************************************************************************///画轨迹
       //移植到其他窗口只需要修改这四个值
        private const int BackX = 220;
        private const int BackY = 20;
        private int Back_XMax = 1355;
        private int Back_YMax = 930;

        private List<int> OrbitLength = new List<int>();    //数据结构----线性链表
        private List<double> OrbitAngle = new List<double>();//数据结构----线性链表
        private List<int> OrbitX = new List<int>();         //数据结构----线性链表
        private List<int> OrbitY = new List<int>();         //数据结构----线性链表
        private List<int> ChangeX = new List<int>();        //数据结构----线性链表
        private List<int> ChangeY = new List<int>();        //数据结构----线性链表
        private int OX_Ever ;
        private int OY_Ever ;
        private int OX ;
        private int OY ;
        private Pen Orbit_Color = new Pen(Color.Green, 2);  //轨迹颜色
        private bool Orbit_Received = false;

        private int How_Long_Orbit = 0;                         //时间点使用

        private void Orbit_Paint_Ready()
        {
            //System.Drawing.Drawing2D.GraphicsPath Orbit = new System.Drawing.Drawing2D.GraphicsPath();//添加文字时使用
            //计算坐标
            if (Orbit_Received)
            {
                if (OrbitX.Count == 0)
                {
                    OrbitX.Add(0);
                    OrbitY.Add(0);
                    ChangeX.Add(0);
                    ChangeY.Add(0);
                }
                if (OrbitX.Count > 0)
                {
                    int i = OrbitLength.Count - 1;
                    OrbitX.Add(OrbitX[OrbitX.Count - 1] + (int)(OrbitLength[i] * Math.Cos(OrbitAngle[i])));//长度，角度链表长度=x，y链表长度+1；
                    OrbitY.Add(OrbitY[OrbitY.Count - 1] - (int)(OrbitLength[i] * Math.Sin(OrbitAngle[i])));
                    ChangeX.Add(0);
                    ChangeY.Add(0);
                }
            }
            /*********************************************************************************************/
            for (int i = 0; i < OrbitX.Count; i++)                                                          //需要每次进行一次坐标转化
            {
                ChangeX[i] = (int)(OrbitX[i] / Orbit_Change_Last);
                ChangeY[i] = (int)(OrbitY[i] / Orbit_Change_Last);
            }

            //获取时间点长度；
            How_Long_Orbit = OrbitX.Count;

        }
 /*****************************************************************************///画赛道
        private Pen Track_Pen = new Pen(Color.White, 20);            //赛道笔
        private List<int> TrackChange_X = new List<int>();        //数据结构----线性链表
        private List<int> TrackChange_Y = new List<int>();        //数据结构----线性链表
        private void Track_Paint_Ready()
        {
            for (int i = 0; i < Track_X.Count; i++)
            {
                TrackChange_X[i] = (int)(Track_X[i] / Orbit_Change_Last);
                TrackChange_Y[i] = (int)(Track_Y[i] / Orbit_Change_Last);
            }
        }
 /*****************************************************************************///画储存轨迹
        private Pen Orbit_Pen1 = new Pen(Color.Blue, 2);            //赛道笔
        private Pen Orbit_Pen2 = new Pen(Color.Yellow, 2);            //赛道笔

        private List<int> OrbitChange_X1 = new List<int>();        //数据结构----线性链表
        private List<int> OrbitChange_Y1 = new List<int>();        //数据结构----线性链表
        private List<int> OrbitChange_X2 = new List<int>();        //数据结构----线性链表
        private List<int> OrbitChange_Y2 = new List<int>();        //数据结构----线性链表

        private List<int> ReadOrbit_X1 = new List<int>();        //数据结构----线性链表
        private List<int> ReadOrbit_Y1 = new List<int>();        //数据结构----线性链表
        private List<int> ReadOrbit_X2 = new List<int>();        //数据结构----线性链表
        private List<int> ReadOrbit_Y2 = new List<int>();        //数据结构----线性链表
        private void ReadOrbit_Paint_Ready()
        {
            int i;
            if ( ReadOrbit_Count % 2 == 1)
            {
                for ( i = 0; i < ReadOrbit_X.Count; i++)
                {
                    ReadOrbit_X1[i] = ReadOrbit_X[i];
                    ReadOrbit_Y1[i] = ReadOrbit_Y[i];
                }
                for (i = 0; i < ReadOrbit_X.Count; i++)
                {
                    OrbitChange_X1[i] = (int)(ReadOrbit_X[i] / Orbit_Change_Last);
                    OrbitChange_Y1[i] = (int)(ReadOrbit_Y[i] / Orbit_Change_Last);
                }
            }
            if ( ReadOrbit_Count % 2 == 0)
            {
                for ( i = 0; i < ReadOrbit_X.Count; i++)
                {
                    ReadOrbit_X2[i] = ReadOrbit_X[i];
                    ReadOrbit_Y2[i] = ReadOrbit_Y[i];
                }
                for (i = 0; i < ReadOrbit_X.Count; i++)
                {
                    OrbitChange_X2[i] = (int)(ReadOrbit_X[i] / Orbit_Change_Last);
                    OrbitChange_Y2[i] = (int)(ReadOrbit_Y[i] / Orbit_Change_Last);
                }
            }
        }





/*****************************************************************************///轨迹赛道原点对比,归一化
        private void Compare_Point()
        {
            if (ReadOrbit_Flag)                     //比对两个缩放倍数，取最大作为历史轨迹缩放
            {
                if (ReadOrbit_Count == 1)
                {
                    ReadOrbit_CHANGE = ReadOrbit_Change1;
                }
                if (ReadOrbit_Count > 1)
                {
                    if (ReadOrbit_Change1 > ReadOrbit_Change2)
                    {
                        ReadOrbit_CHANGE = ReadOrbit_Change1;
                    }
                    if (ReadOrbit_Change1 < ReadOrbit_Change2)
                    {
                        ReadOrbit_CHANGE = ReadOrbit_Change2;
                    }
                }
            }
            if (ReadTrack_Flag && !ReadOrbit_Flag)
            {
                OX = ReadTrack_OX;
                OY = ReadTrack_OY;
                if (Orbit_Change_Last < ReadTrack_Change) Orbit_Change_Last = ReadTrack_Change;
            }
            if (!ReadTrack_Flag && ReadOrbit_Flag)
            {
                OX = ReadOrbit_OX;
                OY = ReadOrbit_OY;
                if (Orbit_Change_Last < ReadOrbit_CHANGE) Orbit_Change_Last = ReadOrbit_CHANGE;
            }
            if (ReadTrack_Flag && ReadOrbit_Flag)
            {
                if ((ReadOrbit_CHANGE >= ReadTrack_Change) && (Orbit_Change_Last < ReadOrbit_CHANGE)) Orbit_Change_Last = ReadOrbit_CHANGE;
                if ((ReadTrack_Change > ReadOrbit_CHANGE) && (Orbit_Change_Last < ReadTrack_Change)) Orbit_Change_Last = ReadTrack_Change;
                OX = ReadTrack_OX;
                OY = ReadTrack_OY;
            }
        }
/*****************************************************************************///图像处理
        private int Change_Compensate = 40;                 //一般取30使得图像不抖动
        private float times = 0.3F;//平移缩放
        private void Compare_Change()
        {
            bool LFlag = false;
            bool RFlag = false;
            bool UFlag = false;
            bool DFlag = false;

            int i = 0;

            //超出最大值
            int Left_Out = 0;
            int Right_Out = 0;
            int Up_Out = 0;
            int Down_Out = 0;

            //各轨迹边际距离
            int dmin_left = 0;
            int dmin_right = 0;
            int dmin_up = 0;
            int dmin_down = 0;
            //边际标志位
            bool d_left = false;
            bool d_right = false;
            bool d_up = false;
            bool d_down = false;
            //处理程序开始
                
            ////////////////////图像缩放大小计算
            if (ReadTrack_Flag)          //赛道
            {
                for (i = 0; i < Track_X.Count; i++)
                {
                    if (TrackChange_X[i] + OX <= BackX)//左
                    {
                       
                        if (!LFlag)
                        {
                            LFlag = true;
                            Left_Out = BackX - TrackChange_X[i] - OX;
                        }
                        else
                        {
                            if (Left_Out < BackX - TrackChange_X[i] - OX) Left_Out = BackX - TrackChange_X[i] - OX;
                        }
                        LFlag = true;
                    }
                    if (TrackChange_X[i] + OX >= BackX + Back_XMax)//右
                    {
                        if (!RFlag)
                        {
                            RFlag = true;
                            Right_Out = TrackChange_X[i] + OX - BackX - Back_XMax;
                        }
                        else
                        {
                            if (Right_Out < TrackChange_X[i] + OX - BackX - Back_XMax) Right_Out = TrackChange_X[i] + OX - BackX - Back_XMax;
                        }
                        RFlag = true;
                    }
                    if (TrackChange_Y[i] + OY <= BackY)//上
                    {
                        if (!UFlag)
                        {
                            UFlag = true;
                            Up_Out = BackY - TrackChange_Y[i] - OY;
                        }
                        else
                        {
                            if (Up_Out < BackY - TrackChange_Y[i] - OY) Up_Out = BackY - TrackChange_Y[i] - OY;
                        }
                        UFlag = true;
                    }
                    if (TrackChange_Y[i] + OY >= BackY + Back_YMax)//下
                    {
                        if (!DFlag)
                        {
                            DFlag = true;
                            Down_Out = TrackChange_Y[i] + OY - BackY - Back_YMax;
                        }
                        else
                        {
                            if (Down_Out < TrackChange_Y[i] + OY - BackY - Back_YMax) Down_Out = TrackChange_Y[i] + OY - BackY - Back_YMax;
                        }
                        DFlag = true;
                    }
                    //边际
                    if (TrackChange_X[i] + OX > BackX)//左
                    {
                        if (!d_left)
                        {
                            d_left = true;
                            dmin_left  = TrackChange_X[i] + OX - BackX;
                        }
                        else
                        {
                            if (TrackChange_X[i] + OX - BackX < dmin_left) dmin_left = TrackChange_X[i] + OX - BackX;
                        }
                        d_left = true;
                    }
                    if (TrackChange_X[i] + OX < BackX + Back_XMax)//右
                    {
                        if (!d_right)
                        {
                            d_right = true;
                            dmin_right  = BackX + Back_XMax - TrackChange_X[i] - OX;
                        }
                        else
                        {
                            if (BackX + Back_XMax - TrackChange_X[i] - OX < dmin_right) dmin_right = BackX + Back_XMax - TrackChange_X[i] - OX;
                        }
                        d_right = true;
                    }
                    if (TrackChange_Y[i] + OY > BackY)//上
                    {
                        if (!d_up)
                        {
                            d_up = true;
                            dmin_up  = TrackChange_Y[i] + OY - BackY;
                        }
                        else 
                        {
                            if (TrackChange_Y[i] + OY - BackY < dmin_up) dmin_up = TrackChange_Y[i] + OY - BackY;
                        }
                        d_up = true;
                    }
                    if (TrackChange_Y[i] + OY < BackY + Back_YMax)//下
                    {
                        if (!d_down)
                        {
                            d_down = true;
                            dmin_down  = BackY + Back_YMax - TrackChange_Y[i] - OY;
                        }
                        else
                        {
                            if (BackY + Back_YMax - TrackChange_Y[i] - OY < dmin_down) dmin_down = BackY + Back_YMax - TrackChange_Y[i] - OY;
                        }
                        d_down = true;
                    }
                }
            }
            if (ReadOrbit_Flag)//////////////////读取轨迹部分
            {
                for (i = 0; i < OrbitChange_X1.Count; i++)
                {
                    if (OrbitChange_X1[i] + OX <= BackX)//左
                    {
                        if (!LFlag)
                        {
                            LFlag = true;
                            Left_Out = BackX - OrbitChange_X1[i] - OX;
                        }
                        else
                        {
                            if (Left_Out < BackX - OrbitChange_X1[i] - OX) Left_Out = BackX - OrbitChange_X1[i] - OX;
                        }
                        LFlag = true;
                    }
                    if (OrbitChange_X1[i] + OX >= BackX + Back_XMax)//右
                    {
                        if (!RFlag)
                        {
                            RFlag = true;
                            Right_Out = OrbitChange_X1[i] + OX - BackX - Back_XMax;
                        }
                        else
                        {
                            if (Right_Out < OrbitChange_X1[i] + OX - BackX - Back_XMax) Right_Out = OrbitChange_X1[i] + OX - BackX - Back_XMax;
                        }
                        RFlag = true;
                    }
                    if (OrbitChange_Y1[i] + OY <= BackY)//上
                    {
                        if (!UFlag)
                        {
                            UFlag = true;
                            Up_Out = BackY - OrbitChange_Y1[i] - OY;
                        }
                        else
                        {
                            if (Up_Out < BackY - OrbitChange_Y1[i] - OY) Up_Out = BackY - OrbitChange_Y1[i] - OY;
                        }
                        UFlag = true;
                    }
                    if (OrbitChange_Y1[i] + OY >= BackY + Back_YMax)//下
                    {
                        if (!DFlag)
                        {
                            DFlag = true;
                            Down_Out = OrbitChange_Y1[i] + OY - BackY - Back_YMax;
                        }
                        else
                        {
                            if (Down_Out < OrbitChange_Y1[i] + OY - BackY - Back_YMax) Down_Out = OrbitChange_Y1[i] + OY - BackY - Back_YMax;
                        }
                        DFlag = true;
                    }
                    //边际
                    if (OrbitChange_X1[i] + OX > BackX)//左
                    {
                        if (!d_left)
                        {
                            d_left = true;
                            dmin_left  = OrbitChange_X1[i] + OX - BackX;
                        }
                        else
                        {
                            if (OrbitChange_X1[i] + OX - BackX < dmin_left) dmin_left = OrbitChange_X1[i] + OX - BackX;
                        }
                        d_left = true;
                    }
                    if (OrbitChange_X1[i] + OX < BackX + Back_XMax)//右
                    {
                        if (!d_right)
                        {
                            d_right = true;
                            dmin_right  = BackX + Back_XMax - OrbitChange_X1[i] - OX;
                        }
                        else
                        {
                            if (BackX + Back_XMax - OrbitChange_X1[i] - OX < dmin_right) dmin_right = BackX + Back_XMax - OrbitChange_X1[i] - OX;
                        }
                        d_right = true;
                    }
                    if (OrbitChange_Y1[i] + OY > BackY)//上
                    {
                        if (!d_up)
                        {
                            d_up = true;
                            dmin_up  = OrbitChange_Y1[i] + OY - BackY;
                        }
                        else
                        {
                            if (OrbitChange_Y1[i] + OY - BackY < dmin_up) dmin_up = OrbitChange_Y1[i] + OY - BackY;
                        }
                        d_up = true;
                    }
                    if (OrbitChange_Y1[i] + OY < BackY + Back_YMax)//下
                    {
                        if (!d_down)
                        {
                            d_down = true;
                            dmin_down  = BackY + Back_YMax - OrbitChange_Y1[i] - OY;
                        }
                        else
                        {
                            if (BackY + Back_YMax - OrbitChange_Y1[i] - OY < dmin_down) dmin_down = BackY + Back_YMax - OrbitChange_Y1[i] - OY;
                        }
                        d_down = true;
                    }
                }
                ////////读取两条轨迹时处理
                if (ReadOrbit_Count > 1)
                {
                    for (i = 0; i < OrbitChange_X2.Count; i++)
                    {
                        if (OrbitChange_X2[i] + OX <= BackX)//左
                        {
                            if (!LFlag)
                            {
                                LFlag = true;
                                Left_Out = BackX - OrbitChange_X2[i] - OX;
                            }
                            else
                            {
                                if (Left_Out < BackX - OrbitChange_X2[i] - OX) Left_Out = BackX - OrbitChange_X2[i] - OX;
                            }
                            LFlag = true;
                        }
                        if (OrbitChange_X2[i] + OX >= BackX + Back_XMax)//右
                        {
                            if (!RFlag)
                            {
                                RFlag = true;
                                Right_Out = OrbitChange_X2[i] + OX - BackX - Back_XMax;
                            }
                            else
                            {
                                if (Right_Out < OrbitChange_X2[i] + OX - BackX - Back_XMax) Right_Out = OrbitChange_X2[i] + OX - BackX - Back_XMax;
                            }
                            RFlag = true;
                        }
                        if (OrbitChange_Y2[i] + OY <= BackY)//上
                        {
                            if (!UFlag)
                            {
                                UFlag = true;
                                Up_Out = BackY - OrbitChange_Y2[i] - OY;
                            }
                            else
                            {
                                if (Up_Out < BackY - OrbitChange_Y2[i] - OY) Up_Out = BackY - OrbitChange_Y2[i] - OY;
                            }
                            UFlag = true;
                        }
                        if (OrbitChange_Y2[i] + OY >= BackY + Back_YMax)//下
                        {
                            if (!DFlag)
                            {
                                DFlag = true;
                                Down_Out = OrbitChange_Y2[i] + OY - BackY - Back_YMax;
                            }
                            else
                            {
                                if (Down_Out < OrbitChange_Y2[i] + OY - BackY - Back_YMax) Down_Out = OrbitChange_Y2[i] + OY - BackY - Back_YMax;
                            }
                            DFlag = true;
                        }
                        //边际
                        if (OrbitChange_X2[i] + OX > BackX)//左
                        {
                            if (!d_left)
                            {
                                d_left = true;
                                dmin_left  = OrbitChange_X2[i] + OX - BackX;
                            }
                            else
                            {
                                if (OrbitChange_X2[i] + OX - BackX < dmin_left) dmin_left = OrbitChange_X2[i] + OX - BackX;
                            }
                            d_left = true;
                        }
                        if (OrbitChange_X2[i] + OX < BackX + Back_XMax)//右
                        {
                            if (!d_right)
                            {
                                d_right = true;
                                dmin_right  = BackX + Back_XMax - OrbitChange_X2[i] - OX;
                            }
                            else
                            {
                                if (BackX + Back_XMax - OrbitChange_X2[i] - OX < dmin_right) dmin_right = BackX + Back_XMax - OrbitChange_X2[i] - OX;
                            }
                            d_right = true;
                        }
                        if (OrbitChange_Y2[i] + OY > BackY)//上
                        {
                            if (!d_up)
                            {
                                d_up = true;
                                dmin_up  = OrbitChange_Y2[i] + OY - BackY;
                            }
                            else
                            {
                                if (OrbitChange_Y2[i] + OY - BackY < dmin_up) dmin_up = OrbitChange_Y2[i] + OY - BackY;
                            }
                            d_up = true;
                        }
                        if (OrbitChange_Y2[i] + OY < BackY + Back_YMax)//下
                        {
                            if (!d_down)
                            {
                                d_down = true;
                                dmin_down  = BackY + Back_YMax - OrbitChange_Y2[i] - OY;
                            }
                            else
                            {
                                if (BackY + Back_YMax - OrbitChange_Y2[i] - OY < dmin_down) dmin_down = BackY + Back_YMax - OrbitChange_Y2[i] - OY;
                            }
                            d_down = true;
                        }
                    }
                }
            }
            //if (Orbit_Received)////////////////实时轨迹处理
            {


                for (i = 0; i < ChangeX.Count; i++)
                {
                    if (ChangeX[i] + OX <= BackX)//左
                    {
                        if (!LFlag)
                        {
                            LFlag = true;
                            Left_Out = BackX - ChangeX[i] - OX;
                        }
                        else
                        {
                            if (Left_Out < BackX - ChangeX[i] - OX) Left_Out = BackX - ChangeX[i] - OX;
                        }
                        LFlag = true;
                    }
                    if (ChangeX[i] + OX >= BackX + Back_XMax)//右
                    {
                        if (!RFlag)
                        {
                            RFlag = true;
                            Right_Out = ChangeX[i] + OX - BackX - Back_XMax;
                        }
                        else
                        {
                            if (Right_Out < ChangeX[i] + OX - BackX - Back_XMax) Right_Out = ChangeX[i] + OX - BackX - Back_XMax;
                        }
                        RFlag = true;
                    }
                    if (ChangeY[i] + OY <= BackY)//上
                    {
                        if (!UFlag)
                        {
                            UFlag = true;
                            Up_Out = BackY - ChangeY[i] - OY;
                        }
                        else
                        {
                            if (Up_Out < BackY - ChangeY[i] - OY) Up_Out = BackY - ChangeY[i] - OY;
                        }
                        UFlag = true;
                    }
                    if (ChangeY[i] + OY >= BackY + Back_YMax)//下
                    {
                        if (!DFlag)
                        {
                            DFlag = true;
                            Down_Out = ChangeY[i] + OY - BackY - Back_YMax;
                        }
                        else
                        {
                            if (Down_Out < ChangeY[i] + OY - BackY - Back_YMax) Down_Out = ChangeY[i] + OY - BackY - Back_YMax;
                        }
                        DFlag = true;
                    }
                    //边际
                    if (ChangeX[i] + OX > BackX)//左
                    {
                        if (!d_left)
                        {
                            d_left = true;
                            dmin_left   = ChangeX[i] + OX - BackX;
                        }
                        else
                        {
                            if (ChangeX[i] + OX - BackX < dmin_left) dmin_left = ChangeX[i] + OX - BackX;
                        }
                        d_left = true;
                    }
                    if (ChangeX[i] + OX < BackX + Back_XMax)//右
                    {
                        if (!d_right)
                        {
                            d_right = true;
                            dmin_right  = BackX + Back_XMax - ChangeX[i] - OX;
                        }
                        else
                        {
                            if (BackX + Back_XMax - ChangeX[i] - OX < dmin_right) dmin_right = BackX + Back_XMax - ChangeX[i] - OX;
                        }
                        d_right = true;
                    }
                    if (ChangeY[i] + OY > BackY)//上
                    {
                        if (!d_up)
                        {
                            d_up = true;
                            dmin_up   = ChangeY[i] + OY - BackY;
                        }
                        else
                        {
                            if (ChangeY[i] + OY - BackY < dmin_up) dmin_up = ChangeY[i] + OY - BackY;
                        }
                        d_up = true;
                    }
                    if (ChangeY[i] + OY < BackY + Back_YMax)//下
                    {
                        if (!d_down)
                        {
                            d_down = true;
                            dmin_down  = BackY + Back_YMax - ChangeY[i] - OY;
                        }
                        else
                        {
                            if (BackY + Back_YMax - ChangeY[i] - OY < dmin_down) dmin_down = BackY + Back_YMax - ChangeY[i] - OY;
                        }
                        d_down = true;
                    }
                }
            }
            
            int Lengh;
            /**************************************************************************************************/
            //图像均不超出
            if (DFlag == false && UFlag == false && RFlag == false && LFlag == false)
            {
                Orbit_Change_New = Orbit_Change_Last;
                Break_Flag = true;
            }
            //图像单边超出，平移
            if (DFlag && UFlag == false && RFlag == false && LFlag == false)//向上移位
            {
                if (Down_Out < dmin_up)
                {
                    OY -= Down_Out;
                    if (OY < BackY) OY = BackY;
                    Orbit_Change_New = Orbit_Change_Last;
                }
                else
                {
                    Lengh = Down_Out - dmin_up;
                    OY = OY - (int)(Lengh/ (Orbit_Change_Last * times));
                    if (OY < BackY) OY = BackY;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                }
                DFlag = false;
            }
            if (UFlag && DFlag == false && RFlag == false && LFlag == false)//向下移位
            {
                if (Up_Out < dmin_down)
                {
                    OY += Up_Out;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                    Orbit_Change_New = Orbit_Change_Last;
                }
                else
                {
                    Lengh = Up_Out - dmin_down;
                    OY = OY + (int)(Lengh / (Orbit_Change_Last * times));
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                }
                UFlag = false;
            }
            if (RFlag && LFlag == false && DFlag == false && UFlag == false)//向左移位
            {
                if (Right_Out < dmin_left)
                {
                    OX -= Right_Out;
                    if (OX < BackX) OX = BackX;
                    Orbit_Change_New = Orbit_Change_Last;
                }
                else
                {
                    Lengh = Right_Out - dmin_left;
                    OX = OX - (int)(Lengh / (Orbit_Change_Last * times));
                    if (OX < BackX) OX = BackX;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                }
                RFlag = false;
            }
            if (LFlag && RFlag == false && DFlag == false && UFlag == false)//向右移位
            {
                if (Left_Out < dmin_right)
                {
                    OX += Left_Out;
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    Orbit_Change_New = Orbit_Change_Last;
                }
                else
                {
                    Lengh = Left_Out - dmin_right;
                    OX = OX + (int)(Lengh / (Orbit_Change_Last * times));
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                }
                LFlag = false;
            }


            //双边超出
            if (LFlag && RFlag == false && DFlag == false && UFlag)//左上
            {
                if (Left_Out < dmin_right && Up_Out < dmin_down)
                {
                    OX = OX + Left_Out;
                    OY = OY + Up_Out;
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                    Orbit_Change_New = Orbit_Change_Last;
                }
                if (Left_Out >= dmin_right && Up_Out < dmin_down)
                {
                    Lengh =Left_Out - dmin_right;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);

                    OX = OX + (int)(Lengh / (Orbit_Change_Last * times));
                    OY = OY + Up_Out;
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                }
                if (Left_Out < dmin_right && Up_Out >= dmin_down)
                {
                    Lengh = Up_Out - dmin_down;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);

                    OX = OX + Left_Out ;
                    OY = OY + (int)(Lengh / (Orbit_Change_Last * times));
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    if (OY  > BackY + Back_YMax) OY = BackY + Back_YMax;
                }
                if (Left_Out >= dmin_right && Up_Out >= dmin_down)
                {
                    if (Left_Out - dmin_right > Up_Out - dmin_down)
                    {
                        Lengh = Left_Out - dmin_right;
                        Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                    }
                    else
                    {
                        Lengh = Up_Out - dmin_down;
                        Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                    }

                    OX = OX + (int)((Left_Out - dmin_right) / (Orbit_Change_Last * times));
                    OY = OY + (int)((Up_Out - dmin_down) / (Orbit_Change_Last * times));
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                }
            }
            if (LFlag && RFlag == false && DFlag && UFlag == false)//左下
            {
                if (Left_Out < dmin_right && Down_Out  < dmin_up )
                {
                    OX = OX + Left_Out;
                    OY = OY - Down_Out;
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    if (OY < BackY) OY = BackY;
                    Orbit_Change_New = Orbit_Change_Last;
                }
                if (Left_Out >= dmin_right && Down_Out < dmin_up)
                {
                    Lengh = Left_Out - dmin_right;
                    OX = OX + (int)(Lengh / (Orbit_Change_Last * times));
                    OY = OY - Down_Out;
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    if (OY < BackY) OY = BackY;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                }
                if (Left_Out < dmin_right && Down_Out >= dmin_up)
                {
                    Lengh = Down_Out - dmin_up;
                    OX = OX + Left_Out;
                    OY = OY - (int)(Lengh / (Orbit_Change_Last * times));
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    if (OY < BackY) OY = BackY;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                }
                if (Left_Out >= dmin_right && Down_Out >= dmin_up)
                {
                    OX = OX + (int)((Left_Out - dmin_right) / (Orbit_Change_Last * times));
                    OY = OY - (int)((Down_Out - dmin_up) / (Orbit_Change_Last * times));
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                    if (OY < BackY ) OY = BackY ;
                    if (Left_Out - dmin_right > Down_Out - dmin_up)
                    {
                        Lengh = Left_Out - dmin_right;
                        Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                    }
                    else
                    {
                        Lengh = Down_Out - dmin_up;
                        Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                    }
                }
            }
            if (LFlag == false && RFlag && DFlag == false && UFlag)//右上
            {
                if (Right_Out  < dmin_left  && Up_Out < dmin_down)
                {
                    OX = OX - Right_Out;
                    OY = OY + Up_Out;
                    if (OX < BackX) OX = BackX;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                    Orbit_Change_New = Orbit_Change_Last;
                }
                if (Right_Out >= dmin_left && Up_Out < dmin_down)
                {
                    Lengh = Right_Out - dmin_left;
                    OX = OX - (int)(Lengh / (Orbit_Change_Last * times));
                    OY = OY + Up_Out;
                    if (OX < BackX) OX = BackX;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                }
                if (Right_Out < dmin_left && Up_Out >= dmin_down)
                {
                    Lengh = Up_Out - dmin_down;
                    OX = OX - Right_Out;
                    OY = OY + (int)(Lengh / (Orbit_Change_Last * times));
                    if (OX < BackX) OX = BackX;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                }
                if (Right_Out >= dmin_left && Up_Out >= dmin_down)
                {
                    OX = OX - (int)((Right_Out - dmin_left) / (Orbit_Change_Last * times));
                    OY = OY + (int)((Up_Out - dmin_down) / (Orbit_Change_Last * times));
                    if (OX < BackX ) OX = BackX ;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                    if (Right_Out - dmin_left > Up_Out - dmin_down)
                    {
                        Lengh = Right_Out - dmin_left;
                        Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                    }
                    else
                    {
                        Lengh = Up_Out - dmin_down;
                        Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                    }
                }
            }
            if (LFlag == false && RFlag && DFlag && UFlag == false)//右下
            {
                if (Right_Out < dmin_left && Down_Out  < dmin_up )
                {
                    OX = OX - Right_Out;
                    OY = OY - Down_Out;
                    if (OX < BackX) OX = BackX;
                    if (OY < BackY) OY = BackY;
                    Orbit_Change_New =  Orbit_Change_Last;
                }
                if (Right_Out >= dmin_left && Down_Out < dmin_up)
                {
                    Lengh = Right_Out - dmin_left;
                    OX = OX - (int)(Lengh / (Orbit_Change_Last * times));
                    OY = OY - Down_Out;
                    if (OX < BackX) OX = BackX;
                    if (OY < BackY) OY = BackY;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                }
                if (Right_Out < dmin_left && Down_Out >= dmin_up)
                {
                    Lengh = Down_Out - dmin_up;
                    OX = OX - Right_Out;
                    OY = OY - (int)(Lengh / (Orbit_Change_Last * times));
                    if (OX < BackX) OX = BackX;
                    if (OY < BackY) OY = BackY;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                }
                if (Right_Out >= dmin_left && Down_Out >= dmin_up)
                {
                    OX = OX - (int)((Right_Out - dmin_left) / (Orbit_Change_Last * times));
                    OY = OY - (int)((Down_Out - dmin_up) / (Orbit_Change_Last * times));
                    if (OX < BackX) OX = BackX;
                    if (OY < BackY ) OY = BackY ;
                    if (Right_Out - dmin_left > Down_Out - dmin_up)
                    {
                        Lengh = Right_Out - dmin_left;
                        Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                    }
                    else
                    {
                        Lengh = Down_Out - dmin_up;
                        Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                    }
                }
            }
            float buff;
            //////图像三边超出
            if (LFlag && RFlag && DFlag == false && UFlag)//下端不超出
            {
                if (Up_Out < dmin_down)
                {
                    OY = OY + Up_Out;
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                }
                else
                {
                    Lengh = Up_Out - dmin_down;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);

                    OY = OY + (int)(Lengh / (Orbit_Change_Last * times));
                    if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
                }
                buff = (float)(Orbit_Change_Last * (Left_Out + Right_Out + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                if (Orbit_Change_New < buff) Orbit_Change_New = buff;
            }
            if (LFlag && RFlag && DFlag && UFlag == false)//上端不超出
            {
                if (Down_Out < dmin_up)
                {
                    OY = OY - Down_Out;
                    if (OY < BackY) OY = BackY;
                }
                else
                {
                    Lengh = Down_Out - dmin_up;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);

                    OY = OY - (int)(Lengh / (Orbit_Change_Last * times));
                    if (OY < BackY) OY = BackY;
                }
                buff = (float)(Orbit_Change_Last * (Left_Out + Right_Out + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                if (Orbit_Change_New < buff) Orbit_Change_New = buff;
            }
            if (LFlag == false && RFlag && DFlag && UFlag)//左端不超出
            {
                if (Right_Out < dmin_left)
                {
                    OX = OX - Right_Out;
                    if (OX < BackX) OX = BackX;
                }
                else
                {
                    Lengh = Right_Out - dmin_left;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                    OX = OX - (int)(Lengh / (Orbit_Change_Last * times));
                    if (OX < BackX) OX = BackX;
                }
                buff = (float)(Orbit_Change_Last * (Up_Out + Down_Out + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                if (Orbit_Change_New < buff) Orbit_Change_New = buff;
            }
            if (LFlag && RFlag == false && DFlag && UFlag)//右端不超出
            {
                if (Left_Out < dmin_right)
                {
                    OX = OX + Left_Out;
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                }
                else
                {
                    Lengh = Left_Out - dmin_right;
                    Orbit_Change_New = (float)(Orbit_Change_Last * (Lengh + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                    OX = OX + (int)(Lengh / (Orbit_Change_Last * times));
                    if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
                }
                buff = (float)(Orbit_Change_Last * (Up_Out + Down_Out + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                if (Orbit_Change_New < buff) Orbit_Change_New = buff;
            }
            //////图像四边超出
            if (LFlag && RFlag && DFlag && UFlag)
            {
                 Orbit_Change_New = Orbit_Change_Last;
                 buff = (float)(Orbit_Change_Last * (Up_Out + Down_Out + Back_YMax + Change_Compensate) * 1.0 / Back_YMax);
                 if (Orbit_Change_New < buff) Orbit_Change_New = buff;
                 buff = (float)(Orbit_Change_Last * (Left_Out + Right_Out + Back_XMax + Change_Compensate) * 1.0 / Back_XMax);
                 if (Orbit_Change_New < buff) Orbit_Change_New = buff;
            }
            /*
            if (OX < BackX) OX = BackX;
            if (OY < BackY) OY = BackY;

            if (OX > BackX + Back_XMax) OX = BackX + Back_XMax;
            if (OY > BackY + Back_YMax) OY = BackY + Back_YMax;
             */
                LFlag = false;
                RFlag = false;
                UFlag = false;
                DFlag = false;

                Left_Out = 0;
                Right_Out = 0;
                Up_Out = 0;
                Down_Out = 0;

                //各轨迹边际距离
                dmin_left = 0;
                dmin_right = 0;
                dmin_up = 0;
                dmin_down = 0;
                //边际标志位
                d_left = false;
                d_right = false;
                d_up = false;
                d_down = false;

        }





/***********************************************************************///存为赛道
        private Int16 count1 = 0;
        private void button3_Click(object sender, EventArgs e)
        {
            count1++;
            if (count1 == 1)
            {
                count2 = 0;
                textBox2.Text = "";
                textBox2.AppendText("是否创建新赛道？");
            }
            if (count1 == 2)
            {
                count1 = 0;
                for (int i = 0; i < 11; i++)
                {
                    string path = Path2 + @"\Track" + @i.ToString() + ".txt";
                    if (File.Exists(path) && File.Exists(Path2 + @"\Track20.txt")) 
                    {
                        textBox2.Text = "";
                        textBox2.AppendText("覆盖赛道Track" + i.ToString());
                        Write_int(path, OrbitX, OrbitY, Orbit_Change_New, OX, OY);
                        break;
                    }
                    if (!File.Exists(path) && !File.Exists(Path2 + @"\Track20.txt"))
                    {
                        textBox2.Text = "";
                        textBox2.AppendText("已创建新赛道Track" + i.ToString());
                        Write_int(path, OrbitX, OrbitY, Orbit_Change_New, OX, OY);
                        break;
                    }
                }
            }
        }

/***********************************************************************///存为轨迹
        private Int16 count2 = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            count2++;
            if (count2 == 1)
            {
                count1 = 0;
                textBox2.Text = "";
                textBox2.AppendText("是否创建新轨迹？");
            }
            if (count2 == 2)
            {
                count2 = 0;
                for (int i = 0; i < 21; i++)
                {
                    string path = Path1 + @"\Orbit" + @i.ToString() + ".txt";
                    if (File.Exists(path) && File.Exists(Path1 + @"\Orbit20.txt"))
                    {
                        textBox2.Text = "";
                        textBox2.AppendText("覆盖轨迹Orbit" + i.ToString());
                        Write_int(path, OrbitX, OrbitY, Orbit_Change_New, OX, OY);
                        break;
                    }
                    if (!File.Exists(path) && !File.Exists(Path1 + @"\Orbit20.txt"))
                    {
                        textBox2.Text = "";
                        textBox2.AppendText("已创建新轨迹Orbit" + i.ToString());
                        Write_int(path, OrbitX, OrbitY, Orbit_Change_New, OX, OY);
                        break;
                    }
                }
            }
        }

/***********************************************************************/ //存轨迹数据子程序
        private void Write_int(string path, List<int> WriteX, List<int> WriteY, double Change,int ox,int oy)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            writer.Write(Change);
            writer.Write(WriteX.Count);
            writer.Write(WriteY.Count);
            writer.Write(ox);
            writer.Write(oy);
            for (int i = 0; i < WriteX.Count; i++)
            {
                writer.Write(WriteX[i]);
            }
            for (int i = 0; i < WriteY.Count; i++)
            {
                writer.Write(WriteY[i]);
            }
            writer.Close();
            fs.Close();
            //清除轨迹
            OrbitX.RemoveRange(0, OrbitX.Count);
            OrbitY.RemoveRange(0, OrbitY.Count);
            OX = OX_Ever;
            OY = OY_Ever;
        }






        private bool Double_Invalidate = false;
/***********************************************************************///读取轨迹
        private List<int> ReadOrbit_X = new List<int>();         //数据结构----线性链表
        private List<int> ReadOrbit_Y = new List<int>();         //数据结构----线性链表

        private int ReadOrbit_OX;
        private int ReadOrbit_OY;
        private int readorbit_ox_empty;
        private int readorbit_oy_empty;

        private bool ReadOrbit_Flag = false;
        private Int16 ReadOrbit_Count = 0;

        private double ReadOrbit_CHANGE = 1;
        private double ReadOrbit_Change1 = 1;
        private double ReadOrbit_Change2 = 1;
        private double Comp_Change = 1;

        private int How_Long_Read1 = 0;                     //时间点使用
        private int How_Long_Read2 = 0;


        private void button5_Click(object sender, EventArgs e)
        {
            bool Message_Flag = true;
            ReadOrbit_Flag = true;
            int X_Count = 0, Y_Count = 0;
            string path = Path1 + @"\Orbit" + textBox1.Text + ".txt";
            //轨迹覆盖

            Orbit_Change_New = 1.0;
            Orbit_Change_Last = 1.0;

            if ( ReadOrbit_Count % 2 == 0)
            {
                OrbitChange_X1.RemoveRange(0, OrbitChange_X1.Count);
                OrbitChange_Y1.RemoveRange(0, OrbitChange_Y1.Count);
                ReadOrbit_X1.RemoveRange(0, ReadOrbit_X1.Count);
                ReadOrbit_Y1.RemoveRange(0, ReadOrbit_Y1.Count);
            }
            if ( ReadOrbit_Count % 2 == 1)
            {
                OrbitChange_X2.RemoveRange(0, OrbitChange_X2.Count);
                OrbitChange_Y2.RemoveRange(0, OrbitChange_Y2.Count);
                ReadOrbit_X2.RemoveRange(0, ReadOrbit_X2.Count);
                ReadOrbit_Y2.RemoveRange(0, ReadOrbit_Y2.Count);
            }
            if (ReadOrbit_Count > 0)
            {
                ReadOrbit_X.RemoveRange(0, ReadOrbit_X.Count);
                ReadOrbit_Y.RemoveRange(0, ReadOrbit_Y.Count);
            }
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);
                try
                {
                    if (ReadOrbit_Count % 2 == 0)
                    {
                        ReadOrbit_Change1 = reader.ReadDouble();
                        Comp_Change = ReadOrbit_Change1;
                    }
                    if (ReadOrbit_Count % 2 == 1)
                    {
                        ReadOrbit_Change2 = reader.ReadDouble();
                        Comp_Change = ReadOrbit_Change2;
                    }
                    //时间点排列数目读取
                    X_Count = reader.ReadInt32();
                    Y_Count = reader.ReadInt32();
                    if (ReadOrbit_Count % 2 == 0)
                    {
                        How_Long_Read1 = X_Count;
                    }
                    if (ReadOrbit_Count % 2 == 1)
                    {
                        How_Long_Read2 = X_Count;
                    }

                    if (ReadOrbit_Count == 0)
                    {
                        ReadOrbit_OX = reader.ReadInt32();
                        ReadOrbit_OY = reader.ReadInt32();
                    }
                    if (ReadOrbit_Count > 0)
                    {
                        if (Comp_Change >= ReadOrbit_Change1 && Comp_Change >= ReadOrbit_Change2)
                        {
                            ReadOrbit_OX = reader.ReadInt32();
                            ReadOrbit_OY = reader.ReadInt32();
                        }
                        else
                        {
                            readorbit_ox_empty = reader.ReadInt32();
                            readorbit_oy_empty = reader.ReadInt32();
                        }
                    }
                    for (int i = 0; i < X_Count; i++)
                    {
                        ReadOrbit_X.Add(reader.ReadInt32());
                    } 
                    for (int i = 0; i < Y_Count; i++)
                    {
                        ReadOrbit_Y.Add(reader.ReadInt32());
                    }
                }
                catch
                {
                    textBox2.Text = "";
                    textBox2.AppendText("读取轨迹失败");
                }
                reader .Close ();
                fs.Close ();
                ReadOrbit_Count++;
            }
            else
            {
                Message_Flag = false;
                textBox2.Text = "";
                textBox2.AppendText("指定轨迹不存在");
            }
            if ( ReadOrbit_Count % 2 == 1)
            {
                for (int i = 0; i < ReadOrbit_X.Count; i++)
                {
                    OrbitChange_X1.Add(0);
                    OrbitChange_Y1.Add(0);
                    ReadOrbit_X1.Add(0);
                    ReadOrbit_Y1.Add(0);
                }
                if (ReadOrbit_Count <= 2 && Message_Flag)
                {
                    textBox2.Text = "";
                    textBox2.AppendText("轨迹读取成功");
                }
                if (ReadOrbit_Count > 2 && Message_Flag)
                {
                    textBox2.Text = "";
                    textBox2.AppendText("覆盖轨迹1");
                }
            }
            if ( ReadOrbit_Count % 2 == 0)
            {
                for (int i = 0; i < ReadOrbit_X.Count; i++)
                {
                    OrbitChange_X2.Add(0);
                    OrbitChange_Y2.Add(0);
                    ReadOrbit_X2.Add(0);
                    ReadOrbit_Y2.Add(0);
                }
                if (ReadOrbit_Count <= 2 && Message_Flag)
                {
                    textBox2.Text = "";
                    textBox2.AppendText("轨迹读取成功");
                }
                if (ReadOrbit_Count > 2 && Message_Flag)
                {
                    textBox2.Text = "";
                    textBox2.AppendText("覆盖轨迹2");
                }
            }
            Double_Invalidate = true;

            ReadLongMax();//求出轨迹最大时间点数
            Invalidate();
        }

/***********************************************************************///读取赛道
        private List<int> Track_X = new List<int>();         //数据结构----线性链表
        private List<int> Track_Y = new List<int>();         //数据结构----线性链表
        private double ReadTrack_Change = 1;
        private int ReadTrack_OX;
        private int ReadTrack_OY;
        private bool ReadTrack_Flag = false;
        private Int16 ReadTrack_Count = 0;


        private int Long_Max = 0;                           //最大时间点计算函数
        private void ReadLongMax()
        {
            if (ReadOrbit_Count == 1 && Orbit_Received)
            {
                if (How_Long_Read1 > How_Long_Orbit) Long_Max = How_Long_Read1;
                else Long_Max = How_Long_Orbit;
            }
            if (ReadOrbit_Count == 1 && !Orbit_Received)
            {
                Long_Max = How_Long_Read1;
            }
            if (ReadOrbit_Count > 1 && Orbit_Received)
            {
                if (How_Long_Read1 > How_Long_Read2) Long_Max = How_Long_Read1;
                else Long_Max = How_Long_Read2;
                if (Long_Max < How_Long_Orbit) Long_Max = How_Long_Orbit;
            }
            if (ReadOrbit_Count > 1 && !Orbit_Received)
            {
                if (How_Long_Read1 > How_Long_Read2) Long_Max = How_Long_Read1;
                else Long_Max = How_Long_Read2;
            }
        }



        private void button6_Click(object sender, EventArgs e)
        {
            ReadLongMax();                                              //时间点使用。每次读取计算最大时间点。
            bool Message_Flag = true;
            ReadTrack_Flag = true;
            int X_Count = 0, Y_Count = 0;
            string path = Path2 + @"\Track" + textBox3.Text + ".txt";
            //赛道覆盖

            Orbit_Change_New = 1.0;
            Orbit_Change_Last = 1.0;

            TrackChange_X.RemoveRange(0, TrackChange_X.Count);
            TrackChange_Y.RemoveRange(0, TrackChange_Y.Count);
            if (ReadTrack_Count>0)
            {
                Track_X.RemoveRange(0, Track_X.Count);
                Track_Y.RemoveRange(0, Track_Y.Count);
            }
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);
                try
                {
                    ReadTrack_Change = reader.ReadDouble();
                    X_Count = reader.ReadInt32();
                    Y_Count = reader.ReadInt32();
                    ReadTrack_OX = reader.ReadInt32();
                    ReadTrack_OY = reader.ReadInt32();
                    for (int i = 0; i < X_Count; i++)
                    {
                        Track_X.Add(reader.ReadInt32());
                    }
                    for (int i = 0; i < Y_Count; i++)
                    {
                        Track_Y.Add(reader.ReadInt32());
                    }
                }
                catch
                {
                    textBox2.Text = "";
                    textBox2.AppendText("读取赛道失败");
                }
                reader.Close();
                fs.Close();
                ReadTrack_Count++;
            }
            else
            {
                Message_Flag = false;
                textBox2.Text = "";
                textBox2.AppendText("指定赛道不存在");
            }
            if (ReadTrack_Count > 1 && Message_Flag)
            {
                textBox2.Text = "";
                textBox2.AppendText("覆盖原赛道");
            }
            if (ReadTrack_Count == 1 && Message_Flag)
            {
                textBox2.Text = "";
                textBox2.AppendText("赛道读取成功");
            }
            for (int i = 0; i < Track_X.Count; i++)
            {
                TrackChange_X.Add(0);
                TrackChange_Y.Add(0);
            }
            Double_Invalidate = true;
            Invalidate();
        }








/***********************************************************************///定时器
        private void Time1_Tick(object sender, EventArgs e)
        {
            ReadLongMax();//求出轨迹最大时间点数

            Point_Time = Convert.ToInt32(textBox4.Text);

            if (Point_Time < Long_Max)
            {
                Point_Time++;
                textBox4.Text = Convert.ToString(Point_Time);
                shibo_start = (short)Point_Time;
                Invalidate();
            }
            else
            {
                textBox2.Text = "";
                textBox2.AppendText("达到最大时间点:" + Convert.ToString(Point_Time));

                button10.BackgroundImage = Properties.Resources.go;//改变图标
                timer1.Enabled = false;

                Point_Time = 0;
                textBox4.Text = Convert.ToString(Point_Time);
                KeyD_Time++;
            }

        }







/***********************************************************************///清除当前轨迹
        private void button13_Click(object sender, EventArgs e)
        {
            ChangeX.RemoveRange(0, ChangeX.Count);
            ChangeY.RemoveRange(0, ChangeX.Count);
            OrbitX.RemoveRange (0,OrbitX.Count);
            OrbitY.RemoveRange(0, OrbitY.Count);

            date0.RemoveRange(0, date0.Count);
            date1.RemoveRange(0, date1.Count);
            date2.RemoveRange(0, date2.Count);
            date3.RemoveRange(0, date3.Count);

            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            if (!ReadTrack_Flag && !ReadOrbit_Flag)
            {
                Orbit_Change_New = 1;
            }
            Orbit_Change_New = 1.0;              
            Orbit_Change_Last = 1.0;
            Invalidate();
        }
/***********************************************************************///清除所有轨迹
        private void button14_Click(object sender, EventArgs e)
        {
            ChangeX.RemoveRange(0, ChangeX.Count);//清除轨迹
            ChangeY.RemoveRange(0, ChangeX.Count);
            OrbitX.RemoveRange(0, OrbitX.Count);
            OrbitY.RemoveRange(0, OrbitY.Count);

            ReadOrbit_X.RemoveRange(0, ReadOrbit_X.Count);//清除读取赛道
            ReadOrbit_Y.RemoveRange(0, ReadOrbit_Y.Count);
            OrbitChange_X1.RemoveRange(0, OrbitChange_X1.Count);
            OrbitChange_Y1.RemoveRange(0, OrbitChange_Y1.Count);
            OrbitChange_X2.RemoveRange(0, OrbitChange_X2.Count);
            OrbitChange_Y2.RemoveRange(0, OrbitChange_Y2.Count);
            ReadOrbit_X1.RemoveRange(0, ReadOrbit_X1.Count);
            ReadOrbit_Y1.RemoveRange(0, ReadOrbit_Y1.Count);
            ReadOrbit_X2.RemoveRange(0, ReadOrbit_X1.Count);
            ReadOrbit_Y2.RemoveRange(0, ReadOrbit_Y1.Count);

            TrackChange_X.RemoveRange(0, TrackChange_X.Count);//清除赛道
            TrackChange_Y.RemoveRange(0, TrackChange_Y.Count);
            Track_X.RemoveRange(0, Track_X.Count);
            Track_Y.RemoveRange(0, Track_Y.Count);


            date0.RemoveRange(0, date0.Count);
            date1.RemoveRange(0, date1.Count);
            date2.RemoveRange(0, date2.Count);
            date3.RemoveRange(0, date3.Count);

            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            //重新初始化起点及变形大小
            OX = OX_Ever;
            OY = OY_Ever;
            ReadOrbit_OX = BackX + Back_XMax / 2;
            ReadOrbit_OY = BackY + Back_YMax / 2;
            ReadTrack_OX = BackX + Back_XMax / 2;
            ReadTrack_OY = BackY + Back_YMax / 2;
            Orbit_Change_New = 1;
            Orbit_Change_Last = 1;
            ReadOrbit_Count = 0;
            ReadTrack_Count = 0;
            ReadOrbit_Change1 = 1;
            ReadOrbit_Change2 = 1;
            ReadTrack_Change = 1;
            ReadOrbit_CHANGE = 1;
            ReadTrack_Flag = false;
            ReadOrbit_Flag = false;

            ////////时间点相关变量
            How_Long_Orbit = 0;
            How_Long_Read1 = 0;
            How_Long_Read2 = 0;
            Long_Max = 0;
            PointX_Read1 = 0;
            PointY_Read1 = 0;
            PointX_Read2 = 0;
            PointY_Read2 = 0;

            PointX_Orbit = 0;
            PointY_Orbit = 0;

            textBox2.Text = "";
            textBox2.AppendText("清理完毕！");

            textBox4.Text = "0";
            Point_Time = 0;


            Orbit_Change_New = 1.0;
            Orbit_Change_Last = 1.0;

            Invalidate();
        }



        


/***********************************************************************///发送起跑，停跑标志位
        private void button7_Click(object sender, EventArgs e)
        {
        }
        private void button8_Click(object sender, EventArgs e)
        {
        }





/***********************************************************************///播放轨迹
        int KeyD_Time = 0;
        private void button10_Click(object sender, EventArgs e)
        {
            if (ReadOrbit_Flag)
            {
                KeyD_Time++;
                if (KeyD_Time % 2 == 1)//开始分析
                {
                    button10.BackgroundImage = Properties.Resources.stop;//改变图标
                    timer1.Enabled = true;
                }
                if (KeyD_Time % 2 == 0)
                {
                    button10.BackgroundImage = Properties.Resources.go;//改变图标
                    timer1.Enabled = false;
                }
            }
            else
            {
                textBox2.Text = "";
                textBox2.AppendText("未读取轨迹！");
            }
            Int16 i;
            i = Convert.ToInt16(label7.Text);
            i = (short)(70 / i);
            timer1.Interval = i;
        }
/***********************************************************************///下一轨迹点
        private void button11_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            ReadLongMax();//求出轨迹最大时间点数

            if (ReadOrbit_Flag)
            {
                Point_Time = Convert.ToInt32(textBox4.Text);

                if (Point_Time < Long_Max)
                {
                    Point_Time++;
                    textBox4.Text = Convert.ToString(Point_Time);
                    shibo_start = (short)Point_Time;
                    Invalidate();
                }
                else
                {
                    textBox4.Text = Convert.ToString(Point_Time);
                    textBox2.Text = "";
                    textBox2.AppendText("达到最大时间点！");
                }
            }
            else
            {
                textBox2.Text = "";
                textBox2.AppendText("未读取轨迹！");
            }
        }
 /***********************************************************************///上一轨迹点
        private void button9_Click(object sender, EventArgs e)
        {

            timer1.Enabled = false;
            ReadLongMax();//求出轨迹最大时间点数

            if (ReadOrbit_Flag)
            {
                Point_Time = Convert.ToInt32(textBox4.Text);

                if (Point_Time > 0)
                {
                    Point_Time--;
                    textBox4.Text = Convert.ToString(Point_Time);
                    shibo_start = (short)Point_Time;
                    Invalidate();
                }
                else
                {
                    textBox4.Text = Convert.ToString(Point_Time);
                    textBox2.Text = "";
                    textBox2.AppendText("到达初始点！");
                }
            }
            else
            {
                textBox2.Text = "";
                textBox2.AppendText("未读取轨迹！");
            }
        }
/***********************************************************************///转到指定轨迹点

        private int Point_Time = 0;
        private void button12_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            ReadLongMax();//求出轨迹最大时间点数

            if (ReadOrbit_Flag)
            {
                if (textBox4.Text == "")
                {
                    textBox2.Text = "";
                    textBox2.AppendText("请输入时间点！");
                }
                else
                {
                     Point_Time = Convert.ToInt32(textBox4.Text);
                    if ( Point_Time < 0)
                    {
                        textBox2.Text = "";
                        textBox2.AppendText("时间不可为负！");
                    }
                    else
                    {
                        if (Point_Time > Long_Max)
                        {
                            textBox2.Text = "";
                            textBox2.AppendText("超出最大时间点！");
                        }
                        else
                        {
                            textBox4.Text = Convert.ToString(Point_Time);
                        }
                    }
                }
            }
                else 
                {
                    textBox2.Text = "";
                    textBox2.AppendText("未读取轨迹！");
                }
            Invalidate();
        }

/***********************************************************************///播放速率增加
        private Int16 Speed = 5;
        private void button15_Click(object sender, EventArgs e)
        {
            if (Speed <= 9)
            {
                Speed++;
                label7.Text = Convert.ToString(Speed);
            }
            else
            {
                Speed = 10;
                textBox2.Text = "";
                textBox2.AppendText("最大播放速度为 10 ！");
            }
            Int16 i;
            i = Convert.ToInt16(label7.Text);
            i = (short)(70 / i);
            timer1.Interval = i;

        }
/***********************************************************************///播放速率减小
        private void button16_Click(object sender, EventArgs e)
        {
            if (Speed >= 2)
            {
                Speed--;
                label7.Text = Convert.ToString(Speed);
            }
            else
            {
                Speed = 1;
                textBox2.Text = "";
                textBox2.AppendText("最小播放速度为 1 ！");
            }
            Int16 i;
            i = Convert.ToInt16(label7.Text);
            i = (short)(70 / i);
            timer1.Interval = i;

        }




/***********************************************************************///
        private void Data_Received(object sender, SerialDataReceivedEventArgs e)
        {
            byte i = 0, j = 0, k = 0; byte flag1 = 0, flag2 = 0, flag3 = 0;
            byte[] Receive0 = new byte[2] { 0, 0 }; byte[] Receive1 = new byte[2] { 0, 0 };
            byte[] Receive2 = new byte[2] { 0, 0 }; byte[] Receive3 = new byte[2] { 0, 0 };
            byte[] Receive4 = new byte[2] { 0, 0 }; byte[] Receive5 = new byte[2] { 0, 0 };
            byte[] Receive6 = new byte[2] { 0, 0 }; byte[] Receive7 = new byte[2] { 0, 0 };
            byte[] data = new byte[14];
            byte testmenber = 0;
            int ch0 = 0, ch1 = 0, ch2 = 0, ch3 = 0, ch4 = 0, ch5 = 0;
            try
            {
                byte[] datatest = new byte[serialPort1.BytesToRead];                                //定义缓冲区，因为串口事件触发时有可能收到不止一个字节

                serialPort1.Read(datatest, 0, 30);

                foreach (byte Member in datatest)                                                   //遍历用法
                {
                    testmenber = Member;
                    if (Member == 0x52) flag1 = 1;
                    if (flag2 == 1) { data[k] = Member; k++; }
                    if (flag1 == 1 && Member == 0xcc) { flag1 = 0; flag2 = 1; }
                    if (k >= 13 && Member == 0x88) { flag1 = 0; flag2 = 0; flag3 = 1; k = 0; break; }



                } if (flag3 == 1)
                {
            textBox2.Text = "";
            textBox2.AppendText("1");
                    j = 0;
                    for (i = 0; i < 2; i++, j++)
                        Receive0[i] = data[j];
                    ch0 = (int)((Receive0[0]) * 256 + (Receive0[1]));

                    for (i = 0; i < 2; i++, j++)
                        Receive1[i] = data[j];
                    ch1 = (int)((Receive1[0]) * 256 + (Receive1[1]));

                    for (i = 0; i < 2; i++, j++)
                        Receive2[i] = data[j];
                    ch2 = (int)((Receive2[0]) * 256 + (Receive2[1]));
                    for (i = 0; i < 2; i++, j++)
                        Receive3[i] = data[j];
                    ch3 = (int)((Receive3[0]) * 256 + (Receive3[1]));
                    for (i = 0; i < 2; i++, j++)
                        Receive4[i] = data[j];
                    ch4 = (int)((Receive4[0]) * 256 + (Receive4[1]));
                    for (i = 0; i < 2; i++, j++)
                        Receive5[i] = data[j];
                    ch5 = (int)((Receive5[0]) * 256 + (Receive5[1]));
                    j = 0; flag3 = 0;
                  //  string str0 = Convert.ToString(ch0, 16);
                  //  textBox5.AppendText("0x" + (str0.Length == 1 ? "0" + str0 : str0) + " ");
                    double b;

                    OrbitLength.Add(ch0);
                    b= ch1 * Math.PI / 180;
                    OrbitAngle.Add(b);
                    Orbit_Received = true;

                    date0.Add(ch2);
                    date1.Add(ch3);
                    date2.Add(ch4);
                    date3.Add(ch5);
                    shibo_start = (short)(date0.Count - 9);

                    Invalidate();
                }
            }
            catch { }

        }



        private List<int> date0 = new List<int>();         //数据结构----线性链表
        private List<int> date1 = new List<int>();         //数据结构----线性链表
        private List<int> date2 = new List<int>();         //数据结构----线性链表
        private List<int> date3 = new List<int>();         //数据结构----线性链表
        private Pen pend1 = new Pen(Color.Red);            //外框
        private Pen pend2 = new Pen(Color.Yellow);            //外框
        private Int16 shibox = 10, shiboy = 220, shibokuan = 190, shibogao = 200, second = 220;
        private Int16 shibo_point=19;
        private Int16 shibo_start = 0;
        private void ShiBo_Paint(object sender, PaintEventArgs e)
        {
            //填充画图区
            e.Graphics.FillRectangle(Brushes.Black, new RectangleF(0, 0, 210, 900));
            //画框
            e.Graphics.DrawLine(Kuang, shibox, shiboy + second, shibox + shibokuan, shiboy + second);
            e.Graphics.DrawLine(Kuang, shibox, shiboy + second, shibox, shiboy + shibogao + second);
            e.Graphics.DrawLine(Kuang, shibox, shiboy + shibogao + second, shibox + shibokuan, shiboy + shibogao + second);
            e.Graphics.DrawLine(Kuang, shibox + shibokuan, shiboy + second, shibox + shibokuan, shiboy + shibogao + second);


            if (checkBox3.Checked)
            {
                if (shibo_start < 0) shibo_start = 0;
                for (int i = shibo_start; i < (shibo_start + 8 < date2.Count - 1 ? shibo_start + 8 : date2.Count - 1); i++)
                {
                    e.Graphics.DrawLine(pend1, shibox + (i - shibo_start) * shibo_point, shiboy + shibogao + second - date2[i] * 200 / 260, shibox + (i - shibo_start + 1) * shibo_point, shiboy + shibogao + second - date2[i + 1] * 200 / 260);
                }
            }
            if (checkBox4.Checked)
            {
                if (shibo_start < 0) shibo_start = 0;
                for (int i = shibo_start; i < (shibo_start + 8 < date3.Count - 1 ? shibo_start + 8 : date3.Count - 1); i++)
                {
                    e.Graphics.DrawLine(pend2, shibox + (i - shibo_start) * shibo_point, shiboy + shibogao + second - date3[i] * 200 / 260, shibox + (i - shibo_start + 1) * shibo_point, shiboy + shibogao + second - date3[i + 1] * 200 / 260);
                }
            }
            //填充画图区
            e.Graphics.FillRectangle(Brushes.Black, new RectangleF(0, 0, 210, 430));
            //画框
            e.Graphics.DrawLine(Kuang, shibox, shiboy, shibox + shibokuan, shiboy);
            e.Graphics.DrawLine(Kuang, shibox, shiboy, shibox, shiboy + shibogao);
            e.Graphics.DrawLine(Kuang, shibox, shiboy + shibogao, shibox + shibokuan, shiboy + shibogao);
            e.Graphics.DrawLine(Kuang, shibox + shibokuan, shiboy, shibox + shibokuan, shiboy + shibogao);

            if (checkBox1.Checked)
            {
                if (shibo_start < 0) shibo_start = 0;
                for (int i = shibo_start; i < (shibo_start + 8 < date0.Count - 1 ? shibo_start + 8 : date0.Count - 1); i++)
                {
                    e.Graphics.DrawLine(pend1, shibox + (i - shibo_start) * shibo_point, shiboy + shibogao - date0[i] * 200 / 1000, shibox + (i - shibo_start + 1) * shibo_point, shiboy + shibogao - date0[i + 1] * 200 / 1000);
                }
            }
            if (checkBox2.Checked)
            {
                if (shibo_start < 0) shibo_start = 0;
                for (int i = shibo_start; i < (shibo_start + 8 < date1.Count - 1 ? shibo_start + 8 : date1.Count - 1); i++)
                {
                    e.Graphics.DrawLine(pend2, shibox + (i - shibo_start) * shibo_point, shiboy + shibogao - date1[i] * 200 / 1000, shibox + (i - shibo_start + 1) * shibo_point, shiboy + shibogao - date1[i + 1] * 200 / 1000);
                }
            }
            e.Graphics.FillRectangle(Brushes.Black, new RectangleF(0, 0, 210, 220));

            if (date0.Count != 0)
            {
                if (shibo_start < 0) shibo_start = 0;
                if (shibo_start >= date0.Count)
                {
                    shibo_start = (short)(date0.Count - 9);
                    textBox5.Text = "";
                    textBox6.Text = "";
                    textBox7.Text = "";
                    textBox8.Text = "";
                }
                else
                {
                    textBox5.Text = Convert.ToString(date0[shibo_start]);
                    textBox6.Text = Convert.ToString(date1[shibo_start]);
                    textBox7.Text = Convert.ToString(date2[shibo_start]);
                    textBox8.Text = Convert.ToString(date3[shibo_start]);
                }
            }
        }

        private void checked_change(object sender, EventArgs e)
        {
            Invalidate();
        }

/***********************************************************************///
    }
}
