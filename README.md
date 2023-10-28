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

- Escape From Tarkov 26835
- BepInEx 5.4.22
- Visual Studio Code
- .NET 6 SDK

## Setup

Copy-paste Live EFT's `EscapeFromTarkov_Data/Managed/` folder to into Modules' `Project/Shared/` folder

## Build (vscode)
1. File > Open Workspace > Modules.code-workspace
2. Terminal > Run Build Task...
3. Copy contents of `/Build` into SPT game folder and overwrite

## Build (VS)
1. Open solution
2. Restore nuget packages
3. Run `dotnet new tool-manifest`
4. Sometimes you need to run `dotnet tool restore`
5. Run `dotnet tool install Cake.Tool`
6. Build solution
7. Copy contents of `/Build` into SPT game folder and overwrite
