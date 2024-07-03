using System.IO;

namespace TwitchTTSUser.Models
{
    public class FileWriter
    {
        private const string UsernameFile = "username.txt";
        private const string MessageFile = "message.txt";

        public FileWriter() 
        {
            ClearFiles();
        }

        ~FileWriter()
        {
            ClearFiles();
        }

        public void WriteMessage(string message) => WriteToFile(MessageFile, message);
        public void WriteUsername(string username) => WriteToFile(UsernameFile, username);

        private void WriteToFile(string filename, string data)
        {
            using (StreamWriter FileWriter = File.CreateText(filename))
            {
                FileWriter.WriteLineAsync(data);
            }
        }

        public void ClearFiles()
        {
            WriteMessage(string.Empty);
            WriteUsername(string.Empty);
        }
    }
}
