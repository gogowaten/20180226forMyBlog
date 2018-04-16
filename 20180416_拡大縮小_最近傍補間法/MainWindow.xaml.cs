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
//最近傍補間法で画像の拡大縮小試してみた(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15462921.html

namespace _20180416_拡大縮小_最近傍補間法
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
            SliderXScale.MouseWheel += SliderScale_MouseWheel;
            SliderYScale.MouseWheel += SliderScale_MouseWheel;

            MyButtonSave.Click += MyButtonSave_Click;
            MyButtonOrigin.Click += MyButtonOrigin_Click;
            MyButton2x2.Click += MyButton2x2_Click;
            MyButton1.Click += MyButton1_Click;
            MyButton2.Click += MyButton2_Click;
            
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
            if (OriginBitmap == null) { return; }
            MyImage.Source = F4今表示している画像をニアレストネイバー法で2倍((BitmapSource)MyImage.Source);
        }

        private void SliderScale_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider s = (Slider)sender;
            if (e.Delta > 0) { s.Value += 0.1; }
            else { s.Value -= 0.1; }
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = F2最近傍補間法カラー(OriginBitmap, SliderXScale.Value, SliderYScale.Value);
        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var b = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Gray8, null, 0);
            MyImage.Source = F1最近傍補間法グレースケール(b, SliderXScale.Value, SliderYScale.Value);
        }




        //今表示している画像をニアレストネイバー法で2倍表示
        private BitmapSource F4今表示している画像をニアレストネイバー法で2倍(BitmapSource source)
        {
            if (source.PixelHeight > 1000)
            {
                if (MessageBox.Show(
                    "縦ピクセル2000以上の大きな画像になる、実行する？",
                    "処理実行確認",
                    MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
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
            int cc = source.Format.BitsPerPixel / 8;//8bitグレースケールなら1、32bppなら4
            for (int y = 0; y < nH; ++y)
            {
                for (int x = 0; x < nW; ++x)
                {
                    nP = y * nStride + (x * cc);
                    p = (y / 2) * stride + ((x / 2) * cc);
                    for (int i = 0; i < cc; ++i)
                    {
                        nPixels[nP + i] = pixels[p + i];
                    }

                }
            }

            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }

        /// <summary>
        /// BitmapSourceを最近傍補間法で拡大縮小
        /// </summary>
        /// <param name="source">pixelsFormat.Pbgra32専用</param>
        /// <param name="xScale">横倍率</param>
        /// <param name="yScale">縦倍率</param>
        /// <returns></returns>
        private BitmapSource F2最近傍補間法カラー(BitmapSource source, double xScale, double yScale)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * yScale), MidpointRounding.AwayFromZero));//普通のintキャストだと10*1.2で11.999は11になる
            int nW = (int)(Math.Round((w * xScale), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            int motoX, motoY;
            for (int y = 0; y < nH; ++y)
            {
                for (int x = 0; x < nW; ++x)
                {
                    motoX = (int)(x * (w / (float)nW));//途中は小数点ありの計算で、最後にintキャストで小数点以下切り捨て
                    motoY = (int)(y * (h / (float)nH));
                    p = motoY * stride + (motoX * 4);//元画像での位置
                    nP = y * nStride + (x * 4);//変換後の位置
                    //変換後の色指定
                    nPixels[nP + 3] = pixels[p + 3];//アルファ
                    nPixels[nP + 2] = pixels[p + 2];//赤
                    nPixels[nP + 1] = pixels[p + 1];//緑
                    nPixels[nP] = pixels[p];//青
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }

        //        【画像処理】最近傍補間法の原理・計算式 | アルゴリズム雑記
        //https://algorithm.joho.info/image-processing/nearest-neighbor-linear-interpolation/
        //        画像処理 - HexeRein
        //http://www7a.biglobe.ne.jp/~fairytale/article/program/graphics.html#nearest_neighbour

        //グレースケール限定
        private BitmapSource F1最近傍補間法グレースケール(BitmapSource source, double xScale, double yScale)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;

            //変換後のサイズは四捨五入
            int nH = (int)(Math.Round((h * yScale), MidpointRounding.AwayFromZero));//普通のintキャストだと10*1.2で11.999になる
            int nW = (int)(Math.Round((w * xScale), MidpointRounding.AwayFromZero));
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            double xx, yy;
            int motoX, motoY;

            for (int y = 0; y < nH; ++y)
            {
                for (int x = 0; x < nW; ++x)
                {
                    //今/(変換後/元)、これならずれない
                    //xx = x / (nW / (float)w);
                    //yy = y / (nH / (float)h);

                    //今/(変換後/元)、これならずれない
                    xx = x * (w / (float)nW);
                    yy = y * (h / (float)nH);

                    //切り捨て
                    motoX = (int)xx;
                    motoY = (int)yy;
                    p = motoY * stride + motoX;

                    //四捨五入はズレる
                    //motoX = (int)(Math.Round(xx, MidpointRounding.AwayFromZero));
                    //motoY = (int)(Math.Round(yy, MidpointRounding.AwayFromZero));
                    //p = motoY * stride + motoX;

                    nP = y * nStride + (x * 1);
                    nPixels[nP] = pixels[p];
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
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