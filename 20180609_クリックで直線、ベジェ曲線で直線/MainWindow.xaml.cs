using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
//マウスクリックでCanvasに直線を描画その3、ベジェ曲線で直線、PolyBezierSegment(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15542192.html

namespace _20180609_クリックで直線_ベジェ曲線で直線
{
    public partial class MainWindow : Window
    {
        //PointCollection MyPointCollection;//これは失敗、これを変更しても反映されない
        PolyBezierSegment MySegment;//これのPointsにPointを追加していく

        public MainWindow()
        {
            InitializeComponent();
            
            MyInitialize();
            MyCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
            MyCanvas.MouseMove += MyCanvas_MouseMove;
            MyCanvas.MouseRightButtonDown += MyCanvas_MouseRightButtonDown;
        }

        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MySegment.Points.Clear();
        }

        //PathGeometry作成してPathのDataに指定
        private void MyInitialize()
        {
            MySegment = new PolyBezierSegment(new PointCollection(), true);
            var pathFigure = new PathFigure();
            pathFigure.Segments.Add(MySegment);
            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            MyPath.Data = pathGeometry;
        }

        //最後のPointをマウスカーソルの位置にする
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MySegment.Points.Count > 0)
            {
                var ps = MySegment.Points;//PointCollection
                //最後のアンカーポイントのインデックスはCount-1
                int ap = ps.Count - 1;
                ps[ap] = e.GetPosition(MyCanvas);//カーソル位置に
            }
        }

        //左クリックで制御点2つとアンカーポイント追加
        //2つの制御点の座標を同じにすると直線になる
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(MyCanvas);
            //最初のクリック時だけはStartPointを指定する
            if (MySegment.Points.Count == 0)
            {
                var pathGeometry = (PathGeometry)MyPath.Data;
                var pathFigureCollection = pathGeometry.Figures;
                pathFigureCollection[0].StartPoint = p;
                //制御点2つとアンカーポイント追加
                MySegment.Points.Add(p);
                MySegment.Points.Add(p);
                MySegment.Points.Add(p);
            }
            else
            {
                //制御点2つとアンカーポイント追加
                MySegment.Points.Add(p);
                MySegment.Points.Add(p);
                MySegment.Points.Add(p);
            }
        }
    }
}
