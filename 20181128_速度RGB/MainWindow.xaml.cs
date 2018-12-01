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

//WPF、Parallel.Invoke、並列実行は昨日より速く(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15775527.html

namespace _20181128_速度RGB
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] AryRed;
        private byte MinRed;
        private byte MaxRed;
        private double AveRed;

        private byte[] AryGre;
        private byte MinGre;
        private byte MaxGre;
        private double AveGre;

        private byte[] AryBlu;
        private byte MinBlu;
        private byte MaxBlu;
        private double AveBlu;

        private byte[] OriginRed;
        private byte[] OriginGreen;
        private byte[] OriginBlue;
        private double OriginAveRed;
        private double OriginAveGreen;
        private double OriginAveBlue;

        public MainWindow()
        {
            InitializeComponent();
            Title = this.ToString();

            Button1.Click += Button1_Click;
            Button2.Click += Button2_Click;
            Button3.Click += Button3_Click;
            Button4.Click += Button4_Click;
            Button5.Click += Button5_Click;
            Button6.Click += Button6_Click;
            //Button7.Click += Button7_Click;
            Button8.Click += Button8_Click;
            Button9.Click += Button9_Click;
            Button10.Click += Button10_Click;
            Button11.Click += Button11_Click;
            MyCheckBox.Click += MyCheckBox_Click;
            //MyTest();
            MyTextBlock.Text = "";

        }

        private void MyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (MyCheckBox.IsChecked == true)
            {
                Button10.IsEnabled = true;
                Button2.IsEnabled = true;
                Button6.IsEnabled = true;
                Button11.IsEnabled = true;
            }
            else
            {
                Button10.IsEnabled = false;
                Button2.IsEnabled = false;
                Button6.IsEnabled = false;
                Button11.IsEnabled = false;
            }
        }

        #region
        private void Button11_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        //private void Button7_Click(object sender, RoutedEventArgs e)
        //{
        //    var b = (Button)sender;
        //    MyTest(int.Parse((string)b.Tag));
        //}

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            MyTest(int.Parse((string)b.Tag));
        }
        #endregion


        //基準の配列とできあがった配列の比較、要素の順番と平均値が一致しているかの判定
        private void IsAgree()
        {
            bool average = true;
            bool array = true;
            for (int i = 0; i < OriginBlue.Length; i++)
            {
                if ((OriginRed[i] == AryRed[i]) && (OriginGreen[i] == AryGre[i]) && (OriginBlue[i] == AryBlu[i]) == false)
                {
                    array = false;
                    break;
                }
                if (OriginAveRed == AveRed && OriginAveGreen == AveGre && OriginAveBlue == AveBlu == false)
                {
                    average = false;
                    break;
                }
            }
            Console.WriteLine($"配列一致={array}, 平均値一致={average}");
        }


        private async void MyTest(int ColorCount)
        {
            const int LOOP_COUNT = 100;
            this.IsEnabled = false;
            //byte[] source = MakeRandomRGBByteArray(1920*1080);
            byte[] source = MakeRandomRGBByteArray(ColorCount);
            //byte[] source = MakeRandomRGBByteArray(1000000);
            AryRed = new byte[ColorCount];
            AryGre = new byte[ColorCount];
            AryBlu = new byte[ColorCount];
            string str = $"ピクセル数 {ColorCount:n0} を {LOOP_COUNT}回処理\n\n";
            //数値の表示形式nは桁区切り表示、0は小数点以下をいくつ表示するか

            //基本のforとifでbyte型を処理、この結果を基準値にする
            string methodName = nameof(TestA_for_if);
            MyTextBlock.Text = methodName + "実行中…";
            str += await Task.Run(() => LoopN(TestA_for_if, source, methodName, LOOP_COUNT));//これの結果を基準にする

            //基準の値を入れる
            int len = AryRed.Length;
            OriginRed = new byte[len];
            OriginGreen = new byte[len];
            OriginBlue = new byte[len];
            for (int i = 0; i < AryRed.Length; i++)
            {
                OriginRed[i] = AryRed[i];
                OriginGreen[i] = AryGre[i];
                OriginBlue[i] = AryBlu[i];
            }
            OriginAveRed = AveRed;
            OriginAveGreen = AveGre;
            OriginAveBlue = AveBlu;


            //Color型をそのまま使う旧式
            methodName = nameof(TestOld);
            MyTextBlock.Text = methodName + "実行中…";
            List<Color> colors = new List<Color>();
            for (int i = 0; i < source.Length; i += 3)
            {
                colors.Add(Color.FromRgb(source[i], source[i + 1], source[i + 2]));
            }
            str += await Task.Run(() => Loop10(TestOld, colors, methodName, LOOP_COUNT));
            IsAgree();

            //ここからbyte型
            methodName = nameof(TestB_LINQ);
            MyTextBlock.Text = methodName + "実行中…";
            str += await Task.Run(() => LoopN(TestB_LINQ, source, methodName, LOOP_COUNT));
            IsAgree();

            methodName = nameof(TestB_LINQ_ParallelFor_AsParallel);
            MyTextBlock.Text = methodName + "実行中…";
            str += await Task.Run(() => LoopN(TestB_LINQ_ParallelFor_AsParallel, source, methodName, LOOP_COUNT));
            IsAgree();

            methodName = nameof(TestC_Separate);
            MyTextBlock.Text = methodName + "実行中…";
            str += await Task.Run(() => LoopN(TestC_Separate, source, methodName, LOOP_COUNT));
            IsAgree();

            methodName = nameof(TestC_Separate_ParallelInvoke);
            MyTextBlock.Text = methodName + "実行中…";
            str += await Task.Run(() => LoopN(TestC_Separate_ParallelInvoke, source, methodName, LOOP_COUNT));
            IsAgree();

            methodName = nameof(TestD_SeparateRGB);
            MyTextBlock.Text = methodName + "実行中…";
            str += await Task.Run(() => LoopN(TestD_SeparateRGB, source, methodName, LOOP_COUNT));
            IsAgree();

            methodName = nameof(TestD_SeparateRGB_ParallelInvoke);
            MyTextBlock.Text = methodName + "実行中…";
            str += await Task.Run(() => LoopN(TestD_SeparateRGB_ParallelInvoke, source, methodName, LOOP_COUNT));
            IsAgree();


            //配列のクリア、意味ないかも？
            colors.Clear();
            ClearArray(source);
            ClearArray(AryRed);
            ClearArray(AryGre);
            ClearArray(AryBlu);
            ClearArray(OriginRed);
            ClearArray(OriginGreen);
            ClearArray(OriginBlue);

            MyTextBlock.Text = "処理完了";
            MessageBox.Show(str);
            this.IsEnabled = true;
            MyTextBlock.Text = "";
            //ガベージコレクション、意味ないかも
            System.GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

        }

        //配列のクリア
        private void ClearArray(Array array)
        {
            Array.Clear(array, 0, array.Length);
        }

        //n回ループ
        private string LoopN(Action<byte[]> action, byte[] source, string methodName, int loopCount)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < loopCount; i++) { action(source); }
            sw.Stop();

            string str = $"{sw.Elapsed.TotalSeconds:00.0000}秒 ({methodName})\n";
            ResultOutput(methodName);
            return str;
        }
        private string Loop10(Action<List<Color>> action, List<Color> source, string methodName, int loopCount)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < loopCount; i++) { action(source); }
            sw.Stop();

            string str = $"{sw.Elapsed.TotalSeconds:00.0000}秒 ({methodName})\n";
            ResultOutput(methodName);
            return str;
        }


        //計算結果の出力
        private void ResultOutput(string method)
        {
            string str = "";
            str += $"R({MinRed:000}, {MaxRed:000}, {AveRed:000.0000}), G({MinGre:000}, {MaxGre:000}, {AveGre:000.0000}), B({MinBlu:000}, {MaxBlu:000}, {AveBlu:000.0000})";
            str += ":" + method;
            Console.WriteLine(str);
        }


        #region 基本形、forとifの組み合わせ
        //for + if
        private void TestA_for_if(byte[] source)
        {
            int len = source.Length / 3;
            InitializeRGBValue(len);
            MinRed = 255; MaxRed = 0;
            MinGre = 255; MaxGre = 0;
            MinBlu = 255; MaxBlu = 0;
            byte r, g, b;
            long tR = 0, tG = 0, tB = 0;
            for (int i = 0; i < len; i++)
            {
                r = source[i * 3];
                g = source[i * 3 + 1];
                b = source[i * 3 + 2];
                AryRed[i] = r; AryGre[i] = g; AryBlu[i] = b;
                tR += r; tG += g; tB += b;
                if (MinRed > r) { MinRed = r; }
                if (MaxRed < r) { MaxRed = r; }
                if (MinGre > g) { MinGre = g; }
                if (MaxGre < g) { MaxGre = g; }
                if (MinBlu > b) { MinBlu = b; }
                if (MaxBlu < b) { MaxBlu = b; }
            }
            AveRed = tR / (double)len;
            AveGre = tG / (double)len;
            AveBlu = tB / (double)len;
        }
        #endregion

        #region TestB LINQのMin、Max、Average
        private void TestB_LINQ(byte[] source)
        {
            int len = source.Length / 3;
            InitializeRGBValue(len);
            for (int i = 0; i < len; i++)
            {
                AryRed[i] = source[i * 3];
                AryGre[i] = source[i * 3 + 1];
                AryBlu[i] = source[i * 3 + 2];
            }
            MinRed = AryRed.Min();
            MaxRed = AryRed.Max();
            AveRed = AryRed.Average(a => a);
            MinGre = AryGre.Min();
            MaxGre = AryGre.Max();
            AveGre = AryGre.Average(a => a);
            MinBlu = AryBlu.Min();
            MaxBlu = AryBlu.Max();
            AveBlu = AryBlu.Average(a => a);

        }

        //Parallel.for + AsParallel
        private void TestB_LINQ_ParallelFor_AsParallel(byte[] source)
        {
            int len = source.Length / 3;
            InitializeRGBValue(len);
            Parallel.For(0, len, i =>
            {
                AryRed[i] = source[i * 3];
                AryGre[i] = source[i * 3 + 1];
                AryBlu[i] = source[i * 3 + 2];
            });

            //Parallel.ForEach(source, i =>
            //{
            //    ArRed[i] = source[i * 3];
            //    ArGre[i] = source[i * 3 + 1];
            //    ArBlu[i] = source[i * 3 + 2];
            //});

            MinRed = AryRed.AsParallel().Min();
            MaxRed = AryRed.AsParallel().Max();
            AveRed = AryRed.AsParallel().Average(a => a);
            MinGre = AryGre.AsParallel().Min();
            MaxGre = AryGre.AsParallel().Max();
            AveGre = AryGre.AsParallel().Average(a => a);
            MinBlu = AryBlu.AsParallel().Min();
            MaxBlu = AryBlu.AsParallel().Max();
            AveBlu = AryBlu.AsParallel().Average(a => a);
        }
        #endregion


        #region TestC RGBごとに別々の処理
        /// <summary>
        /// RGBのどれかを処理
        /// </summary>
        /// <param name="source">RGBが入った配列</param>
        /// <param name="start">Rなら0、Gは1、Bは2を指定</param>
        /// <param name="array"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="ave"></param>
        private void MySet(byte[] source, int start, byte[] array, ref byte min, ref byte max, ref double ave)
        {
            byte b;
            long total = 0;
            for (int i = start; i < source.Length; i += 3)
            {
                b = source[i];
                array[(i - start) / 3] = b;
                if (min > b) { min = b; }
                if (max < b) { max = b; }
                total += b;
            }
            ave = total / (source.Length / 3.0);
        }

        //for + if + 別メソッド
        private void TestC_Separate(byte[] source)
        {
            int len = source.Length / 3;
            InitializeRGBValue(len);

            MinRed = 255; MaxRed = 0;
            MinGre = 255; MaxGre = 0;
            MinBlu = 255; MaxBlu = 0;

            MySet(source, 0, AryRed, ref MinRed, ref MaxRed, ref AveRed);
            MySet(source, 1, AryGre, ref MinGre, ref MaxGre, ref AveGre);
            MySet(source, 2, AryBlu, ref MinBlu, ref MaxBlu, ref AveBlu);
        }

        //for + if + 別メソッド(Parallel.Invoke())
        private void TestC_Separate_ParallelInvoke(byte[] source)
        {
            int len = source.Length / 3;
            InitializeRGBValue(len);

            MinRed = 255; MaxRed = 0;
            MinGre = 255; MaxGre = 0;
            MinBlu = 255; MaxBlu = 0;

            Parallel.Invoke(() =>
            {
                MySet(source, 0, AryRed, ref MinRed, ref MaxRed, ref AveRed);
            }, () =>
            {
                MySet(source, 1, AryGre, ref MinGre, ref MaxGre, ref AveGre);
            }, () =>
            {
                MySet(source, 2, AryBlu, ref MinBlu, ref MaxBlu, ref AveBlu);
            });
        }
        #endregion


        #region TestD RGBごとの別メソッドをParallel.Invokeで並列処理
        private void MyRed(byte[] vs)
        {
            MinRed = 255; MaxRed = 0;
            byte r;
            long total = 0;
            int count = 0;
            for (int i = 0; i < vs.Length; i += 3)
            {
                r = vs[i];
                AryRed[count] = vs[i];
                if (MinRed > r) { MinRed = r; }
                if (MaxRed < r) { MaxRed = r; }
                total += r;
                count++;
            }
            AveRed = total / (vs.Length / 3.0);
        }
        private void MyGreen(byte[] vs)
        {
            MinGre = 255; MaxGre = 0;
            byte g;
            long total = 0;
            int count = 0;
            for (int i = 1; i < vs.Length; i += 3)
            {
                g = vs[i];
                AryGre[count] = vs[i];
                if (MinGre > g) { MinGre = g; }
                if (MaxGre < g) { MaxGre = g; }
                total += g;
                count++;
            }
            AveGre = total / (vs.Length / 3.0);
        }
        private void MyBlue(byte[] vs)
        {
            MinBlu = 255; MaxBlu = 0;
            byte b;
            long total = 0;
            int count = 0;
            for (int i = 2; i < vs.Length; i += 3)
            {
                b = vs[i];
                AryBlu[count] = vs[i];
                if (MinBlu > b) { MinBlu = b; }
                if (MaxBlu < b) { MaxBlu = b; }
                total += b;
                count++;
            }
            AveBlu = total / (vs.Length / 3.0);
        }

        //for + if + RGB個別メソッド
        private void TestD_SeparateRGB(byte[] source)
        {
            int len = source.Length / 3;
            InitializeRGBValue(len);

            MyRed(source); MyGreen(source); MyBlue(source);
        }

        //for + if + RGB個別メソッド(Parallel.Invoke())
        private void TestD_SeparateRGB_ParallelInvoke(byte[] source)
        {
            int len = source.Length / 3;
            InitializeRGBValue(len);

            Parallel.Invoke(() => { MyRed(source); }, () => { MyGreen(source); }, () => { MyBlue(source); });
        }



        #endregion

        #region C1 計算違い＆遅い
        private void MyRed2(byte[] vs)
        {
            MinRed = 255; MaxRed = 0;
            byte r;
            long total = 0;
            Parallel.For(0, vs.Length, i =>
            {
                r = vs[i];
                if (MinRed > r) { MinRed = r; }
                if (MaxRed < r) { MaxRed = r; }
                lock (vs) total += r;
            });
            AveRed = total / (vs.Length / 3.0);
        }
        private void MyGreen2(byte[] vs)
        {
            MinGre = 255; MaxGre = 0;
            byte g;
            long total = 0;
            Parallel.For(1, vs.Length, i =>
            {
                g = vs[i];
                if (MinGre > g) { MinGre = g; }
                if (MaxGre < g) { MaxGre = g; }
                lock (vs) total += g;
            });
            AveGre = total / (vs.Length / 3.0);
        }
        private void MyBlue2(byte[] vs)
        {
            MinBlu = 255; MaxBlu = 0;
            byte b;
            long total = 0;
            Parallel.For(2, vs.Length, i =>
              {
                  b = vs[i];
                  if (MinBlu > b) { MinBlu = b; }
                  if (MaxBlu < b) { MaxBlu = b; }
                  lock (vs) total += b;
              });
            AveBlu = total / (vs.Length / 3.0);
        }

        //for + if + RGB個別メソッド(Parallel.Invoke())
        private void TestC1(byte[] source)
        {
            int len = source.Length / 3;
            InitializeRGBValue(len);

            Parallel.Invoke(() => { MyRed2(source); }, () => { MyGreen2(source); }, () => { MyBlue2(source); });
        }

        #endregion

        #region 以前のColor型で
        private void TestOld(List<Color> colors)
        {
            int count = colors.Count;
            InitializeRGBValue(count);
            MinRed = 255; MaxRed = 0;
            MinGre = 255; MaxGre = 0;
            MinBlu = 255; MaxBlu = 0;
            byte r, g, b;
            Color col;
            long rTotal = 0, gTotal = 0, bTotal = 0;

            for (int i = 0; i < count; i++)
            {
                col = colors[i];
                r = col.R; g = col.G; b = col.B;
                AryRed[i] = r; AryGre[i] = g; AryBlu[i] = b;
                rTotal += r; gTotal += g; bTotal += b;
                if (MinRed > r) { MinRed = r; }
                if (MaxRed < r) { MaxRed = r; }
                if (MinGre > g) { MinGre = g; }
                if (MaxGre < g) { MaxGre = g; }
                if (MinBlu > b) { MinBlu = b; }
                if (MaxBlu < b) { MaxBlu = b; }
            }
            AveRed = rTotal / (double)count;
            AveGre = gTotal / (double)count;
            AveBlu = bTotal / (double)count;
        }

        #endregion




        private void InitializeRGBValue(int len)
        {
            //ArRed = new byte[len];
            //ArGre = new byte[len];
            //ArBlu = new byte[len];
            //Array.Clear(ArRed, 0, ArRed.Length);
            //Array.Clear(ArGre, 0, ArGre.Length);
            //Array.Clear(ArBlu, 0, ArBlu.Length);
            MinRed = 0; MaxRed = 0; AveRed = 0;
            MinGre = 0; MaxGre = 0; AveGre = 0;
            MinBlu = 0; MaxBlu = 0; AveBlu = 0;
        }
        private byte[] MakeRandomRGBByteArray(int count)
        {
            var r = new Random();
            var b = new byte[count * 3];
            r.NextBytes(b);
            return b;
        }


    }
}
