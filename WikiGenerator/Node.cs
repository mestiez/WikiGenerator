using System.Collections.Generic;
using System.IO;

namespace WikiGenerator
{
    public class Node
    {
        public Dictionary<string, Node> ChildDict { get; private set; } = new Dictionary<string, Node>();

        public string Name { get; }
        public string SourceFilePath { get; private set; }
        public string ResultFilePath { get; set; }
        public Node Parent { get; private set; }

        public string FileContents { get; set;  }
        public string MarkdownContent { get; set;  }

        public bool IsCategory => ChildDict.Count > 0;

        public Node(string name)
        {
            Name = name;
            SourceFilePath = name;
        }

        public override string ToString() => Name;

        public Node GetOrCreate(string name)
        {
            if (!ChildDict.TryGetValue(name, out var node))
            {
                node = new Node(name);
                node.Parent = this;
                node.SourceFilePath = Path.Combine(Path.ChangeExtension(SourceFilePath, null), name);
                node.ResultFilePath = Path.ChangeExtension(Path.Combine(Path.ChangeExtension(ResultFilePath, null), name), "html");
                ChildDict.Add(name, node);
            }

            return node;
        }

        public Node GetOrCreateAtPath(string[] breadcrumbs)
        {
            Node lastNode = null;
            foreach (var name in breadcrumbs)
            {
                if (lastNode == null)
                {
                    if (name == Name)
                        return this;

                    lastNode = GetOrCreate(name);
                }
                else
                    lastNode = lastNode.GetOrCreate(name);
            }

            return lastNode;
        }

        public List<Node> GetPathToRoot()
        {
            List<Node> list = new List<Node>();

            RecursiveRootSearch(list);

            return list;
        }

        private void RecursiveRootSearch(List<Node> list)
        {
            list.Add(this);

            if (Parent != null)
                Parent.RecursiveRootSearch(list);
        }
    }
}
