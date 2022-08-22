using Newtonsoft.Json;
using System.IO;
using System.Text;
using UnityEngine;

public class SessionSaver
{
    private const string SessionFileName = "session";
    private string SessionFilePath =>
        string.Format("{0}/{1}", Application.persistentDataPath, SessionFileName);

    public void SaveSessionData(SessionData data)
    {
        Debug.Log($"Save to: {SessionFilePath}");
        WriteToFile(JsonConvert.SerializeObject(data));
    }

    public bool TryRestoreSessionData(out SessionData sessionData)
    {
        sessionData = new SessionData();
        string strdata = ReadFromFile();
        if (string.IsNullOrEmpty(strdata)) return false;

        try
        {
            sessionData = JsonConvert.DeserializeObject<SessionData>(strdata);
            return true;
        }
        catch { return false; }
    }

    public bool IsSessionFileExist() => File.Exists(SessionFilePath);

    private string ReadFromFile()
    {
        if (!IsSessionFileExist()) return string.Empty;

        using (FileStream fstream = File.OpenRead(SessionFilePath))
        {
            byte[] buffer = new byte[fstream.Length];
            fstream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }
    }

    private void WriteToFile(string text)
    {
        if (File.Exists(SessionFilePath))
            File.WriteAllText(SessionFilePath, string.Empty);

        using (FileStream fstream = new FileStream(SessionFilePath, FileMode.OpenOrCreate))
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            fstream.Write(buffer, 0, buffer.Length);
        }
    }

}
