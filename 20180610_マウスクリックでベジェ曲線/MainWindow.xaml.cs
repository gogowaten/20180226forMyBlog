using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
//マウスクリックでCanvasにベジェ曲線で曲線、PolyBezierSegment(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15544835.html

namespace _20180610_マウスクリックでベジェ曲線
{
    public partial class MainWindow : Window
    {
        PolyBezierSegment MySegment;

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

        //Path.Dataを作成
        private void MyInitialize()
        {
            MySegment = new PolyBezierSegment();
            var pf = new PathFigure();
            pf.Segments.Add(MySegment);
            var pg = new PathGeometry();
            pg.Figures.Add(pf);
            MyPath.Data = pg;
        }

        //マウス移動時、終端のアンカーポイント移動とその他の制御点の位置調整
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            PointCollection ps = MySegment.Points;
            if (ps.Count > 5)
            {                
                Point p = e.GetPosition(MyCanvas);//マウスカーソル位置

                //最後のアンカーポイントの座標は今のカーソル座標
                ps[ps.Count - 1] = p;

                Point ap1 = ps[ps.Count - 4];//一個前のアンカーポイント
                Point ap2;//二個前のアンカーポイント
                if (ps.Count < 7)
                {
                    var pg = (PathGeometry)MyPath.Data;
                    var pathFigureCollection = pg.Figures;
                    ap2 = pathFigureCollection[0].StartPoint;
                }
                else
                {
                    ap2 = ps[ps.Count - 7];
                }

                //最後のアンカーポイントと二個前のアンカーポイントとの距離の1 / 4
                double xDiff = (p.X - ap2.X) / 4.0;
                double yDiff = (p.Y - ap2.Y) / 4.0;
                //一個前のアンカーポイントの制御点座標
                ps[ps.Count - 3] = new Point(ap1.X + xDiff, ap1.Y + yDiff);
                ps[ps.Count - 5] = new Point(ap1.X - xDiff, ap1.Y - yDiff);

                //最後のアンカーポイントと一個前のアンカーポイントの距離の1/4
                xDiff = (p.X - ps[ps.Count - 3].X) / 4.0;
                yDiff = (p.Y - ps[ps.Count - 3].Y) / 4.0;
                //最後のアンカーポイントの制御点座標
                ps[ps.Count - 2] = new Point(p.X - xDiff, p.Y - yDiff);
            }
            else if (MySegment.Points.Count > 0)
            {                
                //最後のアンカーポイントのインデックスはCount-1
                ps[ps.Count - 1] = e.GetPosition(MyCanvas);//カーソル位置に
            }
        }

        //マウスクリック時、アンカーポイントと制御点2つを追加
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
