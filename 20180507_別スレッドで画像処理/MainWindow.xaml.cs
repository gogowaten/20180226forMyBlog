using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
//画像処理中のプログレスバー更新とキャンセルボタンで中止(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15494790.html

namespace _20180507_別スレッドで画像処理
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;//元画像用
        CancellationTokenSource MyCancelSource;//キャンセル用

        public MainWindow()
        {
            InitializeComponent();
            OriginBitmap = GetBitmapSourceWithChangePixelFormat2(
                @"D:\ブログ用\チェック用2\NEC_2773_2018_05_05_午後わてん_.jpg", PixelFormats.Pbgra32, 96, 96);
            MyImage.Source = OriginBitmap;

            MyButtonOrigin.Click += MyButtonOrigin_Click;
            MyButton1.Click += MyButton1_Click;
            MyButtonCancel.Click += MyButtonCancel_Click;
            MyButtonCancel.IsEnabled = false;
        }

        private void MyButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            MyCancelSource.Cancel();//キャンセルを発行
        }

        private async void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            MyButtonボタンの有効設定(true);
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            MyCancelSource = new CancellationTokenSource();
            CancellationToken token = MyCancelSource.Token;
            //MyProgressBar.IsIndeterminate = true;
            Progress<int> progress = new Progress<int>(MyProgressUpdate);
            await Task.Run(() => ColorReverce(pixels, w, h, stride, token, progress));
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
            //MyProgressBar.IsIndeterminate = false;
            MyButtonボタンの有効設定(false);
        }

        private void MyButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = OriginBitmap;
        }

        //プログレスバーの表示更新
        private void MyProgressUpdate(int i)
        {
            MyProgressBar.Value = i;
        }

        private void MyButtonボタンの有効設定(bool is処理中)
        {
            if (is処理中 == true)
            {
                MyButtonCancel.IsEnabled = true;
                MyButtonOrigin.IsEnabled = false;
                MyButton1.IsEnabled = false;
            }
            else
            {
                MyButtonCancel.IsEnabled = false;
                MyButtonOrigin.IsEnabled = true;
                MyButton1.IsEnabled = true;
            }
        }
        //色を反転
        private bool ColorReverce(byte[] pixels, int w, int h, int stride,
            CancellationToken token, IProgress<int> progress)
        {
            long p;
            for (int y = 0; y < h; ++y)
            {
                if (token.IsCancellationRequested == true)
                {
                    return false;
                }

                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + x * 4;
                    //RGBを反転、アルファ値はそのまま
                    pixels[p] = (byte)(255 - pixels[p]);
                    pixels[p + 1] = (byte)(255 - pixels[p + 1]);
                    pixels[p + 2] = (byte)(255 - pixels[p + 2]);
                }
                progress.Report(y * 100 / (h - 1));//プログレスバー更新
                Thread.Sleep(10);//0.01秒待機
            }
            //for (int i = 0; i < pixels.Length; i += 4)
            //{
            //    if (token.IsCancellationRequested == true)
            //    {
            //        return false;
            //    }
            //    //RGBを反転、アルファ値はそのまま
            //    for (int j = 0; j < 3; ++j)
            //    {
            //        pixels[i + j] = (byte)(255 - pixels[i + j]);
            //    }
            //}
            return true;
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

