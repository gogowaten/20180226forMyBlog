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
using System.Windows.Controls.Primitives;
using System.IO;


namespace _20180128_8色に減色パターンエディタ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        EzNumericUpDown[][] MatrixNumericUpDown;
        const int MATRIX_ROWS = 4;
        const int MATRIX_COLUMNS = 4;
        EzNumericUpDown MaxRowsNumericUpDown;
        EzNumericUpDown MaxColumnsNumericUpDown;
        EzNumericUpDown DenominatorNumericUpDown;//分母

        string ImageFileFullPath;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            ButtonOrigin.Click += ButtonOrigin_Click;
            ButtonTest1.Click += ButtonTest1_Click;
            ButtonTest2.Click += ButtonTest2_Click;
            ButtonTest3.Click += ButtonTest3_Click;
            ButtonTest4.Click += ButtonTest4_Click;
            ButtonTest5.Click += ButtonTest5_Click;
            ButtonNotDithering.Click += ButtonNotDithering_Click;
            NumericScrollBar.ValueChanged += NumericScrollBar_ValueChanged;
            NumericTextBox.TextChanged += NumericTextBox_TextChanged;

            ButtonGetThresholdMatrix.Click += ButtonGetThresholdMatrix_Click;
            AddMatrixControls();
            SetMatrixNumericUpDown();
            AddNumericUpDown();

            AddButton();

            ComboBoxInitialize();
        }

        //
        private void AddButton()
        {
            var myStack = new StackPanel { Orientation = Orientation.Horizontal };
            var bt = new Button { Content = "変換", Width = 100 };
            bt.Click += ButtonGetThresholdMatrix_Click;
            myStack.Children.Add(bt);

            var b = new Button { Content = "保存", Width = 100 };
            b.Click += B_Click;
            myStack.Children.Add(b);
            MyStackPanel.Children.Add(myStack);
        }

        private void B_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            SaveImage();
        }

        //保存形式コンボボックス初期化
        private void ComboBoxInitialize()
        {
            ComboBpp.Items.Add("4bpp(16色)");
            ComboBpp.Items.Add("8bpp(256色)");
            ComboBpp.Items.Add("24bpp(フルカラー)");
            ComboBpp.Items.Add("32bpp(フルカラー)");
            ComboBpp.Items.Add("1bpp(白黒2値)");
            ComboBpp.SelectedIndex = 0;

        }
        private void ButtonGetThresholdMatrix_Click(object sender, RoutedEventArgs e)
        {
            //GetThresholdMatrix();
            if (OriginBitmap == null) { return; }
            Change8ColorsWithDithering(GetThresholdMatrix());
        }

        private double[][] GetThresholdMatrix()
        {
            int maxRow = MaxRowsNumericUpDown.Value;
            int maxCol = MaxColumnsNumericUpDown.Value;
            double[][] thresholdMatrix = new double[maxRow][];
            for (int i = 0; i < maxRow; ++i)
            {
                double[] temp = new double[maxCol];
                for (int j = 0; j < maxCol; ++j)
                {
                    temp[j] = (double)MatrixNumericUpDown[i][j].Value / DenominatorNumericUpDown.Value;
                }
                thresholdMatrix[i] = temp;
            }
            return thresholdMatrix;
        }

        private void AddMatrixControls()
        {
            MaxRowsNumericUpDown = new EzNumericUpDown(1, MATRIX_ROWS, MATRIX_ROWS);
            MaxColumnsNumericUpDown = new EzNumericUpDown(1, MATRIX_COLUMNS, MATRIX_COLUMNS);
            DenominatorNumericUpDown = new EzNumericUpDown(1, MATRIX_COLUMNS * MATRIX_ROWS + 1, MATRIX_COLUMNS * MATRIX_ROWS + 1);
            MaxRowsNumericUpDown.NumericScrollBar.ValueChanged += ChangeRowColumnCount;
            MaxColumnsNumericUpDown.NumericScrollBar.ValueChanged += ChangeRowColumnCount;

            var sp = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };
            sp.Orientation = Orientation.Horizontal;
            var tb = new TextBlock { Text = "行", VerticalAlignment = VerticalAlignment.Center };
            sp.Children.Add(tb);
            sp.Children.Add(MaxRowsNumericUpDown);
            sp.Children.Add(new TextBlock { Text = "列", VerticalAlignment = VerticalAlignment.Center });
            sp.Children.Add(MaxColumnsNumericUpDown);
            sp.Children.Add(new TextBlock { Text = "分母", VerticalAlignment = VerticalAlignment.Center });
            sp.Children.Add(DenominatorNumericUpDown);
            MyStackPanel.Children.Add(sp);
        }

        private void ChangeMaxValue()
        {
            int maxValue = MaxRowsNumericUpDown.Value * MaxColumnsNumericUpDown.Value + 1;
            for (int row = 0; row < MatrixNumericUpDown.Length; ++row)
            {
                for (int col = 0; col < MatrixNumericUpDown[row].Length; ++col)
                {
                    MatrixNumericUpDown[row][col].Maximum = maxValue;
                }
            }
        }
        private void ChangeRowColumnCount(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int maxRow = MaxRowsNumericUpDown.Value;
            int maxCol = MaxColumnsNumericUpDown.Value;
            for (int row = 0; row < MATRIX_ROWS; ++row)
            {
                for (int col = 0; col < MATRIX_COLUMNS; ++col)
                {
                    if (maxRow - 1 < row || maxCol - 1 < col)
                    {
                        MatrixNumericUpDown[row][col].IsEnabled = false;
                    }
                    else
                    {
                        MatrixNumericUpDown[row][col].IsEnabled = true;
                    }
                }
            }
            
            DenominatorNumericUpDown.Maximum = maxRow * maxCol + 1;
                        
            //DenominatorNumericUpDown.Value = maxRow * maxCol + 1;
            ChangeMaxValue();
        }

        private void SetMatrixNumericUpDown()
        {
            MatrixNumericUpDown = new EzNumericUpDown[MATRIX_ROWS][];

            for (int i = 0; i < MATRIX_ROWS; ++i)
            {
                EzNumericUpDown[] stackPanel = new EzNumericUpDown[MATRIX_COLUMNS];
                for (int j = 0; j < MATRIX_COLUMNS; ++j)
                {
                    stackPanel[j] = new EzNumericUpDown(0, MATRIX_COLUMNS * MATRIX_ROWS + 1);
                }
                MatrixNumericUpDown[i] = stackPanel;
            }
        }


        private void AddNumericUpDown()
        {
            for (int j = 0; j < MatrixNumericUpDown.Length; ++j)
            {
                var panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                for (int i = 0; i < MatrixNumericUpDown[j].Length; ++i)
                {
                    panel.Children.Add(MatrixNumericUpDown[j][i]);
                }
                MyStackPanel.Children.Add(panel);
            }
        }


        #region ボタンイベント


        private void ButtonTest5_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            Matrix5_Bayer3x3();
        }

        private void ButtonTest4_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            Matrix4_Bayer2x2();
        }

        private void ButtonTest3_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            Matrix3_Bayer2x2();
        }

        private void ButtonTest2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            Matrix2_Bayer4x4();
        }

        private void ButtonTest1_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            Matrix1_Bayer2x2();
        }
        //ディザなしボタンクリック時
        private void ButtonNotDithering_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MatrixThreshold();
        }
        #endregion

        //		正規表現の基本 - .NET Tips(VB.NET, C#...)
        //https://dobon.net/vb/dotnet/string/regex.html

        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            double d;
            if (!double.TryParse(textBox.Text, out d))
            {
                textBox.Text = System.Text.RegularExpressions.Regex.Replace(textBox.Text, "[^0-9]", "");
            }
        }

        private void NumericScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OriginBitmap == null) { return; }
            MatrixThreshold();
        }

        private void ButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }

        //画像ファイルドロップ時        
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

        //
        private void MatrixThreshold()
        {
            double[][] thresholdMap = new double[][]
            {
                new double[] { NumericScrollBar.Value / 255 }
            };
            Change8ColorsWithDithering(thresholdMap);
        }


        //指定したしきい値行列をNumericにセット
        private void SetThreshold(int[][] thresholdMatrix)
        {
            int maxRow = thresholdMatrix.Length;
            int maxCol = thresholdMatrix[0].Length;
            for (int y = 0; y < maxRow; ++y)
            {
                for (int x = 0; x < maxCol; ++x)
                {
                    MatrixNumericUpDown[y][x].Value = thresholdMatrix[y][x];
                }
            }
            MaxRowsNumericUpDown.Value = maxRow;
            MaxColumnsNumericUpDown.Value = maxCol;
            DenominatorNumericUpDown.Value = maxCol * maxRow + 1;
        }

        //2x2ディザ
        private void Matrix1_Bayer2x2()
        {
            if (CheckBoxLinkMatrix.IsChecked == false)
            {

                double[][] thresholdMap = new double[][]
                {
                new double[] { 1f / 5f, 3f / 5f },
                new double[] { 4f / 5f, 2f / 5f }
                };
                Change8ColorsWithDithering(thresholdMap);
            }
            else
            {
                int[][] thresholdMatrix = new int[][]
                {
                new int[] { 1, 3 },
                new int[] { 4, 2 }
                };
                SetThreshold(thresholdMatrix);
                Change8ColorsWithDithering(GetThresholdMatrix());
            }

        }

        //4x4ディザ
        private void Matrix2_Bayer4x4()
        {
            if (CheckBoxLinkMatrix.IsChecked == false)
            {
                double[][] thresholdMap = new double[][]
                {
                new double[] { 1f / 17f, 13f / 17f, 4f / 17f, 16f / 17f },
                new double[] { 9f / 17f, 5f / 17f, 12f / 17f, 8f / 17f },
                new double[] { 3f / 17f, 15f / 17f, 2f / 17f, 14f / 17f },
                new double[] { 11f / 17f, 7f / 17f, 10f / 17f, 6f / 17f }
                };
                Change8ColorsWithDithering(thresholdMap);

            }
            else
            {
                int[][] thresholdMatrix = new int[][]
                {
                new int[] { 1, 13 ,4 ,16 },
                new int[] { 9, 5 ,12 ,8 },
                new int[] { 3, 15 ,2 ,14 },
                new int[] { 11, 7 ,10 ,6 }
                };
                SetThreshold(thresholdMatrix);
                Change8ColorsWithDithering(GetThresholdMatrix());

            }
        }

        //4x4ハーフトーン型
        private void Matrix3_Bayer2x2()
        {
            if (CheckBoxLinkMatrix.IsChecked == false)
            {
                double[][] thresholdMap = new double[][]
                {
                new double[] { 11f / 17f, 5f / 17f, 7f / 17f, 9f / 17f },
                new double[] { 13f / 17f, 1f / 17f, 3f / 17f, 15f / 17f },
                new double[] { 8f / 17f, 10f / 17f, 12f / 17f, 6f / 17f },
                new double[] { 4f / 17f, 16f / 17f, 14f / 17f, 2f / 17f }
                };
                Change8ColorsWithDithering(thresholdMap);
            }
            else
            {
                int[][] thresholdMatrix = new int[][]
                {
                new int[] { 11, 5, 7, 9 },
                new int[] { 13, 1, 3, 15 },
                new int[] { 8, 10 ,12 ,6 },
                new int[] { 4, 16 ,14 ,2 }
                };
                SetThreshold(thresholdMatrix);
                Change8ColorsWithDithering(GetThresholdMatrix());
            }
        }

        //4x4渦巻き型
        private void Matrix4_Bayer2x2()
        {
            if (CheckBoxLinkMatrix.IsChecked == false)
            {
                double[][] thresholdMap = new double[][]
                {
                new double[] { 14f / 17f, 8f / 17f, 7f / 17f, 13f / 17f },
                new double[] { 9f / 17f, 2f / 17f, 1f / 17f, 6f / 17f },
                new double[] { 10f / 17f, 3f / 17f, 4f / 17f, 5f / 17f },
                new double[] { 15f / 17f, 11f / 17f, 12f / 17f, 16f / 17f }
                };
                Change8ColorsWithDithering(thresholdMap);
            }
            else
            {
                int[][] thresholdMatrix = new int[][]
                {
                new int[] { 14, 8 ,7 ,13 },
                new int[] { 9, 2 ,1 ,6 },
                new int[] { 10, 3 ,4 ,5 },
                new int[] { 15, 11 ,12 ,16 }
                };
                SetThreshold(thresholdMatrix);
                Change8ColorsWithDithering(GetThresholdMatrix());
            }
        }


        //変則3x3ディザ
        private void Matrix5_Bayer3x3()
        {
            if (CheckBoxLinkMatrix.IsChecked == false)
            {
                double[][] thresholdMap = new double[][]
                {
                new double[] { 1f / 10f, 9f / 10f, 6f / 10f },
                new double[] { 8f / 10f, 2f / 10f, 6f / 10f },
                new double[] { 4f / 10f, 7f / 10f, 3f / 10f }
                };
                Change8ColorsWithDithering(thresholdMap);
            }
            else
            {
                int[][] thresholdMatrix = new int[][]
               {
                    new int[] { 1, 9 ,5 },
                    new int[] { 8, 2 ,6 },
                    new int[] { 4, 7 ,3 }
               };
                SetThreshold(thresholdMatrix);
                Change8ColorsWithDithering(GetThresholdMatrix());
            }
        }





        //ディザパターン(行列)を使って8色に減色
        //PixelFormatがPbgra32のBitmapSourceだけが対象
        private void Change8ColorsWithDithering(double[][] thresholdMap)
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;

            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;//指定ピクセルの配列の位置
            int xx = thresholdMap[0].Length;//しきい値行列の横の要素数
            int yy = thresholdMap.Length;   //しきい値行列の縦の要素数
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    for (int i = 0; i < 3; ++i)
                    {
                        if ((double)pixels[p + i] / 256 < thresholdMap[y % yy][x % xx])
                        { pixels[p + i] = 0; }
                        else { pixels[p + i] = 255; }
                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }



        //画像保存
        private void SaveImage()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.bmp|*.bmp|*.gif|*.gif|*.tiff|*.tiff|*.wdp|*.wdp;*jxr";
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
                        encoder = new BmpBitmapEncoder();
                        break;
                    case 3:
                        encoder = new GifBitmapEncoder();
                        break;
                    case 4:
                        encoder = new TiffBitmapEncoder();
                        break;
                    case 5:
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
            return fileName + "_";
        }
        //対応したPixelFormatに変換する
        private BitmapSource GetSaveImage()
        {
            PixelFormat pixelFormat;
            switch (ComboBpp.SelectedIndex)
            {
                case 0:
                    pixelFormat = PixelFormats.Indexed4;
                    break;
                case 1:
                    pixelFormat = PixelFormats.Indexed8;
                    break;
                case 2:
                    pixelFormat = PixelFormats.Bgr24;
                    break;
                case 3:
                    pixelFormat = PixelFormats.Pbgra32;
                    break;
                case 4:
                    pixelFormat = PixelFormats.BlackWhite;
                    break;

                default:
                    pixelFormat = PixelFormats.Pbgra32;
                    break;
            }
            return new FormatConvertedBitmap((BitmapSource)MyImage.Source, pixelFormat, null, 0);
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












    public class EzNumericUpDown : StackPanel
    {
        public TextBox NumericTextBox;
        public ScrollBar NumericScrollBar;
        public int Maximum
        {
            get { return (int)NumericScrollBar.Maximum; }
            set { NumericScrollBar.Maximum = value; }
        }

        public int Minimum
        {
            get { return (int)NumericScrollBar.Minimum; }
            set { NumericScrollBar.Minimum = value; }
        }

        public int Value
        {
            get { return (int)NumericScrollBar.Value; }
            set { NumericScrollBar.Value = value; }
        }

        public EzNumericUpDown(int min = 0, int max = 1, int value = 1)
        {
            NumericScrollBar = new ScrollBar
            {
                SmallChange = 1,
                LargeChange = 1,
                Minimum = min,
                Maximum = max,
                Value = value,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(180)
            };

            NumericTextBox = new TextBox
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Width = 30
            };

            var binding = new Binding();
            binding.Source = NumericScrollBar;
            binding.Path = new PropertyPath(ScrollBar.ValueProperty);
            binding.Mode = BindingMode.TwoWay;
            NumericTextBox.SetBinding(TextBox.TextProperty, binding);
            NumericTextBox.GotFocus += NumericTextBox_GotFocus; ;

            this.Orientation = Orientation.Horizontal;
            this.Children.Add(NumericTextBox);
            this.Children.Add(NumericScrollBar);

            //マウスホイールで値増減
            NumericScrollBar.MouseWheel += NumericScrollBar_MouseWheel;
            NumericTextBox.MouseWheel += NumericScrollBar_MouseWheel;
        }

        private void NumericTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //NumericTextBox.SelectAll();
            TextBox box = (TextBox)sender;
            this.Dispatcher.InvokeAsync(() => { Task.Delay(10); box.SelectAll(); });
        }


        //マウスホイールで値増減
        private void NumericScrollBar_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                NumericScrollBar.Value++;
            }
            else
            {
                NumericScrollBar.Value--;
            }
        }
    }
}