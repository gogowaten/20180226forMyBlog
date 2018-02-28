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

//単純減色(ポスタライズ)にオーダード(パターン)ディザリング、WPFとC# ( ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15391499.html

namespace _20180227_単純減色にパターンディザ
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
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            ButtonConvert.Click += ButtonConvert_Click;

            ButtonConvertWith1x1Dither.Click += ButtonConvertWith1x1Dither_Click;
            ButtonConvertWith2x2Dither.Click += ButtonConvertWith2x2Dither_Click;
            ButtonConvertWith4x4Dither.Click += ButtonConvertWith4x4Dither_Click;
            ButtonOrigin.Click += ButtonOrigin_Click;
            NumericScrollBar.ValueChanged += NumericScrollBar_ValueChanged;
            NumericScrollBar.MouseWheel += NumericScrollBar_MouseWheel;
            NumericTextBox.MouseWheel += NumericTextBox_MouseWheel;
            NumericTextBox.GotFocus += NumericTextBox_GotFocus;
            NumericTextBox.TextChanged += NumericTextBox_TextChanged;

        }


        //2x2ディザパターンのしきい値行列
        private float[][] Get2x2ditherMatrix()
        {
            return new float[][]
            {
                new float[]{ 1f / 5f, 3f / 5f },
                new float[]{ 4f / 5f, 2f / 5f }
            };
        }

        //ディザパターンなし
        private float[][] Get1x1ditherMatrix()
        {
            return new float[][] { new float[] { 1f / 2f } };
        }

        //4x4ディザパターンのしきい値行列
        private float[][] Get4x4ditherMatrix()
        {
            return new float[][] {
                new float[] { 1f / 17f, 13f / 17f, 4f / 17f, 16f / 17f },
                new float[] { 9f / 17f, 5f / 17f, 12f / 17f, 8f / 17f },
                new float[] { 3f / 17f, 15f / 17f, 2f / 17f, 14f / 17f },
                new float[] { 11f / 17f, 7f / 17f, 10f / 17f, 6f / 17f }
            };
        }

        /// <summary>
        /// 単純減色にディザパターンを使う
        /// </summary>
        /// <param name="ditherMatrix">ディザパターン、中の数値は0から1を指定</param>
        /// <param name="source">変換する画像</param>
        /// <param name="division">階調数、各RGBの分割数、2から256を指定</param>
        private BitmapSource SimpleGensyokuWithPatternDither(float[][] ditherMatrix, BitmapSource source, int division)
        {
            //変換対応表取得
            byte[][][] converter = GetConverterArray4(division, ditherMatrix);
            //converter[][][]
            //index[ディザパターンの縦位置] [横位置] [変換前の値]
            //value[処理ピクセルの縦位置] [横位置] [変換後の値]

            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixles = new byte[h * stride];
            wb.CopyPixels(pixles, stride, 0);
            long p = 0;//処理ピクセルの配列の中での位置
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    //対応表に当てはめて色変換
                    for (int i = 0; i < 3; ++i)//RGB各色
                    {
                        pixles[p + i] = converter[y % converter.Length][x % converter[0].Length][pixles[p + i]];
                        //var neko = x % converter[0].Length;
                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixles, stride, 0);
            return wb;
        }


        //パターンディザも追加
        /// <summary>
        /// 未使用
        /// </summary>
        private void GensyokuNumeric3Table()
        {
            //ディザパターンのしきい値行列
            float[][] ditherMatrix = new float[][]
            {
                new float[]{ 1f / 5f, 3f / 5f },
                new float[]{ 4f / 5f, 2f / 5f }
                    
                //new float[] { 1f / 17f, 13f / 17f, 4f / 17f, 16f / 17f },
                //new float[] { 9f / 17f, 5f / 17f, 12f / 17f, 8f / 17f },
                //new float[] { 3f / 17f, 15f / 17f, 2f / 17f, 14f / 17f },
                //new float[] { 11f / 17f, 7f / 17f, 10f / 17f, 6f / 17f }
                    
                //new float[] {1f/2f}

            };


            int division = (int)NumericScrollBar.Value;//分割数
            float freqency = 255f / (division - 1f);//階調間の差
            //変換対応表取得
            byte[][][] converter = GetConverterArray4(division, ditherMatrix);
            //converter[][][]
            //index[ディザパターンの縦位置] [横位置] [変換前の値]
            //value[処理ピクセルの縦位置] [横位置] [変換後の値]

            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixles = new byte[h * stride];
            wb.CopyPixels(pixles, stride, 0);
            long p = 0;//処理ピクセルの配列の中での位置
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    //対応表に当てはめて色変換
                    for (int i = 0; i < 3; ++i)//RGB各色
                    {
                        pixles[p + i] = converter[y % converter.Length][x % converter[0].Length][pixles[p + i]];
                        var neko = x % converter[0].Length;

                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixles, stride, 0);
            MyImage.Source = wb;

        }



        /// <summary>
        /// 階調数とディザパターンの閾値から変換一覧作成
        /// </summary>
        /// <param name="division">階調数</param>
        /// <param name="threshold">ディザパターンの閾値</param>
        /// <returns></returns>
        private byte[] GetConverterArray3_2(int division, float threshold)
        {
            //階調数4でディザパターン行列の閾値が0.2のとき
            //値は255/(4-1)=85が単位になるので0,85,170,255
            //最初の閾値は255/階調数/0.5*行列の閾値なので
            //255/4/0.5*0.2=25.5
            //続く閾値はこれに255/4=63.75を足していく
            //25.5、89.25、153、216.75

            //閾値一覧作成
            float[] range = new float[division + 1];
            float threshold最初 = (float)(255f / division / 0.5 * threshold);
            range[0] = threshold最初;
            for (int i = 1; i < range.Length; ++i)
            {
                range[i] = threshold最初 + (255f / division * i);
            }

            //指定値一覧作成
            float colorFrequency = 255f / (division - 1f);//色の指定値の間隔
            byte[] color = new byte[division + 1];//一個余裕を持たせて最後には255を入れておく
            for (int i = 0; i < color.Length; ++i)
            {
                if (i * colorFrequency >= 255)
                {
                    color[i] = 255;
                }
                else
                {
                    //color[i] = (byte)(i * colorFrequency);
                    //四捨五入してからbyteにキャストォォぉ
                    color[i] = (byte)Math.Round(
                        (i * colorFrequency), MidpointRounding.AwayFromZero);
                }
            }

            //変換一覧作成
            byte[] neko = new byte[256];
            int thresholdIndex = 0;//閾値一覧のindex
            for (int i = 0; i < neko.Length; ++i)
            {
                //しきい値を超えたら次の閾値に変更する
                if (i > range[thresholdIndex])
                {
                    thresholdIndex++;
                }
                neko[i] = color[thresholdIndex];
            }

            return neko;
        }

        //閾値行列から変換一覧作成
        private byte[][][] GetConverterArray4(int division, float[][] ditherMatrix)
        {
            byte[][][] converterArray = new byte[ditherMatrix.Length][][];
            for (int i = 0; i < ditherMatrix.Length; ++i)
            {
                converterArray[i] = new byte[ditherMatrix[i].Length][];
                for (int j = 0; j < ditherMatrix[i].Length; ++j)
                {
                    converterArray[i][j] = GetConverterArray3_2(division, ditherMatrix[i][j]);
                }
            }

            return converterArray;
        }



        /// <summary>
        /// 単純減色の変換対応表作成
        /// </summary>
        /// <param name="division">分割数(階調数)</param>
        /// <returns></returns>
        private byte[] GetConverterArray(int division)
        {
            //範囲            
            float frequency = 256f / division;//1範囲の大きさ、周波数
            float[] range = new float[division + 1];//3分割なら0，85，170，255になる
            for (int i = 0; i < range.Length; ++i)
            {
                range[i] = i * frequency;
            }

            //指定する値
            frequency = 255f / (division - 1);
            byte[] color = new byte[division];//3分割なら0，127，255になる
            for (int i = 0; i < color.Length; ++i)
            {
                color[i] = (byte)(i * frequency);
            }

            //元の256階調全てに対する変換結果の配列作成、対応表
            byte[] converter = new byte[256];
            int j = 0;
            for (int i = 0; i < 256; ++i)
            {
                if (i >= range[j + 1])
                {
                    j++;
                }
                converter[i] = color[j];
            }
            return converter;
        }
        //        ポスタリゼーション（階調変更）
        //http://www.sm.rim.or.jp/~shishido/post.html
        //対応表を作成しておいて、それに当てはめて判定、速い
        private void GensyokuNumeric2Table()
        {
            int division = (int)NumericScrollBar.Value;//分割数
            //変換対応表取得
            byte[] converter = GetConverterArray(division);

            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixles = new byte[h * stride];
            wb.CopyPixels(pixles, stride, 0);
            long p = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    //対応表に当てはめて色変換
                    pixles[p + 2] = converter[pixles[p + 2]];
                    pixles[p + 1] = converter[pixles[p + 1]];
                    pixles[p + 0] = converter[pixles[p + 0]];
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixles, stride, 0);
            MyImage.Source = wb;
            TextBlockColorCount.Text = Math.Pow(division, 3).ToString();
        }









        #region イベント


        private void ButtonConvertWith4x4Dither_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = SimpleGensyokuWithPatternDither(Get4x4ditherMatrix(), OriginBitmap, (int)NumericScrollBar.Value);
        }

        private void ButtonConvertWith2x2Dither_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = SimpleGensyokuWithPatternDither(Get2x2ditherMatrix(), OriginBitmap, (int)NumericScrollBar.Value);
        }

        private void ButtonConvertWith1x1Dither_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = SimpleGensyokuWithPatternDither(Get1x1ditherMatrix(), OriginBitmap, (int)NumericScrollBar.Value);
        }

        private void NumericTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            this.Dispatcher.InvokeAsync(() => { Task.Delay(10); box.SelectAll(); });
        }

        private void NumericScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBlockColorCount.Text = Math.Pow(NumericScrollBar.Value, 3).ToString();
        }



        private void ButtonConvert_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            GensyokuNumeric2Table();
        }

        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            double d;
            if (!double.TryParse(textBox.Text, out d))
            {
                textBox.Text = System.Text.RegularExpressions.Regex.Replace(textBox.Text, "[^0-9]", "");
            }
        }


        private void NumericTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { NumericScrollBar.Value++; }
            else { NumericScrollBar.Value--; }
        }

        private void NumericScrollBar_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { NumericScrollBar.Value++; }
            else { NumericScrollBar.Value--; }
        }

        private void ButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }


        //画像ファイルドロップ時
        //PixelFormat.Pbgr32に変換してBitmapSource取得
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
            }
        }
        #endregion

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
