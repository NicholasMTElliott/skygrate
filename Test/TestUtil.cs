namespace Test
{
    public static class TestUtil
    {
        public static string CloneTestData(string source, string to)
        {
            int pathLen = source.Length + 1;
            string destination = $".\\ClonedData\\{to}";
            try
            {
                Directory.Delete(destination, true);
            }
            catch (DirectoryNotFoundException ex) { }

            Directory.CreateDirectory(destination);

            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                string subPath = dirPath.Substring(pathLen);
                string newpath = Path.Combine(destination, subPath);
                Directory.CreateDirectory(newpath);
            }

            foreach (string filePath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                string subPath = filePath.Substring(pathLen);
                string newpath = Path.Combine(destination, subPath);
                File.Copy(filePath, newpath);
            }

            return destination;
        }
    }
}