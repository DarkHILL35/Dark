using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;

namespace Sphere
{
    public class SigmentData
    {
        public ushort L1;
        public ushort L2;
        public ushort L3;

        public ushort R1;
        public ushort R2;
        public ushort R3;
    }

    //режимы сферы
    public enum SphereViewType
    {
        Grid,
        Fill,
        CrossSection
    }

    //эллипсы
    public enum SphereEllipseType
    {
        NormCorridor,
        RiskCorridor,
        Ethers
    }

    class AuraSphereNew
    {
        // количество секций в сфере
        private int iLongitudeCount;
        private int iLatitudeCount;

        private double[] angleArrayLatitude;

        private Point3D[,] arrayPoints;
        private double[,] arrayUnitFromColor;

        public SigmentData[] Arrays;

        private int[] massLatitudeCountToSigment = { 6, 6, 8, 8, 8, 6, 6 };

        private SphereViewType viewType;
        private SphereEllipseType ellipseType;

        public ModelVisual3D Model { get; private set; }
        public ModelVisual3D Designations { get; private set; }

        private Model3DGroup sphere;
        private Model3DGroup designations;
        private Model3DGroup ellipseNormCoridor;
        private Model3DGroup ellipseRiskCoridor;
        private Model3DGroup ellipseEither;

        private DiffuseMaterial diffuseMaterial;
        private SolidColorBrush brush;

        private GeometryModel3D[] geometryFill;
        private GeometryModel3D[] geometryGrid;
        private GeometryModel3D[] geometryCrossSection;

        private RotateTransform3D rotateTransform3D;
        private ScaleTransform3D scaleTransform3D;
        private AxisAngleRotation3D axisAngleRotation3D;
        private AxisAngleRotation3D axisAngleRotation3DEllips;
        private TranslateTransform3D translateTransform3D;
        private Transform3DGroup transform3DGroupSphere;
        private Transform3DGroup transform3DGroupEllipse;
        private RotateTransform3D rotateTransform3DIdeal;

        // элементы сетки
        public bool bHorizontalElement;
        public bool bVerticalElement;
        public bool bDiaganalElement;
        public bool bPointElelent;

        private double cameraAngle;

        // угол камеры для расчета сечения
        public double CameraAngle
        {
            set
            {
                cameraAngle = value;
                if (ViewType == SphereViewType.CrossSection)
                {
                    sphere.Children.Clear();
                    UpdatePointCrossSection();
                }
            }
            get => cameraAngle;
        }

        // режи отоброжения сферы
        public SphereViewType ViewType
        {
            set
            {
                viewType = value;

                sphere.Children.Clear();
                switch (viewType)
                {
                    case SphereViewType.Grid:
                        {
                            for (int iGeometry = 0; iGeometry < (iLatitudeCount * iLongitudeCount * 4); iGeometry++)
                            {
                                if (geometryGrid[iGeometry] != null)
                                {
                                    if (iGeometry % 4 == 0 && bVerticalElement)
                                        sphere.Children.Add(geometryGrid[iGeometry]);
                                    else if (iGeometry % 4 == 1 && bHorizontalElement)
                                        sphere.Children.Add(geometryGrid[iGeometry]);
                                    else if (iGeometry % 4 == 2 && bDiaganalElement)
                                        sphere.Children.Add(geometryGrid[iGeometry]);
                                    else if (iGeometry % 4 == 3 && bPointElelent)
                                        sphere.Children.Add(geometryGrid[iGeometry]);
                                }
                            }
                        }
                        break;
                    case SphereViewType.Fill:
                        {
                            for (int iGeometry = 0; iGeometry < (iLatitudeCount * iLongitudeCount); iGeometry++)
                            {
                                sphere.Children.Add(geometryFill[iGeometry]);
                            }
                        }
                        break;
                    case SphereViewType.CrossSection:
                        {
                            UpdatePointCrossSection();
                        }
                        break;
                }
            }
            get => viewType;
        }

