# Modules

BepInEx plugins to alter Escape From Tarkov's behaviour

**Project**        | **Function**
------------------ | --------------------------------------------
Aki.Build          | Build script
Aki.Bundles        | External bundle loader
Aki.Common         | Common utilities used across projects
Aki.Core           | Required patches to start the game
Aki.Custom         | SPT-AKI enhancements to EFT
Aki.Debugging      | Debug utilities (disabled in release builds)
Aki.Reflection     | Reflection utilities used across the project
Aki.SinglePlayer   | Simulating online game while offline

## Privacy
SPT is an open source project. Your commit credentials as author of a commit will be visible by anyone. Please make sure you understand this before submitting a PR.
Feel free to use a "fake" username and email on your commits by using the following commands:
```bash
git config --local user.name "USERNAME"
git config --local user.email "USERNAME@SOMETHING.com"
```

## Requirements
- Escape From Tarkov 28375
- Visual Studio Code -OR- Visual Studio 2022
- .NET 6 SDK

## Project Setup
Copy-paste Live EFT's `EscapeFromTarkov_Data/Managed/` folder to into this project's `Project/Shared/Managed/` folder

## Build (VS Code)
1. File > Open Workspace > Modules.code-workspace
2. Terminal > Run Build Task...
3. Copy contents of `/Build` into SPT game folder and overwrite

## Build (VS 2022)
1. Open solution
2. Restore nuget packages
3. Run `dotnet new tool-manifest`
4. Sometimes you need to run `dotnet tool restore`
5. Run `dotnet tool install Cake.Tool`
6. Build solution
7. Copy contents of `/Build` into SPT game folder and overwrite

## Game Setup
1. Copy Live EFT files into a separate directory (from now on this will be referred to as the "SPT directory")
2. Download BepInEx 5.4.22 x64 ([BepInEx Releases - GitHub](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.22))
3. Extract contents of the BepInEx zip into the root SPT directory
4. Build Modules, Server and Launcher
5. Copy the contents of each project's `Build` folder into the root SPT directory
6. (Optional, but recommended) Download the BepInEx5 version of ConfigurationManager ([ConfigurationManager Releases - GitHub](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases)) and extract the contents of the zip into the root SPT directory
7. (Optional) Edit the BepInEx config (`\BepInEx\config\BepInEx.cfg`) and append `Debug` to the `LogLevels` setting. Example: `LogLevels = Fatal, Error, Warning, Message, Info, Debug`
