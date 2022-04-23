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
    /// Логика взаимодействия для MeshModalWindow.xaml
    /// </summary>
    public partial class MeshModalWindow : Window
    {
        public MeshModalWindow()
        {
            InitializeComponent();
        }

        public double MaxSquare { get; set; }
        public double MinAngle { get; set; }
        public bool IsCancel { get; set; }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MaxSquare = double.Parse(MaxSquareTextBox.Text);
                MinAngle = double.Parse(MinAngleTextBox.Text);
                IsCancel = false;
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Данные введены неверно", "Ошибка");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsCancel = true;
            Close();
        }
    }
}
