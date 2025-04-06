using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var googleIps = await Dns.GetHostAddressesAsync("google.com");
            string addresses = String.Empty;
            foreach (IPAddress ip in googleIps)
            {
                addresses += ip.ToString() + Environment.NewLine;
            }
            MessageBox.Show(addresses);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {

            }
            var ipProps = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = ipProps.GetActiveTcpConnections();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //OpenFileDialog для выбора файла для закачки
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "*.txt";
            openFileDialog.Filter = "Текстовый файл|*.txt";
            //Choose file_name
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            //file_name

            string full_file_name = openFileDialog.FileName;
            string short_file_name = full_file_name.Split('\\').Last();
            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"ftp://212.12.30.90:10021/{short_file_name}");
            ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpWebRequest.Credentials = new NetworkCredential("admin", "WemjdXEB");

            //Открытие файла для загрузки
            FileStream fs = new FileStream(full_file_name, FileMode.Open);
            //Создаем массив данных
            byte[] byte_array = new byte[fs.Length];
            fs.Read(byte_array, 0, byte_array.Length);
            fs.Close();
            ftpWebRequest.ContentLength = byte_array.Length;

            //Берем поток из нашего запроса

            Stream stream = ftpWebRequest.GetRequestStream();
            stream.Write(byte_array, 0, byte_array.Length);
            stream.Close();

            FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse();


            Stream responseStream = response.GetResponseStream();
            byte[] buffer = new byte[1024];
            int size = 0;
            string respons_string = String.Empty;
            while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                respons_string += Encoding.UTF8.GetString(buffer);
            }

            MessageBox.Show(respons_string);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"ftp://212.12.30.90:10021");
            ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            ftpWebRequest.Credentials = new NetworkCredential("admin", "WemjdXEB");

            FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse();


            Stream responseStream = response.GetResponseStream();
            byte[] buffer = new byte[64];
            int size = 0;
            string respons_string = String.Empty;
            DataTable dt = new DataTable();
            while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                respons_string += Encoding.UTF8.GetString(buffer);
            }
            string[] infoFile = respons_string.Split("\r\n");
            Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            Regex.Match(infoFile[0], regex.ToString());
            List<InfoFile> infoFiles = new List<InfoFile>();
            dt.Columns.AddRange(new DataColumn[] {
                new DataColumn("Type", "Type".GetType()),
                new DataColumn("Permition", "Type".GetType()),
                new DataColumn("Size", "Type".GetType()),
                new DataColumn("CreateDate", "Type".GetType()),
                new DataColumn("CreateTime", "Type".GetType()),
                new DataColumn("Name", "Type".GetType()),
                });
            DataGridViewButtonColumn dataGridViewButtonColumn = new DataGridViewButtonColumn();
            dataGridViewButtonColumn.Name = "Delete_Button";
            dataGridViewButtonColumn.Text = "Delete";


            foreach (string file in infoFile)
            {
                Match match = regex.Match(file);
                if (match.Length > 5)
                {
                    // Устанавливаем тип, чтобы отличить файл от папки (используется также для установки рисунка)
                    string type = match.Groups[1].Value == "d" ? "DIR.png" : "FILE.png";

                    // Размер задаем только для файлов, т.к. для папок возвращается
                    // размер ярлыка 4кб, а не самой папки
                    string size_string = "";
                    if (type == "FILE.png")
                        size_string = (Int32.Parse(match.Groups[3].Value.Trim()) / 1024).ToString() + " кБ";
                    string permition = match.Groups[2].Value;
                    string date = match.Groups[4].Value;
                    string time = match.Groups[5].Value;
                    string name = match.Groups[6].Value;

                    InfoFile info = new InfoFile
                    {
                        Type = type,
                        Permition = permition,
                        Size = size_string,
                        CreateDate = date,
                        CreateTime = time,
                        Name = name
                    };
                    infoFiles.Add(info);
                    dt.Rows.Add(info.Type, info.Permition, info.Size, info.CreateDate, info.CreateTime, info.Name);
                }

            }
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = infoFiles;
            // Добавить поле, которое будет возвращать пользователя на директорию выше
            dataGridView1.Columns.Add(dataGridViewButtonColumn);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.Rows)
            {
                dataGridViewRow.Cells[dataGridView1.Columns.Count - 1].Value = "Удалить";
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string file_name = ((DataGridView)sender).Rows[e.RowIndex].Cells[5].Value.ToString();
            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"ftp://212.12.30.90:10021/{file_name}");
            ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            ftpWebRequest.Credentials = new NetworkCredential("admin", "WemjdXEB");
            FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse();
            button4.PerformClick();

        }
    }
}