        public SphereEllipseType EllipseType
        {
            set
            {
                if (ellipseType != value)
                {
                    switch (value)
                    {
                        case SphereEllipseType.NormCorridor:
                            {
                                if (!designations.Children.Contains(ellipseNormCoridor))
                                    designations.Children.Add(ellipseNormCoridor);
                                if (designations.Children.Contains(ellipseRiskCoridor))
                                    designations.Children.Remove(ellipseRiskCoridor);
                                if (designations.Children.Contains(ellipseEither))
                                    designations.Children.Remove(ellipseEither);
                            }
                            break;
                        case SphereEllipseType.RiskCorridor:
                            {
                                if (designations.Children.Contains(ellipseNormCoridor))
                                    designations.Children.Remove(ellipseNormCoridor);
                                if (!designations.Children.Contains(ellipseRiskCoridor))
                                    designations.Children.Add(ellipseRiskCoridor);
                                if (designations.Children.Contains(ellipseEither))
                                    designations.Children.Remove(ellipseEither);
                            }
                            break;
                        case SphereEllipseType.Ethers:
                            {
                                if (designations.Children.Contains(ellipseNormCoridor))
                                    designations.Children.Remove(ellipseNormCoridor);
                                if (designations.Children.Contains(ellipseRiskCoridor))
                                    designations.Children.Remove(ellipseRiskCoridor);
                                if (!designations.Children.Contains(ellipseEither))
                                    designations.Children.Add(ellipseEither);
                            }
                            break;
                        default:
                            {
                                if (!designations.Children.Contains(ellipseNormCoridor))
                                    designations.Children.Add(ellipseNormCoridor);
                                if (designations.Children.Contains(ellipseRiskCoridor))
                                    designations.Children.Remove(ellipseRiskCoridor);
                                if (designations.Children.Contains(ellipseEither))
                                    designations.Children.Remove(ellipseEither);
                            }
                            break;
                    }
                    ellipseType = value;
                }
            }
            get => ellipseType;
        }

        public double Opacity
        {
            set { diffuseMaterial.Brush.Opacity = diffuseMaterial.Brush == null ? 0.5f : value; }
            get  { return diffuseMaterial.Brush == null ? 0.5f : diffuseMaterial.Brush.Opacity; }
        }

        public AuraSphereNew()
        {
            iLongitudeCount = 36;
            iLatitudeCount = 48;

            bHorizontalElement = true;
            bVerticalElement = true;
            bDiaganalElement = true;
            bPointElelent = false;

            // устанавливаем градусы для горизонталей
            angleArrayLatitude = new double[iLatitudeCount];
            for (int iLatitude = 0; iLatitude < iLatitudeCount; iLatitude++)
            {
                if (iLatitude <= 11 || iLatitude >= 36)
                    angleArrayLatitude[iLatitude] = 5;
                else 
                    angleArrayLatitude[iLatitude] = 2.5;
            }

            // заполняем базовые радиусы для сегментов сферы
            Arrays = new SigmentData[7];
            for (int i = 0; i < Arrays.Length; i++)
            {
                //if(i == 1)
                    //Arrays[i] = new SigmentData { L1 = 80, L2 = 20, L3 = 20, R1 = 20, R2 = 20, R3 = 20 };
                //else if(i == 3 || i == 5)
                //if (i == 3)
                //    Arrays[i] = new SigmentData { L1 = 80, L2 = 80, L3 = 80, R1 = 80, R2 = 80, R3 = 80 };
                //else
                    Arrays[i] = new SigmentData { L1 = 60, L2 = 60, L3 = 60, R1 = 60, R2 = 60, R3 = 60 };
            }

            EllipseType = SphereEllipseType.NormCorridor;

            // расчет точек сетки вершин сферы (x,y,z)
            calcArrayPoints();

            // создаем объекты для отображения
            sphere = new Model3DGroup();
            designations = new Model3DGroup();
            Model = new ModelVisual3D();
            Designations = new ModelVisual3D();
            ellipseNormCoridor = new Model3DGroup();
            ellipseRiskCoridor = new Model3DGroup();
            ellipseEither = new Model3DGroup();

            diffuseMaterial = new DiffuseMaterial();
            brush = new SolidColorBrush();

            translateTransform3D = new TranslateTransform3D();
            transform3DGroupSphere = new Transform3DGroup();
            transform3DGroupEllipse = new Transform3DGroup();
            rotateTransform3DIdeal = new RotateTransform3D();
            rotateTransform3D = new RotateTransform3D();
            scaleTransform3D = new ScaleTransform3D();
            axisAngleRotation3D = new AxisAngleRotation3D();
            axisAngleRotation3DEllips = new AxisAngleRotation3D();

            axisAngleRotation3D.Axis = new Vector3D(0, 1, 0); // 1 0 0
            axisAngleRotation3DEllips.Axis = new Vector3D(0, 0, 1); //0 1 0
            rotateTransform3D.Rotation = axisAngleRotation3D;
            transform3DGroupSphere.Children.Add(rotateTransform3D);
            transform3DGroupSphere.Children.Add(translateTransform3D);
            transform3DGroupSphere.Children.Add(scaleTransform3D);
            rotateTransform3DIdeal.Rotation = axisAngleRotation3DEllips;
            transform3DGroupEllipse.Children.Add(rotateTransform3D);
            transform3DGroupEllipse.Children.Add(rotateTransform3DIdeal);
            transform3DGroupEllipse.Children.Add(translateTransform3D);
            transform3DGroupEllipse.Children.Add(scaleTransform3D);

            CameraAngle = Math.PI / 2;

            //инициализация эллипсов
            //норма
            ellipseNormCoridor.Children.Add(CreateEllipseNew(1.0, Colors.LimeGreen)); //0.5
            ellipseNormCoridor.Children.Add(CreateEllipseNew(0.4, Colors.Red)); //0.15

            //коридор
            ellipseRiskCoridor.Children.Add(CreateEllipseNew(1.05, Colors.LimeGreen)); //0.55
            ellipseRiskCoridor.Children.Add(CreateEllipseNew(0.95, Colors.LimeGreen)); //0.45
            ellipseRiskCoridor.Children.Add(CreateEllipseNew(0.4, Colors.Red)); //0.15

            //эфиры
            ellipseEither.Children.Add(CreateEllipseNew(1.0, Colors.Yellow)); //0.6
            ellipseEither.Children.Add(CreateEllipseNew(0.85, Colors.Orange)); //0.45
            ellipseEither.Children.Add(CreateEllipseNew(0.75, Colors.Blue)); //0.35
            ellipseEither.Children.Add(CreateEllipseNew(0.6, Colors.Red)); //0.225

            Model.Content = sphere;
            Designations.Content = designations;

            // отображаем
            drawSphere();
            ViewType = SphereViewType.Grid;
            Opacity = 0.5f;

            sphere.Transform = transform3DGroupSphere;
            ellipseEither.Transform = transform3DGroupEllipse;
            ellipseNormCoridor.Transform = transform3DGroupEllipse;
            ellipseRiskCoridor.Transform = transform3DGroupEllipse;
        }

