using Markdig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WikiGenerator
{
    public class HTMLGenerator
    {
        public readonly WikiMetadata WikiMetadata;
        public readonly Node RootNode;
        private readonly MarkdownPipeline pipeline;
        private readonly KeywordIndex keywordIndex = new KeywordIndex();
        private Dictionary<Node, PageMetadata> pageMetadataDict = new Dictionary<Node, PageMetadata>();
        private string sideBarLinksHTML;

        public HTMLGenerator(WikiMetadata wikiMetadata, Node rootNode)
        {
            WikiMetadata = wikiMetadata;
            RootNode = rootNode;

            pipeline = new MarkdownPipelineBuilder().
                UseAutoIdentifiers().
                UseFigures().
                UseAutoLinks().
                Build();
        }

        public void GenerateWiki()
        {
            pageMetadataDict.Clear();
            BuildPageMetaDataDictionary();

            WriteSearchIndexFile();

            sideBarLinksHTML = GenerateSidebarLinks();
            GenerateNode(RootNode);
        }

        private void WriteSearchIndexFile()
        {
            keywordIndex.Generate(pageMetadataDict, RootNode);
            string json = JsonConvert.SerializeObject(keywordIndex.Index, Formatting.Indented);
            File.WriteAllText(Path.Combine(RootNode.ResultFilePath, WikiConstants.SearchIndexJSPath), "const searchIndex = " + json);
        }

        private string GenerateSidebarLinks()
        {
            return getNodeLink(RootNode);

            string getNodeLink(Node node)
            {
                var meta = GetPageMetadataFromNode(node);

                string link = node == RootNode ? "<span>" : $"<a href=\"{Utils.GetLink(node, RootNode)}\">{meta.Title}</a><span>";
                int maxLinks = node == RootNode ? node.ChildDict.Count : WikiMetadata.MaxSidebarEntries;

                foreach (var pair in node.ChildDict.OrderBy(a => pageMetadataDict[a.Value].Order).Take(maxLinks))
                    link += getNodeLink(pair.Value);

                return link + "</span>";
            }
        }

        private void BuildPageMetaDataDictionary()
        {
            buildNext(RootNode);

            void buildNext(Node node)
            {
                if (GetMetadataString(node.FileContents, out string metadataString, out int eatenCharCount))
                {
                    node.MarkdownContent = node.FileContents.Substring(eatenCharCount);
                    pageMetadataDict.Add(node, ParseMetadata(metadataString));
                }
                else if (node.IsCategory)
                {
                    var dir = Path.GetDirectoryName(node.SourceFilePath);
                    var metaDataFile = Path.Combine(dir, node.Name, WikiConstants.CategoryMetadataPath);
                    if (File.Exists(metaDataFile))
                    {
                        string json = File.ReadAllText(metaDataFile);
                        pageMetadataDict.Add(node, JsonConvert.DeserializeObject<PageMetadata>(json));
                    }
                    else
                        pageMetadataDict.Add(node, new PageMetadata { Title = node.Name });
                }
                else
                    pageMetadataDict.Add(node, new PageMetadata { Title = node.Name });

                foreach (var child in node.ChildDict)
                    buildNext(child.Value);
            }
        }

        private void GenerateNode(Node node)
        {
            //Console.WriteLine("Requested generation of " + node.Name);

            if (node != RootNode)
            {
                string html;

                if (!node.IsCategory)
                {
                    html = GeneratePage(node);
                }
                else
                {
                    html = GenerateCategoryPage(node);
                    node.ResultFilePath = Path.ChangeExtension(node.ResultFilePath, "html");
                }

                File.WriteAllText(node.ResultFilePath, html, System.Text.Encoding.UTF8);
                //Console.WriteLine("Wrote " + node.ResultFilePath);
            }

            foreach (var item in node.ChildDict)
            {
                var child = item.Value;
                GenerateNode(child);
            }
        }

        private string GenerateCategoryPage(Node node)
        {
            var meta = GetPageMetadataFromNode(node);

            string categoryMarkdown = meta?.Description ?? "Here follow all pages under this category.";
            categoryMarkdown += "\n\n";

            foreach (var pair in node.ChildDict.OrderBy(a => pageMetadataDict[a.Value].Order))
            {
                var child = pair.Value;
                var itemMeta = GetPageMetadataFromNode(child);
                categoryMarkdown += $" - [{itemMeta.Title}]({Utils.GetLink(child, RootNode)})\n\n";
            }

            node.MarkdownContent = categoryMarkdown;

            return GeneratePage(node);
        }

        private string GeneratePage(Node node)
        {
            var metaData = pageMetadataDict[node];
            string source = node.MarkdownContent;

            if (!string.IsNullOrWhiteSpace(metaData.Title) && metaData.AddHeader)
                source = $"# {metaData.Title}\n" + source;

            string page = Inject("page");
            string markdownResult = Markdown.ToHtml(source, pipeline);

            string breadCrumbHTML = "";
            List<Node> pathToRoot = node.GetPathToRoot();
            for (int i = pathToRoot.Count - 1; i >= 0; i--)
            {
                Node item = pathToRoot[i];
                if (item == RootNode) continue;
                var itemMeta = GetPageMetadataFromNode(item);
                breadCrumbHTML += $"<a href=\"{Utils.GetLink(item, RootNode)}\">{itemMeta.Title}</a>\n";
            }

            page = page.Replace(WikiConstants.MarkdownResultKey, markdownResult);
            page = page.Replace(WikiConstants.TitleKey, WikiMetadata.Title);
            page = page.Replace(WikiConstants.PageTitleKey, metaData.Title);
            page = page.Replace(WikiConstants.BreadcrumbsKey, breadCrumbHTML);
            page = page.Replace(WikiConstants.SideBarLinksKey, sideBarLinksHTML);

            return page;
        }

        private static bool GetMetadataString(string source, out string result, out int eatenCharCount)
        {
            const string MetaDataTag = "metadata";

            result = "";
            eatenCharCount = 0;

            if (string.IsNullOrWhiteSpace(source))
                return false;

            int startIndex = source.IndexOf(MetaDataTag);
            int endIndex = source.IndexOf("/" + MetaDataTag, startIndex + MetaDataTag.Length);

            if (startIndex != 0 || endIndex <= startIndex)
                return false;

            eatenCharCount = endIndex + MetaDataTag.Length + 1;

            startIndex += MetaDataTag.Length;
            result = source[startIndex..endIndex].Trim();

            return true;
        }

        private static PageMetadata ParseMetadata(string metadataString)
        {
            const string delimiter = " ";
            PageMetadata meta = new PageMetadata();

            var fields = typeof(PageMetadata).GetFields();
            foreach (string line in metadataString.Split(new[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var split = line.Split(delimiter);
                var fieldName = split[0].ToLower();
                var value = string.Join(delimiter, split.Skip(1));
                foreach (var item in fields)
                {
                    if (item.Name.ToLower() == fieldName)
                    {
                        item.SetValue(meta, Convert.ChangeType(value, item.FieldType));
                        //Console.WriteLine($"Wrote page metadata value {fieldName} > {value}");
                    }
                }
            }

            return meta;
        }

        private PageMetadata GetPageMetadataFromNode(Node node)
        {
            return pageMetadataDict[node];
        }

        private string Inject(string name)
        {
            return File.ReadAllText($"{WikiConstants.InjectPath}\\{name}.html");
        }
    }
}
