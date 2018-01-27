namespace ChatApp.Test.Helpers
{
    public class TestSitePathHelper
    {
        public string BasePath { get; set; }

        public readonly string Root = "/chat";

        public string this [string path] => Root + BasePath + path;

        public TestSitePathHelper(string basePath = null)
        {
            BasePath = basePath ?? "";
        }
    }
}