        // поиск индекса сектора для горизонтали
        private int getIndexLatitude(int iLatitude)
        {
            for (int i = 0; i < 7; i++)
            {
                if (iLatitude < massLatitudeCountToSigment[i])
                    return i;
                iLatitude -= massLatitudeCountToSigment[i];
            }
            return 7;
        }

        // получение первого latitude в секции по номеру секции
        private int getFirstLatitudeToSectionFromSection(int iSection)
        {
            int iLatitudeToSection = 0;
            for (int i = 0; i < 7; i++)
            {
                if (i == iSection)
                    return iLatitudeToSection;
                iLatitudeToSection += massLatitudeCountToSigment[i];
            }
            return 100;
        }

        // определяем еденицы точки для сектора и iLongitude
        private int getUnitFromSectionLongitude(int iLongitude, int indexSection)
        {
            int iMaxCount = 200;
            if (indexSection == 0)
            {
                iMaxCount = minTopLenght(0);
            }
            if (indexSection == 6)
            {
                iMaxCount = minTopLenght(iLatitudeCount - 1);
            }

            if (iLongitude < iLongitudeCount / 2)
            {
                if (iLongitude < 6)
                    return iMaxCount < Arrays[indexSection].L1 ? iMaxCount : Arrays[indexSection].L1;
                else if (iLongitude < 12)
                    return iMaxCount < Arrays[indexSection].L3 ? iMaxCount : Arrays[indexSection].L3;
                else
                    return iMaxCount < Arrays[indexSection].L2 ? iMaxCount : Arrays[indexSection].L2;
            }
            else
            {
                if (iLongitude < 24)
                    return iMaxCount < Arrays[indexSection].R2 ? iMaxCount : Arrays[indexSection].R2;
                else if (iLongitude < 30)
                    return iMaxCount < Arrays[indexSection].R3 ? iMaxCount : Arrays[indexSection].R3;
                else
                    return iMaxCount < Arrays[indexSection].R1 ? iMaxCount : Arrays[indexSection].R1;
            }
        }

