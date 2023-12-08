string target = Argument<string>("target", "ExecuteBuild");
bool VSBuilt = Argument<bool>("vsbuilt", false);

#addin nuget:?package=Cake.FileHelpers&version=5.0.0

// Cake API Reference: https://cakebuild.net/dsl/
// setup variables
var buildDir = "./Build";
var delPaths = GetDirectories("./**/*(obj|bin)");
var licenseFile = "../LICENSE.md";
var managedFolder = string.Format("{0}/{1}/{2}", buildDir, "EscapeFromTarkov_Data", "Managed");
var bepInExPluginsFolder = string.Format("{0}/{1}/{2}", buildDir, "BepInEx", "plugins");
var bepInExPluginsSptFolder = string.Format("{0}/{1}", bepInExPluginsFolder, "spt");
var bepInExPatchersFolder = string.Format("{0}/{1}/{2}", buildDir, "BepInEx", "patchers");
var solutionPath = "./Modules.sln";

Setup(context => 
{
    //building from VS will lock the files and fail to clean the project directories. Post-Build event on Aki.Build sets this switch to true to avoid this.
    FileWriteText("./vslock", "lock");
});

Teardown(context => 
{
    if(FileExists("./vslock")) 
    {
        DeleteFile("./vslock"); //remove vslock file
    }    
});

// Clean build directory and remove obj / bin folder from projects
Task("Clean")
    .WithCriteria(!VSBuilt)
    .Does(() => 
    {
        CleanDirectory(buildDir);
    })
    .DoesForEach(delPaths, (directoryPath) => 
    {
        DeleteDirectory(directoryPath, new DeleteDirectorySettings 
        {
            Recursive = true,
            Force = true
        });
    });

// Build solution
Task("Build")
    .IsDependentOn("Clean")
    .WithCriteria(!FileExists("./vslock")) // check for lock file if running from VS
    .Does(() => 
    {
        DotNetBuild(solutionPath, new DotNetBuildSettings
        {
            Configuration = "Release"
        });
    });

// Copy modules, managed dlls, and license to the build folder
Task("CopyBuildData")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CleanDirectory(buildDir);
        CreateDirectory(managedFolder);
        CreateDirectory(bepInExPluginsFolder);
		CreateDirectory(bepInExPluginsSptFolder);
		CreateDirectory(bepInExPatchersFolder);
        CopyFile(licenseFile, string.Format("{0}/LICENSE-Modules.txt", buildDir));
    })
    .DoesForEach(GetFiles("./Aki.*/bin/Release/net471/*.dll"), (dllPath) => //copy modules 
    {
		if(dllPath.GetFilename().ToString().StartsWith("aki_"))
        {
            //Incase you want to see what is being copied for debuging
            //Spectre.Console.AnsiConsole.WriteLine(string.Format("Adding Module: {0}", dllPath.GetFilename()));

            string patcherTransferPath = string.Format("{0}/{1}", bepInExPatchersFolder, dllPath.GetFilename());

            CopyFile(dllPath, patcherTransferPath);
        }
        if(dllPath.GetFilename().ToString().StartsWith("aki-")) 
        {
            //Incase you want to see what is being copied for debuging
            //Spectre.Console.AnsiConsole.WriteLine(string.Format("Adding Module: {0}", dllPath.GetFilename()));
        
            string moduleTransferPath = string.Format("{0}/{1}", bepInExPluginsSptFolder, dllPath.GetFilename());
        
            CopyFile(dllPath, moduleTransferPath);
        }
        else if (dllPath.GetFilename().ToString().StartsWith("Aki.")) // Only copy the custom-built dll's to Managed
        {
            //Incase you want to see what is being copied for debuging
            //Spectre.Console.AnsiConsole.WriteLine(string.Format("Adding managed dll: {0}", dllPath.GetFilename()));

            string fileTransferPath = string.Format("{0}/{1}", managedFolder, dllPath.GetFilename());
        
            CopyFile(dllPath, fileTransferPath);
        }
    });

// Runs all build tasks based on dependency and configuration
Task("ExecuteBuild")
    .IsDependentOn("CopyBuildData");

// Runs target task
RunTarget(target);