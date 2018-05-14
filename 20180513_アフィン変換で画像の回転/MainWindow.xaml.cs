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
//WPFだけどRotateTransformを使わずに画像の回転表示をしてみた(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15505074.html


//参照したところ
//アフィン変換画像処理ソリューション
//http://imagingsolution.blog107.fc2.com/blog-entry-284.html
//アフィン変換っていうのと回転行列はこちらから

//逆行列を理解してみる - デジタル・デザイン・ラボラトリーな日々
//http://yaju3d.hatenablog.jp/entry/2013/07/14/133031
//回転行列の逆行列はこちらから
//逆行列を使わないと穴が空く説明もここがわかりやすかった

//大学1年生と再履生のための線形代数入門
//https://oguemon.com/topic/study/linear-algebra/
//行列についてはこちら

//【すぐわかる線形代数】11.掃き出し法と逆行列 - YouTube
//https://www.youtube.com/watch?v=tFKaHfSB5hM


namespace _20180513_アフィン変換で画像の回転
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;//元画像用
        BitmapSource OriginBitmapColor;//元画像カラー

        public MainWindow()
        {
            InitializeComponent();
            MyButtonOrigin.Click += MyButtonOrigin_Click;
            OriginBitmap = GetBitmapSourceWithChangePixelFormat2(
               @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.jpg", PixelFormats.Gray8, 96, 96);
            MyImage.Source = OriginBitmap;
            OriginBitmapColor = GetBitmapSourceWithChangePixelFormat2(
               @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.jpg", PixelFormats.Pbgra32, 96, 96);

            Slider1.ValueChanged += Slider1_ValueChanged;//リアルタイム変換

            MyButton1.Click += MyButton1_Click;
            MyButton2.Click += MyButton2_Click;
            MyButton3.Click += MyButton3_Click;
            MyButton4.Click += MyButton4_Click;
            MyButton5.Click += MyButton5_Click;
            MyButton6.Click += MyButton6_Click;
        }

        private void MyButton6_Click(object sender, RoutedEventArgs e)
        {
            Kaiten6((float)Slider1.Value);
        }

        private void MyButton5_Click(object sender, RoutedEventArgs e)
        {
            Kaiten5((float)Slider1.Value);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            Kaiten4((float)Slider1.Value);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            Kaiten3((float)Slider1.Value);
        }

        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            Kaiten2((float)Slider1.Value);
        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            Zahyou(30);
        }


        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Kaiten3((float)Slider1.Value);
        }



        //補間に挑戦、できた！
        private void Kaiten6(float kakudo)
        {
            var wb = new WriteableBitmap(OriginBitmapColor);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//元画像のピクセル

            //変換後画像のサイズを適正なものに
            Rect nRect = JustぴったりサイズRect(OriginBitmapColor, kakudo);
            int nw = (int)nRect.Width + 1;//Rectは1からカウントだけどBitmapは0だから+1している
            int nh = (int)nRect.Height + 1;
            var nWb = new WriteableBitmap(nw, nh, 96, 96, wb.Format, wb.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nh * nStride];//回転後のピクセル用
            long p, np;
            //変換後の座標を逆変換して元の座標を求めてその色を入れる
            for (int ny = 0; ny < nh; ny++)//変換後y座標
            {
                for (int nx = 0; nx < nw; nx++)
                {
                    //変換後座標をオフセットしてから
                    Point nPoint = new Point(nx + nRect.X, ny + nRect.Y);
                    //逆回転変換して元の座標取得
                    Point point = GetRotate逆回転座標(nPoint, kakudo);
                    int xx = (int)point.X;//切り捨て
                    int yy = (int)point.Y;
                    //元画像の範囲内に収まった座標だけ色を入れる
                    if (xx >= 0 && xx <= w - 1 && yy >= 0 && yy <= h - 1)
                    {
                        p = yy * stride + xx * 4;
                        byte b, g, r, a;
                        //外周以外のピクセルなら線形補間
                        if (xx < w - 1 && yy < h - 1)
                        {
                            double hx = point.X - xx;//座標の小数点部分
                            double hy = point.Y - yy;
                            //x軸距離補間
                            double b1 = pixels[p] * (1 - hx) + pixels[p + 4] * hx;//上段
                            double b2 = pixels[p + stride] * (1 - hx) + pixels[p + stride + 4] * hx;//下段
                            p += 1;
                            double g1 = pixels[p] * (1 - hx) + pixels[p + 4] * hx;
                            double g2 = pixels[p + stride] * (1 - hx) + pixels[p + stride + 4] * hx;//下段
                            p += 1;
                            double r1 = pixels[p] * (1 - hx) + pixels[p + 4] * hx;
                            double r2 = pixels[p + stride] * (1 - hx) + pixels[p + stride + 4] * hx;//下段
                            p += 1;
                            double a1 = pixels[p] * (1 - hx) + pixels[p + 4] * hx;
                            double a2 = pixels[p + stride] * (1 - hx) + pixels[p + stride + 4] * hx;//下段

                            //y軸の距離で補間、四捨五入
                            b = (byte)Math.Round(b1 * (1 - hy) + b2 * hy, MidpointRounding.AwayFromZero);
                            g = (byte)Math.Round(g1 * (1 - hy) + g2 * hy, MidpointRounding.AwayFromZero);
                            r = (byte)Math.Round(r1 * (1 - hy) + r2 * hy, MidpointRounding.AwayFromZero);
                            a = (byte)Math.Round(a1 * (1 - hy) + a2 * hy, MidpointRounding.AwayFromZero);
                        }
                        //外周ピクセルなら補間なし
                        else
                        {
                            b = pixels[p];
                            g = pixels[p+1];
                            r = pixels[p + 2];
                            a = pixels[p + 3];
                        }

                        np = ny * nStride + nx * 4;
                        //変換後座標に元座標の色を入れる
                        nPixels[np] = b;
                        nPixels[np + 1] = g;
                        nPixels[np + 2] = r;
                        nPixels[np + 3] = a;
                    }
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nw, nh), nPixels, nStride, 0);
            MyImage.Source = nWb;
        }

        //補間も、できた！
        private void Kaiten5(float kakudo)
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//元画像のピクセル

            //変換後画像のサイズを適正なものに
            Rect nRect = JustぴったりサイズRect(OriginBitmap, kakudo);
            int nw = (int)nRect.Width + 1;//Rectは1からカウントだけどBitmapは0だから+1している
            int nh = (int)nRect.Height + 1;
            var nWb = new WriteableBitmap(nw, nh, 96, 96, wb.Format, wb.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nh * nStride];//回転後のピクセル用
            long p, np;
            //変換後の座標を逆変換して元の座標を求めてその色を入れる
            for (int ny = 0; ny < nh; ny++)//変換後y座標
            {
                for (int nx = 0; nx < nw; nx++)
                {
                    //変換後座標をオフセットしてから
                    Point nPoint = new Point(nx + nRect.X, ny + nRect.Y);
                    //逆回転変換して元の座標取得
                    Point point = GetRotate逆回転座標(nPoint, kakudo);
                    int xx = (int)point.X;//切り捨て
                    int yy = (int)point.Y;
                    //元画像の範囲内に収まった座標だけ色を入れる
                    if (xx >= 0 && xx <= w - 1 && yy >= 0 && yy <= h - 1)
                    {
                        p = yy * stride + xx;
                        byte v;
                        //外周以外のピクセルなら線形補間
                        if (xx < w - 1 && yy < h - 1)
                        {
                            var hx = point.X - xx;//座標の小数点部分
                            var hy = point.Y - yy;
                            //x軸距離補間
                            var v1 = pixels[p] * (1 - hx) + pixels[p + 1] * hx;//上段
                            var v2 = pixels[p + stride] * (1 - hx) + pixels[p + stride + 1] * hx;//下段
                            //y軸の距離で補間
                            v = (byte)Math.Round(v1 * (1 - hy) + v2 * hy, MidpointRounding.AwayFromZero);
                        }
                        else { v = pixels[p]; }//外周ピクセルなら補間なし

                        np = ny * nStride + nx;
                        //変換後座標に元座標の色を入れる
                        nPixels[np] = v;
                    }
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nw, nh), nPixels, nStride, 0);
            MyImage.Source = nWb;
        }


        //できた！範囲外にならない、穴も開かない、あと足りないのは補間
        private void Kaiten4(float kakudo)
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//元画像のピクセル

            //変換後画像のサイズを適正なものに
            Rect nRect = JustぴったりサイズRect(OriginBitmap, kakudo);
            int nw = (int)nRect.Width + 1;//Rectは1からカウントだけどBitmapは0だから+1している
            int nh = (int)nRect.Height + 1;
            var nWb = new WriteableBitmap(nw, nh, 96, 96, wb.Format, wb.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nh * nStride];//回転後のピクセル用
            long p, np;
            //変換後の座標を逆変換して元の座標を求めてその色を入れる
            for (int ny = 0; ny < nh; ny++)//変換後y座標
            {
                for (int nx = 0; nx < nw; nx++)
                {
                    //変換後座標をオフセットしてから
                    Point nPoint = new Point(nx + nRect.X, ny + nRect.Y);
                    //逆回転変換して元の座標取得
                    Point point = GetRotate逆回転座標(nPoint, kakudo);
                    int xx = (int)point.X;//切り捨て
                    int yy = (int)point.Y;
                    //元画像の範囲内に収まった座標だけ色を入れる
                    if (xx >= 0 && xx <= w - 1 && yy >= 0 && yy <= h - 1)
                    {
                        p = yy * stride + xx;
                        np = ny * nStride + nx;
                        nPixels[np] = pixels[p];//変換後座標に元座標の色を入れる
                    }
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nw, nh), nPixels, nStride, 0);
            MyImage.Source = nWb;
        }



        //1の改変、回転後の画像サイズと描画位置を考慮したので範囲外になるのはなくなった
        //けど穴はあく
        private void Kaiten3(float kakudo)
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//元画像のピクセル

            //変換後画像のサイズを適正なものに
            Rect nRect = JustぴったりサイズRect(OriginBitmap, kakudo);
            int nw = (int)nRect.Width + 1;//Rectは1からカウントだけどBitmapは0だから+1している
            int nh = (int)nRect.Height + 1;
            var nWb = new WriteableBitmap(nw, nh, 96, 96, wb.Format, wb.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nh * nStride];//回転後のピクセル用
            long p, np;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    //元画像の座標を回転変換
                    Point point = GetRotate回転座標(new Point(x, y), kakudo);
                    //変換後はマイナスになったりするのでその分をオフセット
                    Point nPoint = new Point(point.X - nRect.X, point.Y - nRect.Y);

                    p = y * stride + x;
                    np = ((int)nPoint.Y * nStride + (int)nPoint.X);
                    nPixels[np] = pixels[p];//変換後座標に元座標の色を入れる

                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nw, nh), nPixels, nStride, 0);
            MyImage.Source = nWb;
        }

        /// <summary>
        /// BitmapSourceを回転後のサイズ
        /// </summary>
        /// <param name="source"></param>
        /// <param name="kakudo">角度</param>
        /// <returns></returns>
        private Rect JustぴったりサイズRect(BitmapSource source, float kakudo)
        {
            Point PointUL左上 = GetRotate回転座標(new Point(0, 0), kakudo);
            Point PointUR右上 = GetRotate回転座標(new Point(source.PixelWidth - 1, 0), kakudo);
            Point PointDR右下 = GetRotate回転座標(new Point(source.PixelWidth - 1, source.PixelHeight - 1), kakudo);
            Point PointDL左下 = GetRotate回転座標(new Point(0, source.PixelHeight - 1), kakudo);

            double left = Math.Min(0, Math.Min(PointUR右上.X, Math.Min(PointDR右下.X, PointDL左下.X)));
            double right = Math.Max(0, Math.Max(PointUR右上.X, Math.Max(PointDR右下.X, PointDL左下.X)));
            double bottom = Math.Max(0, Math.Max(PointUR右上.Y, Math.Max(PointDR右下.Y, PointDL左下.Y)));
            double top = Math.Min(0, Math.Min(PointUR右上.Y, Math.Min(PointDR右下.Y, PointDL左下.Y)));
            //Point nPoint左上 = new Point(minWidth, minHeight);
            //Point nPoint右下 = new Point(maxWidth, maxHeight);
            double nWidht = right - left;
            double nHeight = bottom - top;
            return new Rect(left, top, nWidht, nHeight);
        }

        //元画像のピクセルの座標を変換、回転で画像の範囲が広がったぶんを無視するので
        //元画像の位置から外れたところは表示されないし、穴が開く
        private void Kaiten2(float kakudo)
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//元画像のピクセル

            var nWb = new WriteableBitmap(w, h, 96, 96, wb.Format, wb.Palette);
            var nPixels = new byte[h * stride];//回転後のピクセル用

            Rect rect = new Rect(0, 0, w - 1, h - 1);//元画像の四角形の大きさ、枠
            long p, np;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    //座標変換
                    Point point = GetRotate回転座標(new Point(x, y), kakudo);
                    //変換後座標が元の枠内に収まっていたら書き込み
                    if (rect.Contains(point))
                    {
                        p = y * stride + x;
                        np = ((int)point.Y * stride + (int)point.X);
                        nPixels[np] = pixels[p];//変換後座標に元座標の色を入れる
                    }
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, w, h), nPixels, stride, 0);
            MyImage.Source = nWb;
        }

        private void Zahyou(float kakudo)
        {
            //左上を回転の中心としたとき
            //top right 256, 0
            var radians = kakudo / 180 * Math.PI;
            Point oTL = new Point(0, 0);
            Point oTR = new Point(OriginBitmap.PixelWidth, 0);
            Point oDR = new Point(OriginBitmap.PixelWidth, OriginBitmap.PixelHeight);
            Point oDL = new Point(0, OriginBitmap.PixelHeight);
            Point PointUL左上 = GetRotate回転座標(oTL, kakudo);
            Point PointUR右上 = GetRotate回転座標(oTR, kakudo);
            Point PointDR右下 = GetRotate回転座標(oDR, kakudo);
            Point PointDL左下 = GetRotate回転座標(oDL, kakudo);

            double minWidth = Math.Min(0, Math.Min(PointUR右上.X, Math.Min(PointDR右下.X, PointDL左下.X)));
            double maxWidth = Math.Max(0, Math.Max(PointUR右上.X, Math.Max(PointDR右下.X, PointDL左下.X)));
            double maxHeight = Math.Max(0, Math.Max(PointUR右上.Y, Math.Max(PointDR右下.Y, PointDL左下.Y)));
            double minHeight = Math.Min(0, Math.Min(PointUR右上.Y, Math.Min(PointDR右下.Y, PointDL左下.Y)));
            Point nPoint左上 = new Point(minWidth, minHeight);
            Point nPoint右下 = new Point(maxWidth, maxHeight);
            double nWidht = maxWidth - minWidth;
            double nHeight = maxHeight - minHeight;


            var rt = new RotateTransform(kakudo);
            MyImage.RenderTransform = rt;
            //this.Dispatcher.Invoke(()=> { },System.Windows.Threading.DispatcherPriority.Render);
            var k = MyImage.RenderTransform.Transform(oTL);
            var k2 = MyImage.RenderTransform.Transform(oTR);
            var k3 = MyImage.RenderTransform.Transform(oDR);
            var k4 = MyImage.RenderTransform.Transform(oDL);
            var rk = MyImage.RenderTransform.TransformBounds(new Rect(0, 0, OriginBitmap.PixelWidth, OriginBitmap.PixelHeight));
        }


        private Point GetRotate逆回転座標(Point p, float kakudo)
        {
            double radians = Radians角度をラジアン(kakudo);
            return new Point(
                Math.Cos(radians) * p.X + Math.Sin(radians) * p.Y,
                -Math.Sin(radians) * p.X + Math.Cos(radians) * p.Y);
        }

        private Point GetRotate回転座標(Point p, float kakudo)
        {
            double radians = Radians角度をラジアン(kakudo);
            return new Point(
                Math.Cos(radians) * p.X - Math.Sin(radians) * p.Y,
                Math.Sin(radians) * p.X + Math.Cos(radians) * p.Y);
        }
        private double Radians角度をラジアン(float kakudo)
        {
            return kakudo / 180 * Math.PI;
        }

        private void MyButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = OriginBitmap;
            MyImage.RenderTransform = null;
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
