using System;
using System.IO;
using System.Linq;

namespace CFGProjectGenerator
{
    class Program
    {
        static int s_nFileCount = 0;
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine($"Error: No file specified. Gave {args.Length} arguments.");
                foreach (string a in args)
                    Console.WriteLine(a);
                return 1;
            }
            string szFilepath = args[0];

            string szBuf = File.ReadAllText(szFilepath);

            string szAbsPath = Path.GetFullPath(szFilepath);
            string szRelPath = string.Empty;
            DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(szAbsPath));
            while (dirInfo.Exists && dirInfo.Parent != null && dirInfo.GetDirectories().FirstOrDefault(d => d.Name == "GameData") == null)
            {
                dirInfo = dirInfo.Parent;
                szRelPath += "..\\";
            }
            if (!dirInfo.Exists)
            {
                Console.WriteLine("Error: Directory does not exist!");
                return 1;
            }
            DirectoryInfo dirGameData = dirInfo.GetDirectories().FirstOrDefault(d => d.Name == "GameData");
            if (dirGameData == null)
            {
                Console.WriteLine("Error: GameData not found!");
                return 1;
            }

            szRelPath += "GameData\\";

            // Special handling: if there's only one directory in GameData,
            // just start there and don't create an extra folder in the csproj
            if (dirGameData.GetDirectories().Length == 1)
            {
                DirectoryInfo di = dirGameData.GetDirectories()[0];
                szRelPath += di.Name + "\\";
                dirGameData = di;
            }

            string szAllFiles = BuildFileList(dirGameData, string.Empty, szRelPath);

            string szNewPath = szAbsPath.Replace(".csproj.in", ".csproj");
            Console.WriteLine($"Found {s_nFileCount} cfg files.\nWriting {szNewPath}");

            File.WriteAllText(szNewPath, szBuf.Replace("<<content>>", szAllFiles));

            return 0;
        }

        static string BuildFileList(DirectoryInfo curDir, string szPath, string szBasePath)
        {
            string szFiles = string.Empty;
            foreach (FileInfo fi in curDir.GetFiles())
            {
                if (fi.Extension != ".cfg")
                    continue;
                ++s_nFileCount;
                if (string.IsNullOrEmpty(szPath))
                    szFiles += $"    <Content Include=\"{szBasePath}{fi.Name}\" />\n";
                else
                {
                    szFiles += $"    <Content Include=\"{szBasePath}{szPath}{fi.Name}\">\n";
                    szFiles += $"      <Link>{szPath}{fi.Name}</Link>\n    </Content>\n";
                }
            }
            foreach (DirectoryInfo di in curDir.GetDirectories())
            {
                string szExtraPath = di.Name + "\\";
                szFiles += BuildFileList(di, szPath + szExtraPath, szBasePath);
            }

            return szFiles;
        }
    }
}