        // расчет плавного перехода для горизонтали и вертикали
        private double smoothForLatitudeLongitude(int iLongitude, int iLatitude)
        {
            // получаем номера секций
            int indexCurrentSection = getIndexLatitude(iLatitude);
            int indexUpSection = indexCurrentSection == 0 ? indexCurrentSection : indexCurrentSection - 1;
            int indexDownSection = indexCurrentSection == 6 ? indexCurrentSection : indexCurrentSection + 1;

            // получения количество единиц для текущей и соседних облостей
            int iCurrentUnitSection = getUnitFromSectionLongitude(iLongitude, indexCurrentSection);
            int iUpUnitSection = getUnitFromSectionLongitude(iLongitude, indexUpSection);
            int iDownUnitSection = getUnitFromSectionLongitude(iLongitude, indexDownSection);

            // получаем количество latitude в секциях
            int countLatitudeCurrentSection = massLatitudeCountToSigment[indexCurrentSection];
            int iNumLatitude = iLatitude - getFirstLatitudeToSectionFromSection(indexCurrentSection) + 1;

            // если верхнее полушарие то движение вниз, иначе вверх
            if (iLatitude < iLatitudeCount / 2)
            {
                // если центральный сегмент, длина сегмента половина сегмента
                if (indexCurrentSection == 3)
                    countLatitudeCurrentSection /= 2;
                return iUpUnitSection + (double)(iCurrentUnitSection - iUpUnitSection) /
                                        (double)(countLatitudeCurrentSection) * (double)iNumLatitude;
            }
            if (indexCurrentSection == 3)
            {
                countLatitudeCurrentSection /= 2;
                iNumLatitude -= countLatitudeCurrentSection;
            }
            return iCurrentUnitSection + (double)(iDownUnitSection - iCurrentUnitSection) /
                                         (double)(countLatitudeCurrentSection) * (double)iNumLatitude;
        }

        // обновление значений переменных для сферы
        public void updateValues(int[] massValue)
        {
            for (int i = 0; i < 7; i++)
            {
                Arrays[i].L1 = (ushort)massValue[i];
                Arrays[i].L2 = (ushort)massValue[i + 7];
                Arrays[i].L3 = (ushort)massValue[i + 14];
                Arrays[i].R1 = (ushort)massValue[i + 21];
                Arrays[i].R2 = (ushort)massValue[i + 28];
                Arrays[i].R3 = (ushort)massValue[i + 35];
            }

            // расчет точек сетки вершин сферы (x,y,z)
            calcArrayPoints();
            drawSphere();

            // отображаем
            ViewType = ViewType;
        }

        // получение точек из модели
        public int[] getValues()
        {
            int[] massValue = new int[6 * 7];

            for (int i = 0; i < 7; i++)
            {
                massValue[i] = Arrays[i].L1;
                massValue[i + 7] = Arrays[i].L2;
                massValue[i + 14] = Arrays[i].L3;
                massValue[i + 21] = Arrays[i].R1;
                massValue[i + 28] = Arrays[i].R2;
                massValue[i + 35] = Arrays[i].R3;
            }
            return massValue;
        }

        // расчет вершин сферы
        private void calcArrayPoints()
        {
            double angle = 2 * Math.PI / iLongitudeCount;
            double oneAngle = Math.PI / 180;
            double deltaPhi = 0;

            // расчет всех точек сферы
            arrayPoints = new Point3D[iLatitudeCount, iLongitudeCount];
            arrayUnitFromColor = new double[iLatitudeCount, iLongitudeCount];
            for (int iLatitude = 0; iLatitude < iLatitudeCount; iLatitude++)    //48
            {
                double phi = 0;

                double currentPhi = oneAngle * angleArrayLatitude[iLatitude];
                deltaPhi += currentPhi;

//                System.Diagnostics.Debug.WriteLine(iLatitude + ", " + deltaPhi);

                double t = Math.Atan2(Math.Sin(deltaPhi), 0.4 * Math.Cos(deltaPhi));
                double y = Math.Cos(t);

                for (int iLongitude = 0; iLongitude < iLongitudeCount; iLongitude++)    //36
                {
                    // получаем значение для выбранной точки на сфере
                    double iUnit = smoothForLatitudeLongitude(iLongitude, iLatitude);
                    //if(iLongitude == 0)
                    //{
                    //    System.Diagnostics.Debug.WriteLine(iLatitude + " iUnit - " + iUnit);
                    //}

                    // расчет коэф. для вытянутости или более округлой формы 1 - круг
                    double dCoeff = 0.75;
                    if(iUnit < 40)
                    {
                        dCoeff = ((0.75 - 0.4) / 40) * iUnit + 0.4;
                    }
                    else if (iUnit > 80)
                    {
                        dCoeff = 0.05 / 20 * (iUnit - 80) + 0.75;
                    }
                    double radiusCircle = dCoeff * Math.Sin(t);

                    // переводим единицы в радиус
                    double iRadius = (40.0 + iUnit) / 100.0;

                    arrayPoints[iLatitude, iLongitude] = new Point3D(iRadius * radiusCircle * Math.Cos(phi), 
                                                                     iRadius * y, 
                                                                     iRadius * radiusCircle * Math.Sin(phi));
                    // сохранение значение радиуса для точки в unit
                    arrayUnitFromColor[iLatitude, iLongitude] = iUnit;
                    if (iLatitude == iLatitudeCount - 2)
                        arrayUnitFromColor[iLatitude + 1, iLongitude] = iUnit;

                    phi += angle;
                }
            }
        }

