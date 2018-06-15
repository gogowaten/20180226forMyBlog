using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
//Pathを等分したところに印と角度を表示してみた、GetPointAtFractionLength(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15550562.html

namespace _20180614_直線の等分したところに印
{
    public partial class MainWindow : Window
    {
        List<Path> MyListEllipse = new List<Path>();
        List<Label> MyListLabel = new List<Label>();

        public MainWindow()
        {
            InitializeComponent();

            MyInitialize();
        }

        private void SliderStep_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeStep();
        }

        //いろいろ初期化
        private void MyInitialize()
        {
            ChangeStep();
            SliderStep.ValueChanged += SliderStep_ValueChanged;
        }

        //表示をクリア
        private void ClearStepObject()
        {
            foreach (var item in MyListLabel)
            {
                MyCanvas.Children.Remove(item);
            }
            MyListLabel.Clear();
            foreach (var item in MyListEllipse)
            {
                MyCanvas.Children.Remove(item);
            }
            MyListEllipse.Clear();
        }

        //Step(分割)数変更時とかに実行
        private void ChangeStep()
        {
            ClearStepObject();//表示をクリア

            double step = SliderStep.Value;
            var pg = (PathGeometry)MyPath.Data;
            Point p, pt;
            for (int i = 0; i < step + 1; i++)
            {
                //GetPointAtFractionLengthの第1引数は計測する位置の割合を0から1で指定する
                //Path全体からの割合なので、中間を取得したいなら0.5を指定する
                //1個めのoutは指定した位置の座標が返ってくる
                //2個めのoutはタンジェント座標？が返ってくる、Math.Atan2に入れるとラジアンが得られる？
                pg.GetPointAtFractionLength(i / step, out p, out pt);
                //○印の作成
                var path = new Path();
                path.Stroke = Brushes.Blue;
                path.Data = new EllipseGeometry(p, 4, 4);
                MyCanvas.Children.Add(path);
                MyListEllipse.Add(path);

                //角度の計算と表示
                //GetPointAtFractionLengthで得られたタンジェントをMath.Atan2に入れると
                //ラジアンが得られるので、Math.PI*180で角度に変換
                var kakudo = Math.Atan2(pt.X, pt.Y) / Math.PI * 180;
                Label label = new Label();
                label.Content = kakudo;
                Canvas.SetTop(label, p.Y);
                Canvas.SetLeft(label, p.X);
                MyCanvas.Children.Add(label);
                MyListLabel.Add(label);

            }
        }

    }
}
