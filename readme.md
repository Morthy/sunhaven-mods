# Sun Haven mods

These are all mods I have created for Sun Haven. Some are publicly available on Nexus Mods, some are just for myself, friends or testing.

## Compiling

1. Clone the repo
2. Make folder called "Managed" and copy all of the DLLs from the Sun Haven "Managed" folder here
3. Install the bepinex assembly publicizer by running in terminal: `dotnet tool install -g BepInEx.AssemblyPublicizer.Cli`
4. Run these commands

```
assembly-publicizer -f .\Managed\PSS.Database.dll
assembly-publicizer -f .\Managed\SunHaven.Core.dll
```
5. Create a file called `private.targets` that looks like this. Change the GamePath to your Sun Haven directory.

```
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
    <GamePath>C:\Program Files (x86)\Steam\steamapps\common\Sun Haven</GamePath>
    </PropertyGroup>
</Project>
```

## Handling game updates

If the game updates, you need to repeat steps 2 and 4.

## Code info

I have no professional experience in C# and never really used before getting started modding Sun Haven.
Please don't use my code as an example of clean, good, or quality programming. It's purely here as reference.

Pull requests are welcome :)

## Support

If you need help, feel free to post on the Nexus mods discussion/issue tracker for the mod page.

Alternatively for prompter help (or if you feel like you might need to send your safe file), feel free to join my discord:

https://discord.gg/NCdDfYPPyy

This is a multi purpose discord, please use #sunhaven-mods for anything related to modding!
