using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
//WPF、ScrollViewerの中の要素をマウスドラッグ移動しているように見せかける(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15755956.html

namespace _20181112_スクロール_マウスドラッグ
{
    public partial class MainWindow : Window
    {
        Point MyPoint;//マウスクリックの位置用

        public MainWindow()
        {
            InitializeComponent();
            Title = this.ToString();

            string filePath1 = @"D:\ブログ用\チェック用2\NEC_5848_2018_10_27_午後わてん.jpg";
            MyImage.Source = new BitmapImage(new Uri(filePath1));

            MyImage.MouseRightButtonDown += MyImage_MouseRightButtonDown;
            MyImage.MouseRightButtonUp += MyImage_MouseRightButtonUp;
            MyImage.MouseMove += MyImage_MouseMove;
        }

        private void MyImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {            
            MyImage.Cursor = Cursors.Arrow;//カーソル形状を矢印に戻す
            //マウスがScrollViewer外になってもドラッグ移動を有効にしたいときだけ必要
            MyImage.ReleaseMouseCapture();
        }

        private void MyImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //クリック位置取得
            MyPoint = e.GetPosition(this);
            //マウスがScrollViewer外になってもドラッグ移動を有効にしたいときだけ必要
            MyImage.CaptureMouse();
        }

        private void MyImage_MouseMove(object sender, MouseEventArgs e)
        {
            //マウスドラッグ移動の距離だけスクロールさせるには
            //(直前のカーソル位置 - 今のカーソル位置) + (スクロールバーのOffset位置)
            //この値をSetOffsetする
            if (e.RightButton == MouseButtonState.Pressed)
            {
                MyImage.Cursor = Cursors.ScrollAll;//カーソル形状を変更
                //今のマウスの座標
                var mouseP = e.GetPosition(this);
                //マウスの移動距離＝直前の座標と今の座標の差
                var xd = MyPoint.X - mouseP.X;
                var yd = MyPoint.Y - mouseP.Y;
                //xd *= 2;//2倍速
                //yd *= 2;

                //移動距離＋今のスクロール位置
                xd += MyScroll.HorizontalOffset;
                yd += MyScroll.VerticalOffset;
                //スクロール位置の指定
                MyScroll.ScrollToHorizontalOffset(xd);
                MyScroll.ScrollToVerticalOffset(yd);
                
                MyPoint = mouseP;//直前の座標を今の座標に変更
            }
        }

    }
}
