using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ObjectMapper2LinqPadDeployment
{
    class Program
    {
        private static readonly string[] Files = new[]
                                                     {
                                                         "ObjectMapper2LinqPad.dll", "ObjectMapper2LinqPad.pdb",
                                                         "ObjectMapper.dll", "ObjectMapper.pdb",
                                                         "Oracle.DataAccess.dll", "Npgsql.dll"
                                                     };

        static void Main()
        {
            try
            {
                string publicKeyToken = string.Empty;
                var assembly = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(),Files.First()));
                assembly.GetName().GetPublicKeyToken().Select(t => publicKeyToken += t.ToString("x2")).ToList();

                string deploymentPath = string.Format(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    @"LINQPad\Drivers\DataContext\4.0\ObjectMapper2LinqPad ({0})\"), publicKeyToken);

                Console.WriteLine("Output path: " + deploymentPath);

                if (!Directory.Exists(deploymentPath))
                    Directory.CreateDirectory(deploymentPath);

                foreach (string file in Files)
                {
                    if (File.Exists(deploymentPath + file))
                        File.Delete(deploymentPath + file);

                    if (File.Exists(file))
                        File.Copy(file, deploymentPath + file);
                }
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine("ERROR: "+ exc.Message);
            }
        }

    }
}
