using Microsoft.Win32;
using StressStrainStateAnalyzer.DataManagers;
using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Nodes;
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

namespace AnalyzerUI.ModalWindows
{
    /// <summary>
    /// Логика взаимодействия для ExportModalWindow.xaml
    /// </summary>
    public partial class ExportModalWindow : Window
    {
        private readonly IDataManager _dataManager;
        public List<INode> Nodes { get; private set; }
        public List<INode> VerifyNodes { get; private set; }
        public List<IFiniteElement> Elements { get; private set; }

        public bool IsSuccessfull { get; private set; }
        public ExportModalWindow(IDataManager manager)
        {
            InitializeComponent();
            Nodes = new List<INode>();
            Elements = new List<IFiniteElement>();
            _dataManager = manager;
        }

        private void LoadNodesBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Файлы xlsx|*.xlsx|Файлы xls|*.xls";
            if (fileDialog.ShowDialog() == true)
            {
                Nodes = _dataManager.GetNodes(fileDialog.FileName);
                LoadElementsBtn.IsEnabled = true;
            }
        }

        private void LoadElementsBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Файлы xlsx|*.xlsx|Файлы xls|*.xls";
            if (fileDialog.ShowDialog() == true)
            {
                Elements = _dataManager.GetElements(fileDialog.FileName, Nodes);
                LoadStressBtn.IsEnabled = true;
            }
        }

        private void LoadMeshBtn_Click(object sender, RoutedEventArgs e)
        {
            IsSuccessfull = true;
            Close();
        }

        private void LoadStressBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Файлы xlsx|*.xlsx|Файлы xls|*.xls";
            if (fileDialog.ShowDialog() == true)
            {
                VerifyNodes = _dataManager.GetNodesWithStress(fileDialog.FileName);
                LoadElementsBtn.IsEnabled = true;
            }
            LoadMeshBtn.IsEnabled = true;
        }
    }
}
