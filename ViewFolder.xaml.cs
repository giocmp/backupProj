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

namespace ProgettoPDS
{
    /// <summary>
    /// Logica di interazione per ViewFolder.xaml
    /// </summary>
    public partial class ViewFolder : Window
    {
        Client client;
        public ViewFolder(Client client)
        {
            InitializeComponent();
            this.client = client;
            List<Filename> items = new List<Filename>();
            client.view_folders();
            
            
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader myfile = new System.IO.StreamReader("c:\\test.txt");
            while ((line = myfile.ReadLine()) != null)
            {
                items.Add(new Filename() { Title = line });
            }
            
           

            folders.ItemsSource = items;
            myfile.Close();
        }

        private void download_Click(object sender, RoutedEventArgs e)
        {
           
            Filename f = (Filename)folders.SelectedItem;
            //Console.WriteLine(f.Title);
            client.download_file(f);
        }
    }
    public class Filename
    {
        public string Title { get; set; }

    }
}