using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace BlazorCleanRelease.Components.Classes
{
    public class Operations
    {
        public void clearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                clearFolder(di.FullName);
                di.Delete();
            }
        }

        public string GetValueFromLine(string[] lines, string key)
        {
            foreach (var line in lines)
            {
                if (line.StartsWith($"{key}="))
                {
                    return line.Replace($"{key}=", "");
                }
            }
            return "";
        }

        public int GetLineIndex (string[] lines, string findLineIndex)
        {
            for (int line=0; line<lines.Length; line++)
            {
                if (lines[line].StartsWith($"{findLineIndex}="))
                {
                    return line;
                }
            }
            return lines.Length - 1;
        }
    }



}
