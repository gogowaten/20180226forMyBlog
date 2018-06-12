using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Shapes;
//ベジェ曲線の方向線とアンカーポイント、制御点を表示してみた(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15547295.html

namespace _20180612_ベジェ曲線のアンカーと制御点までの直線
{
    public partial class MainWindow : Window
    {
        PolyBezierSegment MySegment;//ベジェ曲線用

        //すべての方向線を1つのPathコントロールで表示
        PathFigureCollection MyControlLines;//アンカーポイントと制御点間の直線(方向線)用

        //制御点ごとに1つのPathコントロールで凝視
        List<EllipseGeometry> MyEllipseGeometry;//●表示用
        List<Path> MyListPathControl;//●表示コントロール用
        //制御点の表示はGeometryGroupクラスを使えば1つのPathコントロールで表示できるかも

        public MainWindow()
        {
            InitializeComponent();

            MyInitialize();
            MyCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
            MyCanvas.MouseMove += MyCanvas_MouseMove;
            MyCanvas.MouseRightButtonDown += MyCanvas_MouseRightButtonDown;
        }

        //右クリック時、すべての座標、●、方向線を削除
        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MySegment.Points.Clear();
            MyControlLines.Clear();
            MyEllipseGeometry.Clear();
            foreach (var item in MyListPathControl)
            {
                MyCanvas.Children.Remove(item);
            }
            MyListPathControl.Clear();
        }

        private void MyInitialize()
        {
            //ベジェ曲線の初期化
            var ps = new PointCollection();
            MySegment = new PolyBezierSegment(ps, true);
            var pf = new PathFigure();
            pf.Segments.Add(MySegment);
            var pg = new PathGeometry();
            pg.Figures.Add(pf);
            MyPath.Data = pg;

            //方向線の初期化
            MyControlLines = new PathFigureCollection();
            pg = new PathGeometry();
            pg.Figures = MyControlLines;
            MyPath2.Data = pg;

            //●表示用リストの初期化
            MyEllipseGeometry = new List<EllipseGeometry>();
            MyListPathControl = new List<Path>();
        }

        //マウス移動時
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            PointCollection ps = MySegment.Points;
            if (MySegment.Points.Count > 5)
            {
                Point p = e.GetPosition(MyCanvas);//マウスカーソル位置
                //最後のアンカーポイントの座標は今のカーソル座標
                ps[ps.Count - 1] = p;
                int ellipseCount = MyEllipseGeometry.Count;
                MyEllipseGeometry[ellipseCount - 1].Center = p;//●

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
                //●表示用の座標指定                
                MyEllipseGeometry[ellipseCount - 3].Center = ps[ps.Count - 3];
                MyEllipseGeometry[ellipseCount - 5].Center = ps[ps.Count - 5];
                //方向線の座標指定
                int cpCount = MyControlLines.Count;
                MyControlLines[cpCount - 2].StartPoint = ap1;
                MyControlLines[cpCount - 2].Segments[0] = new LineSegment(ps[ps.Count - 3], true);
                MyControlLines[cpCount - 3].StartPoint = ap1;
                MyControlLines[cpCount - 3].Segments[0] = new LineSegment(ps[ps.Count - 5], true);

                //最後のアンカーポイントと一個前のアンカーポイントの距離の1/4
                xDiff = (p.X - ps[ps.Count - 3].X) / 4.0;
                yDiff = (p.Y - ps[ps.Count - 3].Y) / 4.0;
                //最後のアンカーポイントの制御点座標
                ps[ps.Count - 2] = new Point(p.X - xDiff, p.Y - yDiff);
                //●
                MyEllipseGeometry[ellipseCount - 2].Center = ps[ps.Count - 2];
                //方向線                
                MyControlLines[cpCount - 1].StartPoint = p;
                MyControlLines[cpCount - 1].Segments[0] = new LineSegment(ps[ps.Count - 2], true);

            }
            else if (MySegment.Points.Count > 0)
            {
                //最後のアンカーポイント位置はカーソル位置                
                ps[ps.Count - 1] = e.GetPosition(MyCanvas);

            }
        }

        //クリック時
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(MyCanvas);
            //最初のクリック時だけはStartPointを指定する
            if (MySegment.Points.Count == 0)
            {
                var pg = (PathGeometry)MyPath.Data;
                var pathFigureCollection = pg.Figures;
                pathFigureCollection[0].StartPoint = p;
                AddEllipse(p);//制御点表示用Path追加
            }

            //ベジェ曲線のセグメントの座標追加
            MySegment.Points.Add(p);
            MySegment.Points.Add(p);
            MySegment.Points.Add(p);
            //方向線のFigureの座標追加
            AddLineFigure(p, p);
            AddLineFigure(p, p);
            //●表示用のPath追加
            AddEllipse(p);
            AddEllipse(p);
            AddEllipse(p);
            
            //一個前のアンカーポイント●の座標決定
            MyEllipseGeometry[MyEllipseGeometry.Count - 4].Center = p;
        }

        //PathFigureCollectionにLineSegmentを追加する
        private void AddLineFigure(Point start, Point end)
        {
            var pf = new PathFigure();
            pf.StartPoint = start;
            pf.Segments.Add(new LineSegment(end, true));
            MyControlLines.Add(pf);
        }

        //●表示用のPathを作成してMyCanvasに追加
        private void AddEllipse(Point p)
        {
            //半径3の青丸
            EllipseGeometry ellipseGeometry = new EllipseGeometry(p, 3, 3);
            MyEllipseGeometry.Add(ellipseGeometry);
            Path path = new Path();
            path.Fill = Brushes.Blue;
            path.Data = ellipseGeometry;
            MyCanvas.Children.Add(path);
            MyListPathControl.Add(path);
        }
    }
}
