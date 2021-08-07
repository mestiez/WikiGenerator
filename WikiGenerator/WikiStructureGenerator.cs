using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace WikiGenerator
{
    public class WikiStructureGenerator
    {
        private HTMLGenerator htmlGenerator;
        private readonly string inputPath;
        private readonly string outputPath;

        private WikiMetadata wikiMetadata;
        private Node RootNode;

        public WikiStructureGenerator(string inputPath, string outputPath)
        {
            this.inputPath = inputPath;
            this.outputPath = outputPath;
        }

        public void Generate()
        {
            RootNode = new Node(inputPath);
            RootNode.ResultFilePath = outputPath;

            wikiMetadata = LoadMetadata();
            htmlGenerator = new HTMLGenerator(wikiMetadata, RootNode);

            foreach (var item in wikiMetadata.Mirror)
            {
                var path = Path.Combine(inputPath, item);
                if (!File.Exists(path) && !Directory.Exists(path)) continue;

                var attr = File.GetAttributes(path);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    Utils.CopyDirectory(path, Path.Combine(outputPath, item));
                }
                else
                {
                    var target = Path.Combine(outputPath, item);
                    File.Copy(item, target);
                }
            }

            var markDownFiles = Directory.EnumerateFiles(inputPath, "*.md", SearchOption.AllDirectories).ToArray();

            bool largeWiki = markDownFiles.Length > 50;
            float processed = 0;
            int step = markDownFiles.Length / 10;
            foreach (var file in markDownFiles)
            {
                string path = Path.GetRelativePath(inputPath, file.Replace("/", "\\"));
                var info = new FileInfo(path);
                string[] breadcrumbs = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);

                var outPath = Path.Combine(outputPath, Path.GetRelativePath(inputPath, file));
                outPath = Path.ChangeExtension(outPath, "html");
                var outInfo = new FileInfo(outPath);
                outInfo.Directory.Create();

                var node = RootNode.GetOrCreateAtPath(breadcrumbs);
                node.ResultFilePath = outPath;
                node.FileContents = File.ReadAllText(file);
                processed++;

                if (largeWiki)
                {
                    if ((int)processed % step == 0)
                        Console.WriteLine("Found {0}% of the files", (int)MathF.Round(processed / markDownFiles.Length * 100));
                }
                else
                    Console.WriteLine("Found " + node.SourceFilePath);
            }

            htmlGenerator.GenerateWiki();

            //Console.ReadKey();
        }

        private WikiMetadata LoadMetadata()
        {
            var wikiDataJson = File.ReadAllText(Path.Combine(inputPath, WikiConstants.WikiJsonPath));
            var wikiData = JsonConvert.DeserializeObject<WikiMetadata>(wikiDataJson);
            return wikiData;
        }
    }
}
