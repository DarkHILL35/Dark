using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Sphere
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool movingSphere;      // переменная для определения начала вращения сферы (по зажатию мышки)
        private double hAngle = 0;      // угол поворота по горизонтали
        private double vAngle = 0;      // угол поворота по вертикали
        private double dRadius = 6.0;   // радиус удаления камеры (в старом коде был 15)

        //переменные, отвечающие за включение/отключение анимации вращения
        private bool rot_left = false;
        private bool rot_right = false;

        private Win32Point absolutePosition;
        private Point initPosition;

        private readonly AuraSphereNew auraSphereNew;
        private readonly Human human;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        };

        public MainWindow()
        {
            movingSphere = false;

            human = new Human();
            auraSphereNew = new AuraSphereNew();
            
            InitializeComponent();
            
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Children.Add(human.Model);
            modelVisual3D.Children.Add(auraSphereNew.Model);
            sphereVisualization.Children.Add(modelVisual3D);

            designationsVisualizer.Children.Add(auraSphereNew.Designations);
            designationsVisualizer.Children.Add(human.Designations);

            //auraSphereNew.EllipseType = SphereEllipseType.NormCorridor;
        }

        private void btnValues_Click(object sender, RoutedEventArgs e)
        {
            WindowValues win = new WindowValues();

            // передача текущих значений от сферы в окно значений
            win.setValue(auraSphereNew.getValues());
            win.ShowDialog();

            if (win.massValue == null)
            {
                return;
            }

            // обновляем сферу
            auraSphereNew.updateValues(win.massValue);
        }

        //обновление камеры
        private void UpdateCamera(bool bZoom = false)
        {
            camera.Position = new Point3D(dRadius * Math.Cos(vAngle) * Math.Cos(hAngle),
                                          dRadius * Math.Sin(vAngle),
                                          dRadius * Math.Cos(vAngle) * Math.Sin(hAngle));

            light.Position = camera.Position;

            designationsCamera.Position = new Point3D(dRadius * Math.Cos(vAngle) * Math.Cos(0),
                                                      dRadius * Math.Sin(vAngle),
                                                      dRadius * Math.Cos(vAngle) * Math.Sin(0));
        }

        // нажатия мышки по 3d сцене
        private void border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            movingSphere = true;
            Mouse.OverrideCursor = Cursors.None;
            GetCursorPos(ref absolutePosition);
            initPosition = Mouse.GetPosition(sphereVisualization);
        }

        // движение мышки по 3d сцене
        private void border_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingSphere)
            {
                var position = Mouse.GetPosition(sphereVisualization);
                var speed = Math.PI * 500.0;

                //поменяла направление движения по обеим осям в обратную сторону (было инвертировано)
                hAngle += (position.X - initPosition.X) * Math.PI / speed;
                hAngle = hAngle < 0 ? Math.PI * 2.0 + hAngle : (hAngle >= Math.PI * 2.0 ? hAngle - Math.PI * 2.0 : hAngle);

                if (auraSphereNew.ViewType != SphereViewType.CrossSection)
                {
                    vAngle += (position.Y - initPosition.Y) * Math.PI / speed;
                    if (vAngle < -1.55)
                        vAngle = -1.55;
                    if (vAngle > 1.55)
                        vAngle = 1.55;
                }
                    
                else
                {
                    vAngle = 0;
                    auraSphereNew.CameraAngle = hAngle + Math.PI / 2;
                }

                UpdateCamera();

                SetCursorPos(absolutePosition.X, absolutePosition.Y);
            }
        }

        // отпускания мышки на 3d сцене
        private void border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            movingSphere = false;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        //выход за приделы 3d сцены
        private void border_MouseLeave(object sender, MouseEventArgs e)
        {
            movingSphere = false;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void visibleCheckedGrid(Visibility bVisible)
        {
            if (cb_horizontal_line != null)
            {
                cb_horizontal_line.Visibility = bVisible;
                cb_vertival_line.Visibility = bVisible;
                cb_diagonal_line.Visibility = bVisible;
                cb_point.Visibility = bVisible;
            }
        }

        private void rb_fill_Checked(object sender, RoutedEventArgs e)
        {
            auraSphereNew.ViewType = SphereViewType.Fill;
            visibleCheckedGrid(Visibility.Hidden);
        }

        private void rb_grid_Checked(object sender, RoutedEventArgs e)
        {
            auraSphereNew.ViewType = SphereViewType.Grid;
            visibleCheckedGrid(Visibility.Visible);
        }

        private void rb_cross_section_Checked(object sender, RoutedEventArgs e)
        {
            // поворот камеры на угол по
            vAngle = 0;
            auraSphereNew.CameraAngle = + Math.PI / 2;
            UpdateCamera();
            auraSphereNew.ViewType = SphereViewType.CrossSection;
            visibleCheckedGrid(Visibility.Hidden);
        }

        private void cb_horizontal_line_Click(object sender, RoutedEventArgs e)
        {
            if(cb_horizontal_line.IsChecked == true)
            {
                cb_point.IsChecked = false;
                auraSphereNew.bPointElelent = false;
            }
            auraSphereNew.bHorizontalElement = cb_horizontal_line.IsChecked.Value;
            auraSphereNew.ViewType = SphereViewType.Grid;
        }

        private void cb_vertival_line_Click(object sender, RoutedEventArgs e)
        {
            if (cb_vertival_line.IsChecked == true)
            {
                cb_point.IsChecked = false;
                auraSphereNew.bPointElelent = false;
            }
            auraSphereNew.bVerticalElement = cb_vertival_line.IsChecked.Value;
            auraSphereNew.ViewType = SphereViewType.Grid;
        }

        private void cb_diagonal_line_Click(object sender, RoutedEventArgs e)
        {
            if (cb_diagonal_line.IsChecked == true)
            {
                cb_point.IsChecked = false;
                auraSphereNew.bPointElelent = false;
            }
            auraSphereNew.bDiaganalElement = cb_diagonal_line.IsChecked.Value;
            auraSphereNew.ViewType = SphereViewType.Grid;
        }

        private void cb_point_Click(object sender, RoutedEventArgs e)
        {
            if (cb_point.IsChecked == true)
            {
                cb_horizontal_line.IsChecked = false;
                cb_vertival_line.IsChecked = false;
                cb_diagonal_line.IsChecked = false;

                auraSphereNew.bHorizontalElement = false;
                auraSphereNew.bVerticalElement = false;
                auraSphereNew.bDiaganalElement = false;
            }
            auraSphereNew.bPointElelent = cb_point.IsChecked.Value;
            auraSphereNew.ViewType = SphereViewType.Grid;
        }

        //изменение прозрачности сферы слайдером
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            auraSphereNew.Opacity = e.NewValue / 100.0;
        }

        //масштабирование слайдером
        private void cameraZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dRadius = 50 - e.NewValue;

            UpdateCamera(true);
        }

        //масштабирование колесиком мышки
        private void border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoom = (e.Delta > 0 ? -0.25 : 0.25);
            if (zoom > 0 && dRadius + zoom > 10)
                return;
            if (zoom < 0 && dRadius - zoom < 2)
                return;
            dRadius += zoom;
            cameraZoomSlider.ValueChanged -= cameraZoomSlider_ValueChanged;
            cameraZoomSlider.Value = 50 - dRadius;
            cameraZoomSlider.ValueChanged += cameraZoomSlider_ValueChanged;
            UpdateCamera(true);
        }

        private void rb_NormCorridor_Checked(object sender, RoutedEventArgs e)
        {
            auraSphereNew.EllipseType = SphereEllipseType.NormCorridor;
        }

        private void rb_RiskCorridor_Checked(object sender, RoutedEventArgs e)
        {
            auraSphereNew.EllipseType = SphereEllipseType.RiskCorridor;
        }

        private void rb_Either_Checked(object sender, RoutedEventArgs e)
        {
            auraSphereNew.EllipseType = SphereEllipseType.Ethers;
        }

        private void rb_Male_Checked(object sender, RoutedEventArgs e)
        {
            human.GetGender = Human.Gender.Male;
        }

        private void rb_Female_Checked(object sender, RoutedEventArgs e)
        {
            human.GetGender = Human.Gender.Female;
        }

        private bool animation;
        private Task taskRotation;

        private void btRotationRight_Click(object sender, RoutedEventArgs e)
        {
            if (rot_right == true)
            {
                animation = false;
                rot_right = false;
                btRotationRight.Background = new SolidColorBrush(Colors.Gainsboro);
                btRotationRight.BorderBrush = new SolidColorBrush(Colors.DimGray);
            }
            else
            {
                rot_right = true;
                RotateRight();
                if (animation == true)
                {
                    rot_left = false;
                    btRotationLeft.Background = new SolidColorBrush(Colors.Gainsboro);
                    btRotationLeft.BorderBrush = new SolidColorBrush(Colors.DimGray);
                }
                btRotationRight.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#bee6fd");
                btRotationRight.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#3c7fb1");
            }
        }

        private void btRotationLeft_Click(object sender, RoutedEventArgs e)
        {
            if (rot_left == true)
            {
                animation = false;
                rot_left = false;
                btRotationLeft.Background = new SolidColorBrush(Colors.Gainsboro);
                btRotationLeft.BorderBrush = new SolidColorBrush(Colors.DimGray);
            }
            else
            {
                rot_left = true;
                RotateLeft();
                if (animation == true)
                {
                    rot_right = false;
                    btRotationRight.Background = new SolidColorBrush(Colors.Gainsboro);
                    btRotationRight.BorderBrush = new SolidColorBrush(Colors.DimGray);
                }
                btRotationLeft.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#bee6fd");
                btRotationLeft.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#3c7fb1");
            }
        }

        private void RotateLeft()
        {
            animation = false;
            if (taskRotation != null)
                taskRotation.Wait();
            animation = true;
            taskRotation = Task.Run(() =>
            {
                while (animation)
                {
                    try
                    {
                        hAngle -= 0.01;
                        if (hAngle > Math.PI * 2)
                            hAngle -= Math.PI * 2;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            auraSphereNew.CameraAngle = hAngle;
                            UpdateCamera();
                        }));

                        Thread.Sleep(15);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Возникла ошибка при вращении 3D-сцены:\n" + e.Message,
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        animation = false;
                        break;
                    };
                }
            });
        }

        private void RotateRight()
        {
            animation = false;
            if (taskRotation != null)
                taskRotation.Wait();
            animation = true;
            taskRotation = Task.Run(() =>
            {
                while (animation)
                {
                    try
                    {
                        hAngle += 0.01;
                        if (hAngle < 0)
                            hAngle += Math.PI * 2;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            auraSphereNew.CameraAngle = hAngle;
                            UpdateCamera();
                        }));

                        Thread.Sleep(15);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Возникла ошибка при вращении 3D-сцены:\n" + e.Message,
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        animation = false;
                        break;
                    };
                }
            });
        }
    }
}
