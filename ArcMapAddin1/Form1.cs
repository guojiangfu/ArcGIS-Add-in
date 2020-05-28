using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geodatabase;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcMapAddin1
{
    public partial class Form1 : Form
    {
        IMxDocument pMxd = null;
        IMap pMap = null;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 矢量点转栅格
        /// </summary>
        /// <param name="xjFeatureLayer">矢量图层</param>
        /// <param name="RasterPath">栅格绝对路径</param>
        /// <param name="CellSize">栅格边长</param>
        /// <param name="SecletctedField">所选字段（高程等）</param>
        /// <returns>返回栅格图层</returns>
        private ILayer xjShpPointToRaster(IFeatureLayer xjFeatureLayer, string RasterPath, double CellSize, string SecletctedField)
        {
            IFeatureClass xjFeatureClass = xjFeatureLayer.FeatureClass;
            IFeatureClassDescriptor xjFeatureClassDescriptor = new FeatureClassDescriptorClass();//using ESRI.ArcGIS.GeoAnalyst;
            xjFeatureClassDescriptor.Create(xjFeatureClass, null, SecletctedField);
            IGeoDataset xjGeoDataset = xjFeatureClassDescriptor as IGeoDataset;

            IWorkspaceFactory xjwsf = new RasterWorkspaceFactoryClass(); //using ESRI.ArcGIS.DataSourcesRaster;
            string xjRasterFolder = System.IO.Path.GetDirectoryName(RasterPath);
            IWorkspace xjws = xjwsf.OpenFromFile(xjRasterFolder, 0);
            IConversionOp xjConversionOp = new RasterConversionOpClass();
            IRasterAnalysisEnvironment xjRasteren = xjConversionOp as IRasterAnalysisEnvironment;

            object xjCellSize = CellSize as object;
            xjRasteren.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref xjCellSize);

            string xjFileName = System.IO.Path.GetFileName(RasterPath);
            IRasterDataset xjdaset2 = xjConversionOp.ToRasterDataset(xjGeoDataset, "TIFF", xjws, xjFileName);

            IRasterLayer xjRasterLayer = new RasterLayerClass();
            xjRasterLayer.CreateFromDataset(xjdaset2);
            ILayer xjLayer = xjRasterLayer;
            xjRasterLayer.Name = xjFileName;

            return xjLayer;
        }
        //加载栅格到地图控件中
        public void LoadRaster(List<string> filePath)
        {
            pMxd = ArcMap.Document as IMxDocument;
            pMap = pMxd.FocusMap;
            IRasterLayer pRasterLy = new RasterLayerClass();
            pRasterLy.CreateFromFilePath(filePath[2]);
            pMap.AddLayer(pRasterLy);
            MessageBox.Show("图层加载成功!");
        }
        //打开shp文件
        public IFeatureLayer OpenShapeFile(List<string> pathList)
        {
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(pathList[0], 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            pFeatureWorkspace.OpenFeatureClass(pathList[1]);
            IFeatureLayer pFLayer = new FeatureLayerClass();
            IFeatureClass pFC = pFeatureWorkspace.OpenFeatureClass(pathList[1]);
            pFLayer.FeatureClass = pFC;
            pFLayer.Name = pFC.AliasName;
            return pFLayer;
        }
        // 获取文件的文件路径和文件名
        public List<string> GetFilePath()
        {
            List<string> pathList = new List<string>();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "打开*.shp文件";
            //openFileDialog1.Filter = "shp文件(*.shp*)|*.shp*";
            openFileDialog1.InitialDirectory = @"E:\GIS底层实验\WorkSpace";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            string filePath = openFileDialog1.FileName;//绝对路径
            string fileFolder = Path.GetDirectoryName(filePath);//shp文件所在的文件夹
            string fileName = System.IO.Path.GetFileName(filePath);//shp文件名
            pathList.Add(fileFolder);
            pathList.Add(fileName);
            pathList.Add(filePath);
            return pathList;
        }
        public List<string> SaveAsPath()
        {
            List<string> pathList = new List<string>();
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "选择文件存储位置";
            saveFileDialog1.InitialDirectory = @"E:\GIS底层实验\WorkSpace";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            string filePath = saveFileDialog1.FileName;//绝对路径
            string fileFolder = Path.GetDirectoryName(filePath);//文件所在的文件夹
            string fileName = Path.GetFileName(filePath);//文件名
            pathList.Add(fileFolder);
            pathList.Add(fileName);
            pathList.Add(filePath);
            return pathList;
        }
        //获取featureClass
        public static IFeatureClass GetFeatureClass(string filePath)
        {
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspaceFactoryLockControl pWorkspaceFactoryLockControl = pWorkspaceFactory as IWorkspaceFactoryLockControl;
            if (pWorkspaceFactoryLockControl.SchemaLockingEnabled)
            {
                pWorkspaceFactoryLockControl.DisableSchemaLocking();
            }
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(filePath), 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(filePath));
            return pFeatureClass;
        }

        //获取字段名添加到comboBox中
        public void GetFieldsName(IFeatureClass pFeatureClass)
        {
            IFields pFields = pFeatureClass.Fields;
            int fieldsCount = pFields.FieldCount;
            for (int i = 0; i < fieldsCount; i++)
            {
                comboBox1.Items.Add(pFields.get_Field(i).Name);
            }
        }

        //获取宽高中的最小值
        public double GetMinWH(string shpfilepath)
        {
            double Xmin;
            double Ymin;
            double Xmax;
            double Ymax;
            List<double> WH = new List<double>();
            //打开.shp文件,读取x,y坐标的信息
            FileStream fs = new FileStream(shpfilepath, FileMode.Open, FileAccess.Read);   //文件流形式
            BinaryReader BinaryFile = new BinaryReader(fs);     //打开二进制文件  

            BinaryFile.ReadBytes(32);  //先读出36个字节
            int shapetype = BinaryFile.ReadInt32();

            Xmin = BinaryFile.ReadDouble();
            Ymin = BinaryFile.ReadDouble();
            Xmax = BinaryFile.ReadDouble();
            Ymax = BinaryFile.ReadDouble();

            WH.Add(Xmax - Xmin);
            WH.Add(Ymax - Ymin);
            return WH.Min();
        }

        //分析的主要实现函数
        private IRasterLayer Analyze(IRasterLayer pRasterLayer)
        {
            IRaster pRaster = pRasterLayer.Raster;
            IRasterProps rasterProps = (IRasterProps)pRaster;

            //设置栅格数据起始点  
            IPnt pBlockSize = new Pnt();
            pBlockSize.SetCoords(rasterProps.Width, rasterProps.Height);

            //选取整个范围  
            IPixelBlock pPixelBlock = pRaster.CreatePixelBlock(pBlockSize);

            //左上点坐标  
            IPnt tlp = new Pnt();
            tlp.SetCoords(0, 0);

            //读入栅格  
            IRasterBandCollection pRasterBands = pRaster as IRasterBandCollection;
            IRasterBand pRasterBand = pRasterBands.Item(0);
            IRawPixels pRawRixels = pRasterBands.Item(0) as IRawPixels;
            pRawRixels.Read(tlp, pPixelBlock);

            //将PixBlock的值组成数组  
            Array pSafeArray = pPixelBlock.get_SafeArray(0) as Array;

            //Array转数组
            double[,] myDoubleArr = new double[pSafeArray.GetLength(0), pSafeArray.GetLength(1)];
            for (int i = 0; i < myDoubleArr.GetLength(0); i++)
            {
                for (int j = 0; j < myDoubleArr.GetLength(1); j++)
                {
                    myDoubleArr[i, j] = Convert.ToDouble( pSafeArray.GetValue(i, j));
                }
            }

            for (int i = 0; i < myDoubleArr.GetLength(0); i++)
            {
                for (int j = 0; j < myDoubleArr.GetLength(1); j++)
                {
                    if (myDoubleArr[i,j] == 255)
                    {
                        myDoubleArr[i, j] = 0;
                    }
                }
            }
            double[,] ZeroArray = GetArray(myDoubleArr, Convert.ToInt32(textBox4.Text));
            double[,] OArray = SumArray(ZeroArray, Convert.ToInt32(textBox4.Text));
            double[,] LastArray =  ReturnLastArray(OArray, Convert.ToInt32(textBox4.Text));
            pPixelBlock.set_SafeArray(0, LastArray);

            //StreamWriter sw = File.AppendText(@"E:\GIS底层实验\WorkSpace\result\arrry.txt");
            //for (int y = 0; y < rasterProps.Height; y++)
            //{
            //    for (int x = 0; x < rasterProps.Width; x++)
            //    {
            //        //int value = Convert.ToInt32(pSafeArray.GetValue(x, y));  
            //        Byte value = Convert.ToByte(pSafeArray.GetValue(x, y));
            //        string TxtCon = ("X:" + Convert.ToString(x) + "," + "Y:" + Convert.ToString(y) + "," + "Value" + pSafeArray.GetValue(x, y) + "\n");
            //        sw.Write(TxtCon);
            //    }
            //}
            //sw.Flush();
            //sw.Close();

            // 编辑raster，将更新的值写入raster中
            IRasterEdit rasterEdit = pRaster as IRasterEdit;
            rasterEdit.Write(tlp, pPixelBlock);
            rasterEdit.Refresh();
            return pRasterLayer;
        }
        //二维数组补网(补零)
        public double[,] GetArray(double[,] array, int r)
        {
            int x = array.GetLength(0);
            int y = array.GetLength(1);
            double[,] myArray = new double[x + 2 * r, y + 2 * r];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    myArray[i + r, j + r] = Convert.ToDouble(array.GetValue(i, j));
                }
            }
            return myArray;
        }

        //累加生成新数组
        public double[,] SumArray(double[,] array, int a)
        {
            double[,] MyArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = a; i < array.GetLength(0) - a; i++)
            {
                for (int j = a; j < array.GetLength(1) - a; j++)
                {
                    double[] b = new double[array.GetLength(0)];
                    for (int s = 0; s < 2 * a + 1; s++)
                    {
                        for (int s1 = 0; s1 < 2 * a + 1; s1++)
                        {
                            b[s] = b[s] + array[i - a + s, j - a + s1];
                        }

                    }
                    for (int s2 = 0; s2 < 2 * a + 1; s2++)
                    {
                        MyArray[i, j] += b[s2];
                    }
                }
            }
            return MyArray;
        }
        //还原会原来数组的大小
        public double[,] ReturnLastArray(double[,] OArray, int r)
        {
            int x = OArray.GetLength(0);
            int y = OArray.GetLength(1);
            double[,] LArray = new double[x - 2 * r, y - 2 * r];
            for (int i = r; i < x - r; i++)
            {
                for (int j = r; j < y - r; j++)
                {
                    LArray[i - r, j - r] = OArray[i, j];
                }
            }
            return LArray;
        }
        //保存分析结果
        public static void SaveRasterLayerTofile(IRasterLayer pRasterLayer, string fileName, string strFileExtension = "TIFF")
        {

            IRaster pRaster = pRasterLayer.Raster;
            IRaster2 pRaster2 = pRaster as IRaster2;

            ISaveAs pSaveAs = pRaster2 as ISaveAs;
            pSaveAs.SaveAs(fileName, null, strFileExtension);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pMxd = ArcMap.Document as IMxDocument;
            pMap = pMxd.FocusMap;
            List<string> filePathList = GetFilePath();
            IFeatureLayer featureLayer = OpenShapeFile(filePathList);
            pMap.AddLayer(featureLayer);
            textBox1.Text = filePathList[2];
            GetFieldsName(featureLayer.FeatureClass);
            comboBox1.SelectedIndex = 0;
            // textBox2.Text = Convert.ToString( Math.Round( GetMinWH(textBox1.Text) / 250,3 ) );
            textBox2.Text = Convert.ToString(GetMinWH(textBox1.Text) / 250);
            textBox3.Text = Convert.ToString(GetMinWH(textBox1.Text) / 250);
            textBox4.Text = Convert.ToString(GetMinWH(textBox1.Text) / 30);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string rasterSavePath = @"E:\GIS底层实验\WorkSpace\MyAddIn\result";
            string filePath = textBox1.Text;
            string fileFolder = Path.GetDirectoryName(filePath);//shp文件所在的文件夹
            string fileName = Path.GetFileName(filePath);//shp文件名
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(fileFolder, 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            pFeatureWorkspace.OpenFeatureClass(fileName);
            IFeatureLayer pFLayer = new FeatureLayerClass();
            IFeatureClass pFC = pFeatureWorkspace.OpenFeatureClass(fileName);
            pFLayer.FeatureClass = pFC;
            pFLayer.Name = pFC.AliasName;
            IFeatureLayer featureLayer = pFLayer;
            ILayer rasterLayer = xjShpPointToRaster(featureLayer, rasterSavePath, Convert.ToDouble(textBox2.Text), comboBox1.SelectedText);
            MessageBox.Show("Successful!");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ReadOnly = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> savePath = SaveAsPath();
            textBox5.Text = savePath[0];
        }

        private void button4_Click(object sender, EventArgs e)
        {
            IRasterLayer pRasterLy = new RasterLayerClass();
            pRasterLy.CreateFromFilePath(@"E:\GIS底层实验\WorkSpace\MyAddIn\result.tif");
            Analyze(pRasterLy);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
