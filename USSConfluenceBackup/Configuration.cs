using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace USSConfluenceBackup
{
  public class Configuration
  {
    public FileInfo Source { get; private set; }
    public DirectoryInfo Temp { get; private set; }
    public DirectoryInfo ConfluenceHome { get; private set; }
    public DirectoryInfo ConfluenceInstall { get; private set; }
    public FileInfo Manifest { get; private set; }
    public TimeSpan RetentionPeriod { get; private set; }
    public string Vault { get; private set; }
    public string Bucket { get; private set; }
    //public string AccessKey { get; private set; }
    //public string SecretKey { get; private set; }

    public Configuration(string sourceFile) : this(new FileInfo(sourceFile)) { }
    public Configuration(FileInfo source)
    {
      #region Validation
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (!source.Exists)
        throw new Exception($"{source.FullName} does not exist");
      #endregion
      Source = source;
      Load();
    }

    private void Load()
    {
      using (var fs = Source.OpenRead())
      using (var reader = new StreamReader(fs))
      using (var json = new JsonTextReader(reader))
        Deserialize((JObject)JsonSerializer.Create().Deserialize(json));
    }

    private void Deserialize(JObject source)
    {
      Temp = new DirectoryInfo(source.Value<String>("temp"));
      ConfluenceHome = new DirectoryInfo(source.Value<String>("home"));
      ConfluenceInstall = new DirectoryInfo(source.Value<String>("install"));
      Manifest = new FileInfo(source.Value<String>("manifest"));
      RetentionPeriod = TimeSpan.FromDays(source.Value<int>("retentionDays"));
      Vault = source.Value<String>("vault");
      Bucket = source.Value<String>("bucket");
      //AccessKey = source.Value<String>("accessKey");
      //SecretKey = source.Value<String>("secretKey");
    }
  }
}
