using System.DirectoryServices;

namespace ADLocal.Core.Entities
{
    public class ADLOU
    {
        public string Path { get; set; }
        public DirectoryEntry DirectoryEntry { get; set; } // het OU-object op de AD

        public ADLOU()
        {

        }
        public ADLOU(string path)
        {
            Path = path;
            DirectorySearcher directorySearcher = 
                new DirectorySearcher(new DirectoryEntry(path, ADLContext.contextUsername, ADLContext.contextPw))
            {
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.Base
            };
            DirectoryEntry = directorySearcher.FindOne().GetDirectoryEntry();
            if (DirectoryEntry != null)
            {
                Path = DirectoryEntry.Path;
            }

        }
        public override string ToString()
        {
            return Path;
        }
    }
}
