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


namespace _20180315_メディアンカット法での色の選び方
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;
        //Border[] MyPalettePan1;
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
        //public enum SplitPriority//分割対象の選択方法
        //{
        //    ManyPixels,//ピクセル数が多いCubeを優先選択
        //    LongSide,//最大長辺を持つCubeを優先選択
        //}
        //public enum ColorSelsection//Cubeの代表色の選択法
        //{
        //    Average,//全体のピクセルの平均色、Cubeの平均色ではない
        //    Core,//中心の色
        //    DistantCore,//中心から最も遠い色
        //    DistantCoreVertex,//中心から最も遠い隅
        //}

        //List<Color> Palette1;

        const int MAX_PALETTE_COLOR_COUNT = 256;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

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

            ButtonReduceColor.Click += ButtonReduceColor_Click;//減色変換
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

        }

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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            CreatePaletteAll();
            stopwatch.Stop();
            TextBlockTime.Text = $"作成時間：{stopwatch.Elapsed.Minutes}分{stopwatch.Elapsed.Seconds}秒{stopwatch.Elapsed.Milliseconds.ToString("000")}";
        }

        private void CreatePaletteAll()
        {
            if (OriginBitmap == null) { return; }

            ButtonCreatePalette.Content = "パレット作成中…";
            var bc = ButtonCreatePalette.Foreground;
            //ButtonCreatePalette.Background = new SolidColorBrush(Colors.Orange);
            ButtonCreatePalette.Foreground = new SolidColorBrush(Colors.Red);
            DoEvents();

            Func<Cube, List<Cube>> FuncSplitBy;
            if (RadioDivideCenter.IsChecked == true) { FuncSplitBy = SplitByLongSide辺の中央; }
            else { FuncSplitBy = SplitByMedian中央値; }

            Func<List<Cube>, int> FuncGetIndexSelect;
            if (RadioSelectLongSide.IsChecked == true)
            {
                FuncGetIndexSelect = GetSelectIndexLongSideCube;
            }
            else if (RadioSelectManyPixels.IsChecked == true)
            {
                FuncGetIndexSelect = GetIndexSelectManyPidxelsCube;
            }
            else
            {
                FuncGetIndexSelect = GetSelectIndexCapacityMaxCube;
            }

            var Pans = new MyWrapPanel[] { Pan1, Pan2, Pan3, Pan4, Pan5, Pan6 };
            var colorMethod = new Func<Cube, Color>[]
            {
                GetColorCubeAverage平均色,
                GetColorCubeMedian,
                GetColorCubeCore中心色,
                GetColorCubeDistantRGBCore,
                GetColorCubeDistantVertexRGBCore,
                GetColorCubeDistantCore                
                
                //GetColorCubeVertexDistantCore
            };

            for (int i = 0; i < Pans.Length; ++i)
            {
                DoEvents();
                if (RadioPixels100k.IsChecked == true)
                {
                    Pans[i].SetColorList(GetColorListByCubeList(SplitCube(GetPixelsColorOfNumber(OriginBitmap, 100000), (int)NumericScrollBar.Value,
                        FuncGetIndexSelect, FuncSplitBy), colorMethod[i]));
                }
                else if (RadioPixelsMillion.IsChecked == true)
                {
                    Pans[i].SetColorList(GetColorListByCubeList(SplitCube(GetPixelsColorOfNumber(OriginBitmap, 1000000), (int)NumericScrollBar.Value,
                        FuncGetIndexSelect, FuncSplitBy), colorMethod[i]));
                }
                else
                {
                    Pans[i].SetColorList(GetColorListByCubeList(SplitCube(new Cube(OriginBitmap), (int)NumericScrollBar.Value,
                        FuncGetIndexSelect, FuncSplitBy), colorMethod[i]));
                }
            }

            ButtonCreatePalette.Content = "パレット作成";
            ButtonCreatePalette.Foreground = bc;
            //Pan1.SetColorList(
            //    GetColorListByCubeList(
            //        SplitCube(
            //            OriginBitmap,
            //            (int)NumericScrollBar.Value,
            //            FuncGetIndexSelect,
            //            FuncSplitBy),
            //        GetColorCubeAverage平均色));

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
            MyImage.Source = ReduceColor一覧表方式(OriginBitmap, Pan6.Palette);
        }

        private void ButtonReduceColor5_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ReduceColor一覧表方式(OriginBitmap, Pan5.Palette);
        }

        private void ButtonReduceColor4_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ReduceColor一覧表方式(OriginBitmap, Pan4.Palette);
        }

        private void ButtonReduceColor3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ReduceColor一覧表方式(OriginBitmap, Pan3.Palette);
        }

        private void ButtonReduceColor2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            //MyImage.Source = ReduceColor指定色で減色(OriginBitmap, Pan2.Palette);
            MyImage.Source = ReduceColor一覧表方式(OriginBitmap, Pan2.Palette);
        }

        private void ButtonReduceColor_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            //MyImage.Source = ReduceColor指定色で減色(OriginBitmap, Pan1.Palette);
            MyImage.Source = ReduceColor一覧表方式(OriginBitmap, Pan1.Palette);
            //一覧表方式じゃないと大きな画像、パレット多色でものすごく時間がかかる、2048ｘ1536でパレット200色だと1分以上2分位かかる
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

        private ConcurrentDictionary<Color, Color> GetList一覧表Test1(byte[] pixels, List<Color> palette)
        {
            int[] iColor = new int[256 * 256 * 256];
            int id;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                id = pixels[i] * 256 * 256 + pixels[i + 1] * 256 + pixels[i + 2];
                iColor[id]++;
            }

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
            int[] iColor = new int[256 * 256 * 256];
            int id;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                id = pixels[i] * 256 * 256 + pixels[i + 1] * 256 + pixels[i + 2];
                iColor[id]++;
            }

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
            int loopCount = 1, splitZeroCount = 0;
            var cubeList = new List<Cube>() { cube };
            var tempCubeList = new List<Cube>();
            int index;
            while (splitCount > loopCount && splitZeroCount < 3)
            {
                // どのCubeを分割するのか選定(最大長辺or最大ピクセル数)
                index = GetIndexOfSplitCube(cubeList);

                //分割してリストに追加
                tempCubeList.Clear();
                tempCubeList.AddRange(SplitBy(cubeList[index]));
                if (tempCubeList[0].AllPixelsColor.Count == 0 || tempCubeList[1].AllPixelsColor.Count == 0)
                {
                    splitZeroCount++;
                    //cubeList.AddRange(tempCubeList);
                    //cubeList.RemoveAt(index);
                    //break;
                }
                cubeList.AddRange(tempCubeList);
                //分割のもとになったCubeをリストから削除
                cubeList.RemoveAt(index);
                //if (tempCubeList[0].AllPixelsColor.Count != 0) { cubeList.Add(tempCubeList[0]); }
                //if (tempCubeList[1].AllPixelsColor.Count != 0) { cubeList.Add(tempCubeList[1]); }

                loopCount++;
            }
            return cubeList;
        }
        private List<Cube> SplitCube(List<Color> listColors,
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



        #region 分割、辺の中央or長辺にある中央値


        //一番長い辺で2分割
        //RGB同じだった場合はRGBの順で優先
        public List<Cube> SplitByLongSide辺の中央(Cube cube)
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
        public List<Cube> SplitByMedian中央値(Cube cube)
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
        #endregion




        #region Cubeの色取得


        //平均色、Cubeの中心の色じゃなくてピクセルの平均
        public Color GetColorCubeAverage平均色(Cube cube)
        {
            List<Color> colorList = cube.AllPixelsColor;
            long r = 0, g = 0, b = 0;
            int cCount = colorList.Count;
            if (cCount == 0)
            {
                return Color.FromRgb(127, 127, 127);
            }

            for (int i = 0; i < cCount; ++i)
            {
                r += colorList[i].R;
                g += colorList[i].G;
                b += colorList[i].B;
            }
            return Color.FromRgb((byte)(r / cCount), (byte)(g / cCount), (byte)(b / cCount));
        }


        //Cubeの中心の色を返す、面白い
        public Color GetColorCubeCore中心色(Cube cube)
        {
            byte r = (byte)((cube.MaxRed - cube.MinRed) / 2 + cube.MinRed);
            byte g = (byte)((cube.MaxGreen - cube.MinGreen) / 2 + cube.MinGreen);
            byte b = (byte)((cube.MaxBlue - cube.MinBlue) / 2 + cube.MinBlue);
            return Color.FromRgb(r, g, b);
        }

        //CubeのピクセルのうちRGB空間の中心から一番遠いピクセルの色
        private Color GetColorCubeDistantRGBCore(Cube cube)
        {
            double distance;
            double max = 0;
            //int lIndex = 0;
            //for (int i = 0; i < cube.AllPixelsColor.Count; ++i)
            //{
            //    distance = GetColorDistance(127.5, 127.5, 127.5, cube.AllPixelsColor[i]);
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
        private Color GetColorCubeDistantVertexRGBCore(Cube cube)
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
        private Color GetColorCubeDistantCore(Cube cube)
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
        ////Cubeの8隅のうちCubeの中心から一番遠い隅
        //private Color GetColorCubeVertexDistantCore(Cube cube)
        //{
        //    double distance;
        //    double max = 0;
        //    int lIndex = 0;
        //    Color coreColor = GetColorCubeCore中心色(cube);//Cubeの中心色
        //    var corner8 = new Color[]//8隅の色
        //    {
        //        Color.FromRgb(cube.MinRed,cube.MinGreen,cube.MinBlue),
        //        Color.FromRgb(cube.MinRed,cube.MinGreen,cube.MaxBlue),
        //        Color.FromRgb(cube.MinRed,cube.MaxGreen,cube.MinBlue),
        //        Color.FromRgb(cube.MaxRed,cube.MinGreen,cube.MinBlue),
        //        Color.FromRgb(cube.MinRed,cube.MaxGreen,cube.MaxBlue),
        //        Color.FromRgb(cube.MaxRed,cube.MinGreen,cube.MaxBlue),
        //        Color.FromRgb(cube.MaxRed,cube.MaxGreen,cube.MinBlue),
        //        Color.FromRgb(cube.MaxRed,cube.MaxGreen,cube.MaxBlue),
        //    };

        //    for (int i = 0; i < corner8.Length; ++i)
        //    {
        //        distance = GetColorDistance(coreColor, corner8[i]);
        //        if (max < distance)
        //        {
        //            max = distance;
        //            lIndex = i;
        //        }
        //    }
        //    return corner8[lIndex];
        //}

        //RGBそれぞれの中央値(メディアン)
        private Color GetColorCubeMedian(Cube cube)
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

        //パレットの色表示用のBorder作成
        private Border[] CreateBorders(Panel panel)
        {
            NumericScrollBar.Maximum = MAX_PALETTE_COLOR_COUNT;
            var pan = new Border[MAX_PALETTE_COLOR_COUNT];
            Border border;
            for (int i = 0; i < pan.Length; i++)
            {
                border = new Border()
                {
                    Width = 18,
                    Height = 18,
                    BorderBrush = new SolidColorBrush(Colors.AliceBlue),
                    BorderThickness = new Thickness(1f),
                    Margin = new Thickness(1f)
                };
                pan[i] = border;
                panel.Children.Add(border);
            }
            panel.Tag = new List<Color>();
            return pan;
        }


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
            }
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
    /// その他にRGBそれぞれの辺の長さ、画像の全ピクセルの色、使用されている色を持つ
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
        public List<Color> AllPixelsColor;//すべてのピクセルの色リスト        
        public List<Color> AllColor;//使用されているすべての色リスト
        public int LengthMax;//Cubeの最大辺長
        public int LengthRed;//赤の辺長
        public int LengthGreen;
        public int LengthBlue;

        //BitmapSourceからCubeを作成
        public Cube(BitmapSource source)
        {
            var bitmap = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
            var wb = new WriteableBitmap(bitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;
            byte cR, cG, cB;
            byte lR = 255, lG = 255, lB = 255, hR = 0, hG = 0, hB = 0;
            AllPixelsColor = new List<Color>();
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    cR = pixels[p + 2]; cG = pixels[p + 1]; cB = pixels[p];
                    AllPixelsColor.Add(Color.FromRgb(cR, cG, cB));
                    if (lR > cR) { lR = cR; }
                    if (lG > cG) { lG = cG; }
                    if (lB > cB) { lB = cB; }
                    if (hR < cR) { hR = cR; }
                    if (hG < cG) { hG = cG; }
                    if (hB < cB) { hB = cB; }
                }
            }
            //重複を除いて使用されている色リスト作成、Distinct
            //IEnumerable<Color> result = AllPixelsColor.Distinct();
            AllColor = AllPixelsColor.Distinct().ToList();//これの処理コストが結構大きい

            MinRed = lR; MinGreen = lG; MinBlue = lB;
            MaxRed = hR; MaxGreen = hG; MaxBlue = hB;
            LengthRed = 1 + MaxRed - MinRed;
            LengthGreen = 1 + MaxGreen - MinGreen;
            LengthBlue = 1 + MaxBlue - MinBlue;
            LengthMax = Math.Max(LengthRed, Math.Max(LengthGreen, LengthBlue));
        }

        //ColorのリストからCube作成
        public Cube(List<Color> color)
        {
            byte lR = 255, lG = 255, lB = 255, hR = 0, hG = 0, hB = 0;
            byte cR, cG, cB;
            AllPixelsColor = new List<Color>();
            foreach (Color item in color)
            {
                cR = item.R; cG = item.G; cB = item.B;
                AllPixelsColor.Add(Color.FromRgb(cR, cG, cB));
                if (lR > cR) { lR = cR; }
                if (lG > cG) { lG = cG; }
                if (lB > cB) { lB = cB; }
                if (hR < cR) { hR = cR; }
                if (hG < cG) { hG = cG; }
                if (hB < cB) { hB = cB; }
            }
            //重複を除いて使用されている色リスト作成、Distinct
            AllColor = AllPixelsColor.Distinct().ToList();//これの処理コストが結構大きい

            MinRed = lR; MinGreen = lG; MinBlue = lB;
            MaxRed = hR; MaxGreen = hG; MaxBlue = hB;
            LengthRed = 1 + MaxRed - MinRed;
            LengthGreen = 1 + MaxGreen - MinGreen;
            LengthBlue = 1 + MaxBlue - MinBlue;
            LengthMax = Math.Max(LengthRed, Math.Max(LengthGreen, LengthBlue));
        }

        //#region 分割


        ////一番長い辺で2分割
        ////RGB同じだった場合はRGBの順で優先
        //public List<Cube> SplitByLongSide()
        //{
        //    if (LengthMax == LengthRed)
        //    {//Rの辺が最長の場合、R要素の中間で2分割                
        //        return SplitRedCube((MinRed + MaxRed) / 2f);
        //    }
        //    else if (LengthMax == LengthGreen)
        //    {
        //        return SplitGreenCube((MinGreen + MaxGreen) / 2f);
        //    }
        //    else
        //    {
        //        return SplitBlueCube((MinBlue + MaxBlue) / 2f);
        //    }
        //}

        ////中央値で2分割
        ////辺の選択は長辺、RGB同じだった場合はRGBの順で優先
        //public List<Cube> SplitByMedian()
        //{
        //    float mid;
        //    List<byte> list = new List<byte>();
        //    if (LengthMax == LengthRed)
        //    {
        //        for (int i = 0; i < this.ListColors.Count; ++i)
        //        {
        //            list.Add(ListColors[i].R);
        //        }
        //        mid = GetMedian(list);
        //        return SplitRedCube(mid);
        //    }
        //    else if (LengthMax == LengthGreen)
        //    {
        //        for (int i = 0; i < this.ListColors.Count; ++i)
        //        {
        //            list.Add(ListColors[i].G);
        //        }
        //        mid = GetMedian(list);
        //        return SplitGreenCube(mid);
        //    }
        //    else
        //    {
        //        for (int i = 0; i < this.ListColors.Count; ++i)
        //        {
        //            list.Add(ListColors[i].B);
        //        }
        //        mid = GetMedian(list);
        //        return SplitBlueCube(mid);
        //    }
        //}

        //private float GetMedian(List<byte> list)
        //{
        //    int lCount = list.Count;
        //    if (list.Count % 2 == 0)
        //    {
        //        return (list[list.Count / 2] + list[list.Count / 2 + 1]) / 2f;
        //    }
        //    else { return list[(list.Count + 1) / 2]; }
        //}

        //private List<Cube> SplitRedCube(float mid)
        //{
        //    List<Color> low = new List<Color>();
        //    List<Color> high = new List<Color>();
        //    foreach (Color item in ListColors)
        //    {
        //        if (item.R < mid) { low.Add(item); }
        //        else { high.Add(item); }
        //    }
        //    return new List<Cube>() { new Cube(low), new Cube(high) };
        //}

        //private List<Cube> SplitGreenCube(float mid)
        //{
        //    List<Color> low = new List<Color>();
        //    List<Color> high = new List<Color>();
        //    foreach (Color item in ListColors)
        //    {
        //        if (item.G < mid) { low.Add(item); }
        //        else { high.Add(item); }
        //    }
        //    return new List<Cube>() { new Cube(low), new Cube(high) };
        //}
        //private List<Cube> SplitBlueCube(float mid)
        //{
        //    List<Color> low = new List<Color>();
        //    List<Color> high = new List<Color>();
        //    foreach (Color item in ListColors)
        //    {
        //        if (item.B < mid) { low.Add(item); }
        //        else { high.Add(item); }
        //    }
        //    return new List<Cube>() { new Cube(low), new Cube(high) };
        //}
        //#endregion

    }

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
                    Margin = new Thickness(1f),
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
