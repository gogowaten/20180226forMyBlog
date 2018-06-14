using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
//曲線Pathを近似の直線PathにするGetFlattenedPathGeometry使ってみた(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15549065.html

namespace _20180613_曲線Pathを直線Pathに変換
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            PathGeometry pathGeometry = MyPath.Data.GetFlattenedPathGeometry();
            StepLine.Data = pathGeometry;
            StepSlider.ValueChanged += StepSlider_ValueChanged;
            StepSlider.MouseWheel += StepSlider_MouseWheel;
            RadioRelative.Checked += Radio_Checked;
            RadioAbsolute.Checked += Radio_Checked;
        }

        //RadioButtonのチェック変更時
        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            ChangeGeometry();
        }

        //Sliderのマウスホイール動作時
        private void StepSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                StepSlider.Value += StepSlider.SmallChange;
            }
            else
            {
                StepSlider.Value -= StepSlider.SmallChange;
            }
        }

        //Sliderの値変更時
        private void StepSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeGeometry();
        }

        //GetFlattenedPathGeometryを使って曲線のPathGeometryから直線のPathGeometryを取得して変更
        private void ChangeGeometry()
        {
            //tolerance(公差、許容できる誤差)は0に近いほど正確だけど、そのぶん計算量が多そう
            //toleranceTypeはよくわからん、Absolute絶対 or Relative相対を指定
            if (RadioAbsolute.IsChecked == true)
            {
                StepLine.Data = MyPath.Data.GetFlattenedPathGeometry(StepSlider.Value, ToleranceType.Absolute);
            }
            else if (RadioRelative.IsChecked == true)
            {
                StepLine.Data = MyPath.Data.GetFlattenedPathGeometry(StepSlider.Value, ToleranceType.Relative);
            }
        }
    }
}
