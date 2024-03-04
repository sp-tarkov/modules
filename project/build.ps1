$buildFolder = "..\Build"
$bepinexFolder = "..\Build\BepInEx"
$bepinexPatchFolder = "..\Build\BepInEx\Patchers"
$bepinexPluginFolder = "..\Build\BepInEx\Plugins"
$bepinexSptFolder = "..\Build\BepInEx\Plugins\spt"
$eftDataFolder = "..\Build\EscapeFromTarkov_Data"
$managedFolder = "..\Build\EscapeFromTarkov_Data\Managed"
$projReleaseFolder = ".\bin\Release\net471"
$licenseFile = "..\..\LICENSE.md"

Write-Host "--------------- Cleaning Output Build Folder ---------------"

# Delete build folder and contents to make sure it's clean
if (Test-Path $buildFolder) { Remove-Item -Path $buildFolder -Recurse -Force }

Write-Host "--------------- Done Cleaning Output Build Folder ---------------"
Write-Host "--------------- Creating Output Build Folders ---------------"

# Create build folder and subfolders if they don't exist
$foldersToCreate = @($buildFolder, $bepinexFolder, $bepinexPatchFolder, $bepinexPluginFolder, $bepinexSptFolder, $eftDataFolder, $managedFolder)
foreach ($folder in $foldersToCreate) {
    if (-not (Test-Path $folder)) { New-Item -Path $folder -ItemType Directory }
}

Write-Host "--------------- Done Creating Output Build Folders ---------------"
Write-Host "--------------- Moving DLLs to $buildFolder ---------------"

# Move DLLs from project's bin\Release folder to the build folder
Copy-Item "$projReleaseFolder\Aki.Common.dll" -Destination $managedFolder
Copy-Item "$projReleaseFolder\Aki.Reflection.dll" -Destination $managedFolder
Copy-Item "$projReleaseFolder\aki_PrePatch.dll" -Destination $bepinexPatchFolder
Copy-Item "$projReleaseFolder\aki-core.dll" -Destination $bepinexSptFolder
Copy-Item "$projReleaseFolder\aki-custom.dll" -Destination $bepinexSptFolder
Copy-Item "$projReleaseFolder\aki-debugging.dll" -Destination $bepinexSptFolder
Copy-Item "$projReleaseFolder\aki-singleplayer.dll" -Destination $bepinexSptFolder
# If any new DLLs need to be copied, add here

Write-Host "--------------- Done Moving DLLs to $buildFolder ---------------"
Write-Host "--------------- Writing License File ---------------"

# Write the contents of the license file to a txt
Get-Content $licenseFile | Out-File "$buildFolder\LICENSE-Modules.txt"

Write-Host "--------------- Done Writing License File ---------------"