        // расчет вершин среза и формирования ребер
        private void UpdatePointCrossSection()
        {
            double oneAngle = Math.PI / 180;
            double deltaPhi = 0;

            Point3D[,] arrayCrossSectionPoints = new Point3D[iLatitudeCount, 2];
            for (int iLatitude = 0; iLatitude < iLatitudeCount; iLatitude++)    //48
            {
                double phi = CameraAngle;

                double currentPhi = oneAngle * angleArrayLatitude[iLatitude];
                deltaPhi += currentPhi;

                double t = Math.Atan2(Math.Sin(deltaPhi), 0.4 * Math.Cos(deltaPhi));

                double y = Math.Cos(t);
                double radiusCircle = 0.75 * Math.Sin(t);

                double oneLongitude = 2 * Math.PI / iLongitudeCount;
                int iFirstLongitude = (int)( phi / oneLongitude);

                // получаем значение для выбранной точки на сфере
                for (int iLongitude = 0; iLongitude < 2; iLongitude++) {
                    double iUnit = smoothForLatitudeLongitude(iLongitude == 0 ? iFirstLongitude : (iFirstLongitude + 18) % 36, iLatitude);

                    // переводим единицы в радиус
                    double iRadius = (40.0 + iUnit) / 100.0;

                    arrayCrossSectionPoints[iLatitude, iLongitude] = new Point3D(iRadius * radiusCircle * Math.Cos(phi),
                                                                                 iRadius * y,
                                                                                 iRadius * radiusCircle * Math.Sin(phi));                    
                    phi += Math.PI;
                }
            }

            geometryCrossSection = new GeometryModel3D[iLatitudeCount * 2];
            for (int iLatitude = 0; iLatitude < iLatitudeCount; iLatitude++) // 48
            {
                for (int iLongitude = 0; iLongitude < 2; iLongitude++) // 36
                {
                    if (iLatitude == 0 || iLatitude == iLatitudeCount - 1)
                    {
                        // построение вершин
                        var topPoint = iLatitude == 0 ? 0 : iLatitude - 1;

                        var point1 = calcTopPoint(topPoint);
                        var point2 = arrayCrossSectionPoints[topPoint, iLongitude];

                        geometryCrossSection[iLongitude * iLatitudeCount + iLatitude] = drawLine(point1, point2, Colors.Blue, 0.01);
                    }
                    else
                    {
                        var point1 = arrayCrossSectionPoints[iLatitude - 1, iLongitude];
                        var point2 = arrayCrossSectionPoints[iLatitude, iLongitude];

                        geometryCrossSection[iLongitude * iLatitudeCount + iLatitude] = drawLine(point1, point2, Colors.Blue, 0.01);
                    }
                }
            }

            // заполняем элементами модель
            for (int iGeometry = 0; iGeometry < (iLatitudeCount * 2); iGeometry++)
            {
                sphere.Children.Add(geometryCrossSection[iGeometry]);
                //geometryCrossSection[iGeometry].Transform = transform3DGroupSphere;
            }
        }

