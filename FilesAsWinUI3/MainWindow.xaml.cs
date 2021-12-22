using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading.Tasks;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FilesAsWinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        int foldersCreated;
        string DeskPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public MainWindow()
        {
            this.InitializeComponent();
            st.IsEnabled = false;
            mt.IsEnabled = false;
            this.Title = "Make me some files";
            od.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //synchronous
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foldersCreated += 1;
            makeSomeFiles(foldersCreated, false);
            updateNumberFile(foldersCreated);
            stopWatch.Stop();
            textBox1.Text = $"Elapsed time: {stopWatch.ElapsedMilliseconds.ToString()}ms. Now check your desktop (or the folder at path {DeskPath}\\ManyManyFiles)";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //asynchronous
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foldersCreated += 1;
            makeSomeFiles(foldersCreated, true);
            updateNumberFile(foldersCreated);
            stopWatch.Stop();
            textBox1.Text = $"Elapsed time: {stopWatch.ElapsedMilliseconds.ToString()}ms. Now check your desktop (or the folder at path {DeskPath}/ManyManyFiles)";
        }

        void fetchFoldersCreated()
        {
            string folder = "ManyManyFiles";
            var file = Path.Combine(DeskPath, folder);
            try
            {
                if (Directory.Exists(file))
                {
                    Console.WriteLine("That path exists already.");
                    return;
                }

                DirectoryInfo di = Directory.CreateDirectory(file);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(file));

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally
            {

                string FileName = "DO NOT OPEN THIS FILE.txt";
                if (File.Exists(Path.Combine(DeskPath, folder, FileName)))
                {
                    StreamReader sr = new StreamReader(Path.Combine(DeskPath, folder, FileName));

                    string str = sr.ReadLine();

                    sr.Close();
                    if (Int32.TryParse(str, out int j))
                    {
                        Console.WriteLine(j);
                        foldersCreated = j;
                    }
                    else
                    {
                        Console.WriteLine("String could not be parsed.");
                        foldersCreated = -1;
                    }
                }
                else
                {
                    File.Create(Path.Combine(DeskPath, folder, FileName)).Close();
                    StreamWriter sw = new StreamWriter(Path.Combine(DeskPath, folder, FileName));
                    sw.Write("0");
                    foldersCreated = 0;
                    sw.Close();
                }
            }
        }

        void updateNumberFile(int num)
        {
            StreamWriter sw = new StreamWriter(Path.Combine(DeskPath, "ManyManyFiles", "DO NOT OPEN THIS FILE.txt"));
            sw.Write(num);
            foldersCreated = num;
            sw.Close();
        }

        void makeSomeFiles(int folderNum, bool multithreaded)
        {
            string folder = Path.Combine(DeskPath, "ManyManyFiles", $"File Batch {folderNum}");
            Directory.CreateDirectory(folder);
            Random rnd = new Random();

            void makeFile()
            {
                string randLetters = "";
                for (int i = 0; i < 8; i++)
                {
                    randLetters += Convert.ToChar(rnd.Next(65, 91));
                }

                string filePath = Path.Combine(folder, $"{randLetters}.json");
                File.Create(filePath).Close();
                StreamWriter sw = new StreamWriter(filePath);
                sw.WriteLine("{\n" + $"\t\"randomCharacters\" : \"{randLetters}\"" + "\n}");
                sw.Close();

            }

            // ASYNC
            if (multithreaded)
            {
                Task[] arr = new Task[5000];
                for (int i = 0; i < 5000; i++)
                {
                    Task t = Task.Run(makeFile);     // ASYNCHRONOUS
                    arr[i] = t;
                }
                Task.WaitAll(arr);
            }
            else
            {
                for (int i = 1; i <= 5000; i++)
                {
                    makeFile();             // SYNCHRONOUS
                }
            }
        }

        async private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            cd.IsEnabled = false;
            st.IsEnabled = false;
            mt.IsEnabled = false;
            od.IsEnabled = false;

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                textBox1.Text = "Picked folder: " + folder.Name + " at " + folder.Path;
                DeskPath = folder.Path;

                cd.IsEnabled = true;
                st.IsEnabled = true;
                mt.IsEnabled = true;
                od.IsEnabled = true;
                fetchFoldersCreated();
            }
            else
            {
                //textBox1.Text = "Operation cancelled.";
                cd.IsEnabled = true;
            }

        }

        private void od_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Path.Combine(DeskPath,"ManyManyFiles"));
        }
    }
}
