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
using System.Diagnostics;


namespace _20180314_k平均法減色変換時間_変換一覧表
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;        
        Border[] MyPalettePanLimited;
        const int MAX_PALETTE_COLOR_COUNT = 20;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString() + "k平均法で減色パレットの作成";
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            
            ButtonChangeColorLimited.Click += ButtonChangeColorLimitedPixelPalette_Click;
            ButtonReduceColor2.Click += ButtonReduceColor2_Click;
            ButtonCreatePaletteWithLimit.Click += ButtonCreatePaletteWithLimit_Click;
            ButtonSaveImage.Click += ButtonSaveImage_Click;
            ButtonOriginImage.Click += ButtonOriginImage_Click;            

            //パレットの色表示用のBorder作成            
            MyPalettePanLimited = AddBorders(MyWrapPanelLimited);

        }

        private void ReNewTextWhichButton(Button button)
        {
            TextBlockWhich.Text = button.Content.ToString();
        }
       

        //Parallelテスト
        private void ButtonReduceColor2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null || MyPalettePanLimited[0].Background == null) { return; }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MyImage.Source = ReduceColor2(OriginBitmap, GetPaletteColorList(MyPalettePanLimited));
            stopwatch.Stop();
            ReNewTextTime(stopwatch.Elapsed, TextBlockTimeRiduceColor, "変換");
            ReNewTextWhichButton((Button)sender);
        }

        private void ButtonOriginImage_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }

        private void ButtonSaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            SaveImage((BitmapSource)MyImage.Source);
        }



        //パレット作成
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



        //減色
        private void ButtonChangeColorLimitedPixelPalette_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null || MyPalettePanLimited[0].Background == null) { return; }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MyImage.Source = ReduceColor(OriginBitmap, GetPaletteColorList(MyPalettePanLimited));
            stopwatch.Stop();
            ReNewTextTime(stopwatch.Elapsed, TextBlockTimeRiduceColor, "変換");
            ReNewTextWhichButton((Button)sender);
        }


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

            pixelColors = GetRandomPixelsColor(source, limitPixel);//指定数ピクセル色取得
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


        /// <summary>
        /// BitmapSourceのピクセルからColor取得、指定数だけランダムに取得
        /// </summary>
        /// <param name="source">PixelFormat.Pbgr32限定</param>
        /// <param name="limit">取得するピクセル数の指定、0か全ピクセル数以上なら全ピクセルを取得</param>
        /// <returns></returns>
        private Color[] GetRandomPixelsColor(BitmapSource source, int limit)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            Color[] color;
            int p = 0;
            if (limit > w * h || limit == 0)
            {//指定0か総ピクセル数以上なら全ピクセルから取得
                color = new Color[h * w];
                for (int i = 0; i < color.Length; ++i)
                {
                    p = i * 4;
                    color[i] = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
                }
            }
            else
            {//全ピクセルからランダムに指定数だけ取得
                color = new Color[limit];
                Random random = new Random();
                int x, y;
                for (int i = 0; i < limit; ++i)
                {
                    x = random.Next(w);
                    y = random.Next(h);
                    p = y * stride + (x * 4);
                    color[i] = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
                }
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


        //パレットを指定した色に変更
        private void ChangePalettePanColor(List<Color> palette, Border[] palettePan)
        {
            PalettePanColorDel(palettePan);//初期化
            for (int i = 0; i < palette.Count; ++i)
            {
                palettePan[i].Background = new SolidColorBrush(palette[i]);
            }
        }


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
                    min = 10000;//充分に大きな数値を入れておく
                    pIndex = 0;
                    //今の色と一番近い色をパレットから探す
                    for (int i = 0; i < palette.Count; ++i)
                    {
                        distance = GetColorDistance(bitmapColor, palette[i]);//今の色と今のパレットの色の距離
                        if (min > distance)
                        {
                            min = distance;
                            pIndex = i;
                        }
                    }
                    //パレットの色に変換
                    bitmapColor = palette[pIndex];
                    pixels[p + 2] = bitmapColor.R;
                    pixels[p + 1] = bitmapColor.G;
                    pixels[p + 0] = bitmapColor.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);

            //PixelFormatを色数に合わせたものに変更してから返す            
            return OptimisationPixelFormat(wb, palette.Count);
        }
        
        //パレットの色で減色2、変換一覧表を使った方法        
        private BitmapSource ReduceColor2(BitmapSource source, List<Color> palette)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;
            Color bitmapColor;
            int[] colorCountArray = GetArray24bitColorCount(source);
            //変換一覧表取得
            Dictionary<Color, Color> converter = GetDictionaryConvertColor(colorCountArray, palette);

            for (int y = 0; y < h; ++y)//この中が一番時間がかかる
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    bitmapColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
                    //変換一覧表を使って色変換
                    bitmapColor = converter[bitmapColor];
                    pixels[p + 2] = bitmapColor.R;
                    pixels[p + 1] = bitmapColor.G;
                    pixels[p + 0] = bitmapColor.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);//4.3秒

            //PixelFormatを色数に合わせたものに変更してから返す            
            return OptimisationPixelFormat(wb, palette.Count);
        }
        

        //表示中の画像の色をカウント
        //RGBをintに変換してそれぞれの色1700万色分ををカウント
        //256*256*256＝16777216個の要素の配列を返す
        //赤ピクセルRGB(255,0,0)が2個だったらint[255]は2、黒が5個だったらint[0]には5が入る
        private int[] GetArray24bitColorCount(BitmapSource source)
        {
            var cb = new FormatConvertedBitmap(source, PixelFormats.Rgb24, source.Palette, 0);
            var wb = new WriteableBitmap(cb);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;// (w * wb.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            long p = 0;
            int[] colors = new int[256 * 256 * 256];//1700万色
            int numColor;
            //RGBをintに変換してint配列に入れる
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 3);
                    //var r = pixels[p];
                    //var g = pixels[p + 1];
                    //var b = pixels[p + 2];
                    numColor = pixels[p] + pixels[p + 1] * 256 + pixels[p + 2] * 256 * 256;
                    colors[numColor]++;
                    //intからRGBに戻す
                    //var nekoR = numColor % 256;
                    //var nekoG = numColor / 256 % 256;
                    //var nekoB = numColor / 256 / 256;
                }
            }
            return colors;
        }

        //変換一覧表作成
        private Dictionary<Color, Color> GetDictionaryConvertColor(int[] colors, List<Color> palette)
        {
            Color bitmapColor = new Color();
            double min = 10000, distance;
            int pIndex = 0;
            Dictionary<Color, Color> converter = new Dictionary<Color, Color>();
            for (int i = 0; i < colors.Length; ++i)
            {
                if (colors[i] != 0)
                {
                    bitmapColor = Color.FromRgb((byte)(i % 256), (byte)(i / 256 % 256), (byte)(i / 256 / 256));
                    min = 10000;
                    for (int j = 0; j < palette.Count; ++j)
                    {
                        distance = GetColorDistance(bitmapColor, palette[j]);
                        if (min > distance)
                        {
                            min = distance;
                            pIndex = j;
                        }
                    }
                    converter.Add(bitmapColor, palette[pIndex]);
                }
            }
            return converter;
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
                ImageFileFullPath = filePath[0];
                TextBlockImagePixelsCount.Text = $"/{OriginBitmap.PixelWidth * OriginBitmap.PixelHeight}総ピクセル";
                TextBlockImageSize.Text = $"画像サイズ：{OriginBitmap.PixelWidth} x {OriginBitmap.PixelHeight}";
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
            catch (Exception)
            {

            }

            return source;
        }
    }
}
