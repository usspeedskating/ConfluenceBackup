using Amazon.Glacier.Transfer;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace USSConfluenceBackup
{
  class Program
  {
    static string[] BACKUP_FILES_INSTALL =
    {
      //"/conf/web.xml",
      "conf/server.xml",
      "confluence/WEB-INF/web.xml",
    };

    static string[] BACKUP_FILES_HOME =
    {
      "confluence.cfg.xml",
      "attachments",
    };

    static void Main(string[] args)
    {
      var config = new Configuration(args.Length == 0 ? "config.json" : args[0]);
      var vault = new GlacierVault(config.Manifest, config.Vault);
      var date = DateTime.Now.Date;


      var tempDB = CreateTempFile(config.Temp, "confluence.db");
      var tempBup = CreateTempFile(config.Temp, "confluence_backup.zip");

      Write("Starting Database Backup");
      BackupDatabase("confluence", tempDB);
      Execute($"zip -r {tempBup.FullName} {GatherBackupFiles(config)} {tempDB.FullName}");
      DeleteFile(tempDB);
      vault.Upload(tempBup, date);
      UploadToS3(tempBup, config.Bucket);
      UploadToS3(vault.Manifest, config.Bucket);
      DeleteFile(tempBup);
      vault.DeleteOlderThan(date - config.RetentionPeriod);
    }

    private static String GatherBackupFiles(Configuration configuration) =>
      String.Join(" ", BACKUP_FILES_HOME.Select(x => Path.Combine(configuration.ConfluenceHome.FullName, x)).Concat(BACKUP_FILES_INSTALL.Select(y => Path.Combine(configuration.ConfluenceInstall.FullName, y))));

    private static FileInfo CreateTempFile(DirectoryInfo tempDirectory, string name)
    {
      var tempFile = new FileInfo(Path.Combine(tempDirectory.FullName, name));
      if (tempFile.Exists)
        DeleteFile(tempFile);

      return tempFile;
    }

    private static void DeleteFile(FileInfo tempFile)
    {
      Console.WriteLine($"rm {tempFile.FullName}");
      tempFile.Delete();
    }

    private static void UploadToS3(FileInfo file, string bucket)
    {
      using (var xfer = new TransferUtility())
        xfer.Upload(file.FullName, bucket);
    }

    private static void Execute(string command)
    {
      Console.WriteLine(command);
      var process = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = "/bin/bash",
          Arguments = $"-c \"{command}\"",
          UseShellExecute = false,
          //RedirectStandardOutput = true,
          CreateNoWindow = true
        }
      };

      process.Start();
      process.WaitForExit();
    }

    private static void BackupDatabase(string dbName, FileInfo backupFile) => Execute($@"runuser -l postgres -c 'pg_dump {dbName} > {backupFile.FullName}'");
    private static void BackupFiles(string dbName, FileInfo backupFile) => Execute($"pg_dump {dbName} > {backupFile.FullName}");

    private static void Write(string message) => Console.WriteLine(message);
  }
}
