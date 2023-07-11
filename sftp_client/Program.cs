using System;
using System.Collections.Generic;
using System.IO;
using Gnu.Getopt;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace sftp_client
{
    class Program
    {
        enum StatusCode {
            Success,
            NotConnected,
            FileNotFound,
            Unknown,
            BadSetting,
            EmptySetting
        }
        enum Option {
            unload,
            download
        }
        struct InputData {
            public string hostName;
            public int port;
            public string userName;
            public string password;
            public string privateKeyPath;
            public Option option;
            public string remotePath;
            public string localPath;
        }

        static int Main(string[] args) {
            StatusCode statusCode = StatusCode.Success;
            InputData inputData = ParsInputData(ref statusCode);

            if (statusCode == StatusCode.Success) {
                SFTPExecute(inputData, ref statusCode);
            }

            return (int)statusCode;
        }
    
        static InputData ParsInputData(ref StatusCode statusCode) {
            InputData inputData = new InputData();
            
            var longOptions = new LongOpt[] {
                new LongOpt("help", Argument.No, null, 'h'),
                new LongOpt("port", Argument.Required, null, 'P'),
                new LongOpt("user", Argument.Required, null, 'u'),
                new LongOpt("password", Argument.Required, null, 'p'),
                new LongOpt("privateKey", Argument.Required, null, 'k'),
                new LongOpt("option", Argument.Required, null, 'o'),
                new LongOpt("remotePath", Argument.Required, null, 'r'),
                new LongOpt("localPath", Argument.Required, null, 'l')
            };

            var parser = new Getopt("SFTPparam", Environment.GetCommandLineArgs(), "hP:u:p:k:o:r:l:", longOptions);

            List<string> argumentsList = new List<string>(parser.Argv);
            
            int option;
            while ((option = parser.getopt()) != -1) {
                if (statusCode == StatusCode.Success) {
                    string strOfOpt;
                    switch (option) {
                        case 'h':
                            Console.WriteLine("Help message");
                            break;
                        case 'P':
                            strOfOpt = parser.Optarg;
                            if (strOfOpt.StartsWith("-") || String.IsNullOrEmpty(strOfOpt)) {
                                statusCode = StatusCode.BadSetting;
                            } else {
                                if (!int.TryParse(strOfOpt, out inputData.port)) {
                                    statusCode = StatusCode.BadSetting;
                                } else {
                                    // Исключение аргументов, которые уже обработали
                                    argumentsList.RemoveAll(argument => argument == strOfOpt);
                                }
                            }
                            break;
                        case 'u':
                            strOfOpt = parser.Optarg;
                            if (strOfOpt.StartsWith("-") || String.IsNullOrEmpty(strOfOpt)) {
                                statusCode = StatusCode.BadSetting;
                            } else {
                                inputData.userName = strOfOpt;
                                // Исключение аргументов, которые уже обработали
                                argumentsList.RemoveAll(argument => argument == strOfOpt);
                            }
                            break;
                        case 'p':
                            strOfOpt = parser.Optarg;
                            if (strOfOpt.StartsWith("-") || String.IsNullOrEmpty(strOfOpt)) {
                                statusCode = StatusCode.BadSetting;
                            } else {
                                inputData.password = strOfOpt;
                                // Исключение аргументов, которые уже обработали
                                argumentsList.RemoveAll(argument => argument == strOfOpt);
                            }
                            break;
                        case 'k':
                            strOfOpt = parser.Optarg;
                            if (strOfOpt.StartsWith("-") || String.IsNullOrEmpty(strOfOpt)) {
                                statusCode = StatusCode.BadSetting;
                            } else {
                                inputData.privateKeyPath = strOfOpt;
                                // Исключение аргументов, которые уже обработали
                                argumentsList.RemoveAll(argument => argument == strOfOpt);
                            }
                            break;
                        case 'o':
                            strOfOpt = parser.Optarg;
                            if (strOfOpt.StartsWith("-") || String.IsNullOrEmpty(strOfOpt)) {
                                statusCode = StatusCode.BadSetting;
                            } else {
                                if (!Option.TryParse(strOfOpt, out inputData.option)) {
                                    statusCode = StatusCode.BadSetting;
                                } else {
                                    // Исключение аргументов, которые уже обработали
                                    argumentsList.RemoveAll(argument => argument == strOfOpt);
                                }
                            }
                            break;
                        case 'r':
                            strOfOpt = parser.Optarg;
                            if (strOfOpt.StartsWith("-") || String.IsNullOrEmpty(strOfOpt)) {
                                statusCode = StatusCode.BadSetting;
                            } else {
                                inputData.remotePath = strOfOpt;
                                // Исключение аргументов, которые уже обработали
                                argumentsList.RemoveAll(argument => argument == strOfOpt);
                            }
                            break;
                        case 'l':
                            strOfOpt = parser.Optarg;
                            if (strOfOpt.StartsWith("-") || String.IsNullOrEmpty(strOfOpt)) {
                                statusCode = StatusCode.BadSetting;
                            } else {
                                inputData.localPath = strOfOpt;
                                // Исключение аргументов, которые уже обработали
                                argumentsList.RemoveAll(argument => argument == strOfOpt);
                            }
                            break;
                        case '?':
                            break;
                    }
                }
            }

            bool isFirst = true;
            int countArguments = 0;
            foreach (string argument in argumentsList) {
                if (!argument.StartsWith("-")) {
                    if (!isFirst) {
                        inputData.hostName = argument;
                        countArguments++;
                    }
                    isFirst = false;
                }
            }

            if (countArguments != 1 || String.IsNullOrEmpty(inputData.hostName)) {
                statusCode = StatusCode.BadSetting;
            }

            return inputData;
        }

        static void SFTPExecute(InputData inputData, ref StatusCode statusCode)
        {
            #region Check_inputData
            if (String.IsNullOrEmpty(inputData.hostName) |
                String.IsNullOrEmpty(inputData.userName) |
                String.IsNullOrEmpty(inputData.remotePath) |
                String.IsNullOrEmpty(inputData.localPath))
            {
                statusCode = StatusCode.EmptySetting;
                return;
            }

            if (inputData.port == 0)
            {
                inputData.port = 22;
            }

            var methodPasswordOrPrivateKey = new List<AuthenticationMethod>();
            if (!String.IsNullOrEmpty(inputData.privateKeyPath))
            {
                PrivateKeyFile privateKeyFile = new PrivateKeyFile(inputData.privateKeyPath);
                methodPasswordOrPrivateKey.Add(new PrivateKeyAuthenticationMethod(inputData.userName, privateKeyFile));
            }
            if(!String.IsNullOrEmpty(inputData.password))
            {
                methodPasswordOrPrivateKey.Add(new PasswordAuthenticationMethod(inputData.userName, inputData.password));
            }
            if (methodPasswordOrPrivateKey.Count <= 0)
            {
                statusCode = StatusCode.EmptySetting;
                return;
            }
            #endregion

            if (statusCode == StatusCode.Success)
            {            
                ConnectionInfo connectionInfo = new ConnectionInfo(
                    inputData.hostName,
                    inputData.port,
                    inputData.userName,
                    methodPasswordOrPrivateKey.ToArray()
                );

                bool connected = false;
                using (SftpClient sftp = new SftpClient(connectionInfo))
                {
                    try
                    {
                        sftp.Connect();
                        connected = true;

                        if (inputData.option == Option.unload)
                        {
                            SFTPUnloadFiles(inputData, sftp, ref statusCode);
                        } 
                        else
                        {
                            SFTPDownloadFiles(inputData, sftp, ref statusCode);
                        }

                        sftp.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        statusCode = StatusCode.Unknown;
                        Console.WriteLine("An exception has been caught " + ex.ToString());
                        /* // если хотим записывать ошибку в log
                        using (StreamWriter writer = new StreamWriter("C:\\templog.txt", true))
                        {
                            writer.WriteLine("An exception has been caught " + ex.ToString());
                            writer.Flush();
                        }
                        */
                    }
                    finally
                    {
                        if (connected)
                        {
                            sftp.Disconnect();
                        }
                    }
                }

                if (!connected)
                {
                    statusCode = StatusCode.NotConnected;
                }
            }
        }

        static void SFTPUnloadFiles(InputData inputData, SftpClient sftp, ref StatusCode statusCode)
        {
            SftpFileAttributes attributes = sftp.GetAttributes(inputData.remotePath);
            if (attributes.IsRegularFile)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(inputData.localPath);
                if (directoryInfo.Exists)
                {
                    statusCode = StatusCode.BadSetting;
                }
                else
                {
                    SFTPUnloadFile(
                        sftp,
                        inputData.localPath,
                        inputData.remotePath,
                        ref statusCode
                    );
                }
            }
            else if (attributes.IsDirectory)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(inputData.localPath);
                if (directoryInfo.Exists)
                {
                    string localDirectory = inputData.localPath;
                    if (!localDirectory.EndsWith(@"\"))
                        localDirectory += @"\";

                    string remoteDirectory = inputData.remotePath;
                    if (!remoteDirectory.StartsWith("/"))
                        remoteDirectory = "/" + remoteDirectory;
                    if (!remoteDirectory.EndsWith("/"))
                        remoteDirectory += "/";

                    string[] localFiles = Directory.GetFiles(localDirectory);
                    foreach (string filePath in localFiles)
                    {
                        if (statusCode != StatusCode.Success)
                            break;

                        SFTPUnloadFile(
                            sftp,
                            filePath,
                            remoteDirectory + Path.GetFileName(filePath),
                            ref statusCode
                        );
                    }
                } 
                else
                {
                    string remoteDirectory = inputData.remotePath;
                    if (!remoteDirectory.StartsWith("/"))
                        remoteDirectory = "/" + remoteDirectory;
                    if (!remoteDirectory.EndsWith("/"))
                        remoteDirectory += "/";

                    SFTPUnloadFile(
                        sftp,
                        inputData.localPath,
                        remoteDirectory + Path.GetFileName(inputData.localPath),
                        ref statusCode
                    );
                }
            }
            else
            {
                statusCode = StatusCode.Unknown;
            }
        }
        static void SFTPUnloadFile(SftpClient sftp, string localPath, string remotePath, ref StatusCode statusCode)
        {
            bool noSuchFile = true;

            using (FileStream fs = new FileStream(localPath, FileMode.Open))
            {
                noSuchFile = false;
                //sftp.BufferSize = 4 * 1024; // Not provided, but if you are sure that the files will be 4kb or less, then you can speed up the work
                sftp.UploadFile(fs, remotePath);
            }

            if (noSuchFile)
            {
                statusCode = StatusCode.FileNotFound;
            }
        }

        static void SFTPDownloadFiles(InputData inputData, SftpClient sftp, ref StatusCode statusCode)
        {
            SftpFileAttributes attributes = sftp.GetAttributes(inputData.remotePath);
            if (attributes.IsRegularFile)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(inputData.localPath);
                if (directoryInfo.Exists)
                {
                    string localPath = inputData.localPath;
                    if (!localPath.EndsWith(@"\"))
                        localPath += @"\";

                    SFTPDownloadFile(
                        sftp,
                        localPath + Path.GetFileName(inputData.remotePath),
                        inputData.remotePath,
                        ref statusCode
                    );
                }
                else
                {
                    SFTPDownloadFile(
                        sftp,
                        inputData.localPath,
                        inputData.remotePath,
                        ref statusCode
                    );

                }
            }
            else if (attributes.IsDirectory)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(inputData.localPath);
                if (directoryInfo.Exists)
                {
                    string localDirectory = inputData.localPath;
                    if (!localDirectory.EndsWith(@"\"))
                        localDirectory += @"\";

                    string remoteDirectory = inputData.remotePath;
                    if (!remoteDirectory.StartsWith("/"))
                        remoteDirectory = "/" + remoteDirectory;
                    if (!remoteDirectory.EndsWith("/"))
                        remoteDirectory += "/";

                    var files = sftp.ListDirectory(remoteDirectory);
                    foreach (SftpFile file in files)
                    {
                        if (statusCode != StatusCode.Success)
                            break;

                        if (!file.IsDirectory)
                        {
                            SFTPDownloadFile(
                                sftp, 
                                localDirectory + file.Name,
                                remoteDirectory + file.Name,
                                ref statusCode
                            );
                        }
                    }
                }
                else
                {
                    statusCode = StatusCode.BadSetting;
                }
            }
            else
            {
                statusCode = StatusCode.Unknown;
            }
        }
        static void SFTPDownloadFile(SftpClient sftp, string localPath, string remotePath, ref StatusCode statusCode)
        {
            bool noSuchFile = true;
            using (var fs = new FileStream(localPath, FileMode.Create))
            {
                noSuchFile = false;
                //sftp.BufferSize = 4 * 1024; // Not provided, but if you are sure that the files will be 4kb or less, then you can speed up the work
                sftp.DownloadFile(remotePath, fs);
            }
            if (noSuchFile)
            {
                statusCode = StatusCode.FileNotFound;
            }
        }
    }
}
