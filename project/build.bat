@echo off
:: Set some Vars to use
set buildFolder=..\Build
set bepinexFolder=..\Build\BepInEx
set bepinexPatchFolder=..\Build\BepInEx\Patchers
set bepinexPluginFolder=..\Build\BepInEx\Plugins
set bepinexSptFolder=..\Build\BepInEx\Plugins\spt
set eftDataFolder=..\Build\EscapeFromTarkov_Data
set managedFolder=..\Build\EscapeFromTarkov_Data\Managed
set projReleaseFolder=.\bin\Release\net471
set licenseFile=..\..\LICENSE.md

echo --------------- Cleaning Output Build Folder ---------------

:: Delete build folder and contents to make sure its clean
if exist %buildFolder% rmdir /s /q %buildFolder%

echo --------------- Done Cleaning Output Build Folder ---------------
echo --------------- Creating Output Build Folders ---------------

:: Create build folder if it doesn't exist
if not exist %buildFolder% mkdir %buildFolder%
if not exist %bepinexFolder% mkdir %bepinexFolder%
if not exist %bepinexPatchFolder% mkdir %bepinexPatchFolder%
if not exist %bepinexPluginFolder% mkdir %bepinexPluginFolder%
if not exist %bepinexSptFolder% mkdir %bepinexSptFolder%
if not exist %eftDataFolder% mkdir %eftDataFolder%
if not exist %managedFolder% mkdir %managedFolder%

echo --------------- Done Creating Output Build Folders ---------------

echo --------------- Moving DLLs to %buildFolder% ---------------

:: Move DLLs from each project's bin\Release folder to the build folder
xcopy "%projReleaseFolder%\Aki.Common.dll" %managedFolder%
xcopy "%projReleaseFolder%\Aki.Reflection.dll" %managedFolder%

xcopy "%projReleaseFolder%\aki_PrePatch.dll" %bepinexPatchFolder%

xcopy "%projReleaseFolder%\aki-core.dll" %bepinexSptFolder%
xcopy "%projReleaseFolder%\aki-custom.dll" %bepinexSptFolder%
xcopy "%projReleaseFolder%\aki-debugging.dll" %bepinexSptFolder%
xcopy "%projReleaseFolder%\aki-singleplayer.dll" %bepinexSptFolder%
:: If any new Dll's need to be copied, add here

echo --------------- Done Moving DLLs to %buildFolder% ---------------
echo --------------- Writing License File ---------------

:: write the contents of the license file to a txt
type %licenseFile% > "%buildFolder%\LICENSE-Modules.txt"

echo --------------- Done Writing License File ---------------