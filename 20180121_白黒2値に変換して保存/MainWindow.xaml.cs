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
using System.Windows.Controls.Primitives;

//カラー画像を1bpp(1bit)白黒画像に変換アプリver1.1、閾値の自動設定、大津の二値化でできたかも(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15779372.html

namespace _20180121_白黒2値に変換して保存
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        int[] MyHistogram;
        string ImageFileFullPath;
        bool IsBinary = false;

        public MainWindow()
        {
            InitializeComponent();
//            自分自身のバージョン情報を取得する - .NET Tips(VB.NET, C#...)
//https://dobon.net/vb/dotnet/file/myversioninfo.html

            var neko = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);            
            this.Title = neko.ProductName + " ver."+ neko.FileVersion;
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            AddComboBoxItem();//コンボボックス初期化  


            ButtonSaveImage.Click += ButtonSaveImage_Click;
            ButtonAverageBrightness.Click += ButtonAverageBrightness_Click;
            ButtonAuto.Click += ButtonAuto_Click;
            ButtonAuto2.Click += ButtonAuto2_Click;
            ButtonToOriginImage.Click += ButtonToOriginImage_Click;
            ButtonEXE.Click += ButtonEXE_Click;
            ScrollNumeric.ValueChanged += ScrollNumeric_ValueChanged;
            ScrollNumeric.MouseWheel += ScrollNumeric_MouseWheel;
            TextNumeric.GotFocus += TextNumeric_GotFocus;
            TextNumeric.MouseWheel += TextNumeric_MouseWheel;
        }

        //2値化実行
        private void ButtonEXE_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ChangeBlackWhite();
            IsBinary = true;
        }

        //元のグレースケール画像に戻す
        private void ButtonToOriginImage_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            IsBinary = false;
            MyImage.Source = OriginBitmap;
        }

        private void ButtonAuto2_Click(object sender, RoutedEventArgs e)
        {
            Auto2();
        }

        private void ButtonAuto_Click(object sender, RoutedEventArgs e)
        {
            AutoBW();
        }

        //テキストボックの上でマウスホイール回でスクロールバーの値を上下
        private void TextNumeric_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Binding binding = BindingOperations.GetBinding(textBox, TextBox.TextProperty);
            ScrollBar scrollBar = (ScrollBar)this.FindName(binding.ElementName);
            if (e.Delta > 0) { scrollBar.Value += 10; }
            else { scrollBar.Value -= 10; }
        }

        //スクロールバーの上でマウスホイール回転で値を上下
        private void ScrollNumeric_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollBar scrollBar = (ScrollBar)sender;
            if (e.Delta > 0) { scrollBar.Value++; }
            else { scrollBar.Value--; }
        }

        //テキストボックスクリック時にテキスト全選択
        private void TextNumeric_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            this.Dispatcher.InvokeAsync(() => { Task.Delay(10); box.SelectAll(); });
        }

        //画像の平均輝度をしきい値に設定
        private void ButtonAverageBrightness_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            int threshold=(int) GetAvegareBrightness(OriginBitmap);
            if (ScrollNumeric.Value == threshold) { ChangeBlackWhite(); }
            else { ScrollNumeric.Value = threshold; }           
        }

        //スクロールバーの値変更したら画像の再2値化
        private void ScrollNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OriginBitmap == null) { return; }
            ChangeBlackWhite();
        }



        //画像保存ボタン
        private void ButtonSaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            if (IsBinary == false) { return; }
            SaveImage();
        }

        //画像ファイルがドロップされた時
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            //8bitグレースケールに変換して読み込む
            OriginBitmap = GetBitmapSourceWithCangePixelFormat2(filePath[0], PixelFormats.Gray8, 96, 96);
            if (OriginBitmap == null)
            {
                MessageBox.Show("not Image");
            }
            else
            {
                MyImage.Source = OriginBitmap;
                MyHistogram = MakeHistogram(OriginBitmap);//ヒストグラム取得
                ImageFileFullPath = System.IO.Path.GetFullPath(filePath[0]);//ファイルのフルパス取得
            }
        }

        //コンボボックス初期化
        private void AddComboBoxItem()
        {
            ComboboxTiffCompress.Items.Add(TiffCompressOption.Ccitt3);
            ComboboxTiffCompress.Items.Add(TiffCompressOption.Ccitt4);
            ComboboxTiffCompress.Items.Add(TiffCompressOption.Default);
            ComboboxTiffCompress.Items.Add(TiffCompressOption.Lzw);
            ComboboxTiffCompress.Items.Add(TiffCompressOption.None);
            ComboboxTiffCompress.Items.Add(TiffCompressOption.Rle);
            ComboboxTiffCompress.Items.Add(TiffCompressOption.Zip);
            ComboboxTiffCompress.SelectedItem = TiffCompressOption.Default;

        }



        /// <summary>
        /// PixelFormatがGray8(8bitグレースケール)のBitmapSourceを閾値で白黒2値に変換
        /// </summary>
        /// <param name="source">PixelFormatがGray8のBitmapSource</param>
        /// <param name="threshold">しきい値</param>
        private void BlackWhite2(BitmapSource source, int threshold)
        {
            int w = source.PixelWidth;
            int h = source.PixelHeight;
            int stride = w;//1ピクセル行のbyte数を指定、Gray8は1ピクセル8bitなのでw * 8 / 8 = w
            byte[] pixels = new byte[h * stride];
            source.CopyPixels(pixels, stride, 0);
            //しきい値で白黒つける
            for (int i = 0; i < pixels.Length; ++i)
            {
                if (pixels[i] < threshold)
                {
                    pixels[i] = 0;
                }
                else
                {
                    pixels[i] = 255;
                }
            }

            //BitmapSource作成、ここでPixelFormatをBlackWhiteにすると画像が崩れるのでそのままで作成
            BitmapSource newBitmap = BitmapSource.Create(w, h, source.DpiX, source.DpiY, source.Format, null, pixels, stride);

            //PixelFormatをBlackWhiteに変換	
            FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(newBitmap, PixelFormats.BlackWhite, null, 0);

            MyImage.Source = convertedBitmap;//表示
        }

        //画像を2値化
        private void ChangeBlackWhite()
        {
            IsBinary = true;
            BlackWhite2(OriginBitmap, (int)ScrollNumeric.Value);
        }



        //        「名前を付けて保存」ダイアログボックスを表示する - .NET Tips(VB.NET, C#...)
        //https://dobon.net/vb/dotnet/form/savefiledialog.html
        //画像保存
        private void SaveImage()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.jpg|*.jpg;*.jpeg|*.bmp|*.bmp|*.gif|*.gif|*.tiff|*.tiff|*.wdp|*.wdp;*jxr";
            saveFileDialog.AddExtension = true;//ファイル名に拡張子追加
            //初期フォルダ指定、開いている画像と同じフォルダ
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(ImageFileFullPath);
            saveFileDialog.FileName = GetSaveFileName();
            if (saveFileDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = null;
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        encoder = new PngBitmapEncoder();
                        break;
                    case 2:
                        encoder = new JpegBitmapEncoder();
                        break;
                    case 3:
                        encoder = new BmpBitmapEncoder();
                        break;
                    case 4:
                        encoder = new GifBitmapEncoder();
                        break;
                    case 5:
                        //tiffは圧縮方式をコンボボックスから取得
                        var tiff = new TiffBitmapEncoder();
                        tiff.Compression = (TiffCompressOption)ComboboxTiffCompress.SelectedItem;
                        encoder = tiff;
                        break;
                    case 6:
                        //wmpはロスレス指定、じゃないと1bppで保存時に画像が崩れるしファイルサイズも大きくなる
                        var wmp = new WmpBitmapEncoder();
                        wmp.ImageQualityLevel = 1.0f;
                        encoder = wmp;
                        break;
                    default:
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(GetSaveImage()));
                using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(fs);
                }

            }
        }

        //保存時の初期ファイル名取得
        private string GetSaveFileName()
        {
            string fileName = "";
            fileName = System.IO.Path.GetFileNameWithoutExtension(ImageFileFullPath);
            if (Radio1bpp.IsChecked == true) { fileName += "_1bpp白黒2値"; }
            else if (Radio8bpp.IsChecked == true) { fileName += "_8bpp白黒2値"; }
            else { fileName += "_32bpp白黒2値"; }
            return fileName;
        }

        //保存画像を取得、表示している2値化画像を指定のbppに対応したPixelFormatに変換する
        private BitmapSource GetSaveImage()
        {
            PixelFormat pixelFormat;
            if (Radio1bpp.IsChecked == true) { pixelFormat = PixelFormats.BlackWhite; }
            else if (Radio8bpp.IsChecked == true) { pixelFormat = PixelFormats.Gray8; }
            else { pixelFormat = PixelFormats.Bgr32; }

            return new FormatConvertedBitmap((BitmapSource)MyImage.Source, pixelFormat, null, 0);
        }

        private BitmapSource ChangePixelFormat(PixelFormat pixelFormat)
        {
            return new FormatConvertedBitmap((BitmapSource)MyImage.Source, pixelFormat, null, 0);
        }




        //画像全体の平均輝度取得
        private double GetAvegareBrightness(BitmapSource source)
        {

            long pixelsCount = 0;
            long totalBrightness = 0;
            for (int i = 0; i < MyHistogram.Length; ++i)
            {
                pixelsCount += MyHistogram[i];
                totalBrightness += MyHistogram[i] * i;
            }
            return totalBrightness / pixelsCount;
        }

        //未使用
        //全体の輝度値の平均をしきい値にする
        private int GetThresholdAverage(BitmapSource source)
        {

            int Pixels画素数 = 0;
            long Sum累計輝度 = 0;
            for (int i = 0; i < 256; ++i)
            {
                Pixels画素数 += MyHistogram[i];
                Sum累計輝度 += MyHistogram[i] * i;
            }
            int Ave平均輝度 = 0;//平均
            Ave平均輝度 = (int)(Sum累計輝度 / Pixels画素数);
            return Ave平均輝度;
        }


        //大津の2値化？
        private void Auto2()
        {
            if (OriginBitmap == null) { return; }
            IsBinary = true;
            int threshold = 1;
            double max = 0;            
            int countAll = CountHistogram(0, 256);
            for (int i = 0; i < 256; i++)
            {
                int count1 = CountHistogram(0, i);
                int count2 = CountHistogram(i, 256);

                if (count1 != 0 && count2 != 0)
                {
                    double ratio1 = count1 / (double)countAll;//a画素割合
                    double ratio2 = count2 / (double)countAll;//b画素割合
                    double ave1 = AverageHistogram(0, i, count1);//a輝度平均
                    double ave2 = AverageHistogram(i, 256, count2);//b輝度平均
                    double diffAvePow = (ave1 - ave2) * (ave1 - ave2);
                    double aa = diffAvePow * ratio1 * ratio2;
                    if (max < aa)
                    {
                        max = aa;
                        threshold = i;
                    }
                }
            }
            if (ScrollNumeric.Value == threshold) { ChangeBlackWhite(); }
            else { ScrollNumeric.Value = threshold; }

        }

        //輝度平均最小値を閾値にする
        private void AutoBW()
        {
            if (OriginBitmap == null) { return; }
            IsBinary = true;
            int threshold = 1;
            double min = double.MaxValue;
            double allAve = AverageHistogram(0, 256, CountHistogram(0, 256));
            for (int i = 1; i < 256; i++)
            {
                int count1 = CountHistogram(0, i);
                int count2 = CountHistogram(i, 256);
                if (count1 != 0 && count2 != 0)
                {
                    double ave1 = AverageHistogram(0, i, count1);
                    double ave2 = AverageHistogram(i, 256, count2);
                    double diffAve = Math.Abs(Math.Abs(ave1 - allAve) - Math.Abs(ave2 - allAve));
                    if (min > diffAve)
                    {
                        min = diffAve;
                        threshold = i;
                    }
                }
            }
            if (ScrollNumeric.Value == threshold) { ChangeBlackWhite(); }
            else { ScrollNumeric.Value = threshold; }

        }

        //未使用
        //
        /// <summary>
        /// ヒストグラムから指定範囲の分散を計算
        /// </summary>
        /// <param name="begin">範囲の始まり</param>
        /// <param name="end">範囲の終わり</param>
        /// <param name="count">範囲の画素数</param>
        /// <param name="average">範囲の平均値</param>
        /// <returns></returns>
        private double VarianceHistogram(int begin, int end, int count, double average)
        {
            double total = 0;
            for (int i = begin; i < end; i++)
            {
                int val = MyHistogram[i];
                double diviation = i - average;
                total += diviation * diviation * i;
            }
            return total / count;
        }

        //
        /// <summary>
        /// ヒストグラムから指定範囲の平均輝度値
        /// </summary>
        /// <param name="begin">範囲の始まり</param>
        /// <param name="end">範囲の終わり</param>
        /// <param name="count">範囲内の画素数</param>
        /// <returns></returns>
        private double AverageHistogram(int begin, int end, int count)
        {
            double total = 0;
            for (int i = begin; i < end; i++)
            {
                total += i * MyHistogram[i];
            }
            return total / count;
        }

        //
        /// <summary>
        /// ヒストグラムから指定範囲のピクセルの個数
        /// </summary>
        /// <param name="begin">範囲の始まり</param>
        /// <param name="end">範囲の終わり</param>
        /// <returns></returns>
        private int CountHistogram(int begin, int end)
        {
            int count = 0;
            for (int i = begin; i < end; i++)
            {
                int val = MyHistogram[i];
                if (val != 0) { count += val; }
            }
            return count;
        }

        //ヒストグラム作成、PixelFormatがGray8専用
        private int[] MakeHistogram(BitmapSource source)
        {
            int[] histogram = new int[256];
            int w = source.PixelWidth;
            int h = source.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * w];
            source.CopyPixels(pixels, stride, 0);
            for (int i = 0; i < pixels.Length; ++i)
            {
                histogram[pixels[i]]++;
            }
            return histogram;
        }


        /// <summary>
        ///  ファイルパスとPixelFormatを指定してBitmapSourceを取得、dpiの変更は任意
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsの中からどれかを指定</param>
        /// <param name="dpiX">無指定なら画像ファイルで指定されているdpiになる</param>
        /// <param name="dpiY">無指定なら画像ファイルで指定されているdpiになる</param>
        /// <returns></returns>
        private BitmapSource GetBitmapSourceWithCangePixelFormat2(
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
