using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
namespace ProgettoPDS
{
    public class Client
        //R: signup
        //L: login
        //S: sendzip
        //U: synch
    {
        private TcpClient tcpclnt;
        private string ipAddress;
        private int port;
        private string username;
        private string password;
      //  private string cookie;
        public Client(){
            this.ipAddress = "192.168.1.45";
            this.port = 11000;
            tcpclnt = new TcpClient();
        }
        public TcpClient Tcpclnt
        {
            get;
            set;
        }
        //todo aggiustare tutte le receive
        public bool receive(){
             byte[] rcv = new byte[1500];
            int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
            if (byteCount > 0)
            {
                string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                if (String.Compare(rx.Substring(0,2), "OK") == 0)
                    return true;
                else return false;
            }
            else return false;
        }
        public  void connect_to_server()
        {
            try
            {
               // TcpClient tcpclnt = new TcpClient();
                Console.WriteLine("Connecting.....");

                var result=tcpclnt.BeginConnect(ipAddress, port,null,null);

                
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                if (!success)
                {
                    Console.WriteLine("Failed to connect");
                }
                // use the ipaddress as in the server program
                else
                {
                    Console.WriteLine("Connected");
                    /*
                    byte[] msg = Encoding.UTF8.GetBytes("ciao giorgio");
                    byte[] rcv =new byte[1500];
                    tcpclnt.Client.Send(msg,SocketFlags.None);
                    int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                    if (byteCount > 0)
                        Console.WriteLine(Encoding.UTF8.GetString(rcv));
                     * */
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        public int signup(string username,string password)
        {
            try
            {
                byte[] credentials = Encoding.UTF8.GetBytes("R"+"."+username+"."+password+"<EOF>");
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx =(string) Encoding.UTF8.GetString(rcv).Clone();
                    //Console.WriteLine(Encoding.UTF8.GetString(rcv));
                   // Console.WriteLine(rx);
                    if (String.Compare(rx,"OK")==0)
                    {
                        //procedo al signup
                        this.username = username;
                        this.password = password;
                        return 1;
                    }
                    else
                    {

                        
                        //chiedo se vuole riprovare il signup o meno
                        return 0;
                    }
                }
                return -1;   
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                return -1;
            }
            finally
            {
                tcpclnt.Client.Close();
            }
        }
        
        public int login(string username,string password)
        {
            try
            {
                byte[] credentials = Encoding.UTF8.GetBytes("L" + "." + username + "." + password + "<EOF>");
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    //Console.WriteLine(Encoding.UTF8.GetString(rcv));
                   // Console.WriteLine(rx);
                    //vedo i primi due caratteri, il resto e' il cookie
                  //  if (String.Compare(rx.Substring(0,2), "OK") == 0)
                    if(String.Compare(rx,"OK")==0)
                    {
                        //procedo al login
        //                cookie = rx.Substring(4, 35);
                        return 1;
                    }
                    else
                    {


                        //chiedo se vuole riprovare il signup o meno
                        return 0;
                    }
                }
                return -1;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                return -1;
            }

        }

        public void send_zip(string path2,string file)
        {
                int l = path2.Length - 3;
                string path = path2.Substring(3, l);
                //invio s
            //lui controlla se esiste gia il percorso. se esiste
                byte[] credentials = Encoding.UTF8.GetBytes("S" + ":" + path);
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                //ricevo ok
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    //invio file
                    if (String.Compare(rx, "OK") == 0)
                    {

                        wrap_send_file(file);
                        
                    }
                }
            
            //ricevo ok
                int byteCount2 = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount2 > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    if (String.Compare(rx, "OK") != 0)
                    {
                        Console.WriteLine("qualcosa e' andato storto");
                    }
                }
            //chiudo socket
                tcpclnt.Client.Close();
        }
        
        //TODO metti in tmp
        public void browse_folder(string filename)
        {
            DirectoryInfo d = new DirectoryInfo(filename);
            foreach (var dir in d.GetDirectories())
                browse_folder(dir.FullName);
            foreach (var file in d.GetFiles())
            {
                Console.WriteLine(file.FullName);
                string hash = compute_md5(file.FullName);

                //string path = @"C:\Users\sds\Desktop\"+username+".txt";
                string path = @"C:\Users\sds\Desktop\file.txt";
                //se il file non esiste ne crea uno nuovo altrimenti lo usa
                //con true fa l'append, con false la write
                //quindi mi sa che devo mettere false
                StreamWriter sw = new StreamWriter(path, false);
                //NUOVO
                int l = file.FullName.Length - 3;
                string namefile = file.FullName.Substring(3, l);
                sw.WriteLine(namefile + " " + hash);


                sw.Close();
            }
            

        }

        public string compute_md5(string filename)
        {
            var md5 = MD5.Create();

            var stream = File.OpenRead(filename);

            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();


        }
        // USARE QUESTA
        public void wrap_send_file(string file)
        {
            StreamWriter sWriter = new StreamWriter(tcpclnt.GetStream());

            byte[] bytes = File.ReadAllBytes(file);

            sWriter.WriteLine(bytes.Length.ToString());
            sWriter.Flush();

        //    sWriter.WriteLine(file);
        //    sWriter.Flush();
            tcpclnt.Client.SendFile(file);
        }

