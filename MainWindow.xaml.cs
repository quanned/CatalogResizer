﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CatalogResizer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int selectedAlgoritm = 0; //default choice - libwebp

        public MainWindow()
        {
            InitializeComponent();
            FilesFolderPathTB.Text = @"D:\photo_for_site\";
            CompressFilesFolderPathTB.Text = @"D:\photo_for_site_crop\";
            IgnoreOneMBCHB.IsEnabled = false;
        }

        private void ChoseFolderWithFikesBTN_Click(object sender, RoutedEventArgs e)
        {
            string path = ChoseFolderPath();
            FilesFolderPathTB.Text = path.ToString() + @"\";
        }

        private void ChoseFolderForCompressFilesBTN_Click(object sender, RoutedEventArgs e)
        {
            string path = ChoseFolderPath2();
            CompressFilesFolderPathTB.Text = path.ToString() + @"\";
        }
        private string ChoseFolderPath2()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            return dialog.FileName.ToString();
            //using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            //{
            //    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            //    return result.ToString();
            //}
        }
        private string ChoseFolderPath()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            return dialog.FileName.ToString();
            /*            // Create a "Save As" dialog for selecting a directory (HACK)
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
                        return "";*/
        }

        private void CompressBTN_Click(object sender, RoutedEventArgs e)
        {
            FilesLB.Items.Clear();
            CompressLB.Items.Clear();

            string pathToFolderWithCropFiles = CompressFilesFolderPathTB.Text;
            string pathToFolderWithFiles = FilesFolderPathTB.Text;

            if (pathToFolderWithFiles == "" || pathToFolderWithCropFiles == "")
            {
                System.Windows.MessageBox.Show("Неправильно выбрана папка", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }

            if (!Directory.Exists(pathToFolderWithCropFiles))
            {
                Directory.CreateDirectory(pathToFolderWithCropFiles);
            }

            if (!Directory.Exists(pathToFolderWithFiles))
            {
                Directory.CreateDirectory(pathToFolderWithFiles);
            }

            string[] filesArray = Directory.GetFiles(pathToFolderWithFiles, "*.jpg");
            foreach (string filename in filesArray)
            {
                FilesLB.Items.Add(filename);
                Log("add inpit file: " + filename);
            }

            int compressLevel = Convert.ToInt32(GetCompressLevel());
            if (compressLevel < 50)
            {
                System.Windows.MessageBox.Show("Некорректное значение степени сжатия", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            int timeout = Convert.ToInt32(TimeOutTB.Text);
            if (timeout <= 500)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Выбран низкий уровень задержки между запусками процессов. \nЭто может привести к появлению большого количества параллельных процессов и нагрузит систему. \nВы действительно хотите продолжить с заданными вами условиями?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No)
                {
                    timeout = 1500;
                }
            }

            var timer = Stopwatch.StartNew();
            if (selectedAlgoritm == 0) //libwebp
            {

                string command = "";
                for (int i = 0; i < filesArray.Length; i++)
                {
                    FileInfo file = new FileInfo(filesArray[i].ToString());
                    if (file.Exists)
                    {
                        long size = file.Length;
                        if (size <= 1000000 & IgnoreOneMBCHB.IsChecked == false)
                        {
                            continue;
                        }
                        else
                        {
                            if (ResizeCHB.IsChecked == true)
                            {
                                string outputFileName = pathToFolderWithCropFiles + filesArray[i].ToString().Remove(0, FilesFolderPathTB.Text.Length);
                                outputFileName = outputFileName.Replace(".jpg", ".webp");
                                command = "/C cwebp -q " + compressLevel + " -resize 1280 1920 " + filesArray[i].ToString() + " -o " + outputFileName;
                            }
                            else
                            {
                                string outputFileName = pathToFolderWithCropFiles + filesArray[i].ToString().Remove(0, FilesFolderPathTB.Text.Length);
                                outputFileName = outputFileName.Replace(".jpg", ".webp");
                                command = "/C cwebp -q " + compressLevel + " " + filesArray[i].ToString() + " -o " + outputFileName;
                            }

                            System.Diagnostics.Process.Start("CMD.exe", command);
                            System.Threading.Thread.Sleep(timeout);
                            //Console.WriteLine(command.ToString());
                            Log("run this command: " + command);
                            command = "";
                            //example: cwebp - q 80 - resize 1920 1080 m.jpg - o m2.jpg
                        }
                    }

                }
            }
            else
            {
                string command = "";
                for (int i = 0; i < filesArray.Length; i++)
                {
                    FileInfo file = new FileInfo(filesArray[i].ToString());
                    if (file.Exists)
                    {
                        long size = file.Length;
                        if (size <= 1000000 & IgnoreOneMBCHB.IsChecked == false)
                        {
                            continue;
                        }
                        else
                        {
                            if (ResizeCHB.IsChecked == true)
                            {
                                command = "/C pingo.exe -jpgquality=" + compressLevel + " -resize=1920 " + "\"" + filesArray[i].ToString() + "\"";
                            }
                            else
                            {
                                command = "/C pingo.exe -jpgquality=" + compressLevel + " \"" + filesArray[i].ToString() + "\"";
                            }

                            System.Diagnostics.Process.Start("CMD.exe", command);
                            System.Threading.Thread.Sleep(timeout);
                            //Console.WriteLine(command.ToString());
                            Log("run this command: " + command);
                            command = "";
                            //example:/C pingo.exe -jpgquality=100 [-resize=1920] m.jpg
                        }

                    }

                }
            }


            timer.Stop();
            int seconds = System.Int32.Parse(((timer.ElapsedMilliseconds / 1000) % 60).ToString());
            int minutes = System.Int32.Parse(((timer.ElapsedMilliseconds / 1000) / 60).ToString());

            string[] cropFilesArray;

            if (selectedAlgoritm == 0)
            {
                cropFilesArray = Directory.GetFiles(pathToFolderWithCropFiles, "*.webp");
                foreach (string filename in cropFilesArray)
                {
                    CompressLB.Items.Add(filename);
                    Log("add output file: " + filename);
                }
            }
            else
            {
                cropFilesArray = Directory.GetFiles(pathToFolderWithFiles, "*.jpg");
                foreach (string filename in cropFilesArray)
                {
                    CompressLB.Items.Add(filename);
                    Log("add output file: " + filename);
                }
            }

            System.Threading.Thread.Sleep(5000);
            System.Windows.MessageBox.Show("Количество измененных файлов: " + cropFilesArray.Length.ToString() + "\nВремя выполнения обработки:" + minutes + ":" + seconds, "Обработка завершена", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void ResizeCHB_Checked(object sender, RoutedEventArgs e)
        {
            /*if (ResizeCHB.IsChecked == false)
            {
                IgnoreOneMBCHB.IsEnabled = false;
            }
            else*/ IgnoreOneMBCHB.IsEnabled = true;
        }

        private void ResizeCHB_Unchecked(object sender, RoutedEventArgs e)
        {
            IgnoreOneMBCHB.IsEnabled = false;
            IgnoreOneMBCHB.IsChecked = false;
        }

        /*private void LibChoiceCB_SelectionChanged(object sender, SelectionChangedEventArgs e, System.Windows.Controls.TextBox compressFilesFolderPathTB, System.Windows.Controls.Button choseFolderForCompressFilesBTN)
        {
            selectedAlgoritm = LibChoiceCB.SelectedIndex;
            if (LibChoiceCB.SelectedIndex != 1)
            {
                compressFilesFolderPathTB.IsEnabled = true;
                choseFolderForCompressFilesBTN.IsEnabled = true;
            }
            else
            {
                compressFilesFolderPathTB.IsEnabled = false;
                choseFolderForCompressFilesBTN.IsEnabled = false;
            }
        }*/

        private void LibChoiceCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedAlgoritm = LibChoiceCB.SelectedIndex;
            if (selectedAlgoritm != 1)
            {
                CompressFilesFolderPathTB.IsEnabled = true;
                ChoseFolderForCompressFilesBTN.IsEnabled = true;
            }
            else
            {
                CompressFilesFolderPathTB.IsEnabled = false;
                ChoseFolderForCompressFilesBTN.IsEnabled = false;
            }
        }


        //NotifyIcon NI = new NotifyIcon();

        /*private void Button_Click(object sender, RoutedEventArgs e)
        {
            NI.BalloonTipText = "Текст";
            NI.BalloonTipTitle = "Заголовок";
            NI.BalloonTipIcon = ToolTipIcon.Info;
            NI.BalloonTipIcon = ToolTipIcon.Info;
            NI.Visible = true;
            NI.ShowBalloonTip(10000);
        }

        private void NI_BalloonTipClosed(Object sender, EventArgs e)
        {
            NI.Visible = false;
        }*/
    }
}
