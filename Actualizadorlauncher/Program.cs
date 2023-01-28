using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using System.Diagnostics;

namespace Actualizadorlauncher
{
    internal class Program
    {        
        static void Main(string[] args)
        {
            Console.WriteLine("La actualizacion ha comenzado por favor ten paciencia");
            Console.WriteLine("Conectando con el servidor...");
            string version;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://");
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential("", "");
            Console.WriteLine("Enviando peticion...");
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string line = streamReader.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                version = line;
            }
            else
            {
                version = "";
            }
            streamReader.Close();
            response.Close();

            Console.WriteLine("Descargando archivos...");

            FtpWebRequest request2 = (FtpWebRequest)WebRequest.Create("ftp://" + version);
            request2.Method = WebRequestMethods.Ftp.DownloadFile;
            request2.Credentials = new NetworkCredential("", "");
            request2.KeepAlive = true;
            request2.UsePassive = false;
            request2.UseBinary = true;

            FtpWebResponse response2 = (FtpWebResponse)request2.GetResponse();

            Stream responseStream = response2.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            using (FileStream writer = new FileStream(".\\" + version, FileMode.Create))
            {
                long length = response2.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[2048];

                readCount = responseStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    writer.Write(buffer, 0, readCount);
                    readCount = responseStream.Read(buffer, 0, bufferSize);
                }
            }

            reader.Close();
            response2.Close();

            Console.WriteLine("Descomprimiendo actualizacion...");

            if (File.Exists(".\\LauncherHeavenV2.exe")) { File.Delete(".\\LauncherHeavenV2.exe"); }            
            if (File.Exists(".\\LauncherHeavenV2.exe.config")) { File.Delete(".\\LauncherHeavenV2.exe.config"); }           
            if (File.Exists(".\\LauncherHeavenV2.pdb")) { File.Delete(".\\LauncherHeavenV2.pdb"); }            
            ZipFile.ExtractToDirectory(".\\" + version, ".\\");
            File.Delete(".\\" + version);

            Console.WriteLine("Descarga completada abriendo el launcher");

            Process.Start(".\\LauncherHeavenV2.exe");
        }
    }
}
