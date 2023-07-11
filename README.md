# **console_sftp_client**
 Console sftp client - это мини консольное приложение для передачи файлов на SFTP сервер. 

 _Не требует установки. Все что нужно это иметь на компьютере версию .Net (4.8) или выше._  
  
#
 _Для сборки советую использовать IDE "Microsoft visual studio"_
 
 Так же можно воспользоваться командной строкой. Пример(в косоле): C:\Windows\Microsoft.NET\Framework\<версия .NET>\msbuild <Путь к проекту>\sftp_client.sln

 Если вы используете IDE "Microsoft visual studio", тогда после автоматической сборки в папке sftp\bin\Release(или Debug) вам необходимы sftp_client.exe (исполняемый файл), Gnu.Getopt.dll (динамическая библиотека для работы с параметрами консольного приложения) и Renci.SshNet.dll (динамическая библиотека для работы с ssh и sftp).  
   
 _`sftp_client.exe, Gnu.Getopt.dll и Renci.SshNet.dll должны находиться в одной директории`_
   
#
## Параметры:

 _`Обязательные параметры отмечены "*"`_

    --help (-h): Выдает информацию о ключах и описание(не готова);
    --port (-P): Указывается порт подключения. Default: 22;
    --user (-u): * Указывается имя пользователя;
    --password (-p): Указывается пароль пользователя;
    --privateKey (-k): Указывается путь к файлу публичного ключа "authorized_keys"
    --option (-o): * Опция. unload - 0 (default), download - 1.
    --remotePath (-r): * Путь к файлу или директории на SFTP сервере.
    --localPath (-l): * Путь к файлу или директории на локальном компьютере.
`ВНИМАНИЕ: адрес хоста, например, "babamblbi.console_sftp_client.ru" - это единственный параметр, который без параметра.`

#
## Пример использования:  
_В консоле(cmd или powershell) необходимо прописать команду:_

`sftp_client.exe "имя sftp сервера" -u "логин" -p "пароль" -r "/директория или файл на sftp" -l "локальный файл или папка" -o 1`

Если указываются локальная папка и папка на SFTP, то все файлы из одной копируются в другую согласно -o.
  
#
## sftp_client.exe возвращает значения:

    Success = 0 - все ок
    NotConnected = 1 - не подключился к SFTP
    FileNotFound = 2 - не найден файл
    Unknown = 3 - не отслеживаемая ошибка
    BadSetting = 4 - не верно указанные параметры
    EmptySetting = 5 - пустые параметры

#
P.S. Утилита создавалась для работы 1С с sftp.
#
&copy; 2023 bamblbi.