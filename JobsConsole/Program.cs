using Hangfire;
using Newtonsoft.Json.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace JobsConsole
{
    public class Program
    {
        public static void Main()
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("Server=localhost,1433;Database=JobsConsole;User Id=sa;Password=j0bsh4ngf1r3!;TrustServerCertificate=True");

            var path = @"C:\temp\jobs-console";

            var options = new BackgroundJobServerOptions
            {
                WorkerCount = 1
            };

            using (var server = new BackgroundJobServer(options))
            {
                Console.WriteLine("Hangfire Server started");
                Console.ForegroundColor = ConsoleColor.Green;

                var jobId = BackgroundJob.Enqueue(() => CreateDirectory(path));
                Console.WriteLine("Directory jobs-console generated.");

                BackgroundJob.ContinueJobWith(jobId, () => ClearDirectory(path));
                Console.WriteLine("Directory jobs-console cleared.");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Enable H4CK3R M0D3? Y/N");
                var cki = Console.ReadKey();
                if (cki.Key.ToString() == "Y")
                {
                    BackgroundJob.Enqueue(() => HackerMode(path));
                }
                else
                {
                    RecurringJob.AddOrUpdate("apod", () => Apod(path).Wait(), Cron.Minutely);
                }
            }
        }

        public static void CreateDirectory(string path)
        {
            System.IO.Directory.CreateDirectory(path);
        }

        public static void ClearDirectory(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static async Task Apod(string path)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            using (var client = new HttpClient())
            {
                var json = JObject.Parse(await client.GetStringAsync("https://go-apod.herokuapp.com/apod"));

                try
                {
                    var txt = path + @"\infos.txt";
                    var hdurl = path + @"\photo.jpg";

                    using (var sw = new StreamWriter(txt))
                    {
                        sw.WriteLine("Copyright: " + json["copyright"]);
                        sw.WriteLine("Date: " + json["date"]);
                        sw.WriteLine("Title: " + json["title"]);
                        sw.WriteLine("Explanation: " + json["explanation"]);
                        sw.WriteLine("Media type: " + json["media_type"]);
                        sw.WriteLine("HD image: " + json["hdurl"]);
                        sw.WriteLine("Image: " + json["url"]);
                        sw.WriteLine("Service version: " + json["service_version"]);
                    }

                    var bytes = await client.GetByteArrayAsync((string)json["hdurl"]);

                    File.WriteAllBytes(hdurl, bytes);

                    Console.WriteLine($"\nCool stuff generated in: {path}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void HackerMode(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var txt = path + @"\h4ck3r_m0d3.txt";

                var processorInfo = new StringBuilder(string.Empty);
                var mgmntClass = new ManagementClass("Win32_OperatingSystem");
                var mgmntObj = mgmntClass.GetInstances();
                var properties = mgmntClass.Properties;

                foreach (ManagementObject obj in mgmntObj)
                {
                    foreach (PropertyData property in properties)
                    {
                        processorInfo.AppendLine(property.Name + ":  " + obj.Properties[property.Name].Value ?? obj.Properties[property.Name].Value.ToString());
                    }
                    processorInfo.AppendLine();
                }

                using (var sw = new StreamWriter(txt))
                {
                    sw.WriteLine(processorInfo);
                }

                Console.WriteLine($"\nH4Ck3R stuff generated in: {txt}.");
            }
            else
            {
                Console.WriteLine("\nH4CK3R M0D3 only available on windows");
            }
        }
    }
}