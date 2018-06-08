using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


//マウスクリックでCanvasに直線を描画その2、Polyline、WPFとC# ( ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15540488.html

namespace _20180607_クリックで直線Polyline
{    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyCanvas.MouseMove += MyCanvas_MouseMove;
            MyCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
            MyCanvas.MouseRightButtonDown += MyCanvas_MouseRightButtonDown;
        }
        
        //右クリックでクリア
        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPolyline.Points.Clear();
        }

        //左クリックで座標追加
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PointCollection points = MyPolyline.Points;
            //最初のクリックのときだけ座標を2個追加する
            if (points.Count == 0)
            {
                points.Add(e.GetPosition(MyCanvas));
            }
            points.Add(e.GetPosition(MyCanvas));
        }

        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            PointCollection points = MyPolyline.Points;
            if (points.Count != 0)
            {                
                points[points.Count - 1] = e.GetPosition(MyCanvas);
            }            
        }
    }
}
