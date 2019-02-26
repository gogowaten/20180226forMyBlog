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

//任意の2色に減色するときディザリングパターンを使う(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15883853.html

namespace _20190224_パターンディザ_任意の2色減色
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource MyBitmapSource;

        public MainWindow()
        {
            InitializeComponent();
            //タイトルバーにアプリの名前表示
            var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Title = info.ProductName;


            //画像ファイルからPixelFormats.Rgb24のbitmapSource作成
            string imagePath = "";
            //imagePath = @"D:\ブログ用\チェック用2\NEC_6009_2018_11_25_午後わてん.jpg";
            //imagePath = @"D:\ブログ用\チェック用2\NEC_6008_2018_11_25_午後わてん.jpg";
            imagePath = @"D:\ブログ用\チェック用2\NEC_6191_2019_02_03_午後わてん_.jpg";

            //imagePath = @"E:\オレ\携帯\2018\NEC_5974.JPG";            
            byte[] vs;//今回は未使用            
            (vs, MyBitmapSource) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Rgb24, 96, 96);


            //任意の1色のbitmapSource作成
            //MyBitmapSource = MakeBitmap(100, 100, 50, 140, 200);//引数は(縦、横、R,G,B)
            //MyBitmapSource = MakeBitmap(100, 100, 100, 200, 148);
            //MyBitmapSource = MakeBitmap(100, 100, 74, 77, 198);
            //MyBitmapSource = MakeBitmap(100, 100, 110, 173, 129);
            //MyBitmapSource = MakeBitmap(100, 100, 255, 0, 255);
            //MyBitmapSource = MakeBitmap(100, 100, 255, 0, 0);


            //減色
            MyImage.Source = Dither2(MyBitmapSource);

        }





        //任意の2色で減色、パターンディザ
        private BitmapSource Dither2(BitmapSource bitmapSource)
        {
            //2色の指定
            byte r1, g1, b1, r2, g2, b2;
            //r1 = 77; g1 = 159; b1 = 166;//色1 青緑
            //r2 = 68; g2 = 72; b2 = 212;//色2 青
            //r1 = 250; g1 = 250; b1 = 150;//色1 黄色
            //r1 = 93; g1 = 102; b1 = 93;//色1 くらい灰色
            //r2 = 182; g2 = 187; b2 = 176;//色2 明るい灰色
            //r1 = 255; g1 = 0; b1 = 0;//色1 赤
            //r2 = 0; g2 = 0; b2 = 255;//色2 青
            r1 = 66; g1 = 70; b1 = 55;//色1
            r2 = 171; g2 = 178; b2 = 146;//色2



            //ディザパターン作成
            double[][] thresholdMatrix;
            //thresholdMatrix = MakeThresholdMatrix4x4();//普通の4x4
            //thresholdMatrix = MakeThresholdMatrix2x2();//普通の2x2
            //thresholdMatrix = MakeThresholdMatrixMagicSquare();//4x4魔方陣
            //thresholdMatrix = MakeThresholdMatrixCompletenesSquare();//4x4完全方陣
            thresholdMatrix = MakeThresholdMatrixCompletenes7x7();//7x7完全方陣

            int xx = thresholdMatrix[0].Length;//しきい値行列の横の要素数
            int yy = thresholdMatrix.Length;   //しきい値行列の縦の要素数

            //減色処理
            var wb = new WriteableBitmap(bitmapSource);
            int w = wb.PixelWidth;
            int h = wb.PixelHeight;
            int stride = wb.BackBufferStride;//横1行のバイト数、PixelFormats.rgb24ならRGB各8bitの合計24bitは3byte、これに横のピクセル数を掛けた値
            byte[] pixels = new byte[h * stride];//色配列用
            wb.CopyPixels(pixels, stride, 0);//色配列取得
            long p = 0;//指定ピクセルの配列上の位置

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    p = y * stride + (x * 3);//3はPixelFormats.rgb24の1ピクセルあたりのbyte数
                    byte rr = pixels[p];//注目ピクセル(元の色)のRの値
                    byte gg = pixels[p + 1];//G
                    byte bb = pixels[p + 2];//B
                    //元の色と色1、色2の距離
                    double distance1 = GetColorDistance(rr, gg, bb, r1, g1, b1);
                    double distance2 = GetColorDistance(rr, gg, bb, r2, g2, b2);

                    //元の色との距離が0(全く同じ色)なら変換の必要がないので違うときだけ処理                    
                    if (distance1 != 0 & distance2 != 0)
                    {
                        double threshold = 0;//混ぜる割合
                        //色1が近いとき
                        if (distance2 > distance1)
                        {
                            //色2との距離からしきい値決定
                            //しきい値より大きければ色2に置き換える、以下なら色1へ置き換える
                            threshold = distance1 / distance2 / 2.0;
                            if (threshold > thresholdMatrix[y % yy][x % xx])
                            {
                                pixels[p] = r2; pixels[p + 1] = g2; pixels[p + 2] = b2;
                            }
                            else
                            {
                                pixels[p] = r1; pixels[p + 1] = g1; pixels[p + 2] = b1;
                            }
                        }
                        //色2が近いとき
                        else
                        {
                            //色1との距離からしきい値決定
                            threshold = distance2 / distance1 / 2.0;
                            if (threshold > thresholdMatrix[y % yy][x % xx])
                            {
                                pixels[p] = r1; pixels[p + 1] = g1; pixels[p + 2] = b1;
                            }
                            else
                            {
                                pixels[p] = r2; pixels[p + 1] = g2; pixels[p + 2] = b2;
                            }
                        }

                    }
                }
            }
            //色配列をWriteableBitmapに書き込んでbitmap作成
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }



        //色の距離
        private double GetColorDistance(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
        {
            return Math.Sqrt(Math.Pow(r2 - r1, 2) + Math.Pow(g2 - g1, 2) + Math.Pow(b2 - b1, 2));
        }

        #region ディザパターン作成
        //2x2ディザパターン作成
        private double[][] MakeThresholdMatrix2x2()
        {
            double[][] thresholdMatrix = new double[][]
            {
                new double[] { 1f / 5f, 3f / 5f },
                new double[] { 4f / 5f, 2f / 5f }
            };
            return thresholdMatrix;
        }
        //4x4ディザ
        private double[][] MakeThresholdMatrix4x4()
        {
            double[][] thresholdMap = new double[][]
            {
                new double[] { 1f / 17f, 13f / 17f, 4f / 17f, 16f / 17f },
                new double[] { 9f / 17f, 5f / 17f, 12f / 17f, 8f / 17f },
                new double[] { 3f / 17f, 15f / 17f, 2f / 17f, 14f / 17f },
                new double[] { 11f / 17f, 7f / 17f, 10f / 17f, 6f / 17f }
            };
            return thresholdMap;
        }

        //4x4魔方陣
        private double[][] MakeThresholdMatrixMagicSquare()
        {
            double[][] thresholdMap = new double[][]
            {
                new double[] { 1f / 17f, 15f / 17f, 14f / 17f, 4f / 17f },
                new double[] { 8f / 17f, 10f / 17f, 11f / 17f, 5f / 17f },
                new double[] { 12f / 17f, 6f / 17f, 7f / 17f, 9f / 17f },
                new double[] { 13f / 17f, 3f / 17f, 2f / 17f, 16f / 17f }
            };
            return thresholdMap;
        }

        //4x4完全方陣
        private double[][] MakeThresholdMatrixCompletenesSquare()
        {
            double[][] thresholdMap = new double[][]
            {
                new double[] { 1f / 17f, 8f / 17f, 13f / 17f, 12f / 17f },
                new double[] { 14f / 17f, 11f / 17f, 2f / 17f, 7f / 17f },
                new double[] { 4f / 17f, 5f / 17f, 16f / 17f, 9f / 17f },
                new double[] { 15f / 17f, 10f / 17f, 3f / 17f, 6f / 17f }
            };
            return thresholdMap;
        }

        //7x7完全方陣
        private double[][] MakeThresholdMatrixCompletenes7x7()
        {
            double[][] thresholdMap = new double[][]
            {
                new double[] {  1, 37, 24, 11, 47, 34, 21 },
                new double[] { 13, 49, 29, 16,  3, 39, 26 },
                new double[] { 18,  5, 41, 28,  8, 44, 31 },
                new double[] { 23, 10, 46, 33, 20,  7, 36 },
                new double[] { 35, 15,  2, 38, 25, 12, 48 },
                new double[] { 40, 27, 14, 43, 30, 17,  4 },
                new double[] { 45, 32, 19,  6, 42, 22,  9 },
            };
            for (int i = 0; i < thresholdMap.Length; i++)
            {
                for (int j = 0; j < thresholdMap[i].Length; j++)
                {
                    thresholdMap[i][j] /= 50.0;
                }
            }
            return thresholdMap;
        }

        #endregion



        /// <summary>
        /// 大きさとRGBを指定してPixelFormats.Rgb24のbitmap作成
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        private BitmapSource MakeBitmap(int width, int height, byte red, byte green, byte blue)
        {
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[height * stride];
            for (int i = 0; i < pixels.Length; i += 3)
            {
                pixels[i] = red;
                pixels[i + 1] = green;
                pixels[i + 2] = blue;
            }
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }



        //bitmapをbyte配列に変換
        private byte[] BitmapToByteArray(BitmapSource source)
        {
            int stride = source.Format.BitsPerPixel / 8 * source.PixelWidth;
            byte[] pixels = new byte[source.PixelHeight * stride];
            source.CopyPixels(pixels, stride, 0);
            return pixels;
        }


        //
        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を取得、ピクセルフォーマットを指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] array, BitmapSource source) MakeByteArrayAndSourceFromImageFile(string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
        {
            byte[] pixels = null;
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
                    pixels = new byte[h * stride];
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
            return (pixels, source);
        }
    }
}
