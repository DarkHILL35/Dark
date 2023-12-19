using HelixToolkit.Wpf;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Sphere
{
    public class Human
    {
        public enum Gender
        {
            Male,
            Female
        }

        private Gender gender;
        public Gender GetGender
        {
            set
            {
                gender = value;
                switch (gender)
                {
                    case Gender.Male:
                        {
                            SetGender(men, chakrasMen, 0.144, -6.7);
                        }
                        break;
                        case Gender.Female:
                        {
                            SetGender(women, chakrasWomen, 0.153, -6.4);
                        }
                        break;
                }
            }
            get => gender;
        }

        private double cameraAngle;
        public double CameraAngle
        {
            set
            {
                cameraAngle = value;
                axisAngleRotation3DCamera.Axis = new Vector3D(0, 0, 1);
                axisAngleRotation3DCamera.Angle = cameraAngle / (Math.PI * 2) * 360;
                rotateTransform3DCamera.Rotation = axisAngleRotation3DCamera;
            }
            get => cameraAngle;
        }

        private Model3DGroup men;
        private Model3DGroup women;
        private Model3DGroup chakrasMen;
        private Model3DGroup chakrasWomen;
        private RotateTransform3D rotateTransform3DCamera;
        private AxisAngleRotation3D axisAngleRotation3DCamera;
        private Transform3DGroup transform3DGroup;
        private ScaleTransform3D scaleTransform3D;
        private TranslateTransform3D translateTransform3D;

        public ModelVisual3D Model { set; get; }
        public ModelVisual3D Designations { set; get; }

        public Human()
        {
            //загрузка мужской и женской моделей
            ObjReader objectReader = new ObjReader();
            men = objectReader.Read($"{Environment.CurrentDirectory}\\human.obj");
            objectReader = new ObjReader();
            women = objectReader.Read($"{Environment.CurrentDirectory}\\women.obj");

            Model = new ModelVisual3D();
            Designations = new ModelVisual3D();

            //translateTransform3D = new TranslateTransform3D();
            //translateTransform3D.OffsetY = -7.4; //смещение по OY -6.0

            //scaleTransform3D = new ScaleTransform3D();
            //масштабирование модели
            //scaleTransform3D.ScaleX = 0.134; //0.16
            //scaleTransform3D.ScaleY = 0.134; //0.16
            //scaleTransform3D.ScaleZ = 0.134; //0.16

            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            AxisAngleRotation3D axisAngleRotation3D = new AxisAngleRotation3D();
            transform3DGroup = new Transform3DGroup();
            axisAngleRotation3DCamera = new AxisAngleRotation3D();
            rotateTransform3DCamera = new RotateTransform3D();

            axisAngleRotation3D.Axis = new Vector3D(0, 1, 0); //проверить значения
            axisAngleRotation3D.Angle = 90;
            rotateTransform3D.Rotation = axisAngleRotation3D;

            transform3DGroup.Children.Add(rotateTransform3D);
            //transform3DGroup.Children.Add(translateTransform3D);
            //transform3DGroup.Children.Add(scaleTransform3D);

            //написать функцию отрисовки квадратов - упростить до ввода двух параметров: координата, цвет (мб толщина)

            //отрисовка квадратов
            chakrasMen = new Model3DGroup();
            //При обводке появляется мерцание - пофиксить
            //обводка
            //chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.3, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.2, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.1, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.1, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.25, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.4, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.45, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));

            //заполнение цветом
            chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.35, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.Red))));
            chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.26, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.Orange))));
            chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.16, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.Yellow))));
            chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.00, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.YellowGreen))));
            chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.12, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.SkyBlue))));
            chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.26, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.Blue))));
            chakrasMen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.31, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.BlueViolet))));

            chakrasWomen = new Model3DGroup();
            //обводка
            //chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.3, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.25, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.1, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.1, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.18, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.32, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));
            //chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.4, 0), 0.03), new DiffuseMaterial(new SolidColorBrush(Colors.Black))));

            //заполнение цветом
            chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.33, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.Red))));
            chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.26, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.Orange))));
            chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.16, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.Yellow))));
            chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, -0.00, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.YellowGreen))));
            chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.12, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.SkyBlue))));
            chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.26, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.Blue))));
            chakrasWomen.Children.Add(new GeometryModel3D(CreateSquare(new Point3D(0, 0.32, 0), 0.025), new DiffuseMaterial(new SolidColorBrush(Colors.BlueViolet))));

            GetGender = Gender.Male;

            Model.Transform = transform3DGroup;
            Designations.Transform = rotateTransform3DCamera;
        }

        //функция для отрисовки квадрата
        private MeshGeometry3D CreateSquare(Point3D position, double size)
        {
            MeshGeometry3D meshGeometry3DEllipse = new MeshGeometry3D();
            meshGeometry3DEllipse.Positions.Add(new Point3D(position.X, position.Y - size / 2, position.Z - size / 2));
            meshGeometry3DEllipse.Positions.Add(new Point3D(position.X, position.Y - size / 2, position.Z + size / 2));
            meshGeometry3DEllipse.Positions.Add(new Point3D(position.X, position.Y + size / 2, position.Z + size / 2));
            meshGeometry3DEllipse.Positions.Add(new Point3D(position.X, position.Y + size / 2, position.Z - size / 2));

            meshGeometry3DEllipse.TriangleIndices.Add(2);
            meshGeometry3DEllipse.TriangleIndices.Add(1);
            meshGeometry3DEllipse.TriangleIndices.Add(0);

            meshGeometry3DEllipse.TriangleIndices.Add(3);
            meshGeometry3DEllipse.TriangleIndices.Add(2);
            meshGeometry3DEllipse.TriangleIndices.Add(0);

            return meshGeometry3DEllipse;
        }

        private void SetGender(Model3DGroup model, Model3DGroup chakras, double size, double dy)
        {
            Model.Content = model;
            Designations.Content = chakras;
            transform3DGroup.Children.Remove(translateTransform3D);
            transform3DGroup.Children.Remove(scaleTransform3D);
            scaleTransform3D = new ScaleTransform3D(size, size, size);
            translateTransform3D = new TranslateTransform3D(0, dy, 0);
            transform3DGroup.Children.Add(translateTransform3D);
            transform3DGroup.Children.Add(scaleTransform3D);
        }
    }
}
