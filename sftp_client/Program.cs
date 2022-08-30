using System;
using System.IO;
using Renci.SshNet;

namespace sftp_client
{
    class Program
    {
        static int Main(string[] args)
        {
            int result = 0;

            // Подключение к SFTP и копирование с компьютера ++
            string host = @"" + args[0];
            string username = @"" + args[1];
            string password = @"" + args[2];
            string remoteDirectory = @"" + args[3];
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
                    // если хотим записывать ошибку в log
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