        // построение сферы
        private void drawSphere()
        {
            geometryFill = new GeometryModel3D[iLatitudeCount * iLongitudeCount];
            geometryGrid = new GeometryModel3D[iLatitudeCount * iLongitudeCount * 4];

            for (int iLatitude = 0; iLatitude < iLatitudeCount; iLatitude++) // 48
            {
                for (int iLongitude = 0; iLongitude < iLongitudeCount; iLongitude++) // 36
                {
                    // вычисления цвета сегмента
                    Color color = Colors.Blue;
                    double iCalcUnit = arrayUnitFromColor[iLatitude, iLongitude] + 
                                       arrayUnitFromColor[iLatitude == 0 ? iLatitude : iLatitude - 1, iLongitude];

                    if (iCalcUnit >= 121.0)
                    {
                        //от 60 до 100 плавно переходим из Blue в Cyan
                        //80 - отрезок от 60 до 100, 120 - iCalcUnit при значении показателя 60
                        //color = Colors.Cyan;
                        color.R = 0;
                        color.G = Convert.ToByte(255 * (iCalcUnit - 120) / 80);
                    }
                        
                    if (iCalcUnit <= 119.0)
                    {
                        //от 60 до 0 плавно переходим из Blue в Magenta
                        //120 - отрезок от 60 до 0, 120 - iCalcUnit при значении показателя 60
                        //color = Colors.Magenta;
                        color.R = Convert.ToByte(255 * (120 - iCalcUnit) / 120);
                        color.G = 0;
                    }

                    color.A = 255;

                    GeometryModel3D geometry;
                    if (iLatitude == 0 || iLatitude == iLatitudeCount - 1)
                    {
                        // построение вершин
                        var topPoint = iLatitude == 0 ? 0 : iLatitude - 1;

                        var point1 = calcTopPoint(topPoint);
                        var point2 = arrayPoints[topPoint, iLongitude];
                        var point3 = arrayPoints[topPoint, iLongitude == iLongitudeCount - 1 ? 0 : iLongitude + 1];

                        geometry = drawTriangle(iLatitude == 0 ? point1 : point3, 
                                                iLatitude == 0 ? point3 : point1, 
                                                point2, 
                                                color);

                        // grid
                        geometryGrid[iLatitude * iLongitudeCount * 4 + iLongitude * 4] = drawLine(point1, point3, color);
                        geometryGrid[iLatitude * iLongitudeCount * 4 + iLongitude * 4 + 1] = drawLine(point3, point2, color);
                        geometryGrid[iLatitude * iLongitudeCount * 4 + iLongitude * 4 + 2] = drawLine(point1, point3, color);

                        // point
                        geometryGrid[iLatitude * iLongitudeCount * 4 + iLongitude * 4 + 3] = drawCube(point1, color);
                    }
                    else
                    {
                        var point1 = arrayPoints[iLatitude - 1, iLongitude];
                        var point2 = arrayPoints[iLatitude - 1, iLongitude == iLongitudeCount - 1 ? 0 : iLongitude + 1];
                        var point3 = arrayPoints[iLatitude, iLongitude];
                        var point4 = arrayPoints[iLatitude, iLongitude == iLongitudeCount - 1 ? 0 : iLongitude + 1];

                        geometry = drawTrapezoid(point1, point2, point3, point4, color);

                        // grid 
                        geometryGrid[iLatitude * iLongitudeCount * 4 + iLongitude * 4] = drawLine(point1, point3, color);
                        geometryGrid[iLatitude * iLongitudeCount * 4 + iLongitude * 4 + 1] = drawLine(point3, point4, color);
                        geometryGrid[iLatitude * iLongitudeCount * 4 + iLongitude * 4 + 2] = drawLine(point1, point4, color);

                        // point
                        geometryGrid[iLatitude * iLongitudeCount * 4 + iLongitude * 4 + 3] = drawCube(point1, color);
                    }
                    geometryFill[iLatitude * iLongitudeCount + iLongitude] = geometry;
                }
            }
        }

        // определение минимальной средней высоты по краям
        private int minTopLenght(int iLatitude)
        {
            int iSection = 0;
            if (iLatitude != 0)
                iSection = 6;

            int iUnit = Arrays[iSection].L1;
            iUnit += Arrays[iSection].L2;
            iUnit += Arrays[iSection].L3;
            iUnit += Arrays[iSection].R1;
            iUnit += Arrays[iSection].R2;
            iUnit += Arrays[iSection].R3;
            iUnit /= 6;

            return iUnit;
        }

        private Point3D calcTopPoint(int iLatitude)
        {
            double iUnit = minTopLenght(iLatitude);

            int iSymbol = 1;
            if (iLatitude != 0)
                iSymbol = -1;
            double iRadius = ((40.0 + iUnit) / 100.0) * iSymbol;

            return new Point3D(0.0, iRadius, 0.0);
        }

        // рисование треугольника
        private GeometryModel3D drawTriangle(Point3D p1, Point3D p2, Point3D p3, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);

