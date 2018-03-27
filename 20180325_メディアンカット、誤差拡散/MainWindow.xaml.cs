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

using System.Collections.Concurrent;
using System.Windows.Threading;
using System.Diagnostics;
//パレットを使った減色で誤差拡散(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15432449.html

namespace _20180325_メディアンカット_誤差拡散
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;
        MyWrapPanel Pan1;
        MyWrapPanel Pan2;
        MyWrapPanel Pan3;
        MyWrapPanel Pan4;
        MyWrapPanel Pan5;
        MyWrapPanel Pan6;
        MyWrapPanel Pan7;

        //public enum SplitPlace//分割場所
        //{
        //    Center,//辺の中央で分割
        //    Median,//中央値で分割
        //}
        public enum SplitPriority//分割対象の選択方法
        {
            LongSide最長辺 = 1,//最大長辺を持つCubeを優先選択
            ManyPixels最多ピクセル数,//ピクセル数が多いCubeを優先選択
            MaxVolume最大体積,//体積
            Variance最大分散Cubeピクセル数考慮,//Cubeの最大分散、RGBそれぞれの分散値の合計が最大、ピクセル数考慮
            Variance最大分散辺ピクセル数考慮,//辺の最大分散、RGBのどれかの分散値が最大、ピクセル数考慮
            Variance最大分散Cube,//Cubeの最大分散、RGBそれぞれの分散値の合計が最大、ピクセル数無視してColorだけ
            Variance最大分散辺,//辺の最大分散、RGBのどれかの分散値が最大、ピクセル数無視してColorだけ
            Distance最大距離ピクセル,//Cubeの中心と全てのピクセルの距離の平均値が最大
            Distance最大距離色,//Cubeの中心とすべての色の距離の平均値が最大
        }
        public enum ColorSelection//Cubeの代表色の選択法
        {
            Average平均色,//全体のピクセルの平均色、Cubeの平均色ではない
            MedianRGBの中央値,
            DistantRGBCore中心から遠い色,//RGB空間の中心から最も遠い色
            DistantRGBVertex中心から遠い隅,//RGB空間の中心から最も遠い隅
            DistantCuveCore中心から遠い色,//Cubeの中心から最も遠い色            
            Core中心色,//中心の色
        }

        //List<Color> Palette1;

        const int MAX_PALETTE_COLOR_COUNT = 256;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            InitializeComboBox();

            ButtonOrigin.Click += ButtonOrigin_Click;
            ButtonSaveImage.Click += ButtonSaveImage_Click;
            ButtonCreatePalette.Click += ButtonCreatePalette_Click;
            //Button1.Click += Button1_Click;//作成1
            //Button2.Click += Button2_Click;
            //Button3.Click += Button3_Click;
            //Button4.Click += Button4_Click;
            //Button5.Click += Button5_Click;
            //Button6.Click += Button6_Click;
            //Button7.Click += Button7_Click;

            ButtonReduceColor.Click += ButtonReduceColor1_Click;//減色変換
            ButtonReduceColor2.Click += ButtonReduceColor2_Click;
            ButtonReduceColor3.Click += ButtonReduceColor3_Click;
            ButtonReduceColor4.Click += ButtonReduceColor4_Click;
            ButtonReduceColor5.Click += ButtonReduceColor5_Click;
            ButtonReduceColor6.Click += ButtonReduceColor6_Click;
            //ButtonReduceColor7.Click += ButtonReduceColor7_Click;

            Pan1 = new MyWrapPanel(MAX_PALETTE_COLOR_COUNT);
            Pan2 = new MyWrapPanel(MAX_PALETTE_COLOR_COUNT);
            Pan3 = new MyWrapPanel(MAX_PALETTE_COLOR_COUNT);
            Pan4 = new MyWrapPanel(MAX_PALETTE_COLOR_COUNT);
            Pan5 = new MyWrapPanel(MAX_PALETTE_COLOR_COUNT);
            Pan6 = new MyWrapPanel(MAX_PALETTE_COLOR_COUNT);
            Pan7 = new MyWrapPanel(MAX_PALETTE_COLOR_COUNT);

            StackPanelPan1.Children.Add(Pan1);
            StackPanelPan2.Children.Add(Pan2);
            StackPanelPan3.Children.Add(Pan3);
            StackPanelPan4.Children.Add(Pan4);
            StackPanelPan5.Children.Add(Pan5);
            StackPanelPan6.Children.Add(Pan6);
            //StackPanelPan7.Children.Add(Pan7);

            //MyPalettePan1 = CreateBorders(MyWrapPanel1);//パレットの色表示用のBorder作成

            //palette = new List<Color>();
            ButtonTest.Click += ButtonTest_Click;
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Restart();
            //MyImage.Source = ReduceColor指定色で減色誤差拡散A(OriginBitmap, Pan1.Palette);
            //stopwatch.Stop();
            //Console.WriteLine($"{nameof(ReduceColor指定色で減色誤差拡散A)}, { stopwatch.Elapsed}");
            //stopwatch.Restart();
            //MyImage.Source = ReduceColor指定色で減色誤差拡散B1(OriginBitmap, Pan1.Palette);
            //stopwatch.Stop();
            //Console.WriteLine($"{nameof(ReduceColor指定色で減色誤差拡散B1)}, { stopwatch.Elapsed}");
            //stopwatch.Restart();
            //MyImage.Source = ReduceColor指定色で減色誤差拡散B2(OriginBitmap, Pan1.Palette);
            //stopwatch.Stop();
            //Console.WriteLine($"{nameof(ReduceColor指定色で減色誤差拡散B2)}, { stopwatch.Elapsed}");

            //MyImage.Source = PaletteTest(OriginBitmap, Pan1.Palette, KeisokuA1);//13second
            //MyImage.Source = PaletteTest(OriginBitmap, Pan1.Palette, KeisokuA2);//5.5
            //MyImage.Source = PaletteTest(OriginBitmap, Pan1.Palette, KeisokuB1);//16
            //MyImage.Source = PaletteTest(OriginBitmap, Pan1.Palette, KeisokuB2);//6.9
            //MyImage.Source = PaletteTest(OriginBitmap, Pan1.Palette, KeisokuC1);//5.0
            //MyImage.Source = PaletteTest(OriginBitmap, Pan1.Palette, KeisokuC2);//1.7
            //MyImage.Source = PaletteTest(OriginBitmap, Pan1.Palette, KeisokuD1);//
            //MyImage.Source = PaletteTest(OriginBitmap, Pan1.Palette, KeisokuD2);//

            ////PaletteTest(OriginBitmap, Pan1.Palette, Keisoku2);//error
            //MessageBox.Show("ok");
        }


        private BitmapSource PaletteTest(BitmapSource source, List<Color> palette, Action<int, int, int, List<Color>, byte[]> action)
        {
            if (OriginBitmap == null) { return source; }
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            var stopWhatch = new Stopwatch();
            stopWhatch.Start();
            action(h, w, stride, palette, pixels);
            stopWhatch.Stop();
            Console.WriteLine($"{action.Method.Name}, {stopWhatch.Elapsed}");

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);

            return OptimisationPixelFormat(wb, palette.Count);
        }
        private void KeisokuA1(int h, int w, int stride, List<Color> palette, byte[] pixels)
        {
            long p;
            double distance;
            double min = double.MaxValue;
            Color pColor = palette[0];
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    min = double.MaxValue;
                    for (int i = 0; i < palette.Count; ++i)
                    {
                        distance = GetColorDistance(palette[i], Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]));
                        if (min > distance)
                        {
                            min = distance;
                            pColor = palette[i];
                        }
                    }
                    pixels[p + 2] = pColor.R; pixels[p + 1] = pColor.G; pixels[p] = pColor.B; pixels[p + 3] = 255;
                }
            }
        }

        private void KeisokuA2(int h, int w, int stride, List<Color> palette, byte[] pixels)
        {
            for (int y = 0; y < h; ++y)
            {
                Parallel.For(0, w, x =>
                {
                    KeisokuA2P(x, y, stride, pixels, palette);
                });
            }
        }
        private void KeisokuA2P(int x, int y, int stride, byte[] pixels, List<Color> palette)
        {
            long p;
            double distance;
            double min = double.MaxValue;
            Color pColor = palette[0];
            p = y * stride + (x * 4);
            for (int i = 0; i < palette.Count; ++i)
            {
                distance = GetColorDistance(palette[i], Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]));
                if (min > distance)
                {
                    min = distance;
                    pColor = palette[i];
                }
            }
            pixels[p + 2] = pColor.R; pixels[p + 1] = pColor.G; pixels[p] = pColor.B; pixels[p + 3] = 255;
        }

        private void KeisokuB1(int h, int w, int stride, List<Color> palette, byte[] pixels)
        {
            long p;
            Color pColor;
            Color[] color = palette.ToArray();
            double[] distance = new double[palette.Count];

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);

                    for (int i = 0; i < palette.Count; ++i)
                    {
                        distance[i] = GetColorDistance(palette[i], Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]));
                    }
                    color = palette.ToArray();
                    Array.Sort(distance, color);
                    pColor = color[0];
                    pixels[p + 2] = pColor.R; pixels[p + 1] = pColor.G; pixels[p] = pColor.B; pixels[p + 3] = 255;
                }
            }
        }

        private void KeisokuB2(int h, int w, int stride, List<Color> palette, byte[] pixels)
        {
            Color[] color = palette.ToArray();
            double[] distance = new double[palette.Count];

            for (int y = 0; y < h; ++y)
            {
                Parallel.For(0, w, x =>
                {
                    color = palette.ToArray();
                    KeisokuB2Para(palette, pixels, y, stride, x, color);
                });
            }
        }
        private void KeisokuB2Para(List<Color> palette, byte[] pixels, int y, int stride, int x, Color[] color)
        {
            long p;
            double[] distance = new double[palette.Count];

            p = y * stride + (x * 4);
            for (int i = 0; i < palette.Count; ++i)
            {
                distance[i] = GetColorDistance(palette[i], Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]));
            }
            Array.Sort(distance, color);
            Color pColor = color[0];
            pixels[p + 2] = pColor.R; pixels[p + 1] = pColor.G; pixels[p] = pColor.B; pixels[p + 3] = 255;
        }
        private void KeisokuC1(int h, int w, int stride, List<Color> palette, byte[] pixels)
        {
            double distance;
            double min = double.MaxValue;
            Color pColor = palette[0];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                min = double.MaxValue;
                for (int j = 0; j < palette.Count; ++j)
                {
                    distance = GetColorDistance(pixels[i + 2], pixels[i + 1], pixels[i], palette[j]);
                    if (min > distance)
                    {
                        min = distance;
                        pColor = palette[j];
                    }
                }
                pixels[i + 2] = pColor.R;
                pixels[i + 1] = pColor.G;
                pixels[i + 0] = pColor.B;
                pixels[i + 3] = 255;
            }
        }

        private void KeisokuC2(int h, int w, int stride, List<Color> palette, byte[] pixels)
        {
            Parallel.For(0, pixels.Length / 4, i =>
              {
                  KeisokuC1Para(i * 4, pixels, palette);
              });
        }
        private void KeisokuC1Para(int i, byte[] pixels, List<Color> palette)
        {
            double min = double.MaxValue;
            double distance;
            Color pColor = palette[0];
            for (int j = 0; j < palette.Count; ++j)
            {
                distance = GetColorDistance(pixels[i + 2], pixels[i + 1], pixels[i], palette[j]);
                if (min > distance)
                {
                    min = distance;
                    pColor = palette[j];
                }
            }
            pixels[i + 2] = pColor.R;
            pixels[i + 1] = pColor.G;
            pixels[i + 0] = pColor.B;
            pixels[i + 3] = 255;
        }
        private void KeisokuD1(int h, int w, int stride, List<Color> palette, byte[] pixels)
        {
            double distance;
            double min = double.MaxValue;
            int index = 0;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                min = double.MaxValue;
                for (int j = 0; j < palette.Count; ++j)
                {
                    distance = GetColorDistance(pixels[i + 2], pixels[i + 1], pixels[i], palette[j]);
                    if (min > distance)
                    {
                        min = distance;
                        index = j;
                    }
                }
                pixels[i + 2] = palette[index].R;
                pixels[i + 1] = palette[index].G;
                pixels[i + 0] = palette[index].B;
                pixels[i + 3] = 255;
            }
        }
        private void KeisokuD2(int h, int w, int stride, List<Color> palette, byte[] pixels)
        {
            Parallel.For(0, pixels.Length / 4, i =>
            {
                KeisokuD2Para(i * 4, pixels, palette);
            });
        }
        private void KeisokuD2Para(int i, byte[] pixels, List<Color> palette)
        {
            double min = double.MaxValue;
            double distance;
            int index = 0;
            for (int j = 0; j < palette.Count; ++j)
            {
                distance = GetColorDistance(pixels[i + 2], pixels[i + 1], pixels[i], palette[j]);
                if (min > distance)
                {
                    min = distance;
                    index = j;
                }
            }
            pixels[i + 2] = palette[index].R;
            pixels[i + 1] = palette[index].G;
            pixels[i + 0] = palette[index].B;
            pixels[i + 3] = 255;
        }
        //private void Keisoku2(int h, int w, int stride, List<Color> palette, byte[] pixels)
        //{
        //    long p;
        //    Color pColor;
        //    SortedList<double, Color> sortedList = new SortedList<double, Color>();
        //    for (int y = 0; y < h; ++y)
        //    {
        //        for (int x = 0; x < w; ++x)
        //        {
        //            p = y * stride + (x * 4);
        //            for (int i = 0; i < palette.Count; ++i)
        //            {
        //                try
        //                {
        //                    sortedList.Add(GetColorDistance(palette[i], Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p])), palette[i]);
        //                }
        //                catch (Exception)
        //                {
        //                }
        //            }
        //            pColor = sortedList.Values[0];
        //        }
        //    }
        //}





        /// <summary>
        /// 画像から等間隔で指定数のピクセルの色を取得、指定数ぴったりにはならない、
        /// 指定数より画像の総ピクセル数が少ないときは総ピクセル数になる
        /// </summary>
        /// <param name="source">PixelFormat.Pbgra32限定</param>
        /// <param name="number">ピクセル数</param>
        /// <returns>PixelFormat.Pbgra32</returns>
        private List<Color> GetPixelsColorOfNumber(BitmapSource source, int number)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;
            //1024ｘ768で9万指定のときは
            double sqrt = Math.Sqrt(number);//指定ピクセル数の平方根、9万指定なら300
            double fy = h / (sqrt * h / w);//縦の等間隔、1024/(300*1024/768)=2.56
            double fx = w / (sqrt * w / h);//横の等間隔、768/(300*768/1024)=3.41333
            double yy = 0, xx = 0;
            List<Color> listColor = new List<Color>();
            for (int y = 0; y < h; y = (int)(yy))
            {
                yy += fy;
                xx = 0;
                for (int x = 0; x < w; x = (int)(xx))
                {
                    xx += fx;
                    p = y * stride + (x * 4);
                    listColor.Add(Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]));
                }
            }
            return listColor;
        }


        private void ButtonCreatePalette_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            CreatePaletteAllSplitBy();//分割場所一覧パレット作成
            stopwatch.Stop();
            TextBlockTime.Text = $"作成時間：{stopwatch.Elapsed.Minutes}分{stopwatch.Elapsed.Seconds}秒{stopwatch.Elapsed.Milliseconds.ToString("000")}";
            this.IsEnabled = true;
        }

        //コンボボックス初期化
        private void InitializeComboBox()
        {
            //分割Cubeの選択方式、列挙体を追加する
            foreach (var item in Enum.GetValues(typeof(SplitPriority)))
            {
                ComboBoxSplitPriority.Items.Add(item);
            }
            ComboBoxSplitPriority.SelectedIndex = 0;
            //foreach (var item in Enum.GetNames(typeof(SplitPriority)))
            //{
            //    combobox1.Items.Add(item);
            //}
            foreach (var item in Enum.GetValues(typeof(ColorSelection)))
            {
                ComboBoxSelectColorMethod.Items.Add(item);
            }
            ComboBoxSelectColorMethod.SelectedIndex = 0;

        }


        //分割場所一覧パレット作成
        private void CreatePaletteAllSplitBy()
        {
            if (OriginBitmap == null) { return; }

            ButtonCreatePalette.Content = "パレット作成中…";
            DoEvents();

            //分割場所の関数
            var divide = new Func<Cube, List<Cube>>[]
            {
                SplitByLongSide辺の中央,
                SplitByMedian中央値,
                SplitByMinVariancePixel,
                SplitByMinVariance
            };

            //分割するCubeの選択関数
            Func<List<Cube>, int> FuncGetIndexSelect;
            switch (ComboBoxSplitPriority.SelectedValue)
            {
                case SplitPriority.LongSide最長辺:
                    FuncGetIndexSelect = GetSelectIndexLongSideCube; break;
                case SplitPriority.ManyPixels最多ピクセル数:
                    FuncGetIndexSelect = GetIndexSelectManyPidxelsCube; break;
                case SplitPriority.MaxVolume最大体積:
                    FuncGetIndexSelect = GetSelectIndexCapacityMaxCube; break;
                case SplitPriority.Variance最大分散Cubeピクセル数考慮:
                    FuncGetIndexSelect = GetSelectIndexOfMaxVarianceCubePixel; break;
                case SplitPriority.Variance最大分散辺ピクセル数考慮:
                    FuncGetIndexSelect = GetSelectIndexOfMaxVarianceSidePixel; break;
                case SplitPriority.Variance最大分散Cube:
                    FuncGetIndexSelect = GetSelectIndexOfMaxVarianceCube; break;
                case SplitPriority.Variance最大分散辺:
                    FuncGetIndexSelect = GetSelectIndexOfMaxVarianceSide; break;
                case SplitPriority.Distance最大距離ピクセル:
                    FuncGetIndexSelect = GetSelectIndexOfMaxDistancePixels; break;
                case SplitPriority.Distance最大距離色:
                    FuncGetIndexSelect = GetSelectIndexOfMaxDistanceColors; break;

                default:
                    FuncGetIndexSelect = GetSelectIndexLongSideCube; break;
            }

            //Cubeから色の選び方
            Func<Cube, Color> FuncGetColorFromCube;
            switch (ComboBoxSelectColorMethod.SelectedValue)
            {
                case ColorSelection.Average平均色:
                    FuncGetColorFromCube = GetLeaderColor1Average平均色; break;
                case ColorSelection.MedianRGBの中央値:
                    FuncGetColorFromCube = GetLeaderColor6RGBMedian; break;
                case ColorSelection.DistantRGBCore中心から遠い色:
                    FuncGetColorFromCube = GetLeaderColor3DistantRGBCore; break;
                case ColorSelection.DistantRGBVertex中心から遠い隅:
                    FuncGetColorFromCube = GetLeaderColor4DistantRGBCoreVertex; break;
                case ColorSelection.DistantCuveCore中心から遠い色:
                    FuncGetColorFromCube = GetLeaderColor5DistantCore; break;
                case ColorSelection.Core中心色:
                    FuncGetColorFromCube = GetLeaderColor2Core中心色; break;
                default:
                    FuncGetColorFromCube = GetLeaderColor1Average平均色; break;

            }
            var Pans = new MyWrapPanel[] { Pan1, Pan2, Pan3, Pan4, Pan5, Pan6 };

            for (int i = 0; i < divide.Length; ++i)
            {
                DoEvents();
                if (RadioPixels100k.IsChecked == true)
                {
                    Pans[i].SetColorList(GetColorListByCubeList(SplitCube(GetPixelsColorOfNumber(OriginBitmap, 100000), (int)NumericScrollBar.Value,
                        FuncGetIndexSelect, divide[i]), FuncGetColorFromCube));
                }
                else if (RadioPixelsMillion.IsChecked == true)
                {
                    Pans[i].SetColorList(GetColorListByCubeList(SplitCube(GetPixelsColorOfNumber(OriginBitmap, 1000000), (int)NumericScrollBar.Value,
                        FuncGetIndexSelect, divide[i]), FuncGetColorFromCube));
                }
                else
                {
                    //var neko = new Cube(OriginBitmap);
                    Pans[i].SetColorList(GetColorListByCubeList(SplitCube(new Cube(GetColorList(OriginBitmap)), (int)NumericScrollBar.Value,
                        FuncGetIndexSelect, divide[i]), FuncGetColorFromCube));
                }
            }

            ButtonCreatePalette.Content = "パレット作成";

        }


        //BitmapSourceをColorのListに変換する
        private List<Color> GetColorList(BitmapSource source)
        {
            var bitmap = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
            var wb = new WriteableBitmap(bitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;

            var ColorList = new List<Color>();
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    ColorList.Add(Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]));
                }
            }
            return ColorList;
        }



        //        コントロールを明示的に更新する - WPFアプリケーションでのApplication.DoEvents の実装(WPFプログラミング)
        //https://www.ipentec.com/document/csharp-wpf-implement-application-doevents

        private void DoEvents()
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            var callBack = new DispatcherOperationCallback(ExitFrames);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callBack, frame);
            Dispatcher.PushFrame(frame);
        }
        private object ExitFrames(object obj)
        {
            ((DispatcherFrame)obj).Continue = false;
            return null;
        }

        private void ButtonReduceColor7_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ReduceColor指定色で減色(OriginBitmap, Pan7.Palette);
        }

        //private void Button7_Click(object sender, RoutedEventArgs e)
        //{
        //    Pan1.SetColorList(BitmapSourceMedianCutでパレット作成(OriginBitmap, (int)NumericScrollBar.Value,
        //        GetIndexSelectManyPidxelsCube, SplitByLongSide辺の中央, GetColorCubeAverage平均色));
        //    Pan2.SetColorList(BitmapSourceMedianCutでパレット作成(OriginBitmap, (int)NumericScrollBar.Value,
        //        GetSelectIndexLongSideCube, SplitByLongSide辺の中央, GetColorCubeAverage平均色));
        //    Pan3.SetColorList(BitmapSourceMedianCutでパレット作成(OriginBitmap, (int)NumericScrollBar.Value,
        //        GetIndexSelectManyPidxelsCube, SplitByLongSide辺の中央, GetColorCubeCore中心色));
        //    Pan4.SetColorList(BitmapSourceMedianCutでパレット作成(OriginBitmap, (int)NumericScrollBar.Value,
        //        GetSelectIndexLongSideCube, SplitByLongSide辺の中央, GetColorCubeCore中心色));
        //    Pan5.SetColorList(BitmapSourceMedianCutでパレット作成(OriginBitmap, (int)NumericScrollBar.Value,
        //        GetIndexSelectManyPidxelsCube, SplitByMedian中央値, GetColorCubeAverage平均色));
        //    Pan6.SetColorList(BitmapSourceMedianCutでパレット作成(OriginBitmap, (int)NumericScrollBar.Value,
        //        GetSelectIndexLongSideCube, SplitByMedian中央値, GetColorCubeAverage平均色));
        //    Pan7.SetColorList(BitmapSourceMedianCutでパレット作成(OriginBitmap, (int)NumericScrollBar.Value,
        //        GetSelectIndexCapacityMaxCube, SplitByMedian中央値, GetColorCubeDistantVertexRGBCore));
        //}

        private void ButtonReduceColor6_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickで減色(Pan6);
        }

        private void ButtonReduceColor5_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickで減色(Pan5);
        }

        private void ButtonReduceColor4_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickで減色(Pan4);
        }

        private void ButtonReduceColor3_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickで減色(Pan3);
        }

        private void ButtonReduceColor2_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickで減色(Pan2);
        }

        private void ButtonReduceColor1_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickで減色(Pan1);
        }
        private void ButtonClickで減色(MyWrapPanel pan)
        {
            if (OriginBitmap == null) { return; }
            if (CheckGosa.IsChecked == true)
            {
                MyImage.Source = ReduceColor指定色で減色誤差拡散B1(OriginBitmap, pan.Palette);
            }
            else
            {
                MyImage.Source = ReduceColor一覧表方式(OriginBitmap, pan.Palette);
                //MyImage.Source = ReduceColor指定色で減色(OriginBitmap, Pan1.Palette);
                //一覧表方式じゃないと大きな画像、パレット多色でものすごく時間がかかる、2048ｘ1536でパレット200色だと1分以上2分位かかる
            }
        }

        //private List<Color> BitmapSourceMedianCutでパレット作成(BitmapSource source, int splitCount,
        //    Func<List<Cube>, int> GetIndexOfSplitCube, Func<Cube, List<Cube>> SplitBy,
        //    Func<Cube, Color> GetColorCube)
        //{
        //    //分割
        //    List<Cube> list = SplitCube(source, splitCount, GetIndexOfSplitCube, SplitBy);
        //    //Cubeから色取得
        //    return GetColorListByCubeList(list, GetColorCube);
        //}


        private void ButtonOrigin_Click(object sender, RoutedEventArgs e)
        {//元の画像に戻す
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }

        #region 減色


        private BitmapSource ReduceColor指定色で減色(BitmapSource source, List<Color> palette)
        {
            if (OriginBitmap == null) { return source; }
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; ++y)
            {
                Parallel.For(0, w, x =>
                {
                    XParallel(y, stride, x, palette, pixels);
                });
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);

            return OptimisationPixelFormat(wb, palette.Count);
        }
        private void XParallel(int y, int stride, int x, List<Color> palette, byte[] pixels)
        {
            var p = y * stride + (x * 4);
            var myColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
            double min, distance;
            int pIndex;

            min = GetColorDistance(myColor, palette[0]);
            pIndex = 0;
            for (int i = 0; i < palette.Count; ++i)
            {
                distance = GetColorDistance(myColor, palette[i]);
                if (min > distance)
                {
                    min = distance;
                    pIndex = i;
                }
            }
            myColor = palette[pIndex];
            pixels[p + 2] = myColor.R;
            pixels[p + 1] = myColor.G;
            pixels[p] = myColor.B;
            pixels[p + 3] = 255;//アルファ値を255に変更、完全不透明にする
        }

        private BitmapSource ReduceColor一覧表方式(BitmapSource source, List<Color> palette)
        {
            if (OriginBitmap == null) { return source; }
            if (palette.Count == 0) { return source; }
            //var rgb24 = new FormatConvertedBitmap(source, PixelFormats.Rgb24, source.Palette, 0);
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            //var neko = GetList一覧表Test1(pixels, palette);
            var neko = GetList一覧表Test2Parallel(pixels, palette);
            //long p;
            //Color myColor;
            for (int y = 0; y < h; ++y)
            {
                Parallel.For(0, w, x =>
                {
                    ReduceColor一覧表方式Sub(y, stride, x, pixels, neko);
                });
                //for (int x = 0; x < w; ++x)
                //{
                //    p = y * stride + (x * 4);
                //    myColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
                //    myColor = neko[myColor];
                //    pixels[p + 2] = myColor.R;
                //    pixels[p + 1] = myColor.G;
                //    pixels[p] = myColor.B;
                //  pixels[p + 3] = 255;//アルファは完全不透明にする
                //}
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return OptimisationPixelFormat(wb, palette.Count);
        }
        private void ReduceColor一覧表方式Sub(int y, int stride, int x, byte[] pixels, ConcurrentDictionary<Color, Color> neko)
        {
            long p = y * stride + (x * 4);
            Color myColor;
            myColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
            myColor = neko[myColor];
            pixels[p + 2] = myColor.R;
            pixels[p + 1] = myColor.G;
            pixels[p] = myColor.B;
            pixels[p + 3] = 255;//アルファは完全不透明にする
        }
        private int[] GetIntColorCount(byte[] pixels)
        {
            int[] iColor = new int[256 * 256 * 256];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                iColor[pixels[i] * 256 * 256 + pixels[i + 1] * 256 + pixels[i + 2]]++;
            }
            return iColor;
        }

        private ConcurrentDictionary<Color, Color> GetList一覧表Test1(byte[] pixels, List<Color> palette)
        {
            int[] iColor;// = new int[256 * 256 * 256];
            //int id;
            //for (int i = 0; i < pixels.Length; i += 4)
            //{
            //    id = pixels[i] * 256 * 256 + pixels[i + 1] * 256 + pixels[i + 2];
            //    iColor[id]++;
            //}
            iColor = GetIntColorCount(pixels);
            var converter = new ConcurrentDictionary<Color, Color>();
            Color myColor;
            for (int i = 0; i < iColor.Length; ++i)
            {
                if (iColor[i] != 0)
                {
                    myColor = Color.FromRgb((byte)(i % 256), (byte)(i / 256 % 256), (byte)(i / 256 / 256));
                    double min = 10000;
                    int pIndex = 0;
                    for (int j = 0; j < palette.Count; ++j)
                    {
                        double distance = GetColorDistance(myColor, palette[j]);
                        if (min > distance)
                        {
                            min = distance;
                            pIndex = j;
                        }
                    }
                    converter.GetOrAdd(myColor, palette[pIndex]);
                }
            }
            return converter;//0.4second
        }
        //↑のParallel版
        private ConcurrentDictionary<Color, Color> GetList一覧表Test2Parallel(byte[] pixels, List<Color> palette)
        {
            int[] iColor;// = new int[256 * 256 * 256];
            iColor = GetIntColorCount(pixels);
            //int id;
            //for (int i = 0; i < pixels.Length; i += 4)
            //{
            //    id = pixels[i] * 256 * 256 + pixels[i + 1] * 256 + pixels[i + 2];
            //    iColor[id]++;
            //}

            var converter = new ConcurrentDictionary<Color, Color>();
            //Color myColor;
            Parallel.For(0, iColor.Length, i =>
            {
                if (iColor[i] != 0)
                {
                    Test2Sub(i, palette, pixels, converter);
                    //myColor = Color.FromRgb((byte)(i % 256), (byte)(i / 256 % 256), (byte)(i / 256 / 256));
                    //double min = 10000;
                    //int pIndex = 0;
                    //for (int j = 0; j < palette.Count; ++j)
                    //{
                    //    double distance = GetColorDistance(myColor, palette[j]);
                    //    if (min > distance)
                    //    {
                    //        min = distance;
                    //        pIndex = j;
                    //    }
                    //}
                    //converter.GetOrAdd(myColor, palette[pIndex]);
                    //↑を別関数にしないとエラーになるし遅くなる0.5
                }
            });
            return converter;//0.32second
        }
        private void Test2Sub(int i, List<Color> palette, byte[] pixels, ConcurrentDictionary<Color, Color> converter)
        {
            Color myColor = Color.FromRgb((byte)(i % 256), (byte)(i / 256 % 256), (byte)(i / 256 / 256));
            double min = 10000;
            int pIndex = 0;

            for (int j = 0; j < palette.Count; ++j)
            {
                double distance = GetColorDistance(myColor, palette[j]);
                if (min > distance)
                {
                    min = distance;
                    pIndex = j;
                }
            }
            converter.GetOrAdd(myColor, palette[pIndex]);
        }

        //誤差拡散
        private BitmapSource ReduceColor指定色で減色誤差拡散A(BitmapSource source, List<Color> palette)
        {
            if (OriginBitmap == null) { return source; }
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            double[] iPixels = new double[h * stride];
            pixels.CopyTo(iPixels, 0);
            long p = 0;
            Color pColor = Colors.Black;
            double gosa = 0;
            double min = double.MaxValue;
            double distance = 0;
            byte[] BGR = new byte[3];
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    //iColor = Color.FromRgb(iPixels[p + 2], pixels[p + 1], pixels[p]);
                    min = double.MaxValue;
                    //パレットから一番近い色を選ぶ
                    foreach (Color item in palette)
                    {
                        distance = GetColorDistance(iPixels[p + 2], iPixels[p + 1], iPixels[p], item);
                        if (min > distance)
                        {
                            min = distance;
                            pColor = item;//近い色
                        }
                    }

                    BGR[0] = pColor.B; BGR[1] = pColor.G; BGR[2] = pColor.R;
                    for (int i = 0; i < 3; ++i)
                    {
                        gosa = (iPixels[p + i] - BGR[i]) / 16f;//誤差
                        double addGosa;
                        long pp;
                        if (x != w - 1)//右端じゃなければ
                        {
                            pp = p + i + 4;
                            addGosa = iPixels[pp] + (gosa * 7f);
                            LimitedGosa(iPixels, addGosa, pp);//右隣へ拡散
                        }
                        if (y < h - 1)//一番下の行じゃなければ
                        {
                            pp = p + stride + i;
                            addGosa = iPixels[pp] + (gosa * 5f);
                            LimitedGosa(iPixels, addGosa, pp);//真下へ拡散
                            if (x != 0)
                            {
                                pp = p + stride + i - 4;
                                addGosa = iPixels[pp] + (gosa * 3f);
                                LimitedGosa(iPixels, addGosa, pp);//左下へ拡散
                            }
                            if (x != w - 1)//右端じゃなければ
                            {//↑は(x < w - 1)とどっちが速い？
                                pp = p + stride + i + 4;
                                addGosa = iPixels[pp] + (gosa * 1f);
                                LimitedGosa(iPixels, addGosa, pp);//右下へ拡散
                            }

                        }
                    }

                    pixels[p + 2] = pColor.R;
                    pixels[p + 1] = pColor.G;
                    pixels[p] = pColor.B;
                    pixels[p + 3] = 255;//アルファ値は255に固定
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            //return wb;
            return OptimisationPixelFormat(wb, palette.Count);
        }
        private void LimitedGosa(double[] iPixels, double addGosa, long pp)
        {
            iPixels[pp] = (addGosa < 0) ? 0 : (addGosa > 255) ? 255 : addGosa;
        }

        private BitmapSource ReduceColor指定色で減色誤差拡散B1(BitmapSource source, List<Color> palette)
        {
            if (OriginBitmap == null) { return source; }
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            double[] iPixels = new double[h * stride];
            pixels.CopyTo(iPixels, 0);
            long pp = 0;
            Color pColor = Colors.Black;
            double gosa = 0;
            double addGosa = 0;
            double min = double.MaxValue;
            double distance = 0;
            byte[] BGR = new byte[3];
            for (int p = 0; p < pixels.Length; p += 4)
            {
                min = double.MaxValue;
                //パレットから一番近い色を選ぶ
                for (int i = 0; i < palette.Count; ++i)
                {
                    distance = GetColorDistance(iPixels[p + 2], iPixels[p + 1], iPixels[p], palette[i]);
                    if (min > distance)
                    {
                        min = distance;
                        pColor = palette[i];
                    }
                }

                BGR[0] = pColor.B;
                BGR[1] = pColor.G;
                BGR[2] = pColor.R;

                for (int c = 0; c < 3; ++c)//B,G,Rの順
                {
                    gosa = (iPixels[p + c] - BGR[c]) / 16f;//誤差を蓄積した元の色-パレットの色                    
                    if ((p + 4) % stride != 0)//右端じゃなければ
                    {
                        pp = p + 4 + c;
                        addGosa = iPixels[pp] + (gosa * 7f);
                        iPixels[pp] = (addGosa < 0) ? 0 : (addGosa > 255) ? 255 : addGosa;
                    }
                    if (p < stride * (h - 1))//一番下の行じゃなければ
                    {
                        pp = stride + p + c;
                        addGosa = iPixels[pp] + (gosa * 5f);
                        iPixels[pp] = (addGosa < 0) ? 0 : (addGosa > 255) ? 255 : addGosa;//真下へ拡散
                        if (p % stride != 0)//左端じゃなければ
                        {
                            pp = stride + p - 4 + c;
                            addGosa = iPixels[pp] + (gosa * 3f);
                            iPixels[pp] = (addGosa < 0) ? 0 : (addGosa > 255) ? 255 : addGosa;//左下へ拡散
                        }
                        if ((p + 4) % stride != 0)
                        {
                            pp = stride + p + 4 + c;
                            addGosa = iPixels[pp] + (gosa * 1f);
                            iPixels[pp] = (addGosa < 0) ? 0 : (addGosa > 255) ? 255 : addGosa;//右下へ拡散
                        }
                    }
                }
                pixels[p + 2] = pColor.R;
                pixels[p + 1] = pColor.G;
                pixels[p] = pColor.B;
                pixels[p + 3] = 255;//アルファ値は255に固定

            }

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return OptimisationPixelFormat(wb, palette.Count);
        }
        //↑のパラレル版だけど失敗、誤差拡散は順番に処理する必要がある
        private BitmapSource ReduceColor指定色で減色誤差拡散B2(BitmapSource source, List<Color> palette)
        {
            if (OriginBitmap == null) { return source; }
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            double[] iPixels = new double[h * stride];
            pixels.CopyTo(iPixels, 0);

            Parallel.For(0, pixels.Length / 4, i =>
              {
                  ReduceColor指定色で減色誤差拡散B2Para(i * 4, palette, iPixels, pixels, stride, h);
              });


            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return OptimisationPixelFormat(wb, palette.Count);
        }

        private void ReduceColor指定色で減色誤差拡散B2Para(int i, List<Color> palette, double[] iPixels, byte[] pixels, int stride, int h)
        {
            double distance = 0;
            double min = double.MaxValue;
            Color pColor = palette[0];
            byte[] BGR = new byte[3];
            double gosa = 0;
            double addGosa = 0;
            long pp;
            //パレットから一番近い色を選ぶ
            for (int j = 0; j < palette.Count; ++j)
            {
                distance = GetColorDistance(iPixels[i + 2], iPixels[i + 1], iPixels[i], palette[j]);
                if (min > distance)
                {
                    min = distance;
                    pColor = palette[j];
                }
            }

            BGR[0] = pColor.B; BGR[1] = pColor.G; BGR[2] = pColor.R;
            for (int c = 0; c < BGR.Length; ++c)
            {
                gosa = (iPixels[i + c] - BGR[c]) / 16f;
                addGosa = 0;
                pp = 0;
                if ((i + 4) % stride != 0)//右端じゃなければ
                {
                    pp = i + 4;
                    addGosa = iPixels[pp] + (gosa * 7f);
                    LimitedGosa(iPixels, addGosa, pp);
                }
                if (i < stride * (h - 1))//一番下の行じゃなければ
                {
                    pp = stride + i;
                    addGosa = iPixels[pp] + (gosa * 5f);
                    LimitedGosa(iPixels, addGosa, pp);//真下へ拡散
                    if (i % stride != 0)//左端じゃなければ
                    {
                        pp = stride + i - 4;
                        addGosa = iPixels[pp] + (gosa * 3f);
                        LimitedGosa(iPixels, addGosa, pp);//左下へ拡散
                    }
                    if ((i + 4) % stride != 0)
                    {
                        pp = stride + i + 4;
                        addGosa = iPixels[pp] + (gosa * 1f);
                        LimitedGosa(iPixels, addGosa, pp);//右下へ拡散
                    }
                }
            }
            pixels[i + 2] = pColor.R;
            pixels[i + 1] = pColor.G;
            pixels[i] = pColor.B;
            pixels[i + 3] = 255;//アルファ値は255に固定
        }







        #endregion






        /// <summary>
        /// bitmapからCubeを作って指定数まで分割
        /// </summary>
        /// <param name="source">PixelFormat.Pbgr32限定</param>
        /// <param name="splitCount">いくつまで分割するのか</param>
        /// <param name="GetIndexOfSplitCube">分割するCubeの選択方法の関数</param>
        /// <param name="SplitBy">分割する場所の指定</param>
        /// <returns></returns>
        private List<Cube> SplitCube(
            Cube cube,
            int splitCount,
            Func<List<Cube>, int> GetIndexOfSplitCube,
            Func<Cube, List<Cube>> SplitBy)
        {
            int loopCount = 1;
            var cubeList = new List<Cube>() { cube };//元のCubeのリスト
            var tempCubeList = new List<Cube>();//2分割されたCubeを一時的に入れるリスト
            var completionList = new List<Cube>();//これ以上分割できないCubeのリスト
            int index;
            //指定数まで分割されるか、これ以上分割できなくなるまでループ
            while (splitCount > loopCount && cubeList.Count > 0)
            {
                // どのCubeを分割するのか選定(最大長辺or最大ピクセル数)
                index = GetIndexOfSplitCube(cubeList);

                //分割してリストに追加
                tempCubeList.Clear();
                tempCubeList.AddRange(SplitBy(cubeList[index]));//2分割

                //2分割した結果どちらかのCubeのピクセル数が0なら、それ以上分割できないってことなので
                //別のリストに追加する
                if (tempCubeList[0].AllPixelsColor.Count == 0 || tempCubeList[1].AllPixelsColor.Count == 0)
                {
                    if (tempCubeList[0].AllPixelsColor.Count == 0)
                    {
                        completionList.Add(tempCubeList[1]);
                    }
                    else { completionList.Add(tempCubeList[0]); }
                }
                //普通に2分割できたら元のリストに追加
                else
                {
                    cubeList.AddRange(tempCubeList);
                    loopCount++;
                }

                //分割のもとになったCubeをリストから削除
                cubeList.RemoveAt(index);

                //loopCount++;
            }
            //
            cubeList.AddRange(completionList);
            return cubeList;
        }
        private List<Cube> SplitCube(
            List<Color> listColors,
            int splitCount,
            Func<List<Cube>, int> GetIndexOfSplitCube,
            Func<Cube, List<Cube>> SplitBy)
        {
            return SplitCube(new Cube(listColors), splitCount, GetIndexOfSplitCube, SplitBy);
        }

        #region 分割するCubeの選択法


        //Cubeのリストからピクセル数最大のCubeのIndexを取得
        private int GetIndexSelectManyPidxelsCube(List<Cube> cubeList)
        {
            int max = 0, index = 0;
            for (int i = 0; i < cubeList.Count; ++i)
            {
                if (max < cubeList[i].AllPixelsColor.Count)
                {
                    max = cubeList[i].AllPixelsColor.Count;
                    index = i;
                }
            }
            return index;
        }
        //リストから最大長辺を持つCubeのIndexを取得
        private int GetSelectIndexLongSideCube(List<Cube> cubeList)
        {
            int max = 0, index = 0;
            for (int i = 0; i < cubeList.Count; ++i)
            {
                if (max < cubeList[i].LengthMax)
                {
                    max = cubeList[i].LengthMax;
                    index = i;
                }
            }
            return index;
        }

        //リストから最大体積を持つCubeのIndexを取得
        private int GetSelectIndexCapacityMaxCube(List<Cube> cubeList)
        {
            int max = 0, index = 0, capa = 0;
            Cube c;

            for (int i = 0; i < cubeList.Count; ++i)
            {
                c = cubeList[i];
                capa = (c.MaxRed - c.MinRed) * (c.MaxGreen - c.MinGreen) * (c.MaxBlue - c.MinBlue);
                if (max < capa)
                {
                    max = capa;
                    index = i;
                }
            }
            return index;
        }

        //最大分散のCubeのindex取得、ピクセル数考慮版
        private int GetSelectIndexOfMaxVarianceCubePixel(List<Cube> cubeList)
        {
            int index = 0;
            double max = 0;
            double variance = 0;

            for (int i = 0; i < cubeList.Count; ++i)
            {
                //分散
                double[] rgbVariance = GetRGBごとの分散Pixel(cubeList[i]);
                variance = rgbVariance[0] + rgbVariance[1] + rgbVariance[2];
                if (max < variance)
                {
                    max = variance;
                    index = i;
                }
            }
            return index;
        }
        //最大分散辺を持つCubeのindex取得、ピクセル数考慮版
        private int GetSelectIndexOfMaxVarianceSidePixel(List<Cube> cubeList)
        {
            int index = 0;
            double max = 0;
            double variance = 0;
            for (int i = 0; i < cubeList.Count; ++i)
            {
                double[] rgbVariance = GetRGBごとの分散Pixel(cubeList[i]);
                variance = Math.Max(rgbVariance[0], Math.Max(rgbVariance[1], rgbVariance[2]));
                if (max < variance)
                {
                    max = variance;
                    index = i;
                }
            }
            return index;
        }

        //最大分散のCubeのindex取得
        //RGBそれぞれの分散の合計が大きいCubeを選ぶ、もしくは中心からの距離の合計が大きいCubeを選ぶ
        private int GetSelectIndexOfMaxVarianceCube(List<Cube> cubeList)
        {
            int MaxVarianceCubeIndex = 0;
            double max = 0;
            double variance = 0;

            //RGBの分散の合計が大きいCubeを選ぶ
            for (int i = 0; i < cubeList.Count; ++i)
            {
                //分散
                double[] rgbVariance = GetRGBごとの分散(cubeList[i]);
                variance = rgbVariance[0] + rgbVariance[1] + rgbVariance[2];
                if (max < variance)
                {
                    max = variance;
                    MaxVarianceCubeIndex = i;
                }
            }

            return MaxVarianceCubeIndex;
        }
        //最大分散辺を持つCubeのindex取得
        private int GetSelectIndexOfMaxVarianceSide(List<Cube> cubeList)
        {
            int index = 0;
            double max = 0;
            double variance = 0;
            for (int i = 0; i < cubeList.Count; ++i)
            {
                double[] rgbVariance = GetRGBごとの分散(cubeList[i]);
                variance = Math.Max(rgbVariance[0], Math.Max(rgbVariance[1], rgbVariance[2]));
                if (max < variance)
                {
                    max = variance;
                    index = i;
                }
            }
            return index;
        }


        /// <summary>
        /// 分散取得
        /// </summary>
        /// <param name="iCube"></param>
        /// <param name="fromPixel">ピクセル数考慮するときはTrue</param>
        /// <returns></returns>
        private double[] GetRGBごとの分散Pixel(Cube iCube)
        {
            double rVar = 0, gVar = 0, bVar = 0;
            long count = 0;
            var rgbVariance = new double[3];
            //Cubeの分散の変数に数値が入っていなければ計算して入れて返す
            if (double.IsNaN(iCube.VarianceMaxFromPixel) == true)
            {
                count = iCube.AllPixelsColor.Count;
                rVar = 0; gVar = 0; bVar = 0;
                //Parallel.ForEach(iCube.AllPixelsColor, item =>
                //{
                //    Variance(item, iCube, rVar, gVar, bVar);
                //});

                //foreach(Color item in iCube.AllPixelsColor)
                //{                    
                //    rVar += Math.Pow(item.R - iCube.RedPixAverage, 2f);//偏差の2乗の合計
                //    gVar += Math.Pow(item.G - iCube.GreenPixAverage, 2f);
                //    bVar += Math.Pow(item.B - iCube.BluePixAverage, 2f);
                //}
                Color pxColor;
                for (int i = 0; i < iCube.AllPixelsColor.Count; i++)
                {
                    pxColor = iCube.AllPixelsColor[i];
                    rVar += Math.Pow(pxColor.R - iCube.RedPixAverage, 2f);//偏差の2乗の合計
                    gVar += Math.Pow(pxColor.G - iCube.GreenPixAverage, 2f);
                    bVar += Math.Pow(pxColor.B - iCube.BluePixAverage, 2f);
                }
                //Cubeの分散の変数に分散を入れる
                iCube.VarianceRedFromPixel = rVar / count;
                iCube.VarianceGreenFromPixel = gVar / count;
                iCube.VarianceBlueFromPixel = bVar / count;
                iCube.VarianceMaxFromPixel = Math.Max(iCube.VarianceRedFromPixel, Math.Max(iCube.VarianceGreenFromPixel, iCube.VarianceBlueFromPixel));
                rgbVariance = new double[] { iCube.VarianceRedFromPixel, iCube.VarianceGreenFromPixel, iCube.VarianceBlueFromPixel };
                //return new double[] { iCube.VarianceRedFromPixel, iCube.VarianceGreenFromPixel, iCube.VarianceBlueFromPixel };
            }
            return rgbVariance;
        }
        private double[] GetRGBごとの分散(Cube iCube)
        {
            double rVar = 0, gVar = 0, bVar = 0;
            long count = 0;
            var rgbVariance = new double[3];
            //Cubeの分散の変数に数値が入っていなければ計算して入れて返す
            if (double.IsNaN(iCube.VarianceMax) == true)
            {//ピクセル数は無視する版
                count = iCube.AllColor.Count;
                rVar = 0; gVar = 0; bVar = 0;
                Color pxColor;
                for (int i = 0; i < iCube.AllColor.Count; i++)
                {
                    pxColor = iCube.AllColor[i];
                    rVar += Math.Pow(pxColor.R - iCube.RedPixAverage, 2f);//偏差の2乗の合計
                    gVar += Math.Pow(pxColor.G - iCube.GreenPixAverage, 2f);
                    bVar += Math.Pow(pxColor.B - iCube.BluePixAverage, 2f);
                }

                //Cubeの分散の変数に分散を入れる
                iCube.VarianceRed = rVar / count;
                iCube.VarianceGreen = gVar / count;
                iCube.VarianceBlue = bVar / count;
                iCube.VarianceMax = Math.Max(iCube.VarianceRed, Math.Max(iCube.VarianceGreen, iCube.VarianceBlue));
                rgbVariance = new double[] { iCube.VarianceRed, iCube.VarianceGreen, iCube.VarianceBlue };
                //return new double[] { iCube.VarianceRed, iCube.VarianceGreen, iCube.VarianceBlue };
            }
            return rgbVariance;
        }

        //Cubeの中心と全てのピクセルの距離の平均値が大きいCubeを選択
        private int GetSelectIndexOfMaxDistancePixels(List<Cube> cubeList)
        {
            //中心からの距離の合計で選ぶ、ピクセル数考慮
            int IndexOfMaxDistanceCubeAllPixels = 0;
            double max = 0;
            double distanceP = 0;
            Cube iCube;
            for (int i = 0; i < cubeList.Count; ++i)
            {
                iCube = cubeList[i];
                foreach (Color item in iCube.AllPixelsColor)//全ピクセル
                {
                    distanceP += GetColorDistance(
                        (byte)iCube.RedPixAverage,
                        (byte)iCube.GreenPixAverage,
                        (byte)iCube.BluePixAverage,
                        item);
                }
                distanceP /= iCube.AllPixelsColor.Count;//全てのピクセルと中心からの平均距離
                if (max < distanceP)
                {
                    max = distanceP;
                    IndexOfMaxDistanceCubeAllPixels = i;
                }
            }
            return IndexOfMaxDistanceCubeAllPixels;
        }
        //Cubeの中心と全ての色の距離の平均値が大きいCubeを選択
        private int GetSelectIndexOfMaxDistanceColors(List<Cube> cubeList)
        {
            //中心からの距離の合計で選ぶ
            int IndexOfMaxDistanceCube = 0;
            double max = 0;
            double distance = 0;
            Cube iCube;
            for (int i = 0; i < cubeList.Count; ++i)
            {
                iCube = cubeList[i];
                foreach (Color item in iCube.AllColor)//全色
                {
                    distance += GetColorDistance(
                        (byte)iCube.RedPixAverage,
                        (byte)iCube.GreenPixAverage,
                        (byte)iCube.BluePixAverage,
                        item);
                }
                distance /= iCube.AllColor.Count;//全ての色と中心からの平均距離
                if (max < distance)
                {
                    max = distance;
                    IndexOfMaxDistanceCube = i;
                }
            }
            return IndexOfMaxDistanceCube;
        }






        #endregion


        //CubeのリストからColorリスト取得、色の選択法を指定
        private List<Color> GetColorListByCubeList(List<Cube> cubeList, Func<Cube, Color> GetColorCube)
        {
            var myColorList = new List<Color>();
            for (int i = 0; i < cubeList.Count; ++i)
            {
                if (cubeList[i].AllPixelsColor.Count != 0)
                {
                    myColorList.Add(GetColorCube(cubeList[i]));
                }
            }
            //パラレルのほうが速いけど誤差程度？あんまりかわんないしパレットの色順が変化するからいまいち
            //Parallel.For(0, cubeList.Count, i =>
            //{
            //    if (cubeList[i].AllPixelsColor.Count != 0)
            //    {
            //        myColorList.Add(GetColorCube(cubeList[i]));
            //    }
            //});
            return myColorList;
        }



        //距離
        private double GetColorDistance(Color c1, Color c2)
        {
            return Math.Sqrt(
                Math.Pow(c1.R - c2.R, 2) +
                Math.Pow(c1.G - c2.G, 2) +
                Math.Pow(c1.B - c2.B, 2));
        }
        private double GetColorDistance(double r, double g, double b, Color c)
        {
            return Math.Sqrt(
                Math.Pow(c.R - r, 2) +
                Math.Pow(c.G - g, 2) +
                Math.Pow(c.B - b, 2));
        }



        #region 分割場所


        //一番長い辺で2分割
        //RGB同じだった場合はRGBの順で優先
        private List<Cube> SplitByLongSide辺の中央(Cube cube)
        {
            if (cube.LengthMax == cube.LengthRed)
            {//Rの辺が最長の場合、R要素の中間で2分割                
                return SplitRedCube(cube, (cube.MinRed + cube.MaxRed) / 2f);
            }
            else if (cube.LengthMax == cube.LengthGreen)
            {
                return SplitGreenCube(cube, (cube.MinGreen + cube.MaxGreen) / 2f);
            }
            else
            {
                return SplitBlueCube(cube, (cube.MinBlue + cube.MaxBlue) / 2f);
            }
        }

        //中央値で2分割、メディアンカット
        //辺の選択は長辺、RGB同じだった場合はRGBの順で優先
        private List<Cube> SplitByMedian中央値(Cube cube)
        {
            float mid;
            List<byte> list = new List<byte>();
            if (cube.LengthMax == cube.LengthRed)
            {
                for (int i = 0; i < cube.AllPixelsColor.Count; ++i)
                {
                    list.Add(cube.AllPixelsColor[i].R);
                }
                mid = GetMedian(list);
                return SplitRedCube(cube, mid);
            }
            else if (cube.LengthMax == cube.LengthGreen)
            {
                for (int i = 0; i < cube.AllPixelsColor.Count; ++i)
                {
                    list.Add(cube.AllPixelsColor[i].G);
                }
                mid = GetMedian(list);
                return SplitGreenCube(cube, mid);
            }
            else
            {
                for (int i = 0; i < cube.AllPixelsColor.Count; ++i)
                {
                    list.Add(cube.AllPixelsColor[i].B);
                }
                mid = GetMedian(list);
                return SplitBlueCube(cube, mid);
            }
        }
        private float GetMedian(List<byte> list)
        {
            list.Sort();
            int lCount = list.Count;
            if (lCount % 2 == 0)
            {
                return (list[lCount / 2] + list[lCount / 2 - 1]) / 2f;
            }
            else { return list[(lCount - 1) / 2]; }
        }
        //ここまでメディアン

        /// <summary>
        /// 赤の辺でCubeを分割して返す
        /// </summary>
        /// <param name="cube">分割するCube</param>
        /// <param name="mid">分割の閾値、これ未満とこれ以上で分ける</param>
        /// <returns></returns>
        private List<Cube> SplitRedCube(Cube cube, float mid)
        {
            //List<Color> low = new List<Color>();
            //List<Color> high = new List<Color>();
            //foreach (Color item in cube.AllPixelsColor)
            //{
            //    if (item.R < mid) { low.Add(item); }
            //    else { high.Add(item); }
            //}
            //なんかパラレルのほうが遅い…
            var low = new ConcurrentBag<Color>();
            var high = new ConcurrentBag<Color>();
            Parallel.ForEach(cube.AllPixelsColor, item =>
            {
                if (item.R < mid) { low.Add(item); }
                else { high.Add(item); }
            });
            return new List<Cube>() { new Cube(low.ToList()), new Cube(high.ToList()) };
        }

        private List<Cube> SplitGreenCube(Cube cube, float mid)
        {
            //List<Color> low = new List<Color>();
            //List<Color> high = new List<Color>();
            //foreach (Color item in cube.AllPixelsColor)
            //{
            //    if (item.G < mid) { low.Add(item); }
            //    else { high.Add(item); }
            //}
            var low = new ConcurrentBag<Color>();
            var high = new ConcurrentBag<Color>();
            Parallel.ForEach(cube.AllPixelsColor, item =>
            {
                if (item.G < mid) { low.Add(item); }
                else { high.Add(item); }
            });
            return new List<Cube>() { new Cube(low.ToList()), new Cube(high.ToList()) };
        }
        private List<Cube> SplitBlueCube(Cube cube, float mid)
        {
            //List<Color> low = new List<Color>();
            //List<Color> high = new List<Color>();
            //foreach (Color item in cube.AllPixelsColor)
            //{
            //    if (item.B < mid) { low.Add(item); }
            //    else { high.Add(item); }
            //}
            var low = new ConcurrentBag<Color>();
            var high = new ConcurrentBag<Color>();
            Parallel.ForEach(cube.AllPixelsColor, item =>
            {
                if (item.B < mid) { low.Add(item); }
                else { high.Add(item); }
            });
            return new List<Cube>() { new Cube(low.ToList()), new Cube(high.ToList()) };
        }


        //最大分散辺を最小分散になるように分割、ピクセル数考慮版
        private List<Cube> SplitByMinVariancePixel(Cube cube)
        {
            if (double.IsNaN(cube.VarianceMaxFromPixel)) { GetRGBごとの分散Pixel(cube); }

            List<int> iList = new List<int>();
            List<Cube> cubeList = new List<Cube>();
            //赤が最大分散辺のとき
            if (cube.VarianceMaxFromPixel == cube.VarianceRedFromPixel)
            {
                iList = GetNoJuuhuku(cube.AllColor, "r");//昇順リスト取得
                cubeList = SplitRedCube(cube, GetMidFromVariance(iList));
            }
            else if (cube.VarianceMaxFromPixel == cube.VarianceGreenFromPixel)
            {
                iList = GetNoJuuhuku(cube.AllColor, "g");
                cubeList = SplitGreenCube(cube, GetMidFromVariance(iList));
            }
            else if (cube.VarianceMaxFromPixel == cube.VarianceBlueFromPixel)
            {
                iList = GetNoJuuhuku(cube.AllColor, "b");
                cubeList = SplitBlueCube(cube, GetMidFromVariance(iList));
            }
            return cubeList;
            //E:\オレ\エクセル\C#.xlsm_色の距離_$C$358
            ////cList = new List<int> { 24, 52, 190, 193, 226, 231, 247, 248, 250 };
        }
        //最大分散辺を最小分散になるように分割、ピクセル数無視版
        private List<Cube> SplitByMinVariance(Cube cube)
        {
            if (double.IsNaN(cube.VarianceMax)) { GetRGBごとの分散(cube); }

            List<int> iList = new List<int>();
            List<Cube> cubeList = new List<Cube>();
            //赤が最大分散辺のとき
            if (cube.VarianceMax == cube.VarianceRed)
            {
                iList = GetNoJuuhuku(cube.AllColor, "r");//昇順リスト取得
                cubeList = SplitRedCube(cube, GetMidFromVariance(iList));
            }
            else if (cube.VarianceMax == cube.VarianceGreen)
            {
                iList = GetNoJuuhuku(cube.AllColor, "g");
                cubeList = SplitGreenCube(cube, GetMidFromVariance(iList));
            }
            else if (cube.VarianceMax == cube.VarianceBlue)
            {
                iList = GetNoJuuhuku(cube.AllColor, "b");
                cubeList = SplitBlueCube(cube, GetMidFromVariance(iList));
            }
            return cubeList;
        }

        //昇順リストを2分割する時、分散後が最小分散になる閾値を返す
        private int GetMidFromVariance(List<int> iList)
        {
            //分割する位置を1つづつ増やす
            //分散後の2つ(foreList、backList)の分散値を合計
            //合計値が一番少なくなる場所(index)を特定
            //その場所になる値を返す
            double foreList, bakcList, min = double.MaxValue;
            int index = 0;
            for (int i = 2; i < iList.Count; ++i)
            {
                foreList = GetVariance(iList.GetRange(0, i));//分散取得
                bakcList = GetVariance(iList.GetRange(i, iList.Count - i));
                //最小分散になる場所を特定する
                if (min > foreList + bakcList)
                {
                    min = foreList + bakcList;
                    index = i;
                }
            }
            return iList[index];
        }

        //分散を取得
        private double GetVariance(List<int> list)
        {
            var ave = list.Average();
            double variance = 0;
            for (int i = 0; i < list.Count; ++i)
            {
                variance += Math.Pow(list[i] - ave, 2f);
            }
            variance /= list.Count;
            return variance;
        }
        //ColorListから重複なしの昇順リスト、RGBどれかを指定
        private List<int> GetNoJuuhuku(List<Color> list, string rgb)
        {
            var sList = new SortedSet<int>();
            for (int i = 0; i < list.Count; ++i)
            {
                try
                {
                    if (rgb == "r") { sList.Add(list[i].R); }
                    else if (rgb == "g") { sList.Add(list[i].G); }
                    else if (rgb == "b") { sList.Add(list[i].B); }
                }
                catch (Exception)
                {
                }
            }
            return sList.ToList();
        }

        #endregion




        #region Cubeの色取得


        //平均色、Cubeの中心の色じゃなくてピクセルの平均
        public Color GetLeaderColor1Average平均色(Cube cube)
        {
            if (cube.AllPixelsColor.Count == 0)
            {
                return Color.FromRgb(127, 127, 127);
            }

            return Color.FromRgb((byte)cube.RedPixAverage, (byte)cube.GreenPixAverage, (byte)cube.BluePixAverage);
        }


        //Cubeの中心の色を返す、面白い
        public Color GetLeaderColor2Core中心色(Cube cube)
        {
            byte r = (byte)((cube.MaxRed - cube.MinRed) / 2 + cube.MinRed);
            byte g = (byte)((cube.MaxGreen - cube.MinGreen) / 2 + cube.MinGreen);
            byte b = (byte)((cube.MaxBlue - cube.MinBlue) / 2 + cube.MinBlue);
            return Color.FromRgb(r, g, b);
        }

        //CubeのピクセルのうちRGB空間の中心から一番遠いピクセルの色
        private Color GetLeaderColor3DistantRGBCore(Cube cube)
        {
            double distance;
            double max = 0;

            Color distantColor = Colors.Black;
            foreach (Color item in cube.AllColor)
            {
                distance = GetColorDistance(127.5, 127.5, 127.5, item);
                if (max < distance)
                {
                    max = distance;
                    distantColor = item;
                }
            }
            return distantColor;
        }


        //Cubeの8隅のうちRGB空間の中心から一番遠い隅
        private Color GetLeaderColor4DistantRGBCoreVertex(Cube cube)
        {
            double distance;
            double max = 0;
            int lIndex = 0;
            var corner8 = new Color[]//8隅の色
            {
                Color.FromRgb(cube.MinRed,cube.MinGreen,cube.MinBlue),
                Color.FromRgb(cube.MinRed,cube.MinGreen,cube.MaxBlue),
                Color.FromRgb(cube.MinRed,cube.MaxGreen,cube.MinBlue),
                Color.FromRgb(cube.MaxRed,cube.MinGreen,cube.MinBlue),
                Color.FromRgb(cube.MinRed,cube.MaxGreen,cube.MaxBlue),
                Color.FromRgb(cube.MaxRed,cube.MinGreen,cube.MaxBlue),
                Color.FromRgb(cube.MaxRed,cube.MaxGreen,cube.MinBlue),
                Color.FromRgb(cube.MaxRed,cube.MaxGreen,cube.MaxBlue),
            };

            for (int i = 0; i < corner8.Length; ++i)
            {
                distance = GetColorDistance(127.5, 127.5, 127.5, corner8[i]);
                if (max < distance)
                {
                    max = distance;
                    lIndex = i;
                }
            }
            return corner8[lIndex];
        }


        //Cubeの中心から一番遠いピクセルの色
        private Color GetLeaderColor5DistantCore(Cube cube)
        {
            float rCore = (cube.MaxRed - cube.MinRed) / 2f;
            float gCore = (cube.MaxGreen - cube.MinGreen) / 2f;
            float bCore = (cube.MaxBlue - cube.MinBlue) / 2f;
            double distance;
            double max = 0;
            //int lIndex = 0;
            //for (int i = 0; i < cube.AllPixelsColor.Count; ++i)
            //{
            //    distance = GetColorDistance(rCore, gCore, bCore, cube.AllPixelsColor[i]);
            //    if (max < distance)
            //    {
            //        max = distance;
            //        lIndex = i;
            //    }
            //}
            //return cube.AllPixelsColor[lIndex];

            Color distantColor = Colors.Black;
            foreach (Color item in cube.AllColor)
            {
                distance = GetColorDistance(rCore, gCore, bCore, item);
                if (max < distance)
                {
                    max = distance;
                    distantColor = item;
                }
            }
            return distantColor;
        }


        //RGBそれぞれの中央値(メディアン)
        private Color GetLeaderColor6RGBMedian(Cube cube)
        {
            var r = new List<byte>();
            var g = new List<byte>();
            var b = new List<byte>();

            foreach (Color item in cube.AllPixelsColor)
            {
                r.Add(item.R);
                g.Add(item.G);
                b.Add(item.B);
            }
            r.Sort();
            g.Sort();
            b.Sort();

            if (r.Count % 2 == 0)
            {
                int back = r.Count / 2;
                return Color.FromRgb(
                    (byte)((r[back] + r[back - 1]) / 2),
                    (byte)((g[back] + g[back - 1]) / 2),
                    (byte)((b[back] + b[back - 1]) / 2));
            }
            else
            {
                int median = (cube.AllPixelsColor.Count - 1) / 2;
                return Color.FromRgb(r[median], g[median], b[median]);
            }
        }

        #endregion




        private void ButtonSaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            SaveImage((BitmapSource)MyImage.Source);
        }

        //PixelFormatを色数に合わせたものに変更
        private BitmapSource OptimisationPixelFormat(BitmapSource source, int colorCount)
        {
            PixelFormat pixelFormat;
            if (colorCount <= 2)
            {
                pixelFormat = PixelFormats.Indexed1;
            }
            else if (colorCount <= 4)
            {
                pixelFormat = PixelFormats.Indexed2;
            }
            else if (colorCount <= 16)
            {
                pixelFormat = PixelFormats.Indexed4;
            }
            else if (colorCount <= 256)
            {
                pixelFormat = PixelFormats.Indexed8;
            }
            else
            {
                pixelFormat = PixelFormats.Bgr24;
            }
            return new FormatConvertedBitmap(source, pixelFormat, null, 0);
        }

        private void SaveImage(BitmapSource source)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.bmp|*.bmp|*.tiff|*.tiff";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(ImageFileFullPath) + "_";
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(ImageFileFullPath);
            if (saveFileDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                if (saveFileDialog.FilterIndex == 1)
                {
                    encoder = new PngBitmapEncoder();
                }
                else if (saveFileDialog.FilterIndex == 2)
                {
                    encoder = new BmpBitmapEncoder();
                }
                else if (saveFileDialog.FilterIndex == 3)
                {
                    encoder = new TiffBitmapEncoder();
                }
                encoder.Frames.Add(BitmapFrame.Create(source));

                using (var fs = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    encoder.Save(fs);
                }
            }
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            OriginBitmap = GetBitmapSourceWithChangePixelFormat2(filePath[0], PixelFormats.Pbgra32, 96, 96);

            if (OriginBitmap == null)
            {
                MessageBox.Show("not Image");
            }
            else
            {
                MyImage.Source = OriginBitmap;
                ImageOrigin.Source = OriginBitmap;
                ImageFileFullPath = filePath[0];
                if (OriginBitmap.PixelWidth < 1024)
                {
                    TextBlockPixelsCount.Text = $" 画像の使用色数：{GetColorCountC1(OriginBitmap)}";
                }
                else
                {
                    TextBlockPixelsCount.Text = $" 画像の使用色数：{GetColorCountA1(OriginBitmap)}";
                }

                TextBlockImageSize.Text = $"画像サイズ：{OriginBitmap.PixelWidth}x{OriginBitmap.PixelHeight}";
                if (OriginBitmap.PixelHeight * OriginBitmap.PixelWidth < 100000)
                {
                    RadioPixelsAll.IsChecked = true;//ピクセル数が10万以下なら走査ピクセル全部にチェック
                }
            }
        }

        //private int GetColorCount(BitmapSource source)
        //{
        //List<Color> cl = GetColorList(source);
        //var bag = new ConcurrentDictionary<Color, int>();
        //try
        //{
        //    Parallel.ForEach(cl, item =>
        //    {
        //        bag.GetOrAdd(item, 1);
        //    });
        //}
        //catch (Exception)
        //{
        //}
        //return bag.Count;

        //↑時間かかりすぎ

        //List<Color> cl = GetColorList(source);
        //return cl.Distinct().ToArray().Length;//これも同じくらい時間かかるけど簡潔なのがいい
        //}
        /// <summary>
        /// 画像の使用色数取得、横ピクセル数が1000以上の時に有効
        /// </summary>
        /// <param name="source">PixelFormat.Pbgra32限定</param>
        /// <returns></returns>
        private int GetColorCountA1(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int[] iColor = new int[256 * 256 * 256];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                iColor[pixels[i] * 256 * 256 + pixels[i + 1] * 256 + pixels[i + 2]]++;
            }

            int colorCount = 0;
            for (int i = 0; i < iColor.Length; ++i)
            {
                if (iColor[i] != 0)
                {
                    colorCount++;
                }
            }
            return colorCount;
        }
        /// <summary>
        /// 画像の使用色数取得、横ピクセル数が1000未満時に有効
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private int GetColorCountC1(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            var list = new HashSet<int>();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                list.Add(pixels[i] * 256 * 256 + pixels[i + 1] * 256 + pixels[i + 2]);
            }
            return list.Count;
        }
        /// <summary>
        ///  ファイルパスとPixelFormatを指定してBitmapSourceを取得、dpiの変更は任意
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsの中からどれかを指定</param>
        /// <param name="dpiX">無指定なら画像ファイルで指定されているdpiになる</param>
        /// <param name="dpiY">無指定なら画像ファイルで指定されているdpiになる</param>
        /// <returns></returns>
        private BitmapSource GetBitmapSourceWithChangePixelFormat2(
            string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
        {
            BitmapSource source = null;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var bf = BitmapFrame.Create(fs);
                    var convertedBitmap = new FormatConvertedBitmap(bf, pixelFormat, null, 0);
                    int w = convertedBitmap.PixelWidth;
                    int h = convertedBitmap.PixelHeight;
                    int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                    byte[] pixels = new byte[h * stride];
                    convertedBitmap.CopyPixels(pixels, stride, 0);
                    //dpi指定がなければ元の画像と同じdpiにする
                    if (dpiX == 0) { dpiX = bf.DpiX; }
                    if (dpiY == 0) { dpiY = bf.DpiY; }
                    //dpiを指定してBitmapSource作成
                    source = BitmapSource.Create(
                        w, h, dpiX, dpiY,
                        convertedBitmap.Format,
                        convertedBitmap.Palette, pixels, stride);
                };
            }
            catch (Exception) { }
            return source;
        }
    }


    /// <summary>
    /// RGBそれぞれの最小値と最大値を持つクラス
    /// その他にRGBそれぞれの辺の長さ、平均値、画像の全ピクセルの色、使用されている色数を持つ
    /// RGBそれぞれの分散値はNaNを入れておいて必要な時に入れる
    /// コンストラクタはbitmapSourceかColorのListから作成するものだけ、それだけのクラス
    /// </summary>
    public class Cube
    {
        public byte MinRed;//最小R
        public byte MinGreen;
        public byte MinBlue;
        public byte MaxRed;//最大赤
        public byte MaxGreen;
        public byte MaxBlue;
        public float RedPixAverage;//ピクセル数を加味した平均赤
        public float GreenPixAverage;
        public float BluePixAverage;
        public double VarianceRedFromPixel;//赤の分散、ピクセル数考慮
        public double VarianceGreenFromPixel;
        public double VarianceBlueFromPixel;
        public double VarianceMaxFromPixel;
        public double VarianceRed;//赤の分散、ピクセル数無視
        public double VarianceGreen;
        public double VarianceBlue;
        public double VarianceMax;
        public List<Color> AllPixelsColor;//すべてのピクセルの色リスト        
        public List<Color> AllColor;//使用されているすべての色リスト
        public int LengthMax;//Cubeの最大辺長
        public int LengthRed;//赤の辺長
        public int LengthGreen;
        public int LengthBlue;

        ////BitmapSourceからCubeを作成
        //public Cube(BitmapSource source)
        //{
        //    var bitmap = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
        //    var wb = new WriteableBitmap(bitmap);
        //    int h = wb.PixelHeight;
        //    int w = wb.PixelWidth;
        //    int stride = wb.BackBufferStride;
        //    byte[] pixels = new byte[h * stride];
        //    wb.CopyPixels(pixels, stride, 0);
        //    long p = 0;

        //    var ColorList = new List<Color>();
        //    for (int y = 0; y < h; ++y)
        //    {
        //        for (int x = 0; x < w; ++x)
        //        {
        //            p = y * stride + (x * 4);
        //            ColorList.Add(Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]));
        //        }
        //    }
        //    new Cube(ColorList);//これだと何も入らない空っぽ

        //}


        //ColorのリストからCube作成
        public Cube(List<Color> color)
        {
            byte lR = 255, lG = 255, lB = 255, hR = 0, hG = 0, hB = 0;
            byte cR, cG, cB;
            long rAdd = 0, gAdd = 0, bAdd = 0;
            AllPixelsColor = new List<Color>();
            foreach (Color item in color)
            {
                cR = item.R; cG = item.G; cB = item.B;
                rAdd += cR; gAdd += cG; bAdd += cB;
                AllPixelsColor.Add(Color.FromRgb(cR, cG, cB));
                if (lR > cR) { lR = cR; }
                if (lG > cG) { lG = cG; }
                if (lB > cB) { lB = cB; }
                if (hR < cR) { hR = cR; }
                if (hG < cG) { hG = cG; }
                if (hB < cB) { hB = cB; }
            }
            //重複を除いて使用されている色リスト作成、Distinct
            AllColor = AllPixelsColor.Distinct().ToList();//これの処理コストが結構大きい？

            MinRed = lR; MinGreen = lG; MinBlue = lB;
            MaxRed = hR; MaxGreen = hG; MaxBlue = hB;
            LengthRed = 1 + MaxRed - MinRed;
            LengthGreen = 1 + MaxGreen - MinGreen;
            LengthBlue = 1 + MaxBlue - MinBlue;
            LengthMax = Math.Max(LengthRed, Math.Max(LengthGreen, LengthBlue));
            //平均値
            float count = color.Count;
            RedPixAverage = rAdd / count;
            GreenPixAverage = gAdd / count;
            BluePixAverage = bAdd / count;
            //分散はとりあえず非数を入れておく
            VarianceRedFromPixel = double.NaN;
            VarianceGreenFromPixel = double.NaN;
            VarianceBlueFromPixel = double.NaN;
            VarianceMaxFromPixel = double.NaN;
            VarianceRed = double.NaN;
            VarianceGreen = double.NaN;
            VarianceBlue = double.NaN;
            VarianceMax = double.NaN;
        }

    }


    /// <summary>
    /// パレット表示用のBorderの配列の管理
    /// </summary>
    public class MyWrapPanel : WrapPanel
    {
        public List<Color> Palette { set; get; }
        private Border[] Pan;
        private int MaxPanCount;

        public MyWrapPanel(int max)
        {
            MaxPanCount = max;
            Pan = new Border[max];
            Border border;
            for (int i = 0; i < Pan.Length; i++)
            {
                border = new Border()
                {
                    Width = 18,
                    Height = 18,
                    BorderBrush = new SolidColorBrush(Colors.AliceBlue),
                    BorderThickness = new Thickness(1f),
                    //Margin = new Thickness(1f,0,0,0),
                    //VerticalAlignment = VerticalAlignment.Center
                };
                Pan[i] = border;
                this.Children.Add(border);
            }
            Palette = new List<Color>();
            VerticalAlignment = VerticalAlignment.Center;
        }

        public void SetColorList(List<Color> listColor)
        {
            ClearColor();
            int cc = (MaxPanCount < listColor.Count) ? MaxPanCount : listColor.Count;
            for (int i = 0; i < cc; ++i)
            {
                Pan[i].Background = new SolidColorBrush(listColor[i]);
            }
            for (int i = 0; i < listColor.Count; ++i)
            {
                Palette.Add(listColor[i]);
            }
        }

        public void ClearColor()
        {
            Palette.Clear();
            foreach (var item in Pan)
            {
                item.Background = null;
            }
        }
    }
}
