using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace 标定
{
    public partial class Form1 : Form
    {
        
        private static HWindow hwindow;
        public HTuple ImagePath = new HTuple();
        HTuple ImageHeight = null, ImageWidth = null;
        private HObject getImage = new HObject();
        private HObject getImage1 = new HObject();
        HObject ho_Image, ho_GrayImage, ho_Regions;
        HObject ho_WiresFilled, ho_Balls, ho_SingleBalls, ho_IntermediateBalls;
        HObject ho_Circle;

        // Local control variables 

        HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
        HTuple hv_Radius = new HTuple(), hv_WorldRow = new HTuple();

        private void button2_Click(object sender, EventArgs e)
        {
            double a = Convert.ToDouble(textBox3.Text);
            double b = Convert.ToDouble(textBox4.Text);
            HOperatorSet.AffineTransPoint2d(hv_HomMat2D1, a, b, out hv_Qx, out hv_Qy);
            textBox5.Text = hv_Qx.ToString();
            textBox6.Text = hv_Qy.ToString();
        }

        HTuple hv_WorldCol = new HTuple(), hv_Index = new HTuple();
        HTuple hv_HomMat2D1 = new HTuple(), hv_SerializedItemHandle = new HTuple();
        HTuple hv_FileHandle = new HTuple(), hv_Qx = new HTuple();
        HTuple hv_Qy = new HTuple();
        // Initialize local and output iconic variables 

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
        HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_WiresFilled);
            HOperatorSet.GenEmptyObj(out ho_Balls);
            HOperatorSet.GenEmptyObj(out ho_SingleBalls);
            HOperatorSet.GenEmptyObj(out ho_IntermediateBalls);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.Rgb1ToGray(getImage, out getImage1);
            HOperatorSet.Threshold(getImage1, out ho_Regions, 13, 71);
            HOperatorSet.DispObj(getImage1, hwindow);
            //填充缺失
            ho_WiresFilled.Dispose();
            HOperatorSet.FillUpShape(ho_Regions, out ho_WiresFilled, "area", 1, 100);

            //开操作 腐蚀和膨胀的结合，即先腐蚀后膨胀
            ho_Balls.Dispose();
            HOperatorSet.OpeningCircle(ho_WiresFilled, out ho_Balls, 15.5);

            ho_SingleBalls.Dispose();
            HOperatorSet.Connection(ho_Balls, out ho_SingleBalls);

            //找到具有目标特征的形状，这边填写的参数是 ‘circularity’ ，就是类圆的图形；
            ho_IntermediateBalls.Dispose();
            HOperatorSet.SelectShape(ho_SingleBalls, out ho_IntermediateBalls, "circularity",
                "and", 0.85, 1.0);

            //确定这些圆形区域的最小外接圆，并将输出的坐标和半径做处理后输出
            hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose();
            HOperatorSet.SmallestCircle(ho_SingleBalls, out hv_Row, out hv_Column, out hv_Radius);

            //生成 虚拟机械坐标 行往下偏移80
            hv_WorldRow.Dispose();
            hv_WorldRow = new HTuple();
            hv_WorldCol.Dispose();
            hv_WorldCol = new HTuple();

            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Row.TupleLength())) - 1); hv_Index = (int)hv_Index + 1)
            {
                if (hv_WorldRow == null)
                    hv_WorldRow = new HTuple();
                hv_WorldRow[hv_Index] = (hv_Row.TupleSelect(hv_Index)) + 80;
                if (hv_WorldCol == null)
                    hv_WorldCol = new HTuple();
                hv_WorldCol[hv_Index] = hv_Column.TupleSelect(hv_Index);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_WorldRow.TupleSelect(hv_Index), hv_WorldCol.TupleSelect(
                        hv_Index), 10);
                }
            }

            //生成标定
            hv_HomMat2D1.Dispose();
            HOperatorSet.VectorToHomMat2d(hv_Row, hv_Column, hv_WorldRow, hv_WorldCol, out hv_HomMat2D1);
            hv_SerializedItemHandle.Dispose();
            HOperatorSet.SerializeHomMat2d(hv_HomMat2D1, out hv_SerializedItemHandle);
            hv_FileHandle.Dispose();
            HOperatorSet.OpenFile("C:/Users/Administrator/Desktop/111.mat", "output_binary",
                out hv_FileHandle);
            HOperatorSet.FwriteSerializedItem(hv_FileHandle, hv_SerializedItemHandle);
            HOperatorSet.CloseFile(hv_FileHandle);
            textBox1.Text = hv_Row.ToString();
            textBox1.Text += hv_Column.ToString();
            textBox2.Text = hv_WorldRow.ToString();
            textBox2.Text += hv_WorldCol.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "JPEG文件|*.jpg*";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImagePath = openFileDialog1.FileName;
                HObject Image;
                HOperatorSet.GenEmptyObj(out Image);
                hwindow = hWindowControl1.HalconWindow;
                hwindow.ClearWindow();
                HOperatorSet.ReadImage(out getImage, ImagePath);
                HOperatorSet.GetImageSize(getImage, out ImageWidth, out ImageHeight);
                HOperatorSet.SetPart(hwindow, 0, 0, ImageHeight - 1, ImageWidth - 1);
                HOperatorSet.DispObj(getImage, hwindow);
            }
        }
    }
}
