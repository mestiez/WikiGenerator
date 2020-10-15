using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace WikiGenerator
{
    public static class WikiGenerator
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                if (args.Length == 0)
                {
                    var exec = AppDomain.CurrentDomain.BaseDirectory;

                    Utils.CopyDirectory(Path.Combine(exec, WikiConstants.InjectPath), WikiConstants.InjectPath);

                    Directory.CreateDirectory("raw\\media");
                    Directory.CreateDirectory("build");

                    File.WriteAllText("build.bat", $"{exec}{nameof(WikiGenerator)}.exe raw build");
                    File.WriteAllText("raw\\wiki.json", JsonConvert.SerializeObject(new WikiMetadata
                    {
                        Title = "Template",
                        Author = "You",
                        MaxSidebarEntries = 25,
                        Mirror = new[] { "media" }
                    }, Formatting.Indented));
                    File.WriteAllText("raw\\index.md", "metadata\ntitle Home\n/metadata\n\nThese are the contents of index.md. Add more files / folders to create a wiki.");
                    return;
                }

                Console.WriteLine("Usage:\n\tWikiGenerator [input folder] [output folder]");
                return;
            }

            string inputFolder = args[0];
            string outputFolder = args[1];

            var generator = new WikiStructureGenerator(inputFolder, outputFolder);
            generator.Generate();
        }
    }

    public struct Utils
    {
        public static void CopyDirectory(string src, string dest)
        {
            DirectoryInfo info = new DirectoryInfo(src);

            Directory.CreateDirectory(dest);

            foreach (var childFile in info.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(src, childFile.FullName);
                var source = Path.Combine(src, relativePath);
                var target = Path.Combine(dest, relativePath);

                FileInfo targetInfo = new FileInfo(target);

                if (!Directory.Exists(targetInfo.Directory.FullName))
                    targetInfo.Directory.Create();

                File.Copy(source, target, true);
            }
        }
    }
}
