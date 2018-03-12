using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MyHSV;
//結論、単純なRGBの距離はかなり有効
//あとは補正で
//基準色のyuvの輝度が極端に低いときは輝度に合わせるとか
//Gの範囲を広げるとか


namespace _20180310_減色_色の距離
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //BitmapSource OriginBitmap;
        //string ImageFileFullPath;
        Border[] MyPalettePanRandam;
        //List<Color> MyPalette;
        const int MAX_PALETTE_COLOR_COUNT = 20;
        Border[] MyPalettePanRGB;
        Border[] MyPalettePanRGB2;
        Border[] MyPalettePanHue;
        Border[] MyPalettePanSaturation;
        Border[] MyPalettePanValue;
        Border[] MyPalettePanHSV円柱Add;
        Border[] MyPalettePanHSV円錐Add;
        Border[] MyPalettePanHSV円柱Euclid;
        Border[] MyPalettePanHSV円錐Euclid;
        Border[] MyPalettePanHSV円柱Tri;
        Border[] MyPalettePanHSV円錐Tri;
        Border[] MyPalettePanHSV2円柱;
        Border[] MyPalettePanXYZ;
        Border[] MyPalettePanLab;


        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            //this.Drop += MainWindow_Drop;

            ReNewBaseColorText();
            //;//パレットの色表示用のBorder作成

            MyPalettePanRandam = CreateBorders(MAX_PALETTE_COLOR_COUNT);
            AddBorder(MyPalettePanRandam, MyWrapPanel);
            ChangeColorPan(MyPalettePanRandam, GetRandamColors(MAX_PALETTE_COLOR_COUNT));


            MyPalettePanRGB = CreateBorders(MAX_PALETTE_COLOR_COUNT);
            AddBorder(MyPalettePanRGB, WrapPanelRGB);
            MyPalettePanRGB2 = CreateBorders(MAX_PALETTE_COLOR_COUNT);
            AddBorder(MyPalettePanRGB2, WrapPanelRGB2);

            MyPalettePanHue = CreateBorders(MAX_PALETTE_COLOR_COUNT);
            AddBorder(MyPalettePanHue, WrapPanelHue);
            MyPalettePanSaturation = CreateBorders(MAX_PALETTE_COLOR_COUNT);
            AddBorder(MyPalettePanSaturation, WrapPanelSaturation);
            MyPalettePanValue = CreateBorders(MAX_PALETTE_COLOR_COUNT);
            AddBorder(MyPalettePanValue, WrapPanelValue);

            MyPalettePanHSV円柱Add = CreateBorders(MAX_PALETTE_COLOR_COUNT);//hsv円柱 add
            AddBorder(MyPalettePanHSV円柱Add, WrapPanelHSV);
            MyPalettePanHSV円錐Add = CreateBorders(MAX_PALETTE_COLOR_COUNT);//hsv円錐 add
            AddBorder(MyPalettePanHSV円錐Add, WrapPanelHSV円錐);

            MyPalettePanHSV円柱Tri = CreateBorders(MAX_PALETTE_COLOR_COUNT);//hsv円柱 tri
            AddBorder(MyPalettePanHSV円柱Tri, WrapPanelHSVTri);
            MyPalettePanHSV円錐Tri = CreateBorders(MAX_PALETTE_COLOR_COUNT);//hsv円錐 tri
            AddBorder(MyPalettePanHSV円錐Tri, WrapPanelHSV円錐Tri);

            MyPalettePanHSV円柱Euclid = CreateBorders(MAX_PALETTE_COLOR_COUNT);//hsv円柱 euclid
            AddBorder(MyPalettePanHSV円柱Euclid, WrapPanelHSVEuclid);
            MyPalettePanHSV円錐Euclid = CreateBorders(MAX_PALETTE_COLOR_COUNT);//hsv円錐 euclid
            AddBorder(MyPalettePanHSV円錐Euclid, WrapPanelHSV円錐Euclid);

            //MyPalettePanHSV2円柱 = CreateBorders(MAX_PALETTE_COLOR_COUNT);//hsv2円柱
            //AddBorder(MyPalettePanHSV2円柱, WrapPanelHSV2);

            MyPalettePanXYZ = CreateBorders(MAX_PALETTE_COLOR_COUNT);
            AddBorder(MyPalettePanXYZ, WrapPanelXYZ);
            MyPalettePanLab = CreateBorders(MAX_PALETTE_COLOR_COUNT);
            AddBorder(MyPalettePanLab, WrapPanelLab);



            Button1.Click += Button1_Click;
            ButtonChangeColor.Click += ButtonChangeColor_Click;
            SortColor();
            ButtonSort.Click += ButtonSort_Click;
            //ButtonRGB2.Click += ButtonRGB2_Click;
        }



        //E:\オレ\エクセル\C#.xlsm_色の距離_$G$26
        //HSV距離三角関数
        /// <summary>
        /// HSVと三角関数を使って2色間の距離を測る
        /// 円錐モデルのHSVを使うときはConicalにTrue
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="bColor"></param>
        /// <param name="Conical">円錐モデル</param>
        /// <returns></returns>
        private double GetColorDistanceHSV円柱or円錐Tryangle(Color c1, Color bColor, bool Conical)
        {
            HSV iHsv, bHsv;
            if (Conical == false)
            {
                iHsv = HSV.Color2HSV(c1);
                bHsv = HSV.Color2HSV(bColor);
            }
            else
            {
                iHsv = HSV.Color2HSV_ConicalModel(c1);
                bHsv = HSV.Color2HSV_ConicalModel(bColor);
            }
            double iRadian = iHsv.Hue / 180 * Math.PI;
            double bRadian = bHsv.Hue / 180 * Math.PI;
            double ix = Math.Cos(iRadian) * iHsv.Saturation;
            double bx = Math.Cos(bRadian) * bHsv.Saturation;
            double iy = Math.Sin(iRadian) * iHsv.Saturation;
            double by = Math.Sin(bRadian) * bHsv.Saturation;
            double distance = Math.Sqrt(
                Math.Pow(ix - bx, 2) +
                Math.Pow(iy - by, 2) +
                Math.Pow(iHsv.Value - bHsv.Value, 2));
            return distance;
        }
        /// <summary>
        /// 円錐モデルのHSVを使うときはConicalにTrue
        /// </summary>
        /// <param name="palette"></param>
        /// <param name="color"></param>
        /// <param name="Conical"></param>
        /// <returns></returns>
        private List<Color> SortHSV円柱or円錐Triangle(List<Color> palette, Color color, bool Conical)
        {
            var sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistanceHSV円柱or円錐Tryangle(palette[i], color, Conical), palette[i]);
            }
            return sortedList.Values.ToList();
        }

        //調整HSV、調整中
        private double GetColorDistanceHSVAll2(Color c1, Color bColor)
        {
            HSV iHsv1 = HSV.Color2HSV(c1);
            HSV bHsv = HSV.Color2HSV(bColor);
            double h = Math.Abs(iHsv1.Hue - bHsv.Hue);
            double s = Math.Abs(iHsv1.Saturation - bHsv.Saturation);
            double v = Math.Abs(iHsv1.Value - bHsv.Value);
            if (h > 180) { h = Math.Abs(h - 360); }
            h /= 180;//0-1に均す
            double diffH = h * ((bHsv.Saturation + bHsv.Value) / 4f);
            //h *= (bHsv.Saturation + bHsv.Value) * 2f;
            //h = h;// / bHsv.Saturation / bHsv.Value;
            s *= (1 - bHsv.Saturation);
            //s *= 180/bHsv.Saturation;
            v *= (1 - bHsv.Value);
            //v *= 180/bHsv.Value;
            double distance = h + s + v;
            return distance;
        }
        private List<Color> SortHSVALL2(List<Color> palette, Color color)
        {
            var sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistanceHSVAll2(palette[i], color), palette[i]);
            }
            return sortedList.Values.ToList();
        }


        //HSV円柱モデルのユークリッド距離
        private double GetColorDistanceHSVEuclidean(Color c1, Color c2, bool Conical)
        {
            HSV iHsv1, iHsv2;
            if (Conical == false)
            {
                iHsv1 = HSV.Color2HSV(c1);
                iHsv2 = HSV.Color2HSV(c2);
            }
            else
            {
                iHsv2 = HSV.Color2HSV_ConicalModel(c2);
                iHsv1 = HSV.Color2HSV_ConicalModel(c1);
            }

            double h = Math.Abs(iHsv1.Hue - iHsv2.Hue);
            double s = Math.Abs(iHsv1.Saturation - iHsv2.Saturation);
            double v = Math.Abs(iHsv1.Value - iHsv2.Value);
            //hは180が反対側の色で一番遠い色
            if (h > 180f) { h = Math.Abs(h - 360f); }
            //hも0-1の値に変換
            h /= 180f;
            double distance = Math.Sqrt(Math.Pow(h, 2f) + Math.Pow(s, 2f) + Math.Pow(v, 2f));
            return distance;
        }
        private List<Color> SortHSV円柱or円錐Euclidean(List<Color> palette, Color color, bool Conical)
        {
            var sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistanceHSVEuclidean(palette[i], color, Conical), palette[i]);
            }
            return sortedList.Values.ToList();
        }

        //HSV距離総合、hsvそれぞれの差を足し算
        private double GetColorDistanceHSV円柱and円錐Add(Color c1, Color c2, bool Conical)
        {
            HSV iHsv2, iHsv1;
            if (Conical == false)
            {
                iHsv2 = HSV.Color2HSV(c2);
                iHsv1 = HSV.Color2HSV(c1);
            }
            else
            {
                iHsv1 = HSV.Color2HSV_ConicalModel(c1);
                iHsv2 = HSV.Color2HSV_ConicalModel(c2);
            }
            double h = Math.Abs(iHsv1.Hue - iHsv2.Hue);
            double s = Math.Abs(iHsv1.Saturation - iHsv2.Saturation);
            double v = Math.Abs(iHsv1.Value - iHsv2.Value);
            //hは180が反対側の色で一番遠い色
            if (h > 180f) { h = Math.Abs(h - 360f); }
            //hも0-1の値に変換
            h /= 180f;
            return h + s + v;
        }
        private List<Color> SortHSVALL(List<Color> palette, Color color, bool Conical)
        {
            var sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistanceHSV円柱and円錐Add(palette[i], color, Conical), palette[i]);
            }
            return sortedList.Values.ToList();
        }

        //HSVそれぞれの距離
        private double GetColorDistanceHSV(Color c1, Color c2, string type)
        {
            HSV iHsv1 = HSV.Color2HSV(c1);
            HSV iHsv2 = HSV.Color2HSV(c2);
            double distance = 0;
            if (type == "h")
            {
                distance = Math.Abs(iHsv1.Hue - iHsv2.Hue);
                if (distance > 180) { distance = Math.Abs(distance - 360); }
            }
            else if (type == "s")
            {
                distance = Math.Abs(iHsv1.Saturation - iHsv2.Saturation) * 10000;
            }
            else if (type == "v")
            {
                distance = Math.Abs(iHsv1.Value - iHsv2.Value) * 10000;
            }
            return distance;
        }
        private List<Color> SortHSV(List<Color> palette, Color color, string type)
        {
            //稀にKeyが重複してエラーになる、なりにくいように事前に10000倍してから連番を足している
            var sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistanceHSV(color, palette[i], type) + i, palette[i]);
            }
            return sortedList.Values.ToList<Color>();
        }

        //Lab
        private double[] RGBtoLab(Color c)
        {
            double[] xyz = RGBtoXYZ(c);
            double x = xyz[0] * 100;
            double y = xyz[1] * 100;
            double z = xyz[2] * 100;
            x /= 95.047;
            y /= 100;
            z /= 108.883;

            x = (x > 0.008856) ? Math.Pow(x, 1f / 3f) : (7.787 * x) + (4 / 29);
            y = (y > 0.008856) ? Math.Pow(y, 1f / 3f) : (7.787 * y) + (4 / 29);
            z = (z > 0.008856) ? Math.Pow(z, 1f / 3f) : (7.787 * z) + (4 / 29);

            double L = (116 * y) - 16;
            double a = 500 * (x - y);
            double b = 200 * (y - z);
            return new double[] { L, a, b };
        }
        private List<Color> SortLab(List<Color> palette, Color color)
        {
            var sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistanceLab(palette[i], color), palette[i]);
            }
            return sortedList.Values.ToList();
        }
        private double GetColorDistanceLab(Color c1, Color c2)
        {
            double[] Lab1 = RGBtoLab(c1);
            double[] Lab2 = RGBtoLab(c2);
            double distance = Math.Sqrt((
                Math.Pow(Lab1[0] - Lab2[0], 2) +
                Math.Pow(Lab1[1] - Lab2[1], 2) +
                Math.Pow(Lab1[2] - Lab2[2], 2)));
            return distance;
        }
        //2色間のLabの差をstringで取得
        private List<string> GetStringDistanceLab(List<Color> palette, Color c)
        {
            List<string> strList = new List<string>();
            double[] cLab = RGBtoLab(c);
            double[] pLab;
            double x, y, z;
            for (int i = 0; i < palette.Count; ++i)
            {
                pLab = RGBtoLab(palette[i]);
                x = Math.Abs(cLab[0] - pLab[0]) * 100;
                y = Math.Abs(cLab[1] - pLab[1]) * 100;
                z = Math.Abs(cLab[2] - pLab[2]) * 100;
                string Lab = "Lab差=" + x.ToString("0") + ", " + y.ToString("0") + ", " + z.ToString("0");
                strList.Add(Lab);
            }
            return strList;
        }

        //XYZ
        private List<Color> SortXYZ(List<Color> palette, Color color)
        {
            var sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistanceXYZ(palette[i], color), palette[i]);
            }
            return sortedList.Values.ToList();
        }
        private double GetColorDistanceXYZ(Color c1, Color c2)
        {
            double[] xyz1 = RGBtoXYZ(c1);
            double[] xyz2 = RGBtoXYZ(c2);
            double distance = Math.Sqrt((
                Math.Pow(xyz1[0] - xyz2[0], 2) +
                Math.Pow(xyz1[1] - xyz2[1], 2) +
                Math.Pow(xyz1[2] - xyz2[2], 2)));
            return distance;
        }
        //2色間のXYZの差をstringで取得
        private List<string> GetStringDistanceXYZ(List<Color> palette, Color c)
        {
            List<string> strList = new List<string>();
            double[] cXyz = RGBtoXYZ(c);
            double x, y, z;
            for (int i = 0; i < palette.Count; ++i)
            {
                double[] pXyz = RGBtoXYZ(palette[i]);
                x = Math.Abs(cXyz[0] - pXyz[0]) * 100;
                y = Math.Abs(cXyz[1] - pXyz[1]) * 100;
                z = Math.Abs(cXyz[2] - pXyz[2]) * 100;
                string xyz = "XYZ差=" + x.ToString("0") + "," + y.ToString("0") + "," + z.ToString("0");
                strList.Add(xyz);
            }
            return strList;
        }

        //        JavaScriptでRGBからLab色空間への変換 - Qiita
        //https://qiita.com/hachisukansw/items/09caabe6bec46a2a0858

        //xyz?
        private double[] RGBtoXYZ(Color c)
        {
            double r = c.R / 255f;
            double g = c.G / 255f;
            double b = c.B / 255f;
            r = (r > 0.04045) ? Math.Pow(((r + 0.055) / 1.055), 2.4) : (r / 12.92);
            g = (g > 0.04045) ? Math.Pow(((g + 0.055) / 1.055), 2.4) : (g / 12.92);
            b = (b > 0.04045) ? Math.Pow(((b + 0.055) / 1.055), 2.4) : (b / 12.92);

            double x = (r * 0.4124) + (g * 0.3576) + (b * 0.1805);
            double y = (r * 0.2126) + (g * 0.7152) + (b * 0.0722);
            double z = (r * 0.0193) + (g * 0.1192) + (b * 0.9505);
            return new double[] { x, y, z };
        }

        //RGBに重み付け、R0.299,G0.587,B0.114
        private double GetColorDistance2(Color c1, Color c2)
        {
            return Math.Sqrt(
                Math.Pow(c1.R * 0.299 - c2.R * 0.299, 2) +
                Math.Pow(c1.G * 0.587 - c2.G * 0.587, 2) +
                Math.Pow(c1.B * 0.114 - c2.B * 0.114, 2));
        }
        //距離
        private double GetColorDistance(Color c1, Color c2)
        {
            return Math.Sqrt(
                Math.Pow(c1.R - c2.R, 2) +
                Math.Pow(c1.G - c2.G, 2) +
                Math.Pow(c1.B - c2.B, 2));
        }

        //距離、小数点RGB
        private double GetColorDistanceDouble(double[] rgb, Color c2)
        {
            return Math.Sqrt(
                Math.Pow(rgb[0] - c2.R, 2) +
                Math.Pow(rgb[1] - c2.G, 2) +
                Math.Pow(rgb[2] - c2.B, 2));
        }

        //BorderパレットのBackground色表示を初期化
        private void BorderBackgroundNull(Border[] border)
        {
            for (int i = 0; i < border.Length; ++i)
            {
                border[i].Background = null;
            }
        }

        //パレットの色表示用のBorder作成
        private Border[] CreateBorders(int createCount)
        {
            Border[] palette = new Border[createCount];

            Border border;
            for (int i = 0; i < createCount; i++)
            {
                border = new Border()
                {
                    Width = 18,
                    Height = 18,
                    BorderBrush = new SolidColorBrush(Colors.AliceBlue),
                    BorderThickness = new Thickness(1f),
                    Margin = new Thickness(1f)
                };
                palette[i] = border;

                ContextMenu contextMenu = new ContextMenu();
                MenuItem item = new MenuItem();
                item.Header = "ok";
                contextMenu.Items.Add(item);
                border.ContextMenu = contextMenu;
            }
            return palette;
        }


        private void AddBorder(Border[] border, Panel panel)
        {
            for (int i = 0; i < border.Length; ++i)
            {
                panel.Children.Add(border[i]);
            }
        }


        //Borderの背景色をパレットの色にする
        private void ChangeColorPan(Border[] border, List<Color> palette, List<string> diffList = null)
        {
            BorderBackgroundNull(border);
            string tip = "";
            double bHue = HSV.Color2HSV(GetBorderBackGroundColor(BorderBaseColor)).Hue;
            double diffHue = 0;
            HSV pHSV;
            for (int i = 0; i < palette.Count; ++i)
            {
                border[i].Background = new SolidColorBrush(palette[i]);
                pHSV = HSV.Color2HSV(palette[i]);
                diffHue = Math.Abs(bHue - pHSV.Hue);
                if (diffHue > 180) { diffHue = Math.Abs(diffHue - 360); }
                tip = palette[i].ToString() + "\n" + pHSV.ToString100() + "\n" + diffHue.ToString();
                border[i].ToolTip = new TextBlock { Text = tip };
                if (diffList != null)
                {
                    ContextMenu contextMenu = new ContextMenu();
                    MenuItem item = new MenuItem();
                    item.Header = diffList[i];
                    contextMenu.Items.Add(item);
                    border[i].ContextMenu = contextMenu;
                }
            }
        }

        //Borderの背景色を取得
        private Color GetBorderBackGroundColor(Border border)
        {
            SolidColorBrush solid = (SolidColorBrush)border.Background;
            return solid.Color;
        }
        //Borderの背景色を取得、複数
        private List<Color> GetBorderColorList(Border[] border)
        {
            List<Color> list = new List<Color>();
            for (int i = 0; i < border.Length; ++i)
            {
                list.Add(GetBorderBackGroundColor(border[i]));
            }
            return list;
        }


        //パレットの色をRGBで並べ替え、重み付け
        private List<Color> SortRGB2(List<Color> palette, Color color)
        {
            SortedList<double, Color> sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistance2(color, palette[i]), palette[i]);
            }
            return sortedList.Values.ToList<Color>();
        }

        //パレットの色をRGBで並べ替え
        private List<Color> SortRGB(List<Color> palette, Color color)
        {
            SortedList<double, Color> sortedList = new SortedList<double, Color>();
            for (int i = 0; i < palette.Count; ++i)
            {
                sortedList.Add(GetColorDistance(color, palette[i]), palette[i]);
            }
            return sortedList.Values.ToList<Color>();
        }

        private void ReNewBaseColorText()
        {
            Color color = GetBorderBackGroundColor(BorderBaseColor);
            HSV ihsv = HSV.Color2HSV(color);
            string neko = "←基準色" + color.ToString() + "  hsv = " + ihsv.ToString100();
            TextBlockBaseColor.Text = neko;
        }
        //ランダム色取得
        private Color GetRandamColor()
        {
            Random random = new Random();
            return Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
        }
        //ランダム色取得、複数
        private List<Color> GetRandamColors(int num)
        {
            Random random = new Random();
            List<Color> list = new List<Color>();
            for (int i = 0; i < num; ++i)
            {
                list.Add(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
            }
            return list;
        }

        private void SortColor()
        {
            List<Color> random = GetBorderColorList(MyPalettePanRandam);
            Color baseColor = GetBorderBackGroundColor(BorderBaseColor);

            List<Color> sortedColors;
            sortedColors = SortRGB(random, baseColor);//RGB
            ChangeColorPan(MyPalettePanRGB, sortedColors);
            sortedColors = SortRGB2(random, baseColor);
            ChangeColorPan(MyPalettePanRGB2, sortedColors);

            sortedColors = SortHSV(random, baseColor, "h");//hsv hue
            ChangeColorPan(MyPalettePanHue, sortedColors);
            sortedColors = SortHSV(random, baseColor, "s");//hsv saturation
            ChangeColorPan(MyPalettePanSaturation, sortedColors);
            sortedColors = SortHSV(random, baseColor, "v");//hsv value
            ChangeColorPan(MyPalettePanValue, sortedColors);

            sortedColors = SortHSVALL(random, baseColor, false);//hsv円柱 add
            ChangeColorPan(MyPalettePanHSV円柱Add, sortedColors);
            sortedColors = SortHSVALL(random, baseColor, true);//hsv円錐 add
            ChangeColorPan(MyPalettePanHSV円錐Add, sortedColors);

            sortedColors = SortHSV円柱or円錐Triangle(random, baseColor, false);//hsv円柱モデル 三角関数
            ChangeColorPan(MyPalettePanHSV円柱Tri, sortedColors);
            sortedColors = SortHSV円柱or円錐Triangle(random, baseColor, true);//hsv_円錐モデル 三角関数
            ChangeColorPan(MyPalettePanHSV円錐Tri, sortedColors);

            sortedColors = SortHSV円柱or円錐Euclidean(random, baseColor, false);//hsv円柱モデル euclid
            ChangeColorPan(MyPalettePanHSV円柱Euclid, sortedColors);
            sortedColors = SortHSV円柱or円錐Euclidean(random, baseColor, true);//hsv円柱モデル euclid
            ChangeColorPan(MyPalettePanHSV円錐Euclid, sortedColors);

            //sortedColors = SortHSVALL2(random, baseColor);//hsv2
            //ChangeColorPan(MyPalettePanHSV2円柱, sortedColors);

            List<string> diffList;
            sortedColors = SortXYZ(random, baseColor);//xyz
            diffList = GetStringDistanceXYZ(sortedColors, baseColor);
            ChangeColorPan(MyPalettePanXYZ, sortedColors, diffList);
            sortedColors = SortLab(random, baseColor);//lab
            diffList = GetStringDistanceLab(sortedColors, baseColor);
            ChangeColorPan(MyPalettePanLab, sortedColors, diffList);

        }
        #region イベント



        private void ButtonChangeColor_Click(object sender, RoutedEventArgs e)
        {
            BorderBaseColor.Background = new SolidColorBrush(GetRandamColor());
            ReNewBaseColorText();
            SortColor();
        }

        private void ButtonSort_Click(object sender, RoutedEventArgs e)
        {
            SortColor();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            ChangeColorPan(MyPalettePanRandam, GetRandamColors(MAX_PALETTE_COLOR_COUNT));
            SortColor();
        }



        #endregion


    }
}



