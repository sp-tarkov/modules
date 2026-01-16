using System;
using System.Diagnostics;
using System.IO;

Console.WriteLine("Starting Build Project Process");
var processStartInfo = new ProcessStartInfo
{
    FileName = "dotnet",
    CreateNoWindow = true,
    UseShellExecute = false,
    RedirectStandardError = true,
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
    Arguments = "build ./SPT.Build/Spt.Build.csproj -c Release"
};

var process = Process.Start(processStartInfo);
process.WaitForExit();

Console.WriteLine(process.StandardError.ReadToEnd());
Console.WriteLine(process.StandardOutput.ReadToEnd());

Console.WriteLine("Setting Path Variables for build script");
var solutionFolder = Directory.GetCurrentDirectory();
var buildFolder = Path.Join(solutionFolder, "Build");
var bepinexFolder = Path.Join(buildFolder, "BepInEx");
var bepinexPatchFolder = Path.Join(buildFolder, "BepInEx/patchers");
var bepinexPluginFolder  = Path.Join(buildFolder, "BepInEx/plugins");
var bepinexSptFolder = Path.Join(buildFolder, "BepInEx/plugins/spt");
var projReleaseFolder = Path.Join(solutionFolder, "SPT.Build/bin/Release/netstandard2.1");
var licenseFile = Path.Join(solutionFolder, "../LICENSE.md");

// Delete build folder and contents to make sure it's clean
Console.WriteLine("Cleaning and creating directories");

if (Directory.Exists(buildFolder))
{
    Directory.Delete(buildFolder, true);
}

Directory.CreateDirectory(buildFolder);
Directory.CreateDirectory(bepinexSptFolder);
Directory.CreateDirectory(bepinexPatchFolder);

// Move DLLs from project's bin-release folder to the build folder
Console.WriteLine("Moving Dll's");
File.Copy(Path.Join(projReleaseFolder, "spt-common.dll"), Path.Join(bepinexSptFolder, "spt-common.dll"));
File.Copy(Path.Join(projReleaseFolder, "spt-reflection.dll"), Path.Join(bepinexSptFolder, "spt-reflection.dll"));
File.Copy(Path.Join(projReleaseFolder, "spt-core.dll"), Path.Join(bepinexSptFolder, "spt-core.dll"));
File.Copy(Path.Join(projReleaseFolder, "spt-custom.dll"), Path.Join(bepinexSptFolder, "spt-custom.dll"));
File.Copy(Path.Join(projReleaseFolder, "spt-debugging.dll"), Path.Join(bepinexSptFolder, "spt-debugging.dll"));
File.Copy(Path.Join(projReleaseFolder, "spt-singleplayer.dll"), Path.Join(bepinexSptFolder, "spt-singleplayer.dll"));
File.Copy(Path.Join(projReleaseFolder, "spt-prepatch.dll"), Path.Join(bepinexPatchFolder, "spt-prepatch.dll"));

// If any new DLLs need to be copied, add here
File.Copy(licenseFile, Path.Join(buildFolder, "LICENSE-Modules.txt"));
Console.WriteLine("Copied License and Finished!");
