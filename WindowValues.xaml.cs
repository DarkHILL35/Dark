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
using System.Text.RegularExpressions;

namespace Sphere
{
    /// <summary>
    /// Логика взаимодействия для WindowValues.xaml
    /// </summary>
    public partial class WindowValues : Window
    {
        public int []massValue;

        private bool isFocused = false;

        public WindowValues()
        {
            InitializeComponent();
        }

        public void setValue(int[] massValue)
        {
            int i = 0;
            foreach (UIElement el in MainView.Children)
            {
                if (el is TextBox)
                {
                    ((TextBox)el).Text = massValue[i].ToString();
                    ++i;
                }
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            massValue = new int[6 * 7];

            // сохраняем новые значения для передачи в основное окно
            foreach (UIElement el in MainView.Children)
            {
                if (el is TextBox)
                {
                    massValue[i] = Int32.Parse(((TextBox)el).Text);
                    ++i;
                }
            }

            // закрываем
            Close();
        }

        // проверка вводимого значения на число
        private void NumberValidationTexBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //выделение всего текста в текстбоксе
            if (isFocused)
            {
                isFocused = false;
                (sender as TextBox).SelectAll();
            }
        }

        private void CellFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            isFocused = true;
            //Если мышь не нажимается, вызываем событие, чтобы выделился весь текст
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                TextBox_SelectionChanged(sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //при загрузке окна выделяем первую точку измерений
            L11.Focus();
            CellFocus(L11, e);
        }
    }
}
