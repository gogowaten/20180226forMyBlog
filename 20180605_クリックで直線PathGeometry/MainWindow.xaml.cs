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

namespace _20180605_クリックで直線PathGeometry
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

        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            iPath.Data = null;//PathDataの初期化
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(MyCanvas);
            if (iPath.Data == null)
            {
                MyPathStartPoint(p);//最初のクリック時
            }
            else
            {
                //クリックした座標をLineSegmentとして追加する
                var pg = (PathGeometry)iPath.Data;
                PathFigure pf = pg.Figures[0];
                pf.Segments.Add(new LineSegment(p, true));
            }
        }

        //マウス移動時は最後にクリックした点から伸びる線を描画する
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (iPath.Data != null)
            {
                Point p = e.GetPosition(MyCanvas);//マウスカーソル位置

                var pg = (PathGeometry)iPath.Data;                
                PathSegmentCollection psc = pg.Figures[0].Segments;
                LineSegment lineSegment = (LineSegment)psc[psc.Count - 1];//終端
                //終端座標を指定(マウスカーソルの位置)
                lineSegment.Point = p;
            }
        }
            
        //最初のクリック時にPathのDataになるPathGeometryを作成
        //開始点と次点を指定する
        private void MyPathStartPoint(Point p)
        {
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = p;//開始点
            //次点をLineSegmentで作成してSegmentsに追加
            pathFigure.Segments.Add(new LineSegment(p, true));
                        
            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            //
            iPath.Data = pathGeometry;
        }
                
    }
}
