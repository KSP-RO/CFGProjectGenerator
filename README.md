**CFG Project Generator**

This simple tool generates a csproj containing all cfg files in GameData. It's useful if you like to work in (IDE of choice) to do all your editing, both of source and of cfg.

Under the Example directory is an example GameData.csproj.in file. The tool uses that as a basis and generates a csproj file, replacing the string `<<content>>` with the relevant `<Content/>` blocks, one per cfg file. Crucially, relative paths are preserved.

Usage: `CFGProjectGenerator.exe path\to\inputfile.csproj.in` -- in this case the tool will move up the directory tree from where inputfile.csproj.in lives until it finds a GameData directory, then recurse down it to find all cfg files. a `inputfile.csproj` will be generated next to the .in file.

Alternate usage: `CFGProjectGenerator.exe path\to\repo\root` -- in this case the tool will recurse into subdirectories to find a file called GameData.csproj.in. At that point it will behave like in the first case, with that as the .in file.