using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Diagnostics;

namespace SingleBlog.Entities
{
    public static class PathUtils
    {
        private const string ImageDirName = "Images";
        private const string DbName = "SingleBlog.db";

        public static string ImagesDirName => ImageDirName;

        public static string ImagesContentRootPath => Path.Join(ProcessRootPath, ImageDirName);
        
        public static string DbFilePath => Path.Join(ProcessRootPath, DbName);

        public static string ProcessRootPath
        {
            get
            {
                var process = Process.GetCurrentProcess();
                return Path.GetDirectoryName(process.MainModule.FileName);
            }
        }
    }
}
