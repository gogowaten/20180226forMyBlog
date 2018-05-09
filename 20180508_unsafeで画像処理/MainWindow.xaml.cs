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
using System.Diagnostics;
//BitmapSourceでの画像処理速度、unsafe、ポインタ、ビルドモード(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15497771.html

///unsafe (unsafe モードの有効化) (C#)
//https://msdn.microsoft.com/ja-jp/library/ct597kb0%28v=vs.80%29.aspx?f=255&MSPPError=-2147217396
//WPF画像処理プログラミング入門 第3回 グレースケール - プロノワ -
//http://www.pronowa.com/room/wpf_imaging003.html

namespace _20180508_unsafeで画像処理
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;//元画像用
        const int LOOP_COUNT = 100;

        public MainWindow()
        {
            InitializeComponent();
            //            WriteableBitmap.Lock メソッド(System.Windows.Media.Imaging)
            //https://msdn.microsoft.com/ja-jp/library/system.windows.media.imaging.writeablebitmap.lock(v=vs.110).aspx
            //|=はor演算、<<はシフト演算
            //int c = 55 << 16;//R...G?
            //c |= 100 << 8;//G
            //c |= 200 << 0;//B...R?
            //var r = c % 256;//R
            //var g = c / 256 % 256;//G
            //var b = c / 256 / 256;//B


            OriginBitmap = GetBitmapSourceWithChangePixelFormat2(
               @"D:\ブログ用\チェック用2\NEC_2773_2018_05_05_午後わてん.jpg", PixelFormats.Pbgra32, 96, 96);
            MyImage.Source = OriginBitmap;

            MyButtonOrigin.Click += MyButtonOrigin_Click;
            MyButton1.Click += MyButton1_Click;
            MyButton2.Click += MyButton2_Click;
            MyButton3.Click += MyButton3_Click;
            MyButton4.Click += MyButton4_Click;
            MyButton5.Click += MyButton5_Click;
            MyButton6.Click += MyButton6_Click;
            MyButton7.Click += MyButton7_Click;
            MyButton8.Click += MyButton8_Click;
            MyButton9.Click += MyButton9_Click;
            MyButton10.Click += MyButton10_Click;
            MyButton11.Click += MyButton11_Click;
            MyButton12.Click += MyButton12_Click;
            MyButton13.Click += MyButton13_Click;
            MyButton14.Click += MyButton14_Click;
            MyButton15.Click += MyButton15_Click;
        }

        private void MyButton15_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce15MyUnsafe2B_Parallel, MyTextBlock15, LOOP_COUNT);
        }

        private void MyButton14_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce14MyUnsafe3B, MyTextBlock14, LOOP_COUNT);
        }

        private void MyButton13_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce13MyUnsafe6, MyTextBlock13, LOOP_COUNT);
        }

        private void MyButton12_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce12MyUnsafe5Parallel, MyTextBlock12, LOOP_COUNT);            
        }

        private void MyButton11_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce11MyUnsafe4Parallel, MyTextBlock11, LOOP_COUNT);
        }

        private void MyButton10_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce10MyUnsafe4, MyTextBlock10, LOOP_COUNT);
        }

        private void MyButton9_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce9MyUnsafe3, MyTextBlock9, LOOP_COUNT);
        }

        private void MyButton8_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce8MyUnsafe2Parallel, MyTextBlock8, LOOP_COUNT);
        }

        private void MyButton7_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce7MyUnsafe2, MyTextBlock7, LOOP_COUNT);
        }

        private void MyButton6_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce6MyUnsafe, MyTextBlock6, LOOP_COUNT);
        }

        private void MyButton5_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce5MyUnsafe, MyTextBlock5, LOOP_COUNT);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce4, MyTextBlock4, LOOP_COUNT);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce3, MyTextBlock3, LOOP_COUNT);
        }

        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce2, MyTextBlock2, LOOP_COUNT);
        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            MyTime計測(ColorReverce1, MyTextBlock1, LOOP_COUNT);
        }

        private void MyButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = OriginBitmap;
        }

        private void MyTime計測(Action action, TextBlock textBlock, int loop)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < loop; i++)
            {
                action();
            }
            stopwatch.Stop();
            //double averageSeconds = stopwatch.Elapsed.TotalSeconds / loop;
            //textBlock.Text = $"{averageSeconds.ToString("0.000")}秒";
            textBlock.Text = $"{stopwatch.Elapsed.TotalSeconds.ToString("0.000")}秒";
        }




        //ColorReverce8MyUnsafe2Parallelを改変、ビット反転にした
        // Lock、Unlock、unsafeでポインタを使う方法をパラレル
        private void ColorReverce15MyUnsafe2B_Parallel()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                Parallel.For(0, h, y =>
                {
                    for (int x = 0; x < w; x++)
                    {
                        byte* p = ptr + (y * stride) + (x * 4);
                        p[0] = (byte)~(p[0]);
                        p[1] = (byte)~(p[1]);
                        p[2] = (byte)~(p[2]);
                    }
                });
            }
            wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            wb.Unlock();
            MyImage.Source = wb;
        }

        //ColorReverce9MyUnsafe3を改変、ビット反転にした        
        private void ColorReverce14MyUnsafe3B()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                for (int i = 0; i < h * stride; i += 4)
                {
                    ptr[i] = (byte)~ptr[i];
                    ptr[i + 1] = (byte)~ptr[i + 1];
                    ptr[i + 2] = (byte)~ptr[i + 2];
                }
            }
            wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            wb.Unlock();
            MyImage.Source = wb;
        }



        //        .NETによる画像処理の高速化Tips：unsafe編（改稿：2015/11/08）
        //https://qiita.com/Nuits/items/da8c11e5b284ad6cb90a#%E3%83%90%E3%82%A4%E3%83%88%E9%85%8D%E5%88%97%E3%81%AF%E3%81%9D%E3%81%AE%E3%81%BE%E3%81%BE%E4%BD%BF%E3%81%86%E3%81%8C%E3%82%A2%E3%83%89%E3%83%AC%E3%82%B9%E3%82%92%E5%9B%BA%E5%AE%9Afixed%E3%81%97%E3%81%A6%E3%83%9D%E3%82%A4%E3%83%B3%E3%82%BF%E3%81%A7%E3%82%A2%E3%82%AF%E3%82%BB%E3%82%B9%E3%81%99%E3%82%8B%E5%A0%B4%E5%90%88

        //
        //1.286 fixedを使ってbyte配列のポインタ、WriteableBitmapを使わずに
        private void ColorReverce13MyUnsafe6()
        {
            int w = OriginBitmap.PixelWidth;
            int h = OriginBitmap.PixelHeight;
            int stride = w * 4;
            byte[] pixels = new byte[h * stride];
            OriginBitmap.CopyPixels(pixels, stride, 0);
            unsafe
            {

                fixed (byte* ptr = pixels)
                {
                    for (int i = 0; i < h * stride; i += 4)
                    {
                        ptr[i] = (byte)(255 - ptr[i]);
                        ptr[i + 1] = (byte)(255 - ptr[i + 1]);
                        ptr[i + 2] = (byte)(255 - ptr[i + 2]);
                    }
                }
            }
            var b = BitmapSource.Create(w, h, 96, 96, PixelFormats.Pbgra32, null, pixels, stride);
            MyImage.Source = b;
        }

        //2.216秒 1重ループ、PixelFormatをアルファ値のないBgr24に変換してif判定をなくした
        private void ColorReverce12MyUnsafe5Parallel()
        {
            var cb = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Bgr24, null, 0);
            var wb = new WriteableBitmap(cb);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                Parallel.For(0, h * stride, i =>
                {
                    ptr[i] = (byte)(255 - ptr[i]);
                });
            }
            wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            wb.Unlock();
            cb = new FormatConvertedBitmap(wb, PixelFormats.Pbgra32, null, 0);
            MyImage.Source = cb;
        }

        //5.497 1重ループ、色反転でもう1重ループ、毎回if判定が入るのでとても遅くなる,パラレルにしても遅い
        private void ColorReverce11MyUnsafe4Parallel()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                Parallel.For(0, h * stride, i =>
                {
                    if ((i + 1) % 4 != 0)
                    {
                        ptr[i] = (byte)(255 - ptr[i]);
                    }
                });
            }
            MyImage.Source = wb;
        }

        //7.412 1重ループ、色反転でもう1重ループ、毎回if判定が入るのでとても遅くなる
        private void ColorReverce10MyUnsafe4()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                for (int i = 0; i < h * stride; ++i)
                {
                    if ((i + 1) % 4 != 0)
                    {
                        ptr[i] = (byte)(255 - ptr[i]);
                    }
                }
            }
            wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            wb.Unlock();
            MyImage.Source = wb;
        }

        
        //1.206 Lock、Unlock、unsafeでポインタを使う方法を1重ループにした
        private void ColorReverce9MyUnsafe3()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                for (int i = 0; i < h * stride; i += 4)
                {
                    ptr[i] = (byte)(255 - ptr[i]);
                    ptr[i + 1] = (byte)(255 - ptr[i + 1]);
                    ptr[i + 2] = (byte)(255 - ptr[i + 2]);
                }
            }
            wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            wb.Unlock();
            MyImage.Source = wb;
        }

        
        //1.266 Lock、Unlock、unsafeでポインタを使う方法をパラレル
        private void ColorReverce8MyUnsafe2Parallel()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                Parallel.For(0, h, y =>
                {
                    for (int x = 0; x < w; x++)
                    {
                        byte* p = ptr + (y * stride) + (x * 4);
                        p[0] = (byte)(255 - p[0]);
                        p[1] = (byte)(255 - p[1]);
                        p[2] = (byte)(255 - p[2]);
                    }
                });
            }
            wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            wb.Unlock();
            MyImage.Source = wb;
        }

        //Lock、Unlockを使わない、これは良くない？→良くないみたい
        //処理の途中でもOSは最適化のためにメモリ上での移動をする
        //1.580 移動するってのはアドレスが変わることなのでまともな処理ができない
        private void ColorReverce7MyUnsafe2()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            //wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        byte* p = ptr + (y * stride) + (x * 4);
                        p[0] = (byte)(255 - p[0]);
                        p[1] = (byte)(255 - p[1]);
                        p[2] = (byte)(255 - p[2]);
                    }
                }
            }
            //wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            //wb.Unlock();
            MyImage.Source = wb;
        }

        //        WriteableBitmapの画素をポインタから操作する - schima.hatenablog.com
        //http://schima.hatenablog.com/entry/20100918/1284817562

        //1.689 Lock、Unlock、unsafeでポインタを使う方法
        private void ColorReverce6MyUnsafe()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        byte* p = ptr + (y * stride) + (x * 4);
                        p[0] = (byte)(255 - p[0]);
                        p[1] = (byte)(255 - p[1]);
                        p[2] = (byte)(255 - p[2]);
                    }
                }
            }
            wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            wb.Unlock();
            MyImage.Source = wb;
        }


        //1.541 Lock、Unlock、unsafeでポインタを使う方法、ビット反転で色反転
        private void ColorReverce5MyUnsafe()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            wb.Lock();
            unsafe
            {
                byte* ptr = (byte*)wb.BackBuffer;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        byte* p = ptr + (y * stride) + (x * 4);
                        //~つけてビット反転
                        p[0] = (byte)~p[0];
                        p[1] = (byte)~p[1];
                        p[2] = (byte)~p[2];
                    }
                }
            }
            wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
            wb.Unlock();
            MyImage.Source = wb;
        }

        //private void ColorReverce5()
        //{
        //    var wb = new WriteableBitmap(OriginBitmap);
        //    int w = wb.PixelWidth;
        //    int h = wb.PixelHeight;
        //    int stride = wb.BackBufferStride;
        //    byte[] pixels = new byte[h * stride];
        //    wb.CopyPixels(pixels, stride, 0);
        //    for (int i = 0; i < pixels.Length; i += 4)
        //    {
        //        //RGBを反転、アルファ値はそのまま
        //        pixels[i] = (byte)(255 - pixels[i]);
        //        pixels[i + 1] = (byte)(255 - pixels[i + 1]);
        //        pixels[i + 2] = (byte)(255 - pixels[i + 2]);
        //    }
        //    wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
        //    MyImage.Source = wb;
        //}

        //3.095 ColorReverce3から変更、RGB変更時にループを使う、これは遅くなった
        private void ColorReverce4()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            for (int i = 0; i < pixels.Length; i += 4)
            {
                //RGBを反転、アルファ値はそのまま
                for (int j = 0; j < 3; ++j)
                {
                    pixels[i + j] = (byte)(255 - pixels[i + j]);
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }

        //2.368 ColorReverce1の2重ループを1重ループに変更
        private void ColorReverce3()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            for (int i = 0; i < pixels.Length; i += 4)
            {
                //RGBを反転、アルファ値はそのまま
                pixels[i] = (byte)(255 - pixels[i]);
                pixels[i + 1] = (byte)(255 - pixels[i + 1]);
                pixels[i + 2] = (byte)(255 - pixels[i + 2]);
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }

        //2.868 ColorReverce1を少し変更
        private void ColorReverce2()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p, py;
            for (int y = 0; y < h; ++y)
            {
                py = y * stride;//ここだけ変更
                for (int x = 0; x < w; ++x)
                {
                    p = py + x * 4;
                    //RGBを反転、アルファ値はそのまま
                    pixels[p] = (byte)(255 - pixels[p]);
                    pixels[p + 1] = (byte)(255 - pixels[p + 1]);
                    pixels[p + 2] = (byte)(255 - pixels[p + 2]);
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }

        //2.623秒、CopyPixelsで色を反転
        private void ColorReverce1()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + x * 4;
                    //RGBを反転、アルファ値はそのまま
                    pixels[p] = (byte)(255 - pixels[p]);
                    pixels[p + 1] = (byte)(255 - pixels[p + 1]);
                    pixels[p + 2] = (byte)(255 - pixels[p + 2]);
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }








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
