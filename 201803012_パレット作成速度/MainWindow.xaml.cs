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
using System.Diagnostics;
//手抜きで時間を短縮、k平均法を使った減色パレットの作成(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15410540.html

namespace _201803012_パレット作成速度
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;
        Border[] MyPalettePan;
        Border[] MyPalettePanLimited;
        const int MAX_PALETTE_COLOR_COUNT = 20;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            //Button1.Click += Button1_Click;
            ButtonChangeColor.Click += ButtonChangeColorPixelPalette_Click;
            ButtonChangeColorLimited.Click += ButtonChangeColorLimitedPixelPalette_Click;
            ButtonCreatePalette.Click += ButtonCreatePalette_Click;
            ButtonCreatePaletteWithLimit.Click += ButtonCreatePaletteWithLimit_Click;
            //ButtonGetColor.Click += ButtonGetColor_Click;

            //パレットの色表示用のBorder作成
            MyPalettePan = AddBorders(MyWrapPanel);
            MyPalettePanLimited = AddBorders(MyWrapPanelLimited);

        }



        //走査ピクセル数からパレット作成
        private void ButtonCreatePaletteWithLimit_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Color[] colors = GetPalette(
                    OriginBitmap,
                    (int)NumericScrollBar.Value,
                    (int)NumericScrollBar2.Value,
                    (int)NumericScrollBarMargin.Value,
                    TextBlockLoopCountLimited);
            stopwatch.Stop();
            ChangePalettePanColor(colors.ToList(), MyPalettePanLimited);
            ReNewTextTime(stopwatch.Elapsed, TextBlockTimePixelLimit, "パレット作成");
        }


        //全ピクセルからパレット作成
        private void ButtonCreatePalette_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Color[] colors = GetPalette(
                OriginBitmap,
                (int)NumericScrollBar.Value,
                0,
                (int)NumericScrollBarMargin.Value,
                TextBlockLoopCount);
            stopwatch.Stop();
            ChangePalettePanColor(colors.ToList(), MyPalettePan);
            ReNewTextTime(stopwatch.Elapsed, TextBlockTimePixelNoLimit, "パレット作成");
        }

        //減色、走査ピクセル数のパレットで
        private void ButtonChangeColorLimitedPixelPalette_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null || MyPalettePanLimited[0].Background == null) { return; }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MyImage.Source = ReduceColor(OriginBitmap, GetPaletteColorList(MyPalettePanLimited));
            stopwatch.Stop();
            ReNewTextTime(stopwatch.Elapsed, TextBlockTimeRiduceColor, "変換");
        }

        //減色、全ピクセルからのパレットで
        private void ButtonChangeColorPixelPalette_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null || MyPalettePan[0].Background == null) { return; }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MyImage.Source = ReduceColor(OriginBitmap, GetPaletteColorList(MyPalettePan));
            stopwatch.Stop();
            ReNewTextTime(stopwatch.Elapsed, TextBlockTimeRiduceColor, "変換");
        }


        //private void Button1_Click(object sender, RoutedEventArgs e)
        //{
        //    パレットの色表示を初期化
        //    PalettePanColorDel();
        //    if (OriginBitmap == null) { return; }
        //    パレットを作成して、画像を減色
        //    MyImage.Source = ReduceColor減色(OriginBitmap, (int)NumericScrollBar.Value);
        //}


        //処理時間表示更新
        private void ReNewTextTime(TimeSpan time, TextBlock textBlock, string prefix)
        {
            textBlock.Text = $"{prefix}時間：{time.Minutes}分{time.Seconds}秒{time.Milliseconds.ToString("000")}";
        }

        //Borderの背景色からパレットの色取得
        private List<Color> GetPaletteColorList(Border[] palettePan)
        {
            Brush pBrush;
            SolidColorBrush pSolid;
            List<Color> palette = new List<Color>();
            for (int i = 0; i < palettePan.Length; ++i)
            {
                pBrush = palettePan[i].Background;
                pSolid = (SolidColorBrush)pBrush;
                if (pSolid != null)
                {
                    palette.Add(pSolid.Color);
                }
            }
            return palette;
        }

        ////パレット作成
        //private void PaletteGensyoku(BitmapSource source, int colorCount)
        //{
        //    Color[] palette = GetPalette(source, colorCount);
        //    for (int i = 0; i < palette.Length; ++i)
        //    {
        //        MyPalette[i].Background = new SolidColorBrush(palette[i]);
        //        Console.WriteLine(palette[i].ToString());
        //    }
        //}


        //パレットの色表示を初期化
        private void PalettePanColorDel(Border[] palettePan)
        {
            for (int i = 0; i < palettePan.Length; ++i)
            {
                palettePan[i].Background = null;
            }
        }

        /// <summary>
        /// k平均法で画像からパレット作成、ループ上限は100回
        /// </summary>
        /// <param name="source">PixelFormat.Pbgr32限定</param>
        /// <param name="colorCount">パレットの色数</param>
        /// <param name="limitPixel">走査するピクセル数の上限、1000あれば十分、画像のピクセル数＜上限のときは全ピクセルを走査</param>
        /// <param name="margin">パレット完成とする新旧パレットの色差、5～20がいい、小さいほど時間かかる</param>
        /// <param name="textBlock">ループ回数を表示するTextBlockを指定</param>
        /// <returns></returns>
        private Color[] GetPalette(BitmapSource source, int colorCount, int limitPixel, int margin, TextBlock textBlock = null)
        {
            Color[] pixelColors;
            if (limitPixel == 0)
            {
                pixelColors = GetAllPixelsColor(source);//画像の全ピクセルのColor取得
            }
            else
            {
                pixelColors = GetRandomPixelsColor(source, limitPixel);//制限数ピクセル取得
            }

            //初期パレット作成
            Color[] oldPalette = GetRandomColorPalette(colorCount);//旧パレット
            Color[] nextPalette = new Color[colorCount];//新パレット
            //2つのパレットの色の差が指定値以下、またはループ回数が100になったらパレット完成
            int loopCount = 0;
            while (loopCount < 100)
            {
                loopCount++;
                nextPalette = GetNewPalette(oldPalette, pixelColors);//新パレットに色振り分け
                if (GetDiffPalettes(oldPalette, nextPalette) < margin) { break; }//色差が指定値以下になったら完成
                //旧パレットに新パレットの色を入れる
                for (int i = 0; i < oldPalette.Length; ++i)
                {
                    oldPalette[i] = nextPalette[i];
                }
            }

            if (textBlock != null)
            {
                textBlock.Text = $"ループ回数：{loopCount}";
            }

            return nextPalette;
        }


        //2つのパレットの色の差を取得
        private double GetDiffPalettes(Color[] bPalette, Color[] nPalette)
        {
            double diff = 0;
            for (int i = 0; i < bPalette.Length; ++i)
            {
                diff += GetColorDistance(bPalette[i], nPalette[i]);
            }
            diff /= bPalette.Length;
            return diff;
        }

        //ピクセルの色をパレットの色で分ける
        //分けた色の平均色を新しいパレットの色にする
        private Color[] GetNewPalette(Color[] palette, Color[] pixelColors)
        {
            //振り分け先の入れ物をパレットの色数分作成
            List<Color>[] colorList = new List<Color>[palette.Length];
            for (int i = 0; i < palette.Length; ++i)
            {
                colorList[i] = new List<Color>();
            }
            //画像の色と比較、近い色のパレットのインデックスのListに追加していく
            double distance, min;
            int pIndex;//palette index
            Color nowColor;
            for (int i = 0; i < pixelColors.Length; i++)
            {
                nowColor = pixelColors[i];
                pIndex = 0;
                min = GetColorDistance(nowColor, palette[0]);//2色間の距離
                for (int j = 1; j < palette.Length; j++)
                {
                    distance = GetColorDistance(nowColor, palette[j]);//2色間の距離
                    if (min > distance)
                    {
                        min = distance;
                        pIndex = j;
                    }
                }
                colorList[pIndex].Add(nowColor);//Listに追加
            }
            Color[] newPalette = new Color[palette.Length];

            for (int i = 0; i < newPalette.Length; i++)
            {
                newPalette[i] = GetAverageGolor(colorList[i]);
            }
            return newPalette;
        }


        //画像の全ピクセルの色をColorの配列にして返す
        private Color[] GetAllPixelsColor(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;
            Color[] color = new Color[h * w];
            for (int i = 0; i < color.Length; ++i)
            {
                p = i * 4;
                color[i] = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
            }
            return color;
        }

        //画像から指定数だけのピクセルカラーをランダムで取得
        //ピクセル数が指定数以下のときは全ピクセルカラーを取得
        private Color[] GetRandomPixelsColor(BitmapSource source, int limit)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            if (limit > w * h)
            {
                return GetAllPixelsColor(source);
            }

            Color[] color = new Color[limit];
            Random random = new Random();
            int p = 0;
            int x, y;
            for (int i = 0; i < limit; ++i)
            {
                x = random.Next(w);
                y = random.Next(h);
                p = y * stride + (x * 4);
                color[i] = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
            }
            return color;
        }


        //初期パレット作成、ランダム色のパレット
        private Color[] GetRandomColorPalette(int paletteCapacity)
        {
            Color[] colors = new Color[paletteCapacity];
            Random random = new Random();
            byte[] r = new byte[3];
            for (int i = 0; i < colors.Length; ++i)
            {
                random.NextBytes(r);
                colors[i] = Color.FromRgb(r[0], r[1], r[2]);
                Console.WriteLine(colors[i].ToString());
            }
            return colors;
        }

        /// <summary>
        /// k平均法を使ってパレットを作成して減色
        /// 色の距離はRGB各色の差の2乗を足したのを√
        /// </summary>
        /// <param name="source">PixelFormatPbgra32のBitmapSource</param>
        /// <param name="colorCount">パレットの色数</param>
        /// <returns>PixelFormatPbgra32のBitmapSource</returns>
        //private BitmapSource ReduceColor減色(BitmapSource source, int colorCount)
        //{
        //    string neko = "start" + "\n";//色と色差の変化の確認用
        //    var wb = new WriteableBitmap(source);
        //    int h = wb.PixelHeight;
        //    int w = wb.PixelWidth;
        //    int stride = wb.BackBufferStride;
        //    byte[] pixels = new byte[h * stride];
        //    wb.CopyPixels(pixels, stride, 0);
        //    long p = 0;
        //    Color myColor;
        //    int pIndex;//パレットのインデックス
        //    double distance, min, diff = 0;//色の距離、最小値、新旧色の距離
        //    //パレット
        //    Color[] palette = GetRandomColorPalette(colorCount);
        //    for (int i = 0; i < palette.Length; ++i)
        //    {
        //        neko += palette[i].ToString() + "\n";
        //    }
        //    //グループ分けした色を入れる色List
        //    List<Color>[] colorList = new List<Color>[palette.Length];

        //    for (int j = 0; j < 100; ++j)
        //    {
        //        //色List作成(初期化)
        //        for (int i = 0; i < palette.Length; ++i)
        //        {
        //            colorList[i] = new List<Color>();
        //        }

        //        for (int y = 0; y < h; ++y)
        //        {
        //            for (int x = 0; x < w; ++x)
        //            {
        //                p = y * stride + (x * 4);
        //                myColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
        //                pIndex = 0;
        //                distance = GetColorDistance(myColor, palette[0]);
        //                min = distance;
        //                //グループ分け
        //                //距離が近(短)いパレットの色のインデックス取得して
        //                //そのインデックスの色Listを追加してグループ分け
        //                for (int i = 1; i < palette.Length; ++i)
        //                {
        //                    distance = GetColorDistance(myColor, palette[i]);
        //                    if (min > distance)
        //                    {
        //                        min = distance;
        //                        pIndex = i;
        //                    }
        //                }
        //                colorList[pIndex].Add(myColor);//色Listに追加
        //            }
        //        }

        //        //グループ分けした色の平均色から新しいパレット作成
        //        Color[] newPalette = new Color[palette.Length];
        //        for (int i = 0; i < newPalette.Length; ++i)
        //        {
        //            myColor = GetAverageGolor(colorList[i]);//平均色取得(新しい色)
        //            neko += myColor.ToString() + "\n";
        //            diff += GetColorDistance(palette[i], myColor);
        //            palette[i] = myColor;//新しい色で上書き
        //        }

        //        //古いパレットと新しいパレットの色の差が1以下ならループ抜け、新パレット完成
        //        TextBlockLoopCount.Text = "ループ回数 ＝ " + j.ToString();
        //        diff /= palette.Length;
        //        neko += diff.ToString() + "\n";
        //        if (diff < 1f) { break; }
        //        diff = 0;
        //    }

        //    //パレットの色表示
        //    for (int i = 0; i < palette.Length; ++i)
        //    {
        //        MyPalettePan[i].Background = new SolidColorBrush(palette[i]);
        //    }
        //    Console.WriteLine(neko);

        //    //パレットの色で減色
        //    for (int y = 0; y < h; ++y)
        //    {
        //        for (int x = 0; x < w; ++x)
        //        {
        //            p = y * stride + (x * 4);
        //            myColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
        //            min = GetColorDistance(myColor, palette[0]);
        //            pIndex = 0;
        //            for (int i = 0; i < palette.Length; ++i)
        //            {
        //                distance = GetColorDistance(myColor, palette[i]);
        //                if (min > distance)
        //                {
        //                    min = distance;
        //                    pIndex = i;
        //                }
        //            }
        //            myColor = palette[pIndex];
        //            pixels[p + 2] = myColor.R;
        //            pixels[p + 1] = myColor.G;
        //            pixels[p] = myColor.B;
        //        }
        //    }
        //    wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
        //    return wb;
        //}

        //パレットを指定した色に変更
        private void ChangePalettePanColor(List<Color> palette, Border[] palettePan)
        {
            PalettePanColorDel(palettePan);//初期化
            for (int i = 0; i < palette.Count; ++i)
            {
                palettePan[i].Background = new SolidColorBrush(palette[i]);
            }
        }

        /// <summary>
        /// k平均法を使ってパレットを作成
        /// 色の距離はRGB各色の差の2乗を足したのを√
        /// </summary>
        /// <param name="source">PixelFormatPbgra32のBitmapSource</param>
        /// <param name="colorsNum">パレットの色数</param>
        /// <returns>指定した数の色</returns>
        //private List<Color> CreatePaletteColor(BitmapSource source, int colorsNum, int colorMargin)
        //{
        //    string neko = "start" + "\n";//色と色差の変化の確認用
        //    var wb = new WriteableBitmap(source);
        //    int h = wb.PixelHeight;
        //    int w = wb.PixelWidth;
        //    int stride = wb.BackBufferStride;
        //    byte[] pixels = new byte[h * stride];
        //    wb.CopyPixels(pixels, stride, 0);
        //    long p = 0;
        //    Color myColor;
        //    int pIndex;//パレットのインデックス
        //    double distance, min, diff = 0;//色の距離、最小値、新旧色の距離
        //    //パレット
        //    List<Color> palette = GetRandomColorPalette(colorsNum).ToList();
        //    for (int i = 0; i < palette.Count; ++i)
        //    {
        //        neko += palette[i].ToString() + "\n";
        //    }
        //    //グループ分けした色を入れる色List
        //    List<Color>[] colorList = new List<Color>[palette.Count];

        //    for (int j = 0; j < 100; ++j)//ループ回数は100以下
        //    {
        //        //色List作成(初期化)
        //        for (int i = 0; i < palette.Count; ++i)
        //        {
        //            colorList[i] = new List<Color>();
        //        }

        //        for (int y = 0; y < h; ++y)
        //        {
        //            for (int x = 0; x < w; ++x)
        //            {
        //                p = y * stride + (x * 4);
        //                myColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
        //                pIndex = 0;
        //                distance = GetColorDistance(myColor, palette[0]);
        //                min = distance;
        //                //グループ分け
        //                //距離が近(短)いパレットの色のインデックス取得して
        //                //そのインデックスの色Listを追加してグループ分け
        //                for (int i = 1; i < palette.Count; ++i)
        //                {
        //                    distance = GetColorDistance(myColor, palette[i]);
        //                    if (min > distance)
        //                    {
        //                        min = distance;
        //                        pIndex = i;
        //                    }
        //                }
        //                colorList[pIndex].Add(myColor);//色Listに追加
        //            }
        //        }

        //        //グループ分けした色の平均色から新しいパレット作成
        //        Color[] newPalette = new Color[palette.Count];
        //        for (int i = 0; i < newPalette.Length; ++i)
        //        {
        //            myColor = GetAverageGolor(colorList[i]);//平均色取得(新しい色)
        //            neko += myColor.ToString() + "\n";
        //            diff += GetColorDistance(palette[i], myColor);
        //            palette[i] = myColor;//新しい色で上書き
        //        }

        //        //古いパレットと新しいパレットの色の差がcolorMargin以下ならループ抜け、新パレット完成
        //        TextBlockLoopCount.Text = "ループ回数 ＝ " + j.ToString();
        //        diff /= palette.Count;
        //        neko += diff.ToString() + "\n";
        //        if (diff < colorMargin) { break; }
        //        diff = 0;
        //    }
        //    return palette;
        //}

        //パレットの色で減色
        private BitmapSource ReduceColor(BitmapSource source, List<Color> palette)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;
            Color bitmapColor;
            double min, distance;
            int pIndex;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    bitmapColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
                    min = GetColorDistance(bitmapColor, palette[0]);
                    pIndex = 0;
                    for (int i = 0; i < palette.Count; ++i)
                    {
                        distance = GetColorDistance(bitmapColor, palette[i]);
                        if (min > distance)
                        {
                            min = distance;
                            pIndex = i;
                        }
                    }
                    bitmapColor = palette[pIndex];
                    pixels[p + 2] = bitmapColor.R;
                    pixels[p + 1] = bitmapColor.G;
                    pixels[p] = bitmapColor.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }

        //距離
        private double GetColorDistance(Color c1, Color c2)
        {
            return Math.Sqrt(
                Math.Pow(c1.R - c2.R, 2) +
                Math.Pow(c1.G - c2.G, 2) +
                Math.Pow(c1.B - c2.B, 2));
        }

        //ColorListの平均色を返す
        private Color GetAverageGolor(List<Color> colorList)
        {
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

        //パレットの色表示用のBorder作成
        private Border[] AddBorders(Panel panel)
        {
            NumericScrollBar.Maximum = MAX_PALETTE_COLOR_COUNT;
            Border[] palettePan = new Border[MAX_PALETTE_COLOR_COUNT];
            Border border;
            for (int i = 0; i < palettePan.Length; i++)
            {
                border = new Border()
                {
                    Width = 20,
                    Height = 20,
                    BorderBrush = new SolidColorBrush(Colors.AliceBlue),
                    BorderThickness = new Thickness(1f),
                    Margin = new Thickness(1f)
                };
                palettePan[i] = border;
                panel.Children.Add(border);
            }
            return palettePan;
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
                ImageFileFullPath = filePath[0];
                TextBlockImagePixelsCount.Text = $"/{OriginBitmap.PixelWidth * OriginBitmap.PixelHeight}総ピクセル";
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
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
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
            catch (Exception)
            {

            }

            return source;
        }
    }
}
