# DependencyMonkey

DependencyMonkey is a resharper plugin developed to ease the time effort of updating local development packages.
It can also be used to update packages to the next major/minor/patch version.

## Project prerequisites

In order for the plugin to operate correctly, a project needs to specify the following properties in the first `ProjectGroup`

**GeneratePackageOnBuild** - When project should produce a NuGet package this tag has to be set to true. 

**Version** - The Version tag has to be set to a valid semantic version like 1.2.3.

_**AssemblyName**_ (optional) - The assembly name of the generated project. If not present we default to the project name.

_**IsPackable**_ (optional) - When missing defaults to true. When set to false we do not generate a when the project is updated.


## How To Install

### Rider

1. Download the latest version of `DependencyMonkey`.
2. Open the plugins page.
3. Click on the settings icon.
4. Select `Install Plugin from Disk...`
5. Select the downloaded `.zip`.

### VS + Resharper 

In theory it should be possible to use the plugin as a resharper extension, however it is not tested at all. Because I have no experience with resharper I cannot provide guidance. Sorry.

## First use

The first time you setup `DependencyMonkey` you have to set a path in the settings page.
Open the settings page and search for `DependencyMonkey`. It is located under the `Tools` tab.
From the directory picker select a path to your local nuget feed. It is important that the selected folder is actually a NuGet feed!

## How to use

Right click on the desired C# project file. Hover over the `DependencyMonkey` item.
From there you have 4 options.

- `Upgrade Prerelease Version` - Will perform the upgrade and increase the prerelease version. The prerelease can be changed from the settings page. 
  - 1.2.3 -> 1.2.4-beta.1
  - 1.2.4-beta.1 -> 1.2.4-beta.2
- `Upgrade Major Version` - Will perform the upgrade and set the version to the next major version. 
  - 1.2.3 -> 2.0.0
- `Upgrade Minor Version` - Will perform the upgrade and set the version to the next minor version.
  - 1.2.3 -> 1.3.0
- `Upgrade Patch Version` - Will perform the upgrade and set the version to the next patch version.
  - 1.2.3 -> 1.2.4

## Settings

The settings can be found when pressing `Alt + Ctrl + S`, or opening the settings page.
Navigate to the tools menu and look for `DependencyMonkey`. Alternatively you can search for `DependencyMonkey` directly.

**Directory** - The path where newly build nuget packages should be copied to. This directory has to be registered as a nuget feed, otherwise the packages will not be able to be resolved.

**PrereleaseTag** - The tag which should be used for local prerelease builds.

**ShowNotifications** - Whether to show additional notifications.

**VersionUpgradeStrategy** - Determines what part of the semantic version should be increased when increasing the version from a non prerelease version to a prerelease version. Example: With the default value 'Patch' the version will go from 1.2.3 -> 1.2.4-beta.1 when upgrading to the next prerelease version.

## How to increase sdk version

In `gradle.properties` set `ProductVersion` to the latest version.
In `Plugin.props` set `SdkVersion` to the latest version.

## How to increase the plugin version

In `gradle.properties` set `PluginVersion` to the next release version.
In `runVisualStudio.ps1` increase the version.

Adjust the `CHANGELOG.md`

## How to build

Run `.\gradlew.bat buildPlugin`.
The artifact is now in `build/distributions`.