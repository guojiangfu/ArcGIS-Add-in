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
            string fileFolder = System.IO.Path.GetDirectoryName(filePath);//shp文件所在的文件夹
            string fileName = System.IO.Path.GetFileName(filePath);//shp文件名
            pathList.Add(fileFolder);
            pathList.Add(fileName);
            pathList.Add(filePath);
            return pathList;
        }
        public List<string> SaveAsPath()
        {
            List<string> pathList = new List<string>();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "选择文件存储位置";
            openFileDialog1.InitialDirectory = @"E:\GIS底层实验\WorkSpace";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            string filePath = openFileDialog1.FileName;//绝对路径
            string fileFolder = System.IO.Path.GetDirectoryName(filePath);//shp文件所在的文件夹
            string fileName = System.IO.Path.GetFileName(filePath);//shp文件名
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

        //获取字段名
        public /*List<string>*/void  GetFieldsName(IFeatureClass pFeatureClass)
        {
            IFields pFields = pFeatureClass.Fields;
            int fieldsCount = pFields.FieldCount;
            for (int i = 0; i < fieldsCount; i++)
            {
                comboBox1.Items.Add(pFields.get_Field(i).Name);
            }
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string rasterSavePath = @"E:\GIS底层实验\WorkSpace\MyAddIn\result";
            string filePath = textBox1.Text;
            string fileFolder = System.IO.Path.GetDirectoryName(filePath);//shp文件所在的文件夹
            string fileName = System.IO.Path.GetFileName(filePath);//shp文件名
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
            textBox4.Text = savePath[0];
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
