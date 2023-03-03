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

## Requirements

- Escape From Tarkov 22032
- BepInEx 5.4.19
- Visual Studio Code
- .NET 6 SDK

## Setup

Copy-paste Live EFT's `EscapeFromTarkov_Data/Managed/` folder to into Modules' `Project/Shared/` folder

## Build

1. File > Open Workspace > Modules.code-workspace
2. Terminal > Run Build Task...
3. Copy-paste content inside `Build` into `%gamedir%`, overwrite when prompted.
