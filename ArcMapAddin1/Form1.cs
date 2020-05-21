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
        //public void LoadRaster()
        //{
        //    string filePath;
        //    filePath = textBox1.Text + "\\" + listBox2.SelectedItem.ToString();
        //    IRasterLayer pRasterLy = new RasterLayerClass();
        //    pRasterLy.CreateFromFilePath(filePath);
        //    axMapControl.Map.AddLayer(pRasterLy);
        //    MessageBox.Show("图层加载成功!");
        //}
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
        public List<string> GetFilePath()
        {
            List<string> pathList = new List<string>();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "打开*.shp文件";
            openFileDialog1.Filter = "shp文件(*.shp*)|*.shp*";
            openFileDialog1.InitialDirectory = @"E:\GIS底层实验\GIS_Develop\Data\Shapefile";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            string filePath = openFileDialog1.FileName;
            string fileFolder = System.IO.Path.GetDirectoryName(filePath);
            string fileName = System.IO.Path.GetFileName(filePath);
            pathList.Add(fileFolder);
            pathList.Add(fileName);
            return pathList;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Title = "打开*.shp文件";
            //openFileDialog1.Filter = "shp文件(*.shp*)|*.shp*";
            //openFileDialog1.InitialDirectory = @"E:\GIS底层实验\GIS_Develop\Data\Shapefile";
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    string filesName = System.IO.Path.GetFileName(openFileDialog1.FileName);//得到文件名不包括路径
            //    string pathName = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);//得到路径
            //    string shpPath = pathName + "\\" + filesName;
            //    IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            //    IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(shpPath, 0);
            //    IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            //    IFeatureClass pFC = pFeatureWorkspace.OpenFeatureClass(filesName);
            //    IFeatureLayer pFLayer = new FeatureLayerClass();
            //    pFLayer.FeatureClass = pFC;
            //    pFLayer.Name = pFC.AliasName;
            //    ILayer pLayer = pFLayer as ILayer;
            //    IMxDocument pMxDocument = ArcMap.Application.Document as IMxDocument;
            //    pMxDocument.AddLayer(pLayer);
            pMxd = ArcMap.Document as IMxDocument;
            pMap = pMxd.FocusMap;

            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Title = "打开*.shp文件";
            //openFileDialog1.Filter = "shp文件(*.shp*)|*.shp*";
            //openFileDialog1.InitialDirectory = @"E:\GIS底层实验\GIS_Develop\Data\Shapefile";
            //if (openFileDialog1.ShowDialog() != DialogResult.OK)
            //{
            //    return;
            //}
            //string pPath = openFileDialog1.FileName;
            //string pFolder = System.IO.Path.GetDirectoryName(pPath);
            //string pFileName = System.IO.Path.GetFileName(pPath);
            //IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            //IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(pFolder, 0);
            //IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            //pFeatureWorkspace.OpenFeatureClass(pFileName);
            //IFeatureLayer pFLayer = new FeatureLayerClass();
            //IFeatureClass pFC = pFeatureWorkspace.OpenFeatureClass(pFileName);
            //pFLayer.FeatureClass = pFC;
            //pFLayer.Name = pFC.AliasName;
            //ILayer pLayer = pFLayer as ILayer;
            //pMap.AddLayer(pLayer);
            //textBox1.Text = pFileName;
            List<string> filePathList = GetFilePath();
            IFeatureLayer featureLayer = OpenShapeFile(filePathList);
            pMap.AddLayer(featureLayer);
            textBox1.Text = filePathList[1];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //ScriptEngine pyEngine = Python.CreateEngine();//创建Python解释器对象
            //dynamic py = pyEngine.ExecuteFile(@"P2R.py");//读取脚本文件
            //string Raster = py.ToDo(textBox1.Text,comboBox1.SelectedValue.ToString(),textBox2.Text,);//调用脚本文件中对应的函数
            string filePath = @"E:/GIS底层实验/WorkSpace/output";
            List<string> filePathList = GetFilePath();
            IFeatureLayer featureLayer = OpenShapeFile(filePathList);
            ILayer rasterLayer = xjShpPointToRaster(featureLayer, filePath, Convert.ToDouble(textBox2.Text), comboBox1.SelectedText);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ReadOnly = true;
        }
    }
}
