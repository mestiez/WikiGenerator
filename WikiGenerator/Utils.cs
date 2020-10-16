using System.IO;

namespace WikiGenerator
{
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

        public static string GetLink(Node target, Node rootNode)
        {
            return "./" + Path.GetRelativePath(rootNode.ResultFilePath, target.ResultFilePath).Replace("\\", "/");
        }
    }
}
