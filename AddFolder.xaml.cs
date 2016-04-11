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
using System.IO;
using System.IO.Compression;
namespace ProgettoPDS
{
    /// <summary>
    /// Logica di interazione per AddFolder.xaml
    /// </summary>
    public partial class AddFolder : Window
    {
        Client client;
        public AddFolder(Client client)
        {
            InitializeComponent();
            this.client = client;
        }

        private void choose_folder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();



            // Get the selected file name and display in a TextBox 
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Open document 
                string filename = dialog.SelectedPath;

                path.Text = filename;
            }

        }
        //
        private void load_folder_Click(object sender, RoutedEventArgs e)
        {
            if (path.Text != "")
            {
                //  ZIP THE FILE
                string startPath = @path.Text;
                //TODO change the path
                string zipPath = @"C:\Users\sds\Desktop\progetto\result.zip";
                // string extractPath = @"C:\Users\sds\Desktop\progetto";
                ZipFile.CreateFromDirectory(startPath, zipPath);
                //ZipFile.ExtractToDirectory(zipPath, extractPath);
                
                //SEND THE ZIP FILE
                client.send_zip(path.Text, zipPath);

                //change window
                ViewFolder view_f = new ViewFolder(client);
                view_f.Show();
                this.Close();
            }
        }
    }
}