            brush = new SolidColorBrush();
            brush.Color = color;
            brush.Opacity = Opacity;

            diffuseMaterial = new DiffuseMaterial(brush);
            //diffuseMaterial.Brush = brush;
            return new GeometryModel3D(mesh, diffuseMaterial);
        }

        // рисование трапеции
        private GeometryModel3D drawTrapezoid(Point3D p1, Point3D p2, Point3D p3, Point3D p4, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p4);
            mesh.Positions.Add(p3);

            brush = new SolidColorBrush();
            brush.Color = color;
            brush.Opacity = Opacity;

            diffuseMaterial = new DiffuseMaterial(brush);
            //diffuseMaterial.Brush = brush;
            return new GeometryModel3D(mesh, diffuseMaterial);
        }

        // рисование линии
        private GeometryModel3D drawLine(Point3D p1, Point3D p2, Color color, double thickness = 0.003)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            double offset = thickness / 2;

            Point3D P11 = new Point3D(p1.X, p1.Y + offset, p1.Z - offset);
            Point3D P12 = new Point3D(p1.X + offset, p1.Y - offset, p1.Z);
            Point3D P13 = new Point3D(p1.X, p1.Y - offset, p1.Z + offset);
            Point3D P14 = new Point3D(p1.X - offset, p1.Y + offset, p1.Z);

            Point3D P21 = new Point3D(p2.X, p2.Y + offset, p2.Z - offset);
            Point3D P22 = new Point3D(p2.X + offset, p2.Y - offset, p2.Z);
            Point3D P23 = new Point3D(p2.X, p2.Y - offset, p2.Z + offset);
            Point3D P24 = new Point3D(p2.X - offset, p2.Y + offset, p2.Z);

            mesh.Positions.Add(P11);
            mesh.Positions.Add(P21);
            mesh.Positions.Add(P12);
            mesh.Positions.Add(P21);
            mesh.Positions.Add(P22);
            mesh.Positions.Add(P12);

            mesh.Positions.Add(P12);
            mesh.Positions.Add(P22);
            mesh.Positions.Add(P13);
            mesh.Positions.Add(P22);
            mesh.Positions.Add(P23);
            mesh.Positions.Add(P13);

            mesh.Positions.Add(P13);
            mesh.Positions.Add(P23);
            mesh.Positions.Add(P14);
            mesh.Positions.Add(P23);
            mesh.Positions.Add(P24);
            mesh.Positions.Add(P14);

            mesh.Positions.Add(P14);
            mesh.Positions.Add(P24);
            mesh.Positions.Add(P11);
            mesh.Positions.Add(P24);
            mesh.Positions.Add(P21);
            mesh.Positions.Add(P11);

            //Point3D p1X = new Point3D(p1.X, p1.Y + thickness, p1.Z + thickness);
            //Point3D p1Y = new Point3D(p1.X + thickness, p1.Y, p1.Z + thickness);
            //Point3D p1Z = new Point3D(p1.X + thickness, p1.Y + thickness, p1.Z);

            //Point3D p2X = new Point3D(p2.X, p2.Y + thickness, p2.Z + thickness);
            //Point3D p2Y = new Point3D(p2.X + thickness, p2.Y, p2.Z + thickness);
            //Point3D p2Z = new Point3D(p2.X + thickness, p2.Y + thickness, p2.Z);

            //mesh.Positions.Add(p1X);
            //mesh.Positions.Add(p2X);
            //mesh.Positions.Add(p1Y);
            //mesh.Positions.Add(p2X);
            //mesh.Positions.Add(p2Y);
            //mesh.Positions.Add(p1Y);

            //mesh.Positions.Add(p1Y);
            //mesh.Positions.Add(p2Y);
            //mesh.Positions.Add(p1Z);
            //mesh.Positions.Add(p2Y);
            //mesh.Positions.Add(p2Z);
            //mesh.Positions.Add(p1Z);

            //mesh.Positions.Add(p1Z);
            //mesh.Positions.Add(p2Z);
            //mesh.Positions.Add(p1X);
            //mesh.Positions.Add(p2Z);
            //mesh.Positions.Add(p2X);
            //mesh.Positions.Add(p1X);

            brush = new SolidColorBrush();
            brush.Color = color;
            brush.Opacity = Opacity;

            diffuseMaterial = new DiffuseMaterial(brush);
            diffuseMaterial.Brush = brush;
            return new GeometryModel3D(mesh, diffuseMaterial);
        }

        private GeometryModel3D drawCube(Point3D p, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            double offset = 0.01;

            Point3D p1 = new Point3D(p.X, p.Y + offset, p.Z + offset);
            Point3D p2 = new Point3D(p.X + offset, p.Y + offset, p.Z + offset);
            Point3D p3 = new Point3D(p.X, p.Y, p.Z + offset);
            Point3D p4 = new Point3D(p.X + offset, p.Y, p.Z + offset);

            Point3D p5 = new Point3D(p.X, p.Y + offset, p.Z);
            Point3D p6 = new Point3D(p.X + offset, p.Y + offset, p.Z);
            Point3D p7 = new Point3D(p.X, p.Y, p.Z);
            Point3D p8 = new Point3D(p.X + offset, p.Y, p.Z);

            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p4);
            mesh.Positions.Add(p3);

            mesh.Positions.Add(p2);
            mesh.Positions.Add(p6);
            mesh.Positions.Add(p4);
            mesh.Positions.Add(p6);
            mesh.Positions.Add(p7);
            mesh.Positions.Add(p4);

            mesh.Positions.Add(p6);
            mesh.Positions.Add(p5);
            mesh.Positions.Add(p7);
            mesh.Positions.Add(p5);
            mesh.Positions.Add(p8);
            mesh.Positions.Add(p7);

            mesh.Positions.Add(p5);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p8);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p3);
            mesh.Positions.Add(p8);

            mesh.Positions.Add(p5);
            mesh.Positions.Add(p6);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p6);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p1);

            mesh.Positions.Add(p3);
            mesh.Positions.Add(p4);
            mesh.Positions.Add(p8);
            mesh.Positions.Add(p4);
            mesh.Positions.Add(p7);
            mesh.Positions.Add(p8);

            brush = new SolidColorBrush();
            brush.Color = color;
            brush.Opacity = Opacity;

            diffuseMaterial = new DiffuseMaterial(brush);
            //diffuseMaterial.Brush = brush;
            return new GeometryModel3D(mesh, diffuseMaterial);
        }

        private Model3DGroup CreateEllipseNew(double value, Color color, double thickness = 0.01)
        {
            Model3DGroup ellipsModel = new Model3DGroup();

            // расчитываем вершины
            double oneAngle = Math.PI / 180;
            double deltaPhi = 0;

            Point3D [,] arrayPointsEllepse = new Point3D[iLatitudeCount + 1, 2];
            for (int iLatitude = 0; iLatitude < iLatitudeCount + 1; iLatitude++)    //48
            {
                if (iLatitude == 0)
                {
                    arrayPointsEllepse[iLatitude, 0] = new Point3D(0.0, value, 0.0);
                    arrayPointsEllepse[iLatitude, 1] = new Point3D(0.0, value, 0.0);
                }
                else
                {
                    double phi = Math.PI / 2; //3

                    double currentPhi = oneAngle * angleArrayLatitude[iLatitude - 1];
                    deltaPhi += currentPhi;

                    double t = Math.Atan2(Math.Sin(deltaPhi), 0.5 * Math.Cos(deltaPhi)); //0.4

                    double y = Math.Cos(t);

                    double iUnit = value * 100 - 40;
                    double dCoeff = 0.75;
                    if (iUnit < 40)
                    {
                        dCoeff = ((0.75 - 0.4) / 40) * iUnit + 0.4;
                    }
                    else if (iUnit > 80)
                    {
                        dCoeff = 0.05 / 20 * (iUnit - 80) + 0.75;
                    }
                    double radiusCircle = dCoeff * Math.Sin(t);

                    for (int iLongitude = 0; iLongitude < 2; iLongitude++)    // 2
                    {
                        arrayPointsEllepse[iLatitude, iLongitude] = new Point3D(value * radiusCircle * Math.Cos(phi),
                                                                                value * y,
                                                                                value * radiusCircle * Math.Sin(phi));

                        phi += Math.PI;
                    }
                }
            }


            for (int iLatitude = 0; iLatitude < iLatitudeCount; iLatitude++) // 48 * 2
            {
                ellipsModel.Children.Add(drawLine(arrayPointsEllepse[iLatitude, 0], arrayPointsEllepse[iLatitude + 1, 0], color, thickness));
                ellipsModel.Children.Add(drawLine(arrayPointsEllepse[iLatitude, 1], arrayPointsEllepse[iLatitude + 1, 1], color, thickness));
            }

            return ellipsModel;
        }

    }
}
