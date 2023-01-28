using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;

namespace LauncherHeavenV2
{
    public partial class Form1 : Form
    {
        public string nombre;
        public string ip = "";
        public string mods;
        public string[] modslista;

        public string ftpusuario = ""; //Usuario
        public string ftpcontraseña = ""; //userheaven
        public string[] ftpmods;

        public int nummodsparadescargar;
        public int nummodsdescargados;

        public string versionactual = "2.4";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ip);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential("", "");
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string version;
            string line = streamReader.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                version = line;
                version = version.Remove(version.Length - 4);
            }
            else
            {
                version = "";
            }
            streamReader.Close();
            response.Close();

            if (version != versionactual)
            {
                MessageBox.Show("Es necesario actualizar el launcher, pulsa OK para continuar", "Detectada version nueva", MessageBoxButtons.OK);
                Process.Start(".\\Actualizadorlauncher.exe");
                Close();
            }

            timer1.Enabled = true;
            textBox1.Text = Properties.Settings.Default.clan;
            textBox2.Text = Properties.Settings.Default.nombre;
            label1.Text = pingear(ip);
            if (File.Exists(".\\Launcher Heaven.exe"))
            {
                File.Delete(".\\Launcher Heaven.exe");
            }
            if (File.Exists(".\\Launcherheaven.exe"))
            {
                File.Delete(".\\Launcherheaven.exe");
            }
            if (File.Exists(".\\Launcherheaven.deps.json"))
            {
                File.Delete(".\\Launcherheaven.deps.json");
            }
            if (File.Exists(".\\Launcherheaven.dll"))
            {
                File.Delete(".\\Launcherheaven.dll");
            }
            if (File.Exists(".\\Launcherheaven.pdb"))
            {
                File.Delete(".\\Launcherheaven.pdb");
            }
            if (File.Exists(".\\Launcherheaven.runtimeconfig.json"))
            {
                File.Delete(".\\Launcherheaven.runtimeconfig.json");
            }
            if (File.Exists(".\\updater.bat"))
            {
                File.Delete(".\\updater.bat");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                if (textBox1.Text != "")
                {
                    nombre = "[" + textBox1.Text + "]" + textBox2.Text;
                }
                else
                {
                    nombre = textBox2.Text;
                }
                Properties.Settings.Default.nombre = textBox2.Text;
                Properties.Settings.Default.clan = textBox1.Text;
                Properties.Settings.Default.Save();

                button1.Enabled = false;
                cambiartextobtn("INICIANDO JUEGO", 15);
                progressBar1.Visible = true;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/Hrve3vK3pH");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/c/Bayonora");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
                label1.Text = pingear(ip);
        }

        public string pingear(string ip)
        {
            Ping hacerping = new Ping();
            PingReply respuestaping = hacerping.Send(ip);
            if (respuestaping.Status == IPStatus.Success)
            {
                label1.ForeColor = Color.Green;
                return "Ping: " + respuestaping.RoundtripTime.ToString() + "ms";
            }
            else
            {               
                label1.ForeColor = Color.Red;
                return "SERVIDOR OFFLINE";
            }
        }

        public void cambiartextobtn(string texto, int tamaño)
        {
            button1.Text = texto;
            button1.Font = new Font("Segoe UI", tamaño, FontStyle.Bold, GraphicsUnit.Point);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] archivoseliminar = Directory.GetFiles(".\\mods");
            foreach (string archivo in archivoseliminar)
            {
                File.Delete(archivo);
            }
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ip);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(ftpusuario, ftpcontraseña);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            List<string> directories = new List<string>();
            string line = streamReader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                directories.Add(line);
                line = streamReader.ReadLine();
            }
            streamReader.Close();
            response.Close();
            ftpmods = directories.ToArray();
            for (int i = 0; i < ftpmods.Length; i++)
            {
                ftpmods[i] = ftpmods[i].Remove(ftpmods[i].Length - 4);
            }
            modslista = Directory.GetDirectories(".\\mods");
            for (int i = 0; i < modslista.Length; i++)
            {
                modslista[i] = modslista[i].Remove(0, 7);
            }

            foreach (string mod in modslista)
            {
                if (!ftpmods.Contains(mod))
                {
                    Directory.Delete(".\\mods\\" + mod, true);
                }
            }

            List<string> modsdescarga = new List<string>();
            foreach (string mod in ftpmods)
            {
                if (!modslista.Contains(mod))
                {
                    modsdescarga.Add(mod);
                }
            }

            nummodsparadescargar = modsdescarga.Count;

            foreach (string mod in modsdescarga)
            {
                FtpWebRequest request2 = (FtpWebRequest)WebRequest.Create("ftp://" + ip + "/" + mod + ".zip");
                request2.Method = WebRequestMethods.Ftp.DownloadFile;
                request2.Credentials = new NetworkCredential(ftpusuario, ftpcontraseña);
                request2.KeepAlive = true;
                request2.UsePassive = false;
                request2.UseBinary = true;

                FtpWebResponse response2 = (FtpWebResponse)request2.GetResponse();

                Stream responseStream = response2.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                using (FileStream writer = new FileStream(".\\mods\\" + mod + ".zip", FileMode.Create))
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
                nummodsdescargados++;
                backgroundWorker1.ReportProgress((nummodsdescargados*100)/nummodsparadescargar);
            }

            backgroundWorker1.ReportProgress(100);
            Thread.Sleep(500);

            foreach (string mod in modsdescarga.ToArray())
            {
                ZipFile.ExtractToDirectory(".\\mods\\" + mod + ".zip", ".\\mods");
                File.Delete(".\\mods\\" + mod + ".zip");
            }

            modslista = Directory.GetDirectories(".\\mods");
            for (int i = 0; i < modslista.Length; i++)
            {
                modslista[i] = modslista[i].Remove(0, 2);
            }

            List<string> modslistaordenar = modslista.ToList();
            modslistaordenar.Remove("@Community-Online-Tools");
            modslistaordenar.Remove("@CF");
            modslistaordenar.Remove("@Trader");
            modslistaordenar.Insert(0, "@CF");
            modslistaordenar.Insert(0, "@Community-Online-Tools");
            modslistaordenar.Add("@Trader");
            modslista = modslistaordenar.ToArray();

            foreach (string mod in modslista)
            {
                mods = mods + mod + ";";
            }
            string[] lineas = File.ReadAllLines(".\\heaven.ini");
            lineas[3] = "CommandLine = -name=" + nombre + "  -connect=" + ip + "  -port=2303 \"-mod=" + mods + "\"";
            File.WriteAllLines(".\\heaven.ini", lineas);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Process.Start("cmd.exe", "/c start \"\" \"Steam\\SmartSteamLoader.exe\" heaven.ini");
            Close();
        }
    }
}
