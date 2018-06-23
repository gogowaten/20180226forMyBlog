using System.Windows;
using System.Windows.Media;
//Polyline折れ線からベジェ曲線作成と曲げ具合(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15562209.html

namespace _20180623_折れ線をベジェ曲線に
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AddPathBezier();
            ButtonVisible.Click += ButtonVisible_Click;
        }

        private void ButtonVisible_Click(object sender, RoutedEventArgs e)
        {
            if (MyPolyline.Visibility == Visibility)
            {
                MyPolyline.Visibility = Visibility.Hidden;
            }
            else { MyPolyline.Visibility = Visibility.Visible; }
        }

        private void AddPathBezier()
        {
            //ベジェ曲線にするPathの初期化
            var seg = new PolyBezierSegment();
            var fig = new PathFigure();
            fig.Segments.Add(seg);
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            MyPathBezier.Data = pg;

            //ベジェ曲線のSegment作成
            PointCollection pc = MyPolyline.Points;
            fig.StartPoint = pc[0];//始点アンカー
            seg.Points.Add(pc[0]);//始点制御点
            for (int i = 1; i < pc.Count - 1; i++)
            {
                seg.Points.Add(pc[i]);//制御点
                seg.Points.Add(pc[i]);//アンカー
                seg.Points.Add(pc[i]);//制御点
            }
            seg.Points.Add(pc[pc.Count - 1]);//終点制御点
            seg.Points.Add(pc[pc.Count - 1]);//終点アンカー

            //各アンカーの制御点座標を調節して、なめらか化
            Point startAP = fig.StartPoint;//始点側アンカー点
            Point AP, endAP;
            double xDiff, yDiff;
            pc = seg.Points;
            for (int i = 2; i < pc.Count - 3; i += 3)
            {
                AP = pc[i];//注目アンカー点
                endAP = pc[i + 3];//終点側アンカー点
                xDiff = (endAP.X - startAP.X) / 4;
                yDiff = (endAP.Y - startAP.Y) / 4;
                //始点側制御点
                pc[i - 1] = new Point(AP.X - xDiff, AP.Y - yDiff);
                //終点側制御点
                pc[i + 1] = new Point(AP.X + xDiff, AP.Y + yDiff);
                startAP = AP;
            }
        }
    }
}
