using Amazon.Glacier.Transfer;
using Amazon.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace USSConfluenceBackup
{
  public class GlacierVault
  {
    public FileInfo Manifest { get; }
    private List<GlacierArchive> _archives = new List<GlacierArchive>();
    private string _name;
    private ArchiveTransferManager _manager;

    public GlacierVault(FileInfo manifest, string name) 
    {
      #region Validation
      if (manifest is null)
        throw new ArgumentNullException(nameof(manifest));
      if (String.IsNullOrWhiteSpace(name))
        throw new ArgumentNullException(nameof(name));
      #endregion
      Manifest = manifest;
      _manager = new ArchiveTransferManager(Amazon.RegionEndpoint.USEast1);
      _name = name;
      Load();
    }

    private void Load()
    {
      if (!Manifest.Exists)
        return;

      using (var fs = Manifest.OpenRead())
      using (var reader = new StreamReader(fs))
      using (var json = new JsonTextReader(reader))
        _archives = new List<GlacierArchive>(((JArray)JsonSerializer.Create().Deserialize(json)).Cast<JObject>().Select(x=>new GlacierArchive(x)));
    }

    private void Save()
    {
      if (File.Exists(Manifest.FullName))
        Manifest.Delete();

      using (var fs = Manifest.OpenWrite())
      using (var writer = new StreamWriter(fs))
      using (var json = new JsonTextWriter(writer))
      {
        json.WriteStartArray();
        foreach (var archive in _archives)
          archive.Serialize(json);
        json.WriteEndArray();
      }
    }

    public void DeleteOlderThan(DateTime date)
    {
      foreach (var archive in _archives.Where(x => x.BackupDate < date).ToList())
      {
        Console.WriteLine($"Deleting old backup file for {archive.BackupDate.ToShortDateString()}");
        Delete(archive);
      }
    }

    public void Enumerate()
    {
      
    }

    public void Purge()
    {

    }

    private void Delete(GlacierArchive archive)
    {
      var task = _manager.DeleteArchiveAsync(_name, archive.ID);
      task.Wait();
      if (task.IsCompletedSuccessfully)
      {
        _archives.Remove(archive);
        Save();
      }
    }

    public void Upload(FileInfo file, DateTime backupDate)
    {
      var task = _manager.UploadAsync(_name, $"Confluence Backup for {backupDate.ToShortDateString()}", file.FullName);
      task.Wait();
      if(task.IsCompletedSuccessfully)
      {
        _archives.Add(new GlacierArchive(task.Result.ArchiveId, backupDate));
        Save();
      }
    }
  }
}
