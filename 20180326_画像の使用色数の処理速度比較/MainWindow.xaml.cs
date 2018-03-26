using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;
using System.Diagnostics;
//処理速度比較、画像の使用色数を数える、重複なしのリストのHashSetも速いけど配列＋ifも速かった(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15430276.html

namespace _20180326_画像の使用色数の処理速度比較
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
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
                //int cCount = 0;
                //Stopwatch stopwatch = new Stopwatch();

                //stopwatch.Start();
                //cCount = GetColorCountA1(OriginBitmap);
                //stopwatch.Stop();
                //TextBlockTimeA.Text = $"{nameof(GetColorCountA1)}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒";
                //Console.WriteLine($"{nameof(GetColorCountA1)}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒");
                //stopwatch.Restart();
                ////cCount = GetColorCountB(OriginBitmap);
                ////stopwatch.Stop();
                ////TextBlockTimeB.Text = $"{nameof(GetColorCountB)}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒";
                ////stopwatch.Restart();
                ////cCount = GetColorCountC(OriginBitmap);
                ////stopwatch.Stop();
                ////TextBlockTimeC.Text = $"{nameof(GetColorCountC)}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒";
                ////stopwatch.Restart();
                //cCount = GetColorCountB1(OriginBitmap);
                //stopwatch.Stop();
                //TextBlockTimeD.Text = $"{nameof(GetColorCountB1)}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒";
                //stopwatch.Restart();
                //cCount = GetColorCountE(OriginBitmap);
                //stopwatch.Stop();
                //TextBlockTimeE.Text = $"{nameof(GetColorCountE)}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒";
                //stopwatch.Restart();
                ////cCount = GetColorCountF(OriginBitmap);
                ////stopwatch.Stop();
                ////TextBlockTimeF.Text = $"{nameof(GetColorCountF)}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒";
                ////stopwatch.Restart();
                //cCount = GetColorCountC1(OriginBitmap);
                //stopwatch.Stop();
                //TextBlockTimeG.Text = $"{nameof(GetColorCountC1)}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒";


                //TextBlockPixelsCount.Text = $" 画像の使用色数：{cCount}";
                TextBlockImageSize.Text = $"画像サイズ：{OriginBitmap.PixelWidth}x{OriginBitmap.PixelHeight}";
                Keisoku();
            }
        }

        private void Keisoku()
        {
            var list = new Func<BitmapSource, int>[]
            {
                GetColorCountA1,
                GetColorCountA2,
                GetColorCountA3,//エラーにはならないけどカウントがおかしい
                GetColorCountA4,//エラーにはならないけどカウントがおかしい
                GetColorCountB1,
                //GetColorCountB2,//配列の境界外エラー
                GetColorCountC1,
                //GetColorCountC2,//配列の境界外エラー
                GetColorCountD1,
                GetColorCountE1,
            };

            int cCount = 0;
            Stopwatch stopwatch = new Stopwatch();
            for (int i = 0; i < list.Length; ++i)
            {
                stopwatch.Restart();
                cCount = list[i](OriginBitmap);
                stopwatch.Stop();
                Console.WriteLine($"{list[i].Method.Name}：{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds.ToString("000")}秒：{cCount}色");
            }
            TextBlockPixelsCount.Text = $" 画像の使用色数：{cCount}";
        }

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
        private int GetColorCountA2(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int[] iColor = new int[256 * 256 * 256];
            Parallel.For(0, pixels.Length / 4, i =>
              {
                  iColor[pixels[i * 4] * 256 * 256 + pixels[i * 4 + 1] * 256 + pixels[i * 4 + 2]]++;
              });

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
        //エラーにはならないけどカウントがおかしい
        private int GetColorCountA3(BitmapSource source)
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
            //Parallel.For(0, iColor.Length, i =>
            //{
            //    if (iColor[i] != 0)
            //    {
            //        colorCount++;
            //    }
            //});
            Parallel.ForEach(iColor, item =>
             {
                 if (item != 0) { colorCount++; }
             });
            return colorCount;
        }
        //エラーにはならないけどカウントがおかしい
        private int GetColorCountA4(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int[] iColor = new int[256 * 256 * 256];
            Parallel.For(0, pixels.Length / 4, i =>
              {
                  iColor[pixels[i * 4] * 256 * 256 + pixels[i * 4 + 1] * 256 + pixels[i * 4 + 2]]++;
              });

            int colorCount = 0;
            Parallel.For(0, iColor.Length, i =>
            {
                if (iColor[i] != 0)
                {
                    colorCount++;
                }
            });
            return colorCount;
        }



        private int GetColorCountB1(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            List<int> list = new List<int>();
            for (int i = 0; i < pixels.Length; i += 4)
            {
                list.Add(pixels[i] * 256 * 256 + pixels[i + 1] * 256 + pixels[i + 2]);
            }
            return list.Distinct().ToArray().Length;
        }

        private int GetColorCountB2(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            List<int> list = new List<int>();
            Parallel.For(0, pixels.Length / 4, i =>
              {
                  list.Add(pixels[i * 4] * 256 * 256 + pixels[i * 4 + 1] * 256 + pixels[i * 4 + 2]);//配列の境界外エラー
              });

            return list.Distinct().ToArray().Length;
        }

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

        private int GetColorCountC2(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            var list = new HashSet<int>();
            Parallel.For(0, pixels.Length / 4, i =>
                {
                    list.Add(pixels[i * 4] * 256 * 256 + pixels[i * 4 + 1] * 256 + pixels[i * 4 + 2]);//配列の境界外エラー
                });

            return list.Count;
        }

        private int GetColorCountD1(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            var list = new ConcurrentBag<int>();
            Parallel.For(0, pixels.Length / 4, i =>
            {
                list.Add(pixels[i * 4] * 256 * 256 + pixels[i * 4 + 1] * 256 + pixels[i * 4 + 2]);
            });

            return list.Distinct().ToArray().Length;
        }

        private int GetColorCountE1(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0, c = 0;
            int[] iColor = new int[256 * 256 * 256];
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    iColor[c] = pixels[p] * 256 * 256 + pixels[p + 1] * 256 + pixels[p + 2];
                    c++;
                }
            }
            return iColor.Distinct().ToArray().Length;
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
}

