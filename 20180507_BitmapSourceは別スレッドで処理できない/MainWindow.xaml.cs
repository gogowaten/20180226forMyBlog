using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
//画像処理中のプログレスバー更新とキャンセルボタンで中止(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15494790.html

namespace _20180507_BitmapSourceは別スレッドで処理できない
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyButtonInteger.Click += MyButtonInteger_Click;
            MyButtonString.Click += MyButtonString_Click;
            MyButtonBitmap.Click += MyButtonBitmap_Click;
            MyButtonBitmap2.Click += MyButtonBitmap2_Click;
        }


        //int
        private async void MyButtonInteger_Click(object sender, RoutedEventArgs e)
        {
            int i = await Task.Run(() => 100 * 2);
            MyTextBlock.Text = i.ToString();
        }
        //string
        private async void MyButtonString_Click(object sender, RoutedEventArgs e)
        {
            int i = await Task.Run(() => ("test文字数").Count());
            MyTextBlock.Text = i.ToString();
        }

        //BitmapSource
        private async void MyButtonBitmap_Click(object sender, RoutedEventArgs e)
        {
            var source = new BitmapImage(new Uri(@"D:\ブログ用\チェック用2\NEC_2820_2018_05_06_午後わてん.jpg"));
            //エラーになる、別スレッドでBitmapSourceを使った処理はできない    
            int i = await Task.Run(() => source.PixelWidth * 2);
            MyTextBlock.Text = i.ToString();
        }

        //BitmapSourceのPixelWidthを入れたint型変数
        private async void MyButtonBitmap2_Click(object sender, RoutedEventArgs e)
        {
            var source = new BitmapImage(new Uri(@"D:\ブログ用\チェック用2\NEC_2820_2018_05_06_午後わてん.jpg"));
            int w = source.PixelWidth;
            //これならOK
            int i = await Task.Run(() => w * 2);
            MyTextBlock.Text = i.ToString();
        }
    }
}
