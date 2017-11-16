using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoAnalysis
{
    class Filter
    {
        public Filter()
        {

        }

        public Color ColorFromHSV(Color color, double hue, double saturation, double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            Color c;

            switch (hi)
            {
                case 0:
                    c = Color.FromArgb(v, t, p);
                    break;
                case 1:
                    c = Color.FromArgb(q, v, p);
                    break;
                case 2:
                    c = Color.FromArgb(p, v, t);
                    break;
                case 3:
                    c = Color.FromArgb(p, q, v);
                    break;
                case 4:
                    c = Color.FromArgb(t, p, v);
                    break;
                default:
                    c = Color.FromArgb(v, p, q);
                    break;
            }

            return c;
        }

        // 색상은 유지하되 고정된 밝기와 채도값으로 필터링
        // - 
        public void ChangeImageToNormalizationHSV(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                Random r = new Random();

                double minBrightness = 0.6;
                double maxBrightness = 0.8;
                double minSaturation = 0.5;
                double maxSaturation = 1.0;

                Double rand = r.NextDouble() * (maxBrightness - minBrightness) + minBrightness;
                double brightness = rand;
                rand = r.NextDouble() * (maxSaturation - minSaturation) + minSaturation;
                double saturation = rand;

                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);
                        Color tc = c;

                        if ((int)c.GetHue() > 1)
                            tc = ColorFromHSV(c, c.GetHue(), saturation, brightness);

                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        public void PilteringByMask(Bitmap source, Bitmap result, double[,] mask)
        {
            //Bitmap newImage = new Bitmap(image);
            int matrixSize = Convert.ToInt32(Math.Sqrt(mask.Length));
            int matrixEdge = ((matrixSize - 1) / 2);
            int tRed, tGreen, tBlue;

            for (int i = matrixEdge; i < source.Width - matrixEdge; i++)
            {
                for (int j = matrixEdge; j < source.Height - matrixEdge; j++)
                {
                    tRed = tGreen = tBlue = 0;

                    for (int row = 0; row < Math.Sqrt(mask.Length); row++)
                    {
                        for (int col = 0; col < Math.Sqrt(mask.Length); col++)
                        {
                            tRed += Convert.ToInt32(mask[row, col] * source.GetPixel(i + row - matrixEdge, j + col - matrixEdge).R);
                            tGreen += Convert.ToInt32(mask[row, col] * source.GetPixel(i + row - matrixEdge, j + col - matrixEdge).G);
                            tBlue += Convert.ToInt32(mask[row, col] * source.GetPixel(i + row - matrixEdge, j + col - matrixEdge).B);
                        }
                    }

                    if (tRed < 0)
                    {
                        tRed = 0;
                    }
                    else if (tRed > Constants.MAXRED)
                    {
                        tRed = Constants.MAXRED;
                    }

                    if (tGreen < 0)
                    {
                        tGreen = 0;
                    }
                    else if (tGreen > Constants.MAXGREEN)
                    {
                        tGreen = Constants.MAXGREEN;
                    }

                    if (tBlue < 0)
                    {
                        tBlue = 0;
                    }
                    else if (tBlue > Constants.MAXBLUE)
                    {
                        tBlue = Constants.MAXBLUE;
                    }

                    result.SetPixel(i, j, Color.FromArgb(tRed, tGreen, tBlue));
                }
            }
        }

        // mask를 사용한 필터링
        // - 
        public void ChangeImageToPiltering(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                double[,] maskGaussian = { { 1 / 16.0, 1 / 8.0, 1 / 16.0 }, { 1 / 8.0, 1 / 4.0, 1 / 8.0 }, { 1 / 16.0, 1 / 8.0, 1 / 16.0 } };
                double[,] maskSharp = { { 0, -1.0, 0 }, { -1.0, 5.0, -1.0 }, { 0, -1.0, 0 } };
                double[,] masksobel = { { -6.0, 0, 6.0 }, { 0, 0, 0 }, { 6.0, 0, -6.0 } };
                double[,] maskDOG = { { 0,      0,      -1.0,   -1.0,   -1.0,   0,      0 },
                                      { 0,      -2.0,   -3.0,   -3.0,   -3.0,   -2.0,   0 },
                                      { -1.0,   -3.0,   5.0,    5.0,    5.0,    -3.0,   -1.0},
                                      { -1.0,   -3.0,   5.0,    16.0,   5.0,    -3.0,   -1.0},
                                      { -1.0,   -3.0,   5.0,    5.0,    5.0,    -3.0,   -1.0},
                                      { 0,      -2.0,   -3.0,   -3.0,   -3.0,   -2.0,   0 },
                                      { 0,      0,      -1.0,   -1.0,   -1.0,   0,      0 } };

                PilteringByMask(source, result, maskGaussian);
            }
        }

        // 그레이 필터링
        // - GrayGerbera
        public void ChangeImageToGray(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);
                        Color tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);
                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // 
        public void ChangeImageCorrection(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);

                        double totalRGB = c.R + c.G + c.B;
                        int maxValue = Math.Max(c.R, Math.Max(c.G, c.B));
                        int addValue = 0;

                        Color tc = new Color();

                        if ((maxValue == c.R) && (maxValue / totalRGB > 0.45))
                        {
                            addValue = (int)(Constants.COLORCORRETRATE * totalRGB) - c.R;
                            tc = Color.FromArgb(Math.Min(Constants.MAXRED, c.R + addValue), Math.Min(Constants.MAXRED, c.R + addValue), Math.Max(0, c.B - addValue / 2));
                        }
                        else if ((maxValue == c.G) && (maxValue / totalRGB > 0.45))
                        {
                            addValue = (int)(Constants.COLORCORRETRATE * totalRGB) - c.G;
                            tc = Color.FromArgb(Math.Max(0, c.R - addValue / 2), Math.Min(Constants.MAXGREEN, c.G + addValue), Math.Min(Constants.MAXGREEN, c.G + addValue));
                        }
                        else if ((maxValue == c.B) && (maxValue / totalRGB > 0.45))
                        {
                            addValue = (int)(Constants.COLORCORRETRATE * totalRGB) - c.B;
                            tc = Color.FromArgb(Math.Min(Constants.MAXBLUE, c.B + addValue), Math.Max(0, c.G - addValue / 2), Math.Min(Constants.MAXBLUE, c.B + addValue));
                        }
                        else
                        {
                            tc = Color.FromArgb(c.R, c.G, c.B);
                        }

                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // R=M, G=Y, B=C
        // - Tulip
        public void ChangeImageToCMY(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);

                        Color tc = new Color();
                        int maxValue = Math.Max(c.R, Math.Max(c.G, c.B));
                        if (maxValue == c.R)
                            tc = Color.FromArgb(c.R, c.G, c.R);
                        else if (maxValue == c.G)
                            tc = Color.FromArgb(c.G, c.G, c.B);
                        else if (maxValue == c.B)
                            tc = Color.FromArgb(c.R, c.B, c.B);
                        else
                            tc = Color.FromArgb(c.R, c.G, c.B);

                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // 반전 필터링
        // - BogBilberry
        public void ChangeImageToInverse(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);
                        Color tc = Color.FromArgb(Constants.MAXRED - c.R, Constants.MAXGREEN - c.G, Constants.MAXBLUE - c.B);
                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // 랜덤적인 RGB 교환
        // - 
        public void ChangeImageToTradeRGB(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                Random r = new Random();
                int rand = r.Next(0, 6);

                byte tRed = 0;
                byte tGreen = 0;
                byte tBlue = 0;

                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);

                        switch (rand)
                        {
                            case 1:
                                tRed = c.R; tGreen = c.B; tBlue = c.G;
                                break;
                            case 2:
                                tRed = c.G; tGreen = c.R; tBlue = c.B;
                                break;
                            case 3:
                                tRed = c.G; tGreen = c.B; tBlue = c.R;
                                break;
                            case 4:
                                tRed = c.B; tGreen = c.R; tBlue = c.G;
                                break;
                            case 5:
                                tRed = c.B; tGreen = c.G; tBlue = c.R;
                                break;
                            default:
                                tRed = c.R; tGreen = c.G; tBlue = c.B;
                                break;
                        }

                        Color tc = Color.FromArgb(tRed, tGreen, tBlue);
                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // RGB → GBR 교환
        // - 
        public void ChangeImageToShiftLeft(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);
                        Color tc = Color.FromArgb(c.G, c.B, c.R);
                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // RGB → BRG 교환
        // - 
        public void ChangeImageToShiftRight(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);
                        Color tc = Color.FromArgb(c.B, c.R, c.G);
                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // 랜덤적인 추출 필터링
        // - 
        public void ChangeImageToExtractionImage(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                Random r = new Random();
                int rand = 2;//r.Next(0, 3);    // 0 : 초록색, 1 : 파랑색, 2 : 빨간색

                Color tc = new Color();

                if (rand == 0)
                {
                    for (int i = 0; i < source.Width; i++)
                    {
                        for (int j = 0; j < source.Height; j++)
                        {
                            Color c = source.GetPixel(i, j);

                            if (60 <= Convert.ToInt32(c.GetHue()) % 360 && 180 > Convert.ToInt32(c.GetHue()) % 360)
                                tc = Color.FromArgb(c.R, c.G, c.B);
                            else
                                tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);

                            result.SetPixel(i, j, tc);
                        }
                    }
                }
                else if (rand == 1)
                {
                    for (int i = 0; i < source.Width; i++)
                    {
                        for (int j = 0; j < source.Height; j++)
                        {
                            Color c = source.GetPixel(i, j);

                            if (180 <= Convert.ToInt32(c.GetHue()) % 360 && 300 > Convert.ToInt32(c.GetHue()) % 360)
                                tc = Color.FromArgb(c.R, c.G, c.B);
                            else
                                tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);

                            result.SetPixel(i, j, tc);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < source.Width; i++)
                    {
                        for (int j = 0; j < source.Height; j++)
                        {
                            Color c = source.GetPixel(i, j);

                            if (300 <= Convert.ToInt32(c.GetHue()) % 360 || 60 > Convert.ToInt32(c.GetHue()) % 360)
                                tc = Color.FromArgb(c.R, c.G, c.B);
                            else
                                tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);

                            result.SetPixel(i, j, tc);
                        }
                    }
                }
            }
        }

        // 밝기값이 낮으면 RGB값 낮춤
        // - 
        public void ChangeImageToHighlightWhite(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);
                        Color tc = Color.FromArgb(0, 0, 0);

                        if (c.GetBrightness() > 0.45)
                            tc = Color.FromArgb(c.R, c.G, c.B);
                        else
                            tc = Color.FromArgb(c.R / 2, c.G / 2, c.B / 2);

                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // 일정 채도값 이하는 회색조로 필터링
        // - Filter.Daisy
        public void ChangeImageToSaturation(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color c = source.GetPixel(i, j);
                        Color tc = Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3);

                        if (c.GetSaturation() > 0.6)
                        {
                            tc = Color.FromArgb(c.R, c.G, c.B);
                        }

                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // RGB값 정규화 후 R 강조 
        // Filter.RedRosa
        public void ChangeImageToPosterizingRED(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                Bitmap temp = new Bitmap(source);

                Color c = new Color();
                Color tc = new Color();

                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        c = source.GetPixel(i, j);

                        int newR = Convert.ToInt32(c.R / 64) * 64;
                        int newG = Convert.ToInt32(c.G / 64) * 64;
                        int newB = Convert.ToInt32(c.B / 64) * 64;

                        tc = Color.FromArgb(newR, newG, newB);

                        if (tc.R > tc.G && tc.R > tc.B && tc.R > 128)
                            tc = Color.FromArgb(
                                Math.Min(Convert.ToInt32(c.R * 1.1), Constants.MAXRED),
                                Convert.ToInt32(c.G * 0.85),
                                Convert.ToInt32(c.B * 0.85)
                                );

                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // 외곽선 강조, 흰색 강조
        // Filter.WhiteMums
        public void ChangeImageToSketch(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                Bitmap temp = new Bitmap(source);

                double[,] masksobel = { { -6.0, 0, 6.0 }, { 0, 0, 0 }, { 6.0, 0, -6.0 } };

                PilteringByMask(source, temp, masksobel);

                ChangeImageToGray(temp, temp);

                Color c = new Color();
                Color tc = new Color();
                Random rand = new Random();

                for (int i = 0; i < temp.Width; i++)
                {
                    for (int j = 0; j < temp.Height; j++)
                    {
                        c = temp.GetPixel(i, j);

                        if (c.R < 64)
                        {
                            int randrgb = rand.Next(0, 5);

                            switch (randrgb)
                            {
                                case 0:
                                    tc = Color.AntiqueWhite;
                                    break;

                                case 1:
                                    tc = Color.FloralWhite;
                                    break;

                                case 2:
                                    tc = Color.GhostWhite;
                                    break;

                                case 3:
                                    tc = Color.White;
                                    break;

                                case 4:
                                    tc = Color.White;
                                    break;

                                default:
                                    tc = Color.White;
                                    break;
                            }

                            //if (sc.R > 75 && sc.G > 75 && sc.B > 75)
                            //    rc = source.GetPixel(i, j);
                        }
                        else
                            tc = Color.Black;

                        result.SetPixel(i, j, tc);
                    }
                }
            }
        }

        // 
        // Filter.Maple
        public void ChangeImageToGradationRed(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                Color c = new Color();
                Color tc = new Color();

                for (int i = 0; i < source.Width; i++)
                {
                    double r = 0.0;

                    for (int j = 0; j < source.Height; j++)
                    {
                        double a = Math.Pow(10.0, Convert.ToDouble(Math.Log10(Constants.MAXRED * 0.75) / source.Height));
                        double diff = Math.Log(Math.E, a);
                        double addRed = Math.Pow(a, j) / diff;
                        //double addRed = Convert.ToDouble(Constants.MAXRED * 0.75) / source.Height;

                        c = source.GetPixel(i, j);

                        if (345 <= Convert.ToInt32(c.GetHue()) % 360 || (15 > Convert.ToInt32(c.GetHue()) % 360 && 0 < Convert.ToInt32(c.GetHue()) % 360))
                        {
                            int gray = Convert.ToInt32(c.R + c.G + c.B) / 3;
                            tc = Color.FromArgb(Math.Min(gray + Convert.ToInt32(r), Constants.MAXRED), gray, gray);
                        }
                        else
                            tc = Color.FromArgb(c.R, c.G, c.B);

                        result.SetPixel(i, j, tc);

                        r = r + addRed;
                    }
                }
            }
        }

        // 
        // Filter.Ginkgo
        public void ChangeImageToGradationYellow(Bitmap source, Bitmap result)
        {
            if (source != null)
            {
                Color c = new Color();
                Color tc = new Color();

                for (int i = 0; i < source.Width; i++)
                {
                    double rg = 0.0;

                    for (int j = 0; j < source.Height; j++)
                    {
                        double a = Math.Pow(10.0, Convert.ToDouble(Math.Log10(Constants.MAXRED * 0.75) / source.Height));
                        double diff = Math.Log(Math.E, a);
                        double addColor = Math.Pow(a, j) / diff;

                        c = source.GetPixel(i, j);

                        if (40 < Convert.ToInt32(c.GetHue()) % 360 && 60 > Convert.ToInt32(c.GetHue()) % 360)
                        {
                            int gray = Convert.ToInt32(c.R + c.G + c.B) / 3;
                            tc = Color.FromArgb(
                                Math.Min(gray + Convert.ToInt32(rg), Constants.MAXRED),
                                Math.Min(gray + Convert.ToInt32(rg), Constants.MAXGREEN),
                                gray);
                        }
                        else
                            tc = Color.FromArgb(c.R, c.G, c.B);

                        result.SetPixel(i, j, tc);

                        rg = rg + addColor;
                    }
                }
            }
        }
    }
}
