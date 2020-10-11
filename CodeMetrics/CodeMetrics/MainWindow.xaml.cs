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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.CodeAnalysis;
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
    }
}
