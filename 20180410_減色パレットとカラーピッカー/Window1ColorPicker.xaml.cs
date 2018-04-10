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
using System.Windows.Shapes;

namespace _20180410_減色パレットとカラーピッカー
{
    public partial class Window1ColorPicker : Window
    {
        public Window1ColorPicker()
        {
            InitializeComponent();
            this.Closing += Window1ColorPicker_Closing;
            ButtonSetColor.Click += ButtonSetColor_Click;
            ButtonCancel.Click += ButtonCancel_Click;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;//非表示
        }

        private void ButtonSetColor_Click(object sender, RoutedEventArgs e)
        {
            //this.Visibility = Visibility.Hidden;
            //色をMainWindowに送る
            MainWindow main = (MainWindow)this.Owner;
            main.SetColor(ColorPicker.PickupColor);
        }
        //閉じるボタンとかで閉じようとした時はキャンセルして非表示
        private void Window1ColorPicker_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