        //NON USARE
        public void wrap_send_file2(string file)
        {

            string fileName = "test.txt";// "Your File Name";
            string filePath = @"C:\FT\";//Your File Path;
            byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);

            byte[] fileData = File.ReadAllBytes(file);
            byte[] clientData = new byte[4 +  fileData.Length];
            byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);

            fileNameLen.CopyTo(clientData, 0);
           // fileNameByte.CopyTo(clientData, 4);
            fileData.CopyTo(clientData, 4 + fileNameByte.Length);
        }

        public void wrap_recv_file()
        {
            //todo metti in cartella tmp
            FileStream fStream = new FileStream(@"C:\Users\sds\Desktop\"+username+".rar", FileMode.Create);

            // read the file in chunks of 1KB
            var buffer = new byte[1024];
            int bytesRead;

            //leggo la lunghezza
            bytesRead = tcpclnt.Client.Receive(buffer);
            string cmdFileSize = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            int length = Convert.ToInt32(cmdFileSize);

            int received = 0;

            while (received < length)
            {
                bytesRead = tcpclnt.Client.Receive(buffer);
                received += bytesRead;
                if (received >= length)
                {
                    bytesRead = bytesRead - (received - length);
                }
                fStream.Write(buffer, 0, bytesRead);

                Console.WriteLine("ricevuti: " + bytesRead + " qualcosa XD");
            }
            fStream.Flush();
            fStream.Close();
            Console.WriteLine("File Ricevuto");

        }

        public void synchronize(string path)
        {
            //login at every synch
            if (login(username, password) !=1)
            {
                //todo stampa qualche errore da qualche parte
                return;
            }
            //INVIO RICHIESTA SYNCH
           // string path = @"C:\Users\sds\Desktop\file.txt";
            byte[] credentials = Encoding.UTF8.GetBytes("S" + ":" + path );
            byte[] rcv = new byte[1500];
            tcpclnt.Client.Send(credentials, SocketFlags.None);
            //RICEVO OK
            int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
            if (byteCount > 0)
            {
                string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                if (String.Compare(rx.Substring(0,2), "OK") == 0)
                {
                   //prendo il nome della cartella
                  // string path2 = rx.Substring(3, rx.Length - 3);
                    
                    //AGGIUNGO C:\
                   string path1=@"C:\";
                   string dir=path1+path;
                   browse_folder(dir);
                    //INVIO FILE CON PERCORSI ED HASH
                   string file= @"C:\Users\sds\Desktop\" + username + ".txt";
                   wrap_send_file(file);
                   wrap_recv_file();
                   //INVIO I NUOVI FILE CHE MI VENGONO RICHIESTI
                    //read file line by line
                  
                   string line;

                   // Read the file and display it line by line.
                   System.IO.StreamReader myfile =new System.IO.StreamReader("c:\\test.txt");
                   while ((line = myfile.ReadLine()) != null)
                   {
                       wrap_send_file(line);
                       //ricevo qualcosa ad ogni invio
                       if (!receive())
                           break;
                   }
                    //l'ultimo ok lo ricevo per forza
                    //todo se va male mostrare qualcosa a schermo altrimenti dire che e' andato bene
                   
                   myfile.Close();

                   // close socket after every synch
                   tcpclnt.Client.Close();
                }
            }   
        }
        //return 1 if folder already present
        public int view_folders(){
            if (login(username, password) !=1)
            {
                //todo stampa qualche errore da qualche parte
                return 0;
            }
            //INVIO RICHIESTA SYNCH
           // string path = @"C:\Users\sds\Desktop\file.txt";
            byte[] credentials = Encoding.UTF8.GetBytes("L");
            byte[] rcv = new byte[1500];
            tcpclnt.Client.Send(credentials, SocketFlags.None);

            //controllare. in questo momento ricevo un mess, se ok poi ricevo il file che mostro
            //altrimenti ricevo no perche non ancora presente la cartella
            if(!receive()){
                return 0;
            }
            else{

                wrap_recv_file();
            
            //l'ultimo ok lo ricevo per forza
            //todo se va male mostrare qualcosa a schermo altrimenti dire che e' andato bene



            // close socket after every synch
            tcpclnt.Client.Close();
            return 1;
            }
            
        }
        public void download_file(Filename f)
        {
            if (login(username, password) != 1)
            {
                //todo stampa qualche errore da qualche parte
                return;
            }
            //INVIO RICHIESTA SYNCH
            // string path = @"C:\Users\sds\Desktop\file.txt";
            byte[] credentials = Encoding.UTF8.GetBytes("D:"+f);
            byte[] rcv = new byte[1500];
            tcpclnt.Client.Send(credentials, SocketFlags.None);

            wrap_recv_file();

            //todo se va male mostrare qualcosa a schermo altrimenti dire che e' andato bene



            // close socket after every synch
            tcpclnt.Client.Close();
        }
  
    }
}
