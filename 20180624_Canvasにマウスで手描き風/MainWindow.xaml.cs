using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
//マウスドラッグ(移動)で線を描画、CanvasにPolyline(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15563685.html

namespace _20180624_Canvasにマウスで手描き風
{
    public partial class MainWindow : Window
    {
        bool IsDrawing = false;
        Polyline MyPolyline;
        List<Polyline> MyListPolyline = new List<Polyline>();
        List<Path> MyListEllipsePath = new List<Path>();

        public MainWindow()
        {
            InitializeComponent();
            MyCanvas.MouseMove += MyCanvas_MouseMove;
            MyCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
            MyCanvas.MouseLeftButtonUp += MyCanvas_MouseLeftButtonUp;

            ButtonClear.Click += ButtonClear_Click;
        }
        

        #region ●表示クリア処理
        //●表示クリア
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            ClearPolyline();
            ClearEllipsePoints();
        }
        private void ClearPolyline()
        {
            foreach (var item in MyListPolyline)
            {
                //Canvasから削除
                MyCanvas.Children.Remove(item);
            }
            //リストからも削除
            MyListPolyline.Clear();
        }
        private void ClearEllipsePoints()
        {
            foreach (var item in MyListEllipsePath)
            {
                MyCanvas.Children.Remove(item);
            }
            MyListEllipsePath.Clear();
        }
        #endregion

        #region ●作成、表示
        //●のGeometryを1つのPathにまとめて表示
        private void AddEllipsePoints2()
        {
            if (MyListPolyline.Count == 0) { return; }
            ClearEllipsePoints();

            var pg = new PathGeometry();
            var path = new Path();
            path.Fill = Brushes.Orange;
            path.Stroke = Brushes.Yellow;
            MyCanvas.Children.Add(path);
            MyListEllipsePath.Add(path);
            path.Data = pg;
            pg.FillRule = FillRule.Nonzero;

            PointCollection ps;
            for (int i = 0; i < MyListPolyline.Count; i++)
            {
                ps = MyListPolyline[i].Points;
                for (int j = 0; j < ps.Count; j++)
                {
                    pg.AddGeometry(new EllipseGeometry(ps[j], 2, 2));
                }
            }
        }
        #endregion


        #region マウスで描画
        //左クリック離したとき、描画の終了処理
        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDrawing == true)
            {
                IsDrawing = false;
                MyCanvas.ReleaseMouseCapture();//マウスキャプチャ開放
                //Tagに入れたPointsはもとに戻すときに使う
                MyPolyline.Tag = MyPolyline.Points;
                //クリックのみでドラッグ移動しなかったときは
                //作成したPolylineを削除
                if (MyPolyline.Points.Count == 0)
                {
                    ClearEllipsePoints();
                    MyListPolyline.Remove(MyPolyline);
                    MyCanvas.Children.Remove(MyPolyline);
                }
                //●を表示
                ClearEllipsePoints();
                AddEllipsePoints2();
            }
        }

        //左クリック時
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsDrawing = true;//描画フラグ
            MyCanvas.CaptureMouse();//マウスがCanvas外に出でも感知できるように
            //Polyline作成してCanvasに追加表示
            MyPolyline = new Polyline();
            MyPolyline.Stroke = Brushes.Blue;
            MyPolyline.StrokeThickness = 20;
            MyPolyline.StrokeLineJoin = PenLineJoin.Round;
            MyListPolyline.Add(MyPolyline);
            MyCanvas.Children.Add(MyPolyline);
        }

        //マウス移動時
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //左ボタンが押されていたらPolylineのPointsにカーソル位置を追加
            if (e.LeftButton == MouseButtonState.Pressed && IsDrawing == true)
            {
                MyPolyline.Points.Add(e.GetPosition(MyCanvas));
            }

        }
        #endregion 
    }
}
