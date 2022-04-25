using System;
using System.Windows;

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
