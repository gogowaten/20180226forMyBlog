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

namespace _20180427_画像拡大_LanczosN
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;

        public MainWindow()
        {
            InitializeComponent();
            Title = this.ToString();
            AllowDrop = true;
            Drop += MainWindow_Drop;
            SliderScaleX.MouseWheel += Slider_MouseWheel;
            SliderScaleY.MouseWheel += Slider_MouseWheel;
            SliderRange.MouseWheel += Slider_MouseWheel;

            MyButtonOrigin.Click += MyButtonOrigin_Click;
            MyButton2x2.Click += MyButton2x2_Click;
            MyButtonSave.Click += MyButtonSave_Click;

            MyButtonGray.Click += MyButtonGray_Click;
            MyButtonColor.Click += MyButtonColor_Click;
            MyButtonGrayN.Click += MyButtonGrayN_Click;
            MyButtonColorN.Click += MyButtonColorN_Click;

            MyButtonGrayTest.Click += MyButtonGrayTest_Click;
        }

        private void MyButtonGrayTest_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var b = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Gray8, null, 0);
            MyImage.Source = F1Lanczos2B_Gray(b, SliderScaleX.Value, SliderScaleY.Value);
        }


        private void MyButtonColorN_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = F1LanczosN_Color(OriginBitmap, SliderScaleX.Value, SliderScaleY.Value, (int)SliderRange.Value);
        }

        private void MyButtonGrayN_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var b = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Gray8, null, 0);
            MyImage.Source = F1LanczosN_Gray(b, SliderScaleX.Value, SliderScaleY.Value, (int)SliderRange.Value);
        }

        private void MyButtonColor_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = F1Lanczos2A_Color(OriginBitmap, SliderScaleX.Value, SliderScaleY.Value);
        }

        private void MyButtonGray_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var b = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Gray8, null, 0);
            MyImage.Source = F1Lanczos2A_Gray(b, SliderScaleX.Value, SliderScaleY.Value);
        }

        //        Lanczos（ランチョス法）【ついでにSpline36】
        //https://www.rainorshine.asia/2015/10/12/post2602.html



        //グレースケール限定、LanczosN、任意n
        private BitmapSource F1LanczosN_Gray(BitmapSource source, double scaleX, double scaleY, int a)
        {
            if (source == null) { return null; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * scaleY), MidpointRounding.AwayFromZero));
            int nW = (int)(Math.Round((w * scaleX), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            double motoX, motoY;
            double kitenX, kitenY;

            double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
            double tate倍率 = (h - 1) / (nH - 1.0f);
            double[] wX = new double[a * 2];//ウェイト
            double[] wY = new double[a * 2];
            double hokan = 0;//補間した値;
            int[,] pValuesNxN = new int[a * 2, a * 2];//元画像の参照する4x4ピクセルの値

            for (int y = 0; y < nH; ++y)
            {
                motoY = y * tate倍率;
                kitenY = (motoY - motoY % 1) - (a - 1);
                for (int i = 0; i < a * 2; ++i)
                {
                    wY[i] = GetLanczosWeitgt(Math.Abs(motoY - kitenY - i), a);
                }
                for (int x = 0; x < nW; ++x)
                {
                    motoX = x * yoko倍率;//注目座標の元画像での座標                    
                    kitenX = (motoX - motoX % 1) - (a - 1);
                    //xのウェイト取得
                    for (int i = 0; i < a * 2; ++i)
                    {
                        wX[i] = GetLanczosWeitgt(Math.Abs(motoX - kitenX - i), a);
                    }
                    //元画像の4x4の部分の値取得
                    pValuesNxN = GetPixesValueN(a, motoX, motoY, pixels, stride, w, h);
                    //ウェイトと値をかけた値の合計
                    hokan = 0;
                    double wTotal = 0;
                    for (int yy = 0; yy < a * 2; ++yy)
                    {
                        for (int xx = 0; xx < a * 2; ++xx)
                        {
                            hokan += pValuesNxN[yy, xx] * wX[xx] * wY[yy];
                            wTotal += wX[xx] * wY[yy];//縦横のウェイトを掛け算したのを合計していく
                        }
                    }

                    //これ重要、ないとまだら模様になる
                    hokan /= wTotal;
                    //合計が0から255以外なら切り捨てて収める
                    hokan = hokan > 255 ? 255 : hokan < 0 ? 0 : hokan;
                    //四捨五入
                    hokan = Math.Round(hokan, MidpointRounding.AwayFromZero);
                    //変形後のpixelsに入れる
                    nP = y * nStride + (x * 1);
                    nPixels[nP] = (byte)hokan;
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }

        private BitmapSource F1LanczosN_Color(BitmapSource source, double scaleX, double scaleY, int a)
        {
            if (source == null) { return null; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * scaleY), MidpointRounding.AwayFromZero));
            int nW = (int)(Math.Round((w * scaleX), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            double motoX, motoY;
            double kitenX, kitenY;

            double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
            double tate倍率 = (h - 1) / (nH - 1.0f);
            double[] wX = new double[a * 2];//ウェイト
            double[] wY = new double[a * 2];
            double[] hokan = new double[4];//補間した値;
            int[,,] pValuesNxN = new int[a * 2, a * 2, 4];//元画像の参照する4x4ピクセルの値

            for (int y = 0; y < nH; ++y)
            {
                motoY = y * tate倍率;
                kitenY = (motoY - motoY % 1) - (a - 1);
                for (int i = 0; i < a * 2; ++i)
                {
                    wY[i] = GetLanczosWeitgt(Math.Abs(motoY - kitenY - i), a);
                }
                for (int x = 0; x < nW; ++x)
                {
                    motoX = x * yoko倍率;//注目座標の元画像での座標                    
                    kitenX = (motoX - motoX % 1) - (a - 1);
                    //xのウェイト取得
                    for (int i = 0; i < a * 2; ++i)
                    {
                        wX[i] = GetLanczosWeitgt(Math.Abs(motoX - kitenX - i), a);
                    }
                    //ウェイトと値をかけた値の合計初期化
                    for (int i = 0; i < 4; ++i) { hokan[i] = 0.0; }

                    //元画像のNxNの部分の値取得
                    pValuesNxN = GetPixesValueColorN(a, motoX, motoY, pixels, stride, w, h);

                    double wTotal = 0;
                    for (int yy = 0; yy < a * 2; ++yy)
                    {
                        for (int xx = 0; xx < a * 2; ++xx)
                        {
                            for (int c = 0; c < 4; c++)
                            {
                                hokan[c] += pValuesNxN[yy, xx, c] * wX[xx] * wY[yy];
                            }
                            wTotal += wX[xx] * wY[yy];//縦横のウェイトを掛け算したのを合計していく
                        }
                    }
                    for (int i = 0; i < 4; ++i)
                    {
                        //これ重要、ないとまだら模様になる
                        hokan[i] /= wTotal;
                        //合計が0から255以外なら切り捨てて収める
                        hokan[i] = hokan[i] > 255 ? 255 : hokan[i] < 0 ? 0 : hokan[i];
                        //四捨五入
                        hokan[i] = Math.Round(hokan[i], MidpointRounding.AwayFromZero);
                        //変形後のpixelsに入れる
                        nP = y * nStride + (x * 4) + i;
                        nPixels[nP] = (byte)hokan[i];
                    }

                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }





        //グレースケール限定、Lanczos2
        private BitmapSource F1Lanczos2A_Gray(BitmapSource source, double scaleX, double scaleY)
        {
            if (source == null) { return null; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * scaleY), MidpointRounding.AwayFromZero));
            int nW = (int)(Math.Round((w * scaleX), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            double motoX, motoY;
            double dx, dy;

            double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
            double tate倍率 = (h - 1) / (nH - 1.0f);
            double[] wX = new double[4];//ウェイト
            double[] wY = new double[4];
            double hokan = 0;//補間した値;
            int[,] pValues4x4 = new int[4, 4];//元画像の参照する4x4ピクセルの値

            for (int y = 0; y < nH; ++y)
            {
                motoY = y * tate倍率;
                dy = motoY % 1;
                //4x4のyのウェイト取得
                wY[0] = GetLanczosWeitgt(1 + dy, 2);
                wY[1] = GetLanczosWeitgt(dy, 2);
                wY[2] = GetLanczosWeitgt(1 - dy, 2);
                wY[3] = GetLanczosWeitgt(2 - dy, 2);
                for (int x = 0; x < nW; ++x)
                {
                    motoX = x * yoko倍率;//注目座標の元画像での座標
                    dx = motoX % 1;
                    //4x4のxのウェイト取得
                    wX[0] = GetLanczosWeitgt(1 + dx, 2);
                    wX[1] = GetLanczosWeitgt(dx, 2);
                    wX[2] = GetLanczosWeitgt(1 - dx, 2);
                    wX[3] = GetLanczosWeitgt(2 - dx, 2);

                    //元画像の4x4の部分の値取得
                    pValues4x4 = GetPixesValue(motoX, motoY, pixels, stride, w, h);
                    //ウェイトと値をかけた値の合計
                    hokan = 0;
                    double wTotal = 0;
                    for (int i = 0; i < 4; ++i)
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            hokan += pValues4x4[j, i] * wX[j] * wY[i];
                            wTotal += wX[j] * wY[i];//縦横のウェイトを掛け算したのを合計していく
                        }
                    }
                    
                    //これ重要、ないとまだら模様になる
                    hokan /= wTotal;
                    //合計が0から255以外なら切り捨てて収める
                    hokan = hokan > 255 ? 255 : hokan < 0 ? 0 : hokan;
                    //四捨五入
                    hokan = Math.Round(hokan, MidpointRounding.AwayFromZero);
                    //変形後のpixelsに入れる
                    nP = y * nStride + (x * 1);
                    nPixels[nP] = (byte)hokan;
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }

        //Pbgra32限定、Lanczos2
        private BitmapSource F1Lanczos2A_Color(BitmapSource source, double scaleX, double scaleY)
        {
            if (source == null) { return null; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * scaleY), MidpointRounding.AwayFromZero));
            int nW = (int)(Math.Round((w * scaleX), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            double motoX, motoY;
            double dx, dy;

            double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
            double tate倍率 = (h - 1) / (nH - 1.0f);
            double[] wX = new double[4];//ウェイト
            double[] wY = new double[4];
            double[] hokan = new double[4];//補間した値;
            int[,,] pValues4x4 = new int[4, 4, 4];//元画像の参照する4x4ピクセルの値

            for (int y = 0; y < nH; ++y)
            {
                motoY = y * tate倍率;
                dy = motoY % 1;
                //4x4のyのウェイト取得
                wY[0] = GetLanczosWeitgt(1 + dy, 2);
                wY[1] = GetLanczosWeitgt(dy, 2);
                wY[2] = GetLanczosWeitgt(1 - dy, 2);
                wY[3] = GetLanczosWeitgt(2 - dy, 2);
                for (int x = 0; x < nW; ++x)
                {
                    motoX = x * yoko倍率;//注目座標の元画像での座標
                    dx = motoX % 1;
                    //4x4のxのウェイト取得
                    wX[0] = GetLanczosWeitgt(1 + dx, 2);
                    wX[1] = GetLanczosWeitgt(dx, 2);
                    wX[2] = GetLanczosWeitgt(1 - dx, 2);
                    wX[3] = GetLanczosWeitgt(2 - dx, 2);

                    //元画像の4x4の部分の値取得
                    pValues4x4 = GetPixesValueColor(motoX, motoY, pixels, stride, w, h);
                    //ウェイトと値をかけた値の合計
                    for (int i = 0; i < 4; ++i) { hokan[i] = 0.0; }
                    double wTotal = 0;
                    for (int i = 0; i < 4; ++i)
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            for (int c = 0; c < 4; ++c)
                            {
                                hokan[c] += pValues4x4[j, i, c] * wX[j] * wY[i];
                            }
                            //wTotal += wX[j];
                            //wTotal += wY[i];
                            wTotal += wX[j] * wY[i];//縦横のウェイトを掛け算したのを合計していく
                        }
                    }

                    for (int i = 0; i < 4; ++i)
                    {
                        //これ重要、ないとまだら模様になる
                        hokan[i] /= wTotal;
                        //合計が0から255以外なら切り捨てて収める
                        hokan[i] = hokan[i] > 255 ? 255 : hokan[i] < 0 ? 0 : hokan[i];
                        //四捨五入
                        hokan[i] = Math.Round(hokan[i], MidpointRounding.AwayFromZero);
                        //変形後のpixelsに入れる
                        nP = y * nStride + (x * 4) + i;
                        nPixels[nP] = (byte)hokan[i];
                    }

                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }




        //グレースケール限定、Lanczos2、2018/04/25やっとできた！けど遅い…前のほうが速い
        private BitmapSource F1Lanczos2B_Gray(BitmapSource source, double scaleX, double scaleY)
        {
            if (source == null) { return null; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * scaleY), MidpointRounding.AwayFromZero));
            int nW = (int)(Math.Round((w * scaleX), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            double motoX, motoY;
            double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
            double tate倍率 = (h - 1) / (nH - 1.0f);

            double hokan = 0;//補間した値;
            double totalW = 0.0;

            int ix, iy;
            int tx, ty;
            double xWeight, yWeight;
            for (int y = 0; y < nH; ++y)
            {
                motoY = y * tate倍率;
                ty = (int)Math.Truncate(motoY);//小数点部分切り捨て
                for (int x = 0; x < nW; ++x)
                {
                    hokan = 0.0;
                    totalW = 0.0;
                    motoX = x * yoko倍率;
                    tx = (int)Math.Truncate(motoX);

                    for (int yy = ty - 1; yy <= ty + 2; ++yy)
                    {
                        yWeight = GetLanczosWeitgt(Math.Abs(motoY - yy), 2);
                        if (yWeight == 0) { continue; }
                        //元画像への参照座標yyが範囲外なら範囲内に収める
                        iy = (yy < 0) ? 0 : yy > h - 1 ? h - 1 : yy;
                        for (int xx = tx - 1; xx <= tx + 2; ++xx)
                        {
                            xWeight = GetLanczosWeitgt(Math.Abs(motoX - xx), 2);
                            if (xWeight == 0) { continue; }
                            ix = (xx < 0) ? 0 : (xx > w - 1) ? w - 1 : xx;
                            hokan += pixels[iy * stride + (ix * 1)] * yWeight * xWeight;
                            totalW += yWeight * xWeight;
                        }
                    }

                    hokan /= totalW;
                    hokan = Math.Round(hokan, MidpointRounding.AwayFromZero);
                    hokan = hokan < 0 ? 0 : hokan > 255 ? 255 : hokan;
                    nPixels[y * nStride + x * 1] = (byte)hokan;
                }
            }
           

            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }






        //Lanczosの重み取得
        private double GetLanczosWeitgt(double d, int n)
        {
            if (d == 0) { return 1; }
            if (d > n) { return 0; }
            return Sinc(d) * Sinc(d / n);
        }
        //窓関数…？
        private double Sinc(double x)
        {
            return Math.Sin(Math.PI * x) / (Math.PI * x);
        }








        //今表示している画像をニアレストネイバー法で2倍表示
        private BitmapSource F4今表示している画像をニアレストネイバー法で2倍(BitmapSource source)
        {
            if (source == null) { return null; }
            if (source.PixelHeight > 1000)
            {
                if (MessageBox.Show("縦ピクセル2000以上の大きな画像になる、実行する?", "処理実行確認", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    return source;
                }
            }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int nH = h * 2;
            int nW = w * 2;
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0, p = 0;
            int bpp = source.Format.BitsPerPixel / 8;
            for (int y = 0; y < nH; ++y)
            {
                for (int x = 0; x < nW; ++x)
                {
                    nP = y * nStride + (x * bpp);
                    p = (y / 2) * stride + ((x / 2) * bpp);
                    for (int i = 0; i < bpp; ++i)
                    {
                        nPixels[nP + i] = pixels[p + i];
                    }

                }
            }

            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }



        /// <summary>
        /// 参照する元画像の4x4の値取得,8bitグレースケール画像専用
        /// </summary>
        /// <param name="motoX">元画像のx座標</param>
        /// <param name="motoY"></param>
        /// <param name="pixels">元画像のpixels</param>
        /// <param name="stride"></param>
        /// <param name="w">元画像のpixels.Width</param>
        /// <param name="h"></param>
        /// <returns></returns>
        private int[,] GetPixesValue(double motoX, double motoY, byte[] pixels, int stride, int w, int h)
        {
            int[,] pv = new int[4, 4];

            //元座標から1+小数点部分を引いたところが起点(左上)になる
            int mx = (int)(motoX - (motoX % 1)) - 1;//4マスの左
            int my = (int)(motoY - (motoY % 1)) - 1;//4マスの上
            int xx, yy;
            int cx = 0, cy = 0;
            //参照する座標がマイナスなら0座標の値を入れる、
            //参照する座標が右端を超えたら右端の値を入れる
            //参照する座標が最下段を超えたら最下段の値を入れる
            for (int y = my; y < my + 4; ++y)
            {
                for (int x = mx; x < mx + 4; ++x)
                {
                    if (y < 0) { yy = 0; }
                    else if (y > h - 1) { yy = h - 1; }
                    else { yy = y; }

                    if (x < 0) { xx = 0; }
                    else if (x > w - 1) { xx = w - 1; }
                    else { xx = x; }

                    pv[cx, cy] = pixels[yy * stride + (xx * 1)];
                    cx++;
                }
                cy++;
                cx = 0;
            }
            return pv;
        }

        //参照する元画像のn*nの値取得、gray8用
        private int[,] GetPixesValueN(int n, double motoX, double motoY, byte[] pixels, int stride, int w, int h)
        {
            int[,] pv = new int[n * 2, n * 2];

            //元座標から1+小数点部分を引いたところが起点(左上)になる
            int mx = (int)(motoX - (motoX % 1)) - (n - 1);//4マスの左
            int my = (int)(motoY - (motoY % 1)) - (n - 1);//4マスの上
            int xx, yy;
            int cx = 0, cy = 0;
            //参照する座標がマイナスなら0座標の値を入れる、
            //参照する座標が右端を超えたら右端の値を入れる
            //参照する座標が最下段を超えたら最下段の値を入れる
            for (int y = my; y < my + n * 2; ++y)
            {
                for (int x = mx; x < mx + n * 2; ++x)
                {
                    if (y < 0) { yy = 0; }
                    else if (y > h - 1) { yy = h - 1; }
                    else { yy = y; }

                    if (x < 0) { xx = 0; }
                    else if (x > w - 1) { xx = w - 1; }
                    else { xx = x; }

                    pv[cy, cx] = pixels[yy * stride + (xx * 1)];
                    cx++;
                }
                cy++;
                cx = 0;
            }
            return pv;
        }

        //参照する元画像の4*4ピクセルの値取得、Pbgra32用
        private int[,,] GetPixesValueColor(double motoX, double motoY, byte[] pixels, int stride, int w, int h)
        {
            int[,,] pv = new int[4, 4, 4];//x,y,color chanel


            //元座標から1+小数点部分を引いたところが起点(左上)になる
            int mx = (int)(motoX - (motoX % 1)) - 1;//4マスの左
            int my = (int)(motoY - (motoY % 1)) - 1;//4マスの上
            int xx, yy;
            int cx = 0, cy = 0;
            long p = 0;
            //参照する座標がマイナスなら0座標の値を入れる、
            //参照する座標が右端を超えたら右端の値を入れる
            //参照する座標が最下段を超えたら最下段の値を入れる
            for (int y = my; y < my + 4; ++y)
            {
                for (int x = mx; x < mx + 4; ++x)
                {
                    if (y < 0) { yy = 0; }
                    else if (y > h - 1) { yy = h - 1; }
                    else { yy = y; }

                    if (x < 0) { xx = 0; }
                    else if (x > w - 1) { xx = w - 1; }
                    else { xx = x; }

                    p = yy * stride + (xx * 4);
                    pv[cx, cy, 0] = pixels[p + 0];//青
                    pv[cx, cy, 1] = pixels[p + 1];//緑
                    pv[cx, cy, 2] = pixels[p + 2];//赤
                    pv[cx, cy, 3] = pixels[p + 3];//アルファ
                    cx++;
                }
                cy++;
                cx = 0;
            }
            return pv;
        }

        //参照する元画像のn*nピクセルの値取得、Pbgra32用
        private int[,,] GetPixesValueColorN(int n, double motoX, double motoY, byte[] pixels, int stride, int w, int h)
        {
            int[,,] pv = new int[n * 2, n * 2, 4];//x,y,color chanel


            //元座標から1+小数点部分を引いたところが起点(左上)になる
            int mx = (int)(motoX - (motoX % 1)) - (n - 1);//4マスの左
            int my = (int)(motoY - (motoY % 1)) - (n - 1);//4マスの上
            int xx, yy;
            int cx = 0, cy = 0;
            long p = 0;
            //参照する座標がマイナスなら0座標の値を入れる、
            //参照する座標が右端を超えたら右端の値を入れる
            //参照する座標が最下段を超えたら最下段の値を入れる
            for (int y = my; y < my + n * 2; ++y)
            {
                for (int x = mx; x < mx + n * 2; ++x)
                {
                    if (y < 0) { yy = 0; }
                    else if (y > h - 1) { yy = h - 1; }
                    else { yy = y; }

                    if (x < 0) { xx = 0; }
                    else if (x > w - 1) { xx = w - 1; }
                    else { xx = x; }

                    p = yy * stride + (xx * 4);
                    pv[cy, cx, 0] = pixels[p + 0];//青
                    pv[cy, cx, 1] = pixels[p + 1];//緑
                    pv[cy, cx, 2] = pixels[p + 2];//赤
                    pv[cy, cx, 3] = pixels[p + 3];//アルファ
                    cx++;
                }
                cy++;
                cx = 0;
            }
            return pv;
        }




        private void MyButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            SaveImage((BitmapSource)MyImage.Source);
        }
        private void MyButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }

        private void MyButton2x2_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) { return; }
            MyImage.Source = F4今表示している画像をニアレストネイバー法で2倍((BitmapSource)MyImage.Source);
        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider s = (Slider)sender;
            if (e.Delta > 0) { s.Value += 0.1; }
            else { s.Value -= 0.1; }
        }


        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            //OriginBitmap = GetBitmapSourceWithChangePixelFormat2(filePath[0], PixelFormats.Gray8, 96, 96);
            OriginBitmap = GetBitmapSourceWithChangePixelFormat2(filePath[0], PixelFormats.Pbgra32, 96, 96);
            if (OriginBitmap == null)
            {
                MessageBox.Show("not Image");
            }
            else
            {
                MyImage.Source = OriginBitmap;
                ImageFileFullPath = filePath[0];
            }
        }

        private void SaveImage(BitmapSource source)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.bmp|*.bmp|*.tiff|*.tiff|*.jpg|*.jpg";
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
                else if (saveFileDialog.FilterIndex == 4)
                {
                    var je = new JpegBitmapEncoder();
                    je.QualityLevel = 96;
                    encoder = je;
                }

                encoder.Frames.Add(BitmapFrame.Create(source));
                using (var fs = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    encoder.Save(fs);
                }
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