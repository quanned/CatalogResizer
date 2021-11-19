﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace CatalogResizer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FilesFolderPathTB.Text = @"D:\photo_for_site\";
            CompressFilesFolderPathTB.Text = @"D:\photo_for_site_crop\";
        }

        private void ChoseFolderWithFikesBTN_Click(object sender, RoutedEventArgs e)
        {
            string path = ChoseFolderPath();
            FilesFolderPathTB.Text = path.ToString() + @"\";
        }

        private void ChoseFolderForCompressFilesBTN_Click(object sender, RoutedEventArgs e)
        {
            string path = ChoseFolderPath();
            CompressFilesFolderPathTB.Text = path.ToString() + @"\";
        }

        private string ChoseFolderPath()
        {
            // Create a "Save As" dialog for selecting a directory (HACK)
            var dialog = new Microsoft.Win32.SaveFileDialog();
            var textbox = new TextBox();
            dialog.InitialDirectory = textbox.Text; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                // Our final value is in path
                textbox.Text = path;
            return path;
            }
            return "";
        }

        private void CompressBTN_Click(object sender, RoutedEventArgs e)
        {
            string pathToFolderWithCropFiles = CompressFilesFolderPathTB.Text; 
            string pathToFolderWithFiles = FilesFolderPathTB.Text;

            if (pathToFolderWithFiles == "")
            {
                MessageBox.Show("Неправильно выбрана папка", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string[] filesArray = Directory.GetFiles(pathToFolderWithFiles, "*.jpg");
            foreach (string filename in filesArray)
            {
                FilesLB.Items.Add(filename);
                Log("add inpit file: " + filename);
            }

            //if (!Directory.Exists(pathToFolderWithCropFiles))
            //{
            //    Directory.CreateDirectory(pathToFolderWithCropFiles);
            //}

            int compressLevel = Convert.ToInt32(GetCompressLevel());
            if (compressLevel < 50)
            {
                MessageBox.Show("Некорректное значение степени сжатия", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            int timeout = Convert.ToInt32(TimeOutTB.Text);
            if(timeout <= 500)
            {
                MessageBoxResult result = MessageBox.Show("Выбран низкий уровень задержки между запусками процессов. \nЭто может привести к появлению большого количества параллельных процессов и нагрузит систему. \nВы действительно хотите продолжить с заданными вами условиями?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if(result == MessageBoxResult.No)
                {
                    timeout = 1500;
                }
            }

            var timer = Stopwatch.StartNew();

            string command = "";
            for (int i = 0; i < filesArray.Length; i++)
            {
                if (ResizeCHB.IsChecked == true)
                {
                    command = "/C cwebp -q " + compressLevel + " -resize 1280 1920 " + filesArray[i].ToString() + " -o " + pathToFolderWithCropFiles + filesArray[i].ToString().Remove(0, FilesFolderPathTB.Text.Length);
                }
                else
                {
                    command = "/C cwebp -q " + compressLevel + " " + filesArray[i].ToString() + " -o " + pathToFolderWithCropFiles + filesArray[i].ToString().Remove(0, FilesFolderPathTB.Text.Length);
                }
                
                System.Diagnostics.Process.Start("CMD.exe", command);
                System.Threading.Thread.Sleep(timeout);
                //Console.WriteLine(command.ToString());
                Log("run this command: " + command);
                command = "";
                //example: cwebp - q 80 - resize 1920 1080 m.jpg - o m2.jpg
            }

            timer.Stop();
            int seconds = System.Int32.Parse(((timer.ElapsedMilliseconds / 1000) % 60).ToString());
            int minutes = System.Int32.Parse(((timer.ElapsedMilliseconds / 1000) / 60).ToString());

            string[] cropFilesArray = Directory.GetFiles(pathToFolderWithCropFiles, "*.jpg");
            foreach (string filename in cropFilesArray)
            {
                CompressLB.Items.Add(filename);
                Log("add output file: " + filename);
            }

            System.Threading.Thread.Sleep(5000);
            MessageBox.Show("Количество отформатированных файлов: " + cropFilesArray.Length.ToString() + "\nВремя выполнения обработки:" + minutes + seconds, "Обработка завершена", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private int GetCompressLevel()
        {
            if (RB100.IsChecked == true)
            {
                return 100;
            }
            else if (RB90.IsChecked == true)
            {
                return 90;
            }
            else if (RB80.IsChecked == true)
            {
                return 80;
            }
            else if (RB70.IsChecked == true)
            {
                return 70;
            }
            else if (RB60.IsChecked == true)
            {
                return 60;
            }
            else if (RB50.IsChecked == true)
            {
                return 50;
            }
            else return 100;
        }

        private void TimeOutTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
        }

        private void FilesLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(FilesLB.SelectedItem.ToString());
        }

        private void CompressLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(CompressLB.SelectedItem.ToString());
        }
    }
}