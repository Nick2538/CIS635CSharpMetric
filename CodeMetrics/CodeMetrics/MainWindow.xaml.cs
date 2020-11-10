using System;
using System.Collections.Generic;
using System.Windows;
using Analysis;
using System.IO;

namespace CodeMetrics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> RecurseDirectory(string filepath, string extension)
        {
            List<string> files = new List<string>();
            foreach (var file in Directory.GetFiles(filepath))
            {
                if (file.EndsWith(extension))
                {
                    files.Add(file);
                }
            }
            foreach (var directory in Directory.GetDirectories(filepath))
            {
                foreach (var file in RecurseDirectory(directory, extension))
                {
                    if (file.EndsWith(extension))
                    {
                        files.Add(file);
                    }
                }
            }
            return files;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetricButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in RecurseDirectory(ProjectTextBox.Text, ".cs"))
            {
                CSharpCodeAnalyzer analyzer = new CSharpCodeAnalyzer(file);
                foreach (var cls in analyzer.IterSubClasses()) {
                    double avg = cls.AverageMethodSize();
                    Dictionary<string, int> usages = cls.MethodUsage();
                    Console.WriteLine();
                }
            }
        }
    }
}
