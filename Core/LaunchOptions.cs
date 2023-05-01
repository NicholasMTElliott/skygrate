namespace Skyward.Skygrate.Core
{
    public class LaunchOptions
    {
        public string Version => "20230428.01";

        public string ApplicationName { get; set; }

        public string BaseDatabaseImage { get; set; }

        public string InstanceName { get; set; }

        public int PublicPort { get; set; }

        public string DbName { get; set; }

        public string DbUsername { get; set; }

        public string DbPassword { get; set; }

        public string ParameterCheck => MD5.CreateMD5(String.Join("\n", new List<string> {
                this.Version,
                this.ApplicationName,
                this.BaseDatabaseImage,
                this.InstanceName,
                this.PublicPort.ToString(),
                this.DbPassword,
                this.DbUsername,
                this.DbName
            }));
    }
}
