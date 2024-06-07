# Modules

BepInEx plugins to alter Escape From Tarkov's behaviour

**Project**        | **Function**
------------------ | --------------------------------------------
SPT.Build          | Build script
SPT.Bundles        | External bundle loader
SPT.Common         | Common utilities used across projects
SPT.Core           | Required patches to start the game
SPT.Custom         | SPT enhancements to EFT
SPT.Debugging      | Debug utilities (disabled in release builds)
SPT.Reflection     | Reflection utilities used across the project
SPT.SinglePlayer   | Simulating online game while offline

## Privacy
SPT is an open source project. Your commit credentials as author of a commit will be visible by anyone. Please make sure you understand this before submitting a PR.
Feel free to use a "fake" username and email on your commits by using the following commands:
```bash
git config --local user.name "USERNAME"
git config --local user.email "USERNAME@SOMETHING.com"
```

## Requirements
- Escape From Tarkov 30473
- Visual Studio Code -OR- Visual Studio 2022
- .NET 6 SDK
- [PowerShell v7](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows)
    - Can also be installed via: `dotnet tool update --global PowerShell`

## Project Setup
Copy-paste Live EFT's `EscapeFromTarkov_Data/Managed/` folder to into this project's `Project/Shared/Managed/` folder

## Build (VS Code)
1. File > Open Workspace > Modules.code-workspace
2. Terminal > Run Build Task...
3. Copy contents of `/Build` into SPT game folder and overwrite

## Build (VS 2022)
1. Open solution
2. Restore nuget packages
3. Build solution
4. Copy contents of `/Build` into SPT game folder and overwrite

## Game Setup
1. Copy Live EFT files into a separate directory (from now on this will be referred to as the "SPT directory")
2. Download BepInEx 5.4.23.1 x64 ([BepInEx Releases - GitHub](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.1))
3. Extract contents of the BepInEx zip into the root SPT directory
4. Build Modules, Server and Launcher
5. Copy the contents of each project's `Build` folder into the root SPT directory
6. (Optional, but recommended) Download the BepInEx5 version of ConfigurationManager ([ConfigurationManager Releases - GitHub](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases)) and extract the contents of the zip into the root SPT directory. The default keybind for opening the menu will be `F1`
7. (Optional) Edit the BepInEx config (`\BepInEx\config\BepInEx.cfg`) and append `Debug` to the `LogLevels` setting. Example: `LogLevels = Fatal, Error, Warning, Message, Info, Debug`
