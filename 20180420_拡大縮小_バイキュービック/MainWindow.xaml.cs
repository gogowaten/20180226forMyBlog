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

//バイキュービック法で画像の拡大してみようとしたけど、難しすぎた(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15469745.html

namespace _20180420_拡大縮小_バイキュービック
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
            SliderCubicFactor.MouseWheel += Slider_MouseWheel;


            //MyButtonCubicGray.Click += MyButtonCubicGray_Click;
            MyButtonCubicGray2.Click += MyButtonCubicGray2_Click;
            MyButtonCubicColor.Click += MyButtonCubicColor_Click;
            //MyButtonCubicColor2.Click += MyButtonCubicColor2_Click;

            MyButtonOrigin.Click += MyButtonOrigin_Click;
            MyButton2x2.Click += MyButton2x2_Click;
            MyButtonSave.Click += MyButtonSave_Click;
            
        }

        private void MyButtonCubicGray2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var b = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Gray8, null, 0);
            //Stopwatch s = new Stopwatch();
            //s.Start();

            MyImage.Source = F1バイキュービックグレースケール2(b, SliderScaleX.Value, SliderScaleY.Value, SliderCubicFactor.Value);
            //s.Stop();
            //MessageBox.Show($"{s.Elapsed.Seconds}秒{s.Elapsed.Milliseconds.ToString("000")}");
        }

        //private void MyButtonCubicGray_Click(object sender, RoutedEventArgs e)
        //{
        //    if (OriginBitmap == null) { return; }
        //    var b = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Gray8, null, 0);
        //    //Stopwatch s = new Stopwatch();
        //    //s.Start();
        //    MyImage.Source = F1バイキュービックグレースケール1(b, SliderScaleX.Value, SliderScaleY.Value, SliderCubicFactor.Value);
        //    //s.Stop();
        //    //MessageBox.Show($"{s.Elapsed.Seconds}秒{s.Elapsed.Milliseconds.ToString("000")}");
        //}

        private void MyButtonCubicColor_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) { return; }
            //Stopwatch s = new Stopwatch();
            //s.Start();

            MyImage.Source = F1バイキュービックColor1(OriginBitmap, SliderScaleX.Value, SliderScaleY.Value, SliderCubicFactor.Value);
            //s.Stop();
            //MessageBox.Show($"{s.Elapsed.Seconds}秒{s.Elapsed.Milliseconds.ToString("000")}");
        }

        //private void MyButtonCubicColor2_Click(object sender, RoutedEventArgs e)
        //{
        //    if (OriginBitmap == null) { return; }
        //    MyImage.Source = F1バイキュービックColor2(OriginBitmap, SliderScaleX.Value, SliderScaleY.Value, SliderCubicFactor.Value);
        //}



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



        //速い方のグレースケール
        private BitmapSource F1バイキュービックグレースケール2(BitmapSource source, double xScale, double yScale, double factor)
        {
            if (source == null) { return null; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * yScale), MidpointRounding.AwayFromZero));
            int nW = (int)(Math.Round((w * xScale), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            double motoX, motoY;
            double dx, dy;

            double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
            double tate倍率 = (h - 1) / (nH - 1.0f);
            double[] dX = new double[4];//xの距離
            double[] dY = new double[4];
            double[] wX = new double[4];//ウェイト
            double[] wY = new double[4];
            double hokan = 0;//補間した値;

            int[,] pValues4x4 = new int[4, 4];//元画像の参照する4x4ピクセルの値
            for (int y = 0; y < nH; ++y)
            {
                motoY = y * tate倍率;
                dy = motoY % 1;
                //4x4のyの距離
                dY[0] = 1 + dy; dY[1] = dy; dY[2] = 1 - dy; dY[3] = 2 - dy;
                //4x4のウェイト取得
                for (int i = 0; i < 4; ++i)
                {
                    wY[i] = GetWeight(dY[i], factor);
                }
                for (int x = 0; x < nW; ++x)
                {
                    motoX = x * yoko倍率;//注目座標の元画像での座標
                    dx = motoX % 1;
                    //4x4のxの距離
                    dX[0] = 1 + dx; dX[1] = dx; dX[2] = 1 - dx; dX[3] = 2 - dx;

                    for (int i = 0; i < 4; ++i)
                    {
                        wX[i] = GetWeight(dX[i], factor);
                    }
                    //元画像の4x4の部分の値取得
                    pValues4x4 = GetPixesValue(motoX, motoY, pixels, stride, w, h);
                    //ウェイトと値をかけた値の合計
                    hokan = 0;
                    for (int i = 0; i < 4; ++i)
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            hokan += pValues4x4[j, i] * wX[j] * wY[i];
                        }
                    }
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

        //遅い方のグレースケール,未使用
        //private BitmapSource F1バイキュービックグレースケール1(BitmapSource source, double xScale, double yScale, double factor)
        //{
        //    if (source == null) { return null; }
        //    var wb = new WriteableBitmap(source);
        //    int h = wb.PixelHeight;
        //    int w = wb.PixelWidth;
        //    int stride = wb.BackBufferStride;
        //    var pixels = new byte[h * stride];
        //    wb.CopyPixels(pixels, stride, 0);

        //    //変換後のサイズは四捨五入
        //    int nH = (int)(Math.Round((h * yScale), MidpointRounding.AwayFromZero));
        //    int nW = (int)(Math.Round((w * xScale), MidpointRounding.AwayFromZero));
        //    var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
        //    int nStride = nWb.BackBufferStride;
        //    var nPixels = new byte[nH * nStride];
        //    long nP = 0;

        //    double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
        //    double tate倍率 = (h - 1) / (nH - 1.0f);

        //    double motoX, motoY;//元画像での座標用
        //    double dx, dy;//縦横距離確認用
        //    int kitenX, kitenY;//参照する4x4の左上座標表
        //    int nX, nY;
        //    double hTotal;
        //    byte pValue;//ピクセルの値、確認用
        //    double xWeight, yWeight;//ウェイト取得確認用

        //    for (int y = 0; y < nH; ++y)
        //    {
        //        //起点-1から起点+2までの4ピクセルを参照する
        //        motoY = y * tate倍率;//注目座標の元画像でのy座標
        //        kitenY = (int)motoY - 1;//4x4の一番上
        //        for (int x = 0; x < nW; ++x)
        //        {
        //            motoX = x * yoko倍率;//注目座標の元画像での座標
        //            kitenX = (int)motoX - 1;//4x4の一番左、
        //            hTotal = 0;
        //            //double wTotal = 0.0;
        //            for (int i = 0; i < 4; ++i)
        //            {

        //                nY = kitenY + i;
        //                dy = Math.Abs(nY - motoY);//縦距離
        //                yWeight = (nY < 0 || nY > h - 1) ? 0 : GetWeight(dy, factor);
        //                //範囲外ならウェイトを0、範囲内ならウェイト取得
        //                nY = (nY < 0) ? 0 : (nY > h - 1) ? h - 1 : nY;//範囲以外だったら端の値を代入する時用
        //                for (int j = 0; j < 4; ++j)
        //                {
        //                    nX = kitenX + j;
        //                    dx = Math.Abs(nX - motoX);//横距離
        //                    xWeight = (nX < 0 || nX > w - 1) ? 0 : xWeight = GetWeight(dx, factor);//ウェイト取得
        //                    nX = (nX < 0) ? 0 : (nX > w - 1) ? w - 1 : nX;//範囲以外だったら端の値を代入する時用
        //                    pValue = pixels[nY * stride + nX];//ピクセルの値

        //                    hTotal += pValue * xWeight * yWeight;//ウェイト*値を合計していく
        //                    //wTotal += xWeight * yWeight;//xyのウェイトを掛け算したのを合計していく
        //                }
        //            }
        //            //hTotal /= wTotal;//補間の合計をウェイトの合計で割り算、これがよくわからん、いらない？
        //            //if (wTotal != 1) { MessageBox.Show(wTotal.ToString()); }
        //            //合計が0から255以外なら切り捨てて収める
        //            hTotal = (hTotal < 0) ? 0 : (hTotal > 255) ? 255 : hTotal;
        //            //四捨五入
        //            hTotal = Math.Round(hTotal, MidpointRounding.AwayFromZero);
        //            //変形後のpixelsに入れる
        //            nP = y * nStride + (x * 1);
        //            nPixels[nP] = (byte)hTotal;
        //        }
        //    }
        //    nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
        //    return nWb;
        //}




        //カラー
        private BitmapSource F1バイキュービックColor1(BitmapSource source, double xScale, double yScale, double factor)
        {
            if (source == null) { return null; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * yScale), MidpointRounding.AwayFromZero));
            int nW = (int)(Math.Round((w * xScale), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;

            double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
            double tate倍率 = (h - 1) / (nH - 1.0f);

            double motoX, motoY;//元画像での座標用
            double dx, dy;//縦横距離確認用
            int kitenX, kitenY;//参照する4x4の左上座標表
            int nX, nY;
            double[] hTotal = new double[4];
            double xWeight, yWeight;//ウェイト取得確認用

            for (int y = 0; y < nH; ++y)
            {
                //起点-1から起点+2までの4ピクセルを参照する
                motoY = y * tate倍率;//注目座標の元画像でのy座標
                kitenY = (int)motoY - 1;//4x4の一番上
                for (int x = 0; x < nW; ++x)
                {
                    motoX = x * yoko倍率;//注目座標の元画像での座標
                    kitenX = (int)motoX - 1;//4x4の一番左、
                    for (int i = 0; i < 4; i++) { hTotal[i] = 0.0; }
                    for (int yy = 0; yy < 4; ++yy)
                    {
                        nY = kitenY + yy;
                        dy = Math.Abs(nY - motoY);//縦距離
                        //範囲外ならウェイトを0、範囲内ならウェイト取得
                        //yWeight = (nY < 0 || nY > h - 1) ? 0 : GetWeight(dy, factor);
                        //範囲外でも普通にウェイト取得
                        yWeight = GetWeight(Math.Abs(dy), factor);
                        nY = (nY < 0) ? 0 : (nY > h - 1) ? h - 1 : nY;//範囲以外だったら端の値を代入する時用
                        for (int xx = 0; xx < 4; ++xx)
                        {
                            nX = kitenX + xx;
                            dx = Math.Abs(nX - motoX);//横距離
                            //xWeight = (nX < 0 || nX > w - 1) ? 0 : xWeight = GetWeight(dx, factor);//ウェイト取得
                            xWeight = GetWeight(Math.Abs(dx), factor);
                            nX = (nX < 0) ? 0 : (nX > w - 1) ? w - 1 : nX;//範囲以外だったら端の値を代入する時用

                            p = nY * stride + (nX * 4);//ピクセルの場所
                            //ウェイト*値を合計していく
                            hTotal[0] += pixels[p] * xWeight * yWeight;//青
                            hTotal[1] += pixels[p + 1] * xWeight * yWeight;//緑
                            hTotal[2] += pixels[p + 2] * xWeight * yWeight;//赤
                            hTotal[3] += pixels[p + 3] * xWeight * yWeight;//透明？                     
                        }
                    }
                    nP = y * nStride + (x * 4);
                    for (int i = 0; i < 4; ++i)
                    {
                        //合計が0から255以外なら切り捨てて収める
                        hTotal[i] = (hTotal[i] < 0) ? 0 : (hTotal[i] > 255) ? 255 : hTotal[i];
                        //四捨五入
                        hTotal[i] = Math.Round(hTotal[i], MidpointRounding.AwayFromZero);
                        //変形後のpixelsに入れる
                        nPixels[nP + i] = (byte)hTotal[i];
                    }

                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }

        //遅い方のカラー、未使用
        //private BitmapSource F1バイキュービックColor2(BitmapSource source, double xScale, double yScale, double factor)
        //{
        //    if (source == null) { return null; }
        //    var wb = new WriteableBitmap(source);
        //    int h = wb.PixelHeight;
        //    int w = wb.PixelWidth;
        //    int stride = wb.BackBufferStride;
        //    var pixels = new byte[h * stride];
        //    wb.CopyPixels(pixels, stride, 0);

        //    //変換後のサイズは四捨五入
        //    int nH = (int)(Math.Round((h * yScale), MidpointRounding.AwayFromZero));
        //    int nW = (int)(Math.Round((w * xScale), MidpointRounding.AwayFromZero));
        //    var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
        //    int nStride = nWb.BackBufferStride;
        //    var nPixels = new byte[nH * nStride];
        //    double motoX, motoY;
        //    double dx, dy;

        //    double yoko倍率 = (w - 1) / (nW - 1.0f);//変形後から見た倍率は長さで求めているから-1.0している
        //    double tate倍率 = (h - 1) / (nH - 1.0f);
        //    double[] dX = new double[4];//xの距離
        //    double[] dY = new double[4];
        //    double[] wX = new double[4];//ウェイト
        //    double[] wY = new double[4];
        //    double[] hokan = new double[4];//補間した値;

        //    Color[,] pValues4x4 = new Color[4, 4];//元画像の参照する4x4ピクセルの値
        //    for (int y = 0; y < nH; ++y)
        //    {
        //        motoY = y * tate倍率;
        //        dy = motoY % 1;
        //        //4x4のyの距離
        //        dY[0] = 1 + dy; dY[1] = dy; dY[2] = 1 - dy; dY[3] = 2 - dy;
        //        //4x4のウェイト取得
        //        for (int i = 0; i < 4; ++i)
        //        {
        //            wY[i] = GetWeight(dY[i], factor);
        //        }
        //        for (int x = 0; x < nW; ++x)
        //        {
        //            motoX = x * yoko倍率;//注目座標の元画像での座標
        //            dx = motoX % 1;
        //            //4x4のxの距離
        //            dX[0] = 1 + dx; dX[1] = dx; dX[2] = 1 - dx; dX[3] = 2 - dx;

        //            for (int i = 0; i < 4; ++i)
        //            {
        //                wX[i] = GetWeight(dX[i], factor);
        //                hokan[i] = 0.0;
        //            }
        //            //元画像の4x4の部分の値取得
        //            pValues4x4 = GetPixesValueColor(motoX, motoY, pixels, stride, w, h);
        //            //ウェイトと値をかけた値の合計                    
        //            for (int i = 0; i < 4; ++i)
        //            {
        //                for (int j = 0; j < 4; ++j)
        //                {
        //                    hokan[0] += pValues4x4[j, i].B * wX[j] * wY[i];
        //                    hokan[1] += pValues4x4[j, i].G * wX[j] * wY[i];
        //                    hokan[2] += pValues4x4[j, i].R * wX[j] * wY[i];
        //                    hokan[3] += pValues4x4[j, i].A * wX[j] * wY[i];
        //                }
        //            }
        //            for (int i = 0; i < 4; ++i)
        //            {
        //                //合計が0から255以外なら切り捨てて収める
        //                hokan[i] = hokan[i] > 255 ? 255 : hokan[i] < 0 ? 0 : hokan[i];
        //                //四捨五入
        //                hokan[i] = Math.Round(hokan[i], MidpointRounding.AwayFromZero);
        //                //変形後のpixelsに入れる
        //                nPixels[y * nStride + (x * 4) + i] = (byte)hokan[i];
        //            }

        //        }
        //    }
        //    nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
        //    return nWb;
        //}










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
        private double GetWeight(double d, double factor)
        {
            //if (d == 0) { return 1.0; }//これはないほうが速い、誤差程度
            //if (d == 1) { return 0.0; }
            if (0 <= d && d < 1.0)
            {
                //return 1 - (2 * d * d) + (d * d * d);//factor=-1のとき
                return (factor + 2) * Math.Pow(d, 3) - (factor + 3) * Math.Pow(d, 2) + 1;
            }
            else if (1.0 <= d && d < 2.0)
            {
                //return 4 - (8 * d) + (5 * d * d) - (d * d * d);
                return factor * Math.Pow(d, 3) - 5 * factor * Math.Pow(d, 2) + 8 * factor * d - 4 * factor;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 参照する元画像の4x4のcolor取得,pixelformat.pbgra32画像専用
        /// </summary>
        /// <param name="motoX"></param>
        /// <param name="motoY"></param>
        /// <param name="pixels"></param>
        /// <param name="stride"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        private Color[,] GetPixesValueColor(double motoX, double motoY, byte[] pixels, int stride, int w, int h)
        {
            Color[,] pv = new Color[4, 4];
            long p = 0;
            //元座標から1+小数点部分を引いたところが起点(左上)になる
            int mx = (int)(motoX - (motoX % 1)) - 1;//4マスの左
            int my = (int)(motoY - (motoY % 1)) - 1;//4マスの上
            int xx, yy;
            int cx = 0, cy = 0;

            for (int y = my; y < my + 4; ++y)
            {
                //参照する座標が0以下なら0の値を入れる
                //同じように右端や下限を超えていたら端の値を入れる
                if (y < 0) { yy = 0; }
                else if (y > h - 1) { yy = h - 1; }
                else { yy = y; }
                for (int x = mx; x < mx + 4; ++x)
                {
                    if (x < 0) { xx = 0; }
                    else if (x > w - 1) { xx = w - 1; }
                    else { xx = x; }
                    p = yy * stride + (xx * 4);
                    pv[cx, cy] = Color.FromArgb(pixels[p + 3], pixels[p + 2], pixels[p + 1], pixels[p]);
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
