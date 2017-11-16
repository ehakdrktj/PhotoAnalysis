using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Windows.Forms.DataVisualization.Charting;

namespace PhotoAnalysis
{
    public partial class MainForm : Form
    {
        Form2 form2;
        FilterImageForm filterForm;

        Image originalImage;
        Bitmap resizeImage;
        double totalBrightness;
        double totalSaturation;


        Bitmap source;
        Bitmap result;

        //Bitmap[] changerImage = new Bitmap[6];

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            LoadImage();
        }
        void LoadImage()
        {
            totalBrightness = 0.0f;
            totalSaturation = 0.0f;

            string filePath = OpenImageFile();
            if (filePath != "")
            {
                Bitmap oBitmap = new Bitmap(originalImage);
                var totalRGB = SetImageColorToRGB(resizeImage);

                label1.Text = "이미지 파일 포맷 : " + GetImageFormatName(originalImage);
                label1.Text += "\n\n";
                label1.Text += "해상도 : " + GetImagePixel(oBitmap);
                label1.Text += "\n\n";

                GetImageColorFromRGB(totalRGB.Item1, totalRGB.Item2, totalRGB.Item3);

                int[] hueMatrix = SetImageColorToHue(resizeImage);
                GetImageColorFromHue(hueMatrix);

                double brighPer = GetTotalBrightnessPercent(resizeImage);
                double satuPer = GetTotalSaturationPercent(resizeImage);

                label1.Text += "밝기 : " + Math.Round(brighPer, 2).ToString() + "%    (0% : 어두움 ~ 100% : 밝음)";
                label1.Text += "\n\n";
                label1.Text += "채도 : " + Math.Round(satuPer, 2).ToString() + "%     (0% : 회색 ~ 100% : 단색)";
                label1.Text += "\n\n";
            }

            form2 = new Form2();
            filterForm = new FilterImageForm();
        }

        // 이미지 파일 크기 조정하여 그리기
        public string OpenImageFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All Files(*.*)|*.*|PNG File(*.png)|*.png|Bitmap File(*.bmp)|*.bmp|JPEG File(*.jpg)|*.jpg";
            openFileDialog1.Title = "Select a Image File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                originalImage = Image.FromFile(openFileDialog1.FileName);

                int reWidth = Math.Min(originalImage.Width, Constants.MAXWIDTH);
                int reHeight = Math.Min(originalImage.Height, Constants.MAXHEIGHT);

                //if (originalImage.Width < reWidth)
                //    reWidth = originalImage.Width;

                //if (originalImage.Height < reHeight)
                //    reHeight = originalImage.Height;

                /* 
                 * 너비 800 기준으로 높이를 비율에 맞게 조정한다.   
                 * 조정한 높이값이 450 이상일때는, 높이값을 450으로 맞추고 이 비율에 맞게 너비값을 다시 조정한다.
                 */
                if (originalImage.Height * reWidth / originalImage.Width > reHeight)
                {
                    reWidth = originalImage.Width * reHeight / originalImage.Height;
                }
                else
                {
                    reHeight = originalImage.Height * reWidth / originalImage.Width;
                }

                Size resize = new Size(reWidth, reHeight);

                resizeImage = new Bitmap(originalImage, resize);
                this.pictureBox1.Image = resizeImage;

                button1.Visible = false;
            }

            return openFileDialog1.FileName;
        }

        // 이미지 파일 정보 가져오기
        public string GetImageFormatName(Image image)
        {
            return new ImageFormatConverter().ConvertToString(image.RawFormat);
        }

        // 해상도 가져오기
        public string GetImagePixel(Bitmap bitmap)
        {
            return bitmap.Width + " X " + bitmap.Height;
        }

        // 색분포 저장 RGB
        public Tuple<int, int, int> SetImageColorToRGB(Bitmap bitmap)
        {
            int red = 0;
            int green = 0;
            int blue = 0;

            if (bitmap != null)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        Color c = bitmap.GetPixel(i, j);

                        totalBrightness += c.GetBrightness();
                        totalSaturation += c.GetSaturation();

                        red += c.R;
                        green += c.G;
                        blue += c.B;
                    }
                }
            }

            return Tuple.Create(red, green, blue);
        }

        // 색분포 저장 HUE
        public int[] SetImageColorToHue(Bitmap bitmap)
        {
            int[] matrix = new int[Constants.MAXHUE + 1];

            if (bitmap != null)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        Color c = bitmap.GetPixel(i, j);
                        int hueValue = (int)c.GetHue();
                        matrix[hueValue] += 1;
                    }
                }
            }

            return matrix;
        }

        // 색분포 불러오기 RGB
        public void GetImageColorFromRGB(int totalRed, int totalGreen, int totalBlue)
        {
            chart1.Series.Clear();

            Series sRGB = chart1.Series.Add("rgb");
            sRGB.ChartType = SeriesChartType.Pie;

            double totalRGB = totalRed + totalGreen + totalBlue;
            double dRedPer = totalRed / totalRGB * 100;
            double dGreenPer = totalGreen / totalRGB * 100;
            double dBluePer = totalBlue / totalRGB * 100;

            sRGB.Points.Add(totalRed);
            sRGB.Points.Add(totalGreen);
            sRGB.Points.Add(totalBlue);

            sRGB.Points[0].Color = Color.FromArgb(Constants.MAXRED, Constants.MAXGREEN - 210, Constants.MAXBLUE - 210);
            sRGB.Points[1].Color = Color.FromArgb(Constants.MAXRED - 210, Constants.MAXGREEN, Constants.MAXBLUE - 210);
            sRGB.Points[2].Color = Color.FromArgb(Constants.MAXRED - 210, Constants.MAXGREEN - 210, Constants.MAXBLUE);

            sRGB.Points[0].LegendText = Math.Round(dRedPer, 2).ToString() + "%";
            sRGB.Points[1].LegendText = Math.Round(dGreenPer, 2).ToString() + "%";
            sRGB.Points[2].LegendText = Math.Round(dBluePer, 2).ToString() + "%";
        }

        // 색분포 불러오기 HUE
        public void GetImageColorFromHue(int[] matrix)
        {
            chart2.Series.Clear();
            Series sHue = chart2.Series.Add("HUE"); //새로운 series 생성
            sHue.ChartType = SeriesChartType.Column;

            int minNum = 0;
            int maxNum = 0;

            for (int i = 0; i < Constants.MAXHUE; i++)
            {
                sHue.Points.AddXY(i, matrix[i]);

                if (minNum > matrix[i])
                {
                    minNum = matrix[i];
                }

                if (maxNum < matrix[i])
                {
                    maxNum = matrix[i];
                }
            }

            chart2.ChartAreas[0].AxisY.Minimum = minNum;
            chart2.ChartAreas[0].AxisY.Maximum = maxNum;
            chart2.ChartAreas[0].AxisX.Minimum = -1;
            chart2.ChartAreas[0].AxisX.Maximum = Constants.MAXHUE + 1;

            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "{,}";
            chart2.ChartAreas[0].AxisY.LabelStyle.Format = "{,}";
            chart2.ChartAreas[0].AxisX.Interval = Constants.MAXHUE + 1;
            chart2.ChartAreas[0].AxisY.Interval = maxNum;

            chart2.ChartAreas[0].AxisX.IsMarginVisible = false;
            chart2.ChartAreas[0].AxisY.IsMarginVisible = false;

            chart2.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;


            pictureBox2.Size = new Size(chart2.Width - 7, 10);
            pictureBox2.Visible = true;
        }

        public double GetTotalBrightnessPercent(Bitmap bitmap)
        {
            return totalBrightness / (bitmap.Width * bitmap.Height) * 100;
        }

        public double GetTotalSaturationPercent(Bitmap bitmap)
        {
            return totalSaturation / (bitmap.Width * bitmap.Height) * 100;
        }

        public void PrintImage(Bitmap image, PictureBox p)
        {
            Size resize = new Size(image.Width, image.Height);

            if (resize.Width >= p.Width)
                resize = new Size(p.Width, p.Width * resize.Height / resize.Width);

            if (resize.Height >= p.Height)
                resize = new Size(p.Height * resize.Width / resize.Height, p.Height);

            image = new Bitmap(image, resize);

            p.Image = image;
        }

        public void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap result = new Bitmap(resizeImage);

                Filter f = new Filter();

                f.ChangeImageToPiltering(resizeImage, result);
                PrintImage(result, form2.pictureBox1);

                f.ChangeImageToCMY(resizeImage, result);
                PrintImage(result, form2.pictureBox2);

                f.ChangeImageToInverse(resizeImage, result);
                PrintImage(result, form2.pictureBox3);

                f.ChangeImageToTradeRGB(resizeImage, result);
                PrintImage(result, form2.pictureBox4);

                f.ChangeImageToHighlightWhite(resizeImage, result);
                PrintImage(result, form2.pictureBox5);

                f.ChangeImageToExtractionImage(resizeImage, result);
                PrintImage(result, form2.pictureBox6);

                int width = form2.pictureBox1.Width + form2.pictureBox2.Width + form2.pictureBox3.Width + 60;
                int height = form2.pictureBox1.Height + form2.pictureBox4.Height + 80;

                form2.Size = new Size(width, height);
                form2.pictureBox1.Location = new System.Drawing.Point(20, 20);
                form2.pictureBox2.Location = new System.Drawing.Point(20 + form2.pictureBox1.Width, 20);
                form2.pictureBox3.Location = new System.Drawing.Point(20 + form2.pictureBox1.Width + form2.pictureBox2.Width, 20);
                form2.pictureBox4.Location = new System.Drawing.Point(20, 20 + form2.pictureBox1.Height);
                form2.pictureBox5.Location = new System.Drawing.Point(20 + form2.pictureBox1.Width, 20 + form2.pictureBox1.Height);
                form2.pictureBox6.Location = new System.Drawing.Point(20 + form2.pictureBox1.Width + form2.pictureBox2.Width, 20 + form2.pictureBox1.Height);

                form2.ShowDialog();
            }
            catch (Exception ex)
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                source = new Bitmap(originalImage);
                result = new Bitmap(originalImage);

                Filter f = new Filter();

                int width = Math.Min(result.Width, 1280);
                int height = result.Height * width / result.Width;
                
                filterForm.Size = new Size(width + 60, height + 80);
                filterForm.pictureBox1.Size = new Size(width, height);
                filterForm.pictureBox1.Location = new System.Drawing.Point(20, 20);

                f.ChangeImageToGradationYellow(source, result);
                f.ChangeImageToGradationRed(result, result);
                f.ChangeImageToPiltering(result, result);

                filterForm.pictureBox1.Image = result;

                filterForm.ShowDialog();
            }
            catch (Exception ex)
            {

            }
        }

        ////public void ChangeImageToNormalizationHSV(Bitmap bitmap)
        ////{
        ////    if (bitmap != null)
        ////    {
        ////        Bitmap hsvImage = new Bitmap(bitmap);

        ////        Random r = new Random();

        ////        double minBrightness = 0.6;
        ////        double maxBrightness = 0.8;
        ////        double minSaturation = 0.5;
        ////        double maxSaturation = 1.0;

        ////        Double rand = r.NextDouble() * (maxBrightness - minBrightness) + minBrightness;
        ////        double brightness = rand;
        ////        rand = r.NextDouble() * (maxSaturation - minSaturation) + minSaturation;
        ////        double saturation = rand;

        ////        for (int i = 0; i < bitmap.Width; i++)
        ////        {
        ////            for (int j = 0; j < bitmap.Height; j++)
        ////            {
        ////                Color c = bitmap.GetPixel(i, j);
        ////                Color tc = c;

        ////                if ((int)c.GetHue() > 1)
        ////                    tc = ColorToHSV(c, c.GetHue(), saturation, brightness);

        ////                hsvImage.SetPixel(i, j, tc);
        ////            }
        ////        }

        ////        PrintImage(hsvImage, form2.pictureBox1);
        ////    }
        ////}

        //public Bitmap PilteringByMask(Bitmap image, double[,] mask)
        //{
        //    Bitmap newImage = new Bitmap(image);
        //    int matrixSize = Convert.ToInt32(Math.Sqrt(mask.Length));
        //    int matrixEdge = ((matrixSize - 1) / 2);
        //    int tRed, tGreen, tBlue;
        //    //int red, green, blue;

        //    for (int i = matrixEdge; i < image.Width - matrixEdge; i++)
        //    {
        //        for (int j = matrixEdge; j < image.Height - matrixEdge; j++)
        //        {
        //            tRed = tGreen = tBlue = 0;
        //            //red = green = blue = 0;

        //            for (int row = 0; row < Math.Sqrt(mask.Length); row++)
        //            {
        //                for (int col = 0; col < Math.Sqrt(mask.Length); col++)
        //                {
        //                    tRed += Convert.ToInt32(mask[row, col] * image.GetPixel(i + row - matrixEdge, j + col - matrixEdge).R);
        //                    tGreen += Convert.ToInt32(mask[row, col] * image.GetPixel(i + row - matrixEdge, j + col - matrixEdge).G);
        //                    tBlue += Convert.ToInt32(mask[row, col] * image.GetPixel(i + row - matrixEdge, j + col - matrixEdge).B);
        //                }
        //            }

        //            //if (matrixSize == 7)
        //            //{
        //            //    red = Convert.ToInt32(Math.Sqrt(Math.Pow(tRed, 2) + Math.Pow(tGreen, 2) + Math.Pow(tBlue, 2)));
        //            //    green = Convert.ToInt32(Math.Sqrt(Math.Pow(tRed, 2) + Math.Pow(tGreen, 2) + Math.Pow(tBlue, 2)));
        //            //    blue = Convert.ToInt32(Math.Sqrt(Math.Pow(tRed, 2) + Math.Pow(tGreen, 2) + Math.Pow(tBlue, 2)));
        //            //}

        //            if (tRed < 0)
        //            {
        //                tRed = 0;
        //            }
        //            else if (tRed > Constants.MAXRED)
        //            {
        //                tRed = Constants.MAXRED;
        //            }

        //            if (tGreen < 0)
        //            {
        //                tGreen = 0;
        //            }
        //            else if (tGreen > Constants.MAXGREEN)
        //            {
        //                tGreen = Constants.MAXGREEN;
        //            }

        //            if (tBlue < 0)
        //            {
        //                tBlue = 0;
        //            }
        //            else if (tBlue > Constants.MAXBLUE)
        //            {
        //                tBlue = Constants.MAXBLUE;
        //            }                   

        //            newImage.SetPixel(i, j, Color.FromArgb(tRed, tGreen, tBlue));
        //        }
        //    }

        //    return newImage;
        //}

        //public void ChangeImageToPiltering(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        double[,] maskGaussian = { { 1 / 16.0, 1 / 8.0, 1 / 16.0 }, { 1 / 8.0, 1 / 4.0, 1 / 8.0 }, { 1 / 16.0, 1 / 8.0, 1 / 16.0 } };
        //        double[,] maskSharp = { { 0, -1.0, 0 }, { -1.0, 5.0, -1.0 }, { 0, -1.0, 0 } };
        //        double[,] masksobel = { { -6.0, 0, 6.0 }, { 0, 0, 0}, { 6.0, 0, -6.0 } };
        //        double[,] maskDOG = { { 0,      0,      -1.0,   -1.0,   -1.0,   0,      0 },
        //                              { 0,      -2.0,   -3.0,   -3.0,   -3.0,   -2.0,   0 },
        //                              { -1.0,   -3.0,   5.0,    5.0,    5.0,    -3.0,   -1.0},
        //                              { -1.0,   -3.0,   5.0,    16.0,   5.0,    -3.0,   -1.0},
        //                              { -1.0,   -3.0,   5.0,    5.0,    5.0,    -3.0,   -1.0},
        //                              { 0,      -2.0,   -3.0,   -3.0,   -3.0,   -2.0,   0 },
        //                              { 0,      0,      -1.0,   -1.0,   -1.0,   0,      0 } };

        //        //double[,] maskRandomValue = new double[3, 3];

        //        //Random r = new Random();
        //        //double[] value = new double[9];
        //        //double zerosum = 0.0;

        //        //for (int i = 0; i < value.Length - 1; i++)
        //        //{
        //        //    value[i] = r.Next(0,2) == 0 ? -r.NextDouble() : r.NextDouble();
        //        //    zerosum += value[i];
        //        //}

        //        //value[value.Length - 1] = -zerosum;

        //        //maskRandomValue[0, 0] = value[0];
        //        //maskRandomValue[0, 1] = value[1];
        //        //maskRandomValue[0, 2] = value[2];
        //        //maskRandomValue[1, 2] = value[3];
        //        //maskRandomValue[2, 2] = value[4];
        //        //maskRandomValue[2, 1] = value[5];
        //        //maskRandomValue[2, 0] = value[6];
        //        //maskRandomValue[1, 0] = value[7];
        //        //maskRandomValue[1, 1] = value[8];

        //        Bitmap newImage = PilteringByMask(bitmap, masksobel);

        //        PrintImage(newImage, form2.pictureBox1);
        //    }
        //}

        //public Bitmap ChangeImageToGray(Bitmap bitmap)
        //{
        //    Bitmap newImage = new Bitmap(128, 128);

        //    if (bitmap != null)
        //    {
        //        newImage = new Bitmap(bitmap);

        //        for (int i = 0; i < bitmap.Width; i++)
        //        {
        //            for (int j = 0; j < bitmap.Height; j++)
        //            {
        //                Color c = bitmap.GetPixel(i, j);
        //                Color tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);
        //                newImage.SetPixel(i, j, tc);
        //            }
        //        }
        //    }

        //    return newImage;
        //}

        //public void ChangeImageCorrection(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        Bitmap newImage = new Bitmap(bitmap);

        //        for (int i = 0; i < bitmap.Width; i++)
        //        {
        //            for (int j = 0; j < bitmap.Height; j++)
        //            {
        //                Color c = bitmap.GetPixel(i, j);

        //                double totalRGB = c.R + c.G + c.B;
        //                int maxValue = Math.Max(c.R, Math.Max(c.G, c.B));
        //                int addValue = 0;
        //                Color tc = new Color();
        //                if ((maxValue == c.R) && (maxValue / totalRGB > 0.45))
        //                {
        //                    addValue = (int)(Constants.COLORCORRETRATE * totalRGB) - c.R;
        //                    tc = Color.FromArgb(Math.Min(Constants.MAXRED, c.R + addValue), Math.Min(Constants.MAXRED, c.R + addValue), Math.Max(0, c.B - addValue/2));
        //                }
        //                else if ((maxValue == c.G) && (maxValue / totalRGB > 0.45))
        //                {
        //                    addValue = (int)(Constants.COLORCORRETRATE * totalRGB) - c.G;
        //                    tc = Color.FromArgb(Math.Max(0, c.R - addValue/2), Math.Min(Constants.MAXGREEN, c.G + addValue), Math.Min(Constants.MAXGREEN, c.G + addValue));
        //                }
        //                else if ((maxValue == c.B) && (maxValue / totalRGB > 0.45))
        //                {
        //                    addValue = (int)(Constants.COLORCORRETRATE * totalRGB) - c.B;
        //                    tc = Color.FromArgb(Math.Min(Constants.MAXBLUE, c.B + addValue), Math.Max(0, c.G - addValue/2), Math.Min(Constants.MAXBLUE, c.B + addValue));
        //                }
        //                else
        //                {
        //                    tc = Color.FromArgb(c.R, c.G, c.B);
        //                }

        //                newImage.SetPixel(i, j, tc);
        //            }
        //        }

        //        PrintImage(newImage, form2.pictureBox2);
        //    }
        //}

        //public void ChangeImageToCMY(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        Bitmap newImage = new Bitmap(bitmap);

        //        for (int i = 0; i < bitmap.Width; i++)
        //        {
        //            for (int j = 0; j < bitmap.Height; j++)
        //            {
        //                Color c = bitmap.GetPixel(i, j);

        //                Color tc = new Color();
        //                int maxValue = Math.Max(c.R, Math.Max(c.G, c.B));
        //                if (maxValue == c.R)
        //                    tc = Color.FromArgb(c.R, c.G, c.R);
        //                else if (maxValue == c.G)
        //                    tc = Color.FromArgb(c.G, c.G, c.B);
        //                else if (maxValue == c.B)
        //                    tc = Color.FromArgb(c.R, c.B, c.B);
        //                else
        //                    tc = Color.FromArgb(c.R, c.G, c.B);

        //                newImage.SetPixel(i, j, tc);
        //            }
        //        }

        //        PrintImage(newImage, form2.pictureBox2);
        //    }
        //}

        //public void ChangeImageToInverse(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        Bitmap newImage = new Bitmap(bitmap);

        //        for (int i = 0; i < bitmap.Width; i++)
        //        {
        //            for (int j = 0; j < bitmap.Height; j++)
        //            {
        //                Color c = bitmap.GetPixel(i, j);
        //                Color tc = Color.FromArgb(Constants.MAXRED - c.R, Constants.MAXGREEN - c.G, Constants.MAXBLUE - c.B);
        //                newImage.SetPixel(i, j, tc);
        //            }
        //        }

        //        PrintImage(newImage, form2.pictureBox3);
        //    }
        //}

        //public void ChangeImageToTradeRGB(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        Bitmap newImage = new Bitmap(bitmap);

        //        Random r = new Random();
        //        int rand = r.Next(0, 6);

        //        byte tRed = 0;
        //        byte tGreen = 0;
        //        byte tBlue = 0;

        //        for (int i = 0; i < bitmap.Width; i++)
        //        {
        //            for (int j = 0; j < bitmap.Height; j++)
        //            {
        //                Color c = bitmap.GetPixel(i, j);

        //                switch (rand)
        //                {
        //                    case 1:
        //                        tRed = c.R; tGreen = c.B; tBlue = c.G;
        //                        break;
        //                    case 2:
        //                        tRed = c.G; tGreen = c.R; tBlue = c.B;
        //                        break;
        //                    case 3:
        //                        tRed = c.G; tGreen = c.B; tBlue = c.R;
        //                        break;
        //                    case 4:
        //                        tRed = c.B; tGreen = c.R; tBlue = c.G;
        //                        break;
        //                    case 5:
        //                        tRed = c.B; tGreen = c.G; tBlue = c.R;
        //                        break;
        //                    default:
        //                        tRed = c.R; tGreen = c.G; tBlue = c.B;
        //                        break;
        //                }

        //                Color tc = Color.FromArgb(tRed, tGreen, tBlue);
        //                newImage.SetPixel(i, j, tc);
        //            }
        //        }

        //        PrintImage(newImage, form2.pictureBox4);
        //    }
        //}

        ///*
        //public void ChangeImageToShiftLeft(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        Bitmap hsvImage = new Bitmap(bitmap);

        //        for (int i = 0; i < bitmap.Width; i++)
        //        {
        //            for (int j = 0; j < bitmap.Height; j++)
        //            {
        //                Color c = bitmap.GetPixel(i, j);
        //                Color tc = Color.FromArgb(c.G, c.B, c.R);
        //                hsvImage.SetPixel(i, j, tc);
        //            }
        //        }

        //        Size resize = new Size(hsvImage.Width / 2, hsvImage.Height / 2);
        //        hsvImage = new Bitmap(hsvImage, resize);

        //        form2.pictureBox4.Image = hsvImage;
        //    }
        //}

        //public void ChangeImageToShiftRight(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        Bitmap hsvImage = new Bitmap(bitmap);

        //        for (int i = 0; i < bitmap.Width; i++)
        //        {
        //            for (int j = 0; j < bitmap.Height; j++)
        //            {
        //                Color c = bitmap.GetPixel(i, j);
        //                Color tc = Color.FromArgb(c.B, c.R, c.G);
        //                hsvImage.SetPixel(i, j, tc);
        //            }
        //        }

        //        Size resize = new Size(hsvImage.Width / 2, hsvImage.Height / 2);
        //        hsvImage = new Bitmap(hsvImage, resize);

        //        form2.pictureBox5.Image = hsvImage;
        //    }
        //}
        //*/

        //public void ChangeImageToExtractionImage(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        Bitmap newImage = new Bitmap(bitmap);

        //        Random r = new Random();
        //        int rand = r.Next(0, 3);    // 0 : 초록색, 1 : 파랑색, 2 : 빨간색

        //        Color tc = new Color();

        //        if (rand == 0)
        //        {
        //            for (int i = 0; i < bitmap.Width; i++)
        //            {
        //                for (int j = 0; j < bitmap.Height; j++)
        //                {
        //                    Color c = bitmap.GetPixel(i, j);

        //                    if (60 <= Convert.ToInt32(c.GetHue()) % 360 && 180 > Convert.ToInt32(c.GetHue()) % 360)
        //                        tc = Color.FromArgb(c.R, c.G, c.B);
        //                    else
        //                        tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);

        //                    newImage.SetPixel(i, j, tc);
        //                }
        //            }
        //        }
        //        else if (rand == 1)
        //        {
        //            for (int i = 0; i < bitmap.Width; i++)
        //            {
        //                for (int j = 0; j < bitmap.Height; j++)
        //                {
        //                    Color c = bitmap.GetPixel(i, j);

        //                    if (180 <= Convert.ToInt32(c.GetHue()) % 360 && 300 > Convert.ToInt32(c.GetHue()) % 360)
        //                        tc = Color.FromArgb(c.R, c.G, c.B);
        //                    else
        //                        tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);

        //                    newImage.SetPixel(i, j, tc);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i < bitmap.Width; i++)
        //            {
        //                for (int j = 0; j < bitmap.Height; j++)
        //                {
        //                    Color c = bitmap.GetPixel(i, j);

        //                    if (300 <= Convert.ToInt32(c.GetHue()) % 360 || 60 > Convert.ToInt32(c.GetHue()) % 360)
        //                        tc = Color.FromArgb(c.R, c.G, c.B);
        //                    else
        //                        tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);

        //                    newImage.SetPixel(i, j, tc);
        //                }
        //            }
        //        }

        //        PrintImage(newImage, form2.pictureBox6);
        //    }
        //}

        //public void ChangeImageToHighlightWhite(Bitmap bitmap)
        //{
        //    if (bitmap != null)
        //    {
        //        Bitmap newImage = new Bitmap(bitmap);

        //        for (int i = 0; i < bitmap.Width; i++)
        //        {
        //            for (int j = 0; j < bitmap.Height; j++)
        //            {
        //                Color c = bitmap.GetPixel(i, j);

        //                Color tc = Color.FromArgb(0, 0, 0);
        //                if (c.GetBrightness() > 0.45)
        //                    tc = Color.FromArgb(c.R, c.G, c.B);
        //                else
        //                    tc = Color.FromArgb(c.R / 2, c.G / 2, c.B / 2);

        //                newImage.SetPixel(i, j, tc);
        //            }
        //        }

        //        PrintImage(newImage, form2.pictureBox5);
        //    }
        //}

        //public Color ColorToHSV(Color color, double hue, double saturation, double value)
        //{
        //    int max = Math.Max(color.R, Math.Max(color.G, color.B));
        //    int min = Math.Min(color.R, Math.Min(color.G, color.B));

        //    int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        //    double f = hue / 60 - Math.Floor(hue / 60);

        //    value = value * 255;
        //    int v = Convert.ToInt32(value);
        //    int p = Convert.ToInt32(value * (1 - saturation));
        //    int q = Convert.ToInt32(value * (1 - f * saturation));
        //    int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        //    Color c;

        //    switch (hi)
        //    {
        //        case 0:
        //            c = Color.FromArgb(v, t, p);
        //            break;
        //        case 1:
        //            c = Color.FromArgb(q, v, p);
        //            break;
        //        case 2:
        //            c = Color.FromArgb(p, v, t);
        //            break;
        //        case 3:
        //            c = Color.FromArgb(p, q, v);
        //            break;
        //        case 4:
        //            c = Color.FromArgb(t, p, v);
        //            break;
        //        default:
        //            c = Color.FromArgb(v, p, q);
        //            break;
        //    }

        //    return c;
        //}

        //public string GetMaxColor(int[] matrix)
        //{
        //    //KnownColor[] color = new KnownColor[100];
        //    System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
        //    KnownColor[] allColors = new KnownColor[colorsArray.Length];

        //    Array.Copy(colorsArray, allColors, colorsArray.Length);

        //    string sMaxNumColor = "White or Black";

        //    for (int i = 0; i < allColors.Length; i++)
        //    {
        //        Color c = Color.FromName(allColors[i].ToString());

        //        Console.WriteLine((int)c.GetHue() + "   " + maxColorNum);
        //        if ((int)c.GetHue() == maxColorNum)
        //        {
        //            sMaxNumColor = c.ToKnownColor().ToString();
        //        }
        //    }

        //    return sMaxNumColor;
        //}
    }

    static class Constants
    {
        public const int MAXWIDTH = 700;
        public const int MAXHEIGHT = 450;

        public const int MAXRED = 255;
        public const int MAXGREEN = 255;
        public const int MAXBLUE = 255;

        public const int MAXHUE = 359;

        public const double COLORCORRETRATE = 0.5; 
    }
}
