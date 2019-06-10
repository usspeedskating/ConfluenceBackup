using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USSConfluenceBackup
{
  public class GlacierArchive
  {
    public string ID { get; set; }
    public DateTime BackupDate { get; set; }

    public GlacierArchive(string id, DateTime backupDate)
    {
      ID = id;
      BackupDate = backupDate;
    }

    public GlacierArchive(JObject source) => Deserialize(source);

    public void Serialize(JsonWriter writer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("id");
      writer.WriteValue(ID);
      writer.WritePropertyName("date");
      writer.WriteValue(BackupDate);
      writer.WriteEndObject();
    }

    private void Deserialize(JObject source)
    {
      ID = source.Value<String>("id");
      BackupDate = source.Value<DateTime>("date");
    }
  }
}
