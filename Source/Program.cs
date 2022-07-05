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
                Console.WriteLine($"Error: No path or file specified.");
                PrintHelp();
                return 1;
            }
            if (args[0] == "-h" || args[0] == "--help")
            {
                PrintHelp();
                return 0;
            }
            string szFilepath = args[0];
            string szAbsPath = Path.GetFullPath(szFilepath);
            if (Directory.Exists(szAbsPath) && !File.Exists(szAbsPath))
            {
                // Find a "GameData.csproj.in" in some subdirectory
                string szTemp = PathToProject(new DirectoryInfo(szAbsPath));
                if (string.IsNullOrEmpty(szTemp))
                {
                    Console.WriteLine($"Could not find GameData.csproj.in under directory {szAbsPath}!");
                    return 1;
                }
                szAbsPath = szTemp;
            }
            if (!File.Exists(szAbsPath))
            {
                Console.WriteLine($"File {szAbsPath} does not exist!");
                return 1;
            }

            string szBuf = File.ReadAllText(szAbsPath);

            
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

        /// <summary>
        /// Prints help text
        /// </summary>
        static void PrintHelp()
        {
            Console.WriteLine("Usage: `CFGProjectGenerator.exe path/to/inputfile.csproj.in`");
            Console.WriteLine("Or: `CFGProjectGenerator.exe rootpath`\n\t(under which there is a GameData.csproj.in in that or some subdir)");
            Console.WriteLine("In the .csproj.in file, the string `<<content>>` will be replaces with all cfg files found in GameData");
        }

        /// <summary>
        /// Finds "GameData.csproj.in" in some subdir
        /// and returns the full path
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        static string PathToProject(DirectoryInfo dir)
        {
            FileInfo fi = dir.GetFiles().FirstOrDefault(f => f.Name == "GameData.csproj.in");
            if (fi != null)
                return fi.FullName;

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                string szPath = PathToProject(di);
                if (!string.IsNullOrEmpty(szPath))
                    return szPath;
            }

            return string.Empty;
        }

        /// <summary>
        /// Recursively build a list of all cfg files in subdirectories
        /// </summary>
        /// <param name="curDir">Directory to search</param>
        /// <param name="szPath">current path to display</param>
        /// <param name="szBasePath">file system path prepended to szPath</param>
        /// <returns></returns>
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
