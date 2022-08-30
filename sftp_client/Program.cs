using System;
using System.IO;
using Renci.SshNet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sftp_client
{
    class Program
    {
        static int Main(string[] args)
        {
            int result = 0;

            // Подключение к SFTP и копирование с компьютера ++
            string host = @"" + args[0]; //@"exchange.utrace.ru";
            string username = @"" + args[1]; //"xantis_xantis_1c_test_tcoorx";
            string password = @"" + args[2]; // "lak71q4jqClu5Nu";
            string remoteDirectory = @"" + args[3]; // "/Archive";
            string pathFromFile = @"" + args[4]; // Полный путь к файлу

            bool connected = false;
            bool noSuchFile = true;
            using (SftpClient sftp = new SftpClient(host, username, password)) {
                try {
                    sftp.Connect();
                    connected = true;
                    sftp.ChangeDirectory(remoteDirectory);
                    using (FileStream fs = new FileStream(pathFromFile, FileMode.Open))
                    {
                        noSuchFile = false;
                        sftp.BufferSize = 4 * 1024;
                        sftp.UploadFile(fs, Path.GetFileName(pathFromFile));
                    }
                    sftp.Disconnect();
                }
                catch (Exception ex) {
                    result = -1; // непредвиденная ошибка
                    /*
                    using (StreamWriter writer = new StreamWriter("C:\\templog.txt", true))
                    {
                        writer.WriteLine("An exception has been caught " + ex.ToString());
                        writer.Flush();
                    }
                    */
                }
            }
            // Подключение к SFTP и копирование с компьютера --

            if (!connected) {
                result = 1; // Не подключился к SFTP
            } else if (noSuchFile) {
                result = 2; // Не найден файл
            }

            return result;
        }
    }
}
