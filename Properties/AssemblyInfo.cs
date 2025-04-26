using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(_afterlifeMod.BuildInfo.Description)]
[assembly: AssemblyDescription(_afterlifeMod.BuildInfo.Description)]
[assembly: AssemblyCompany(_afterlifeMod.BuildInfo.Company)]
[assembly: AssemblyProduct(_afterlifeMod.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + _afterlifeMod.BuildInfo.Author)]
[assembly: AssemblyTrademark(_afterlifeMod.BuildInfo.Company)]
[assembly: AssemblyVersion(_afterlifeMod.BuildInfo.Version)]
[assembly: AssemblyFileVersion(_afterlifeMod.BuildInfo.Version)]
[assembly: MelonInfo(typeof(_afterlifeMod._afterlifeMod), _afterlifeMod.BuildInfo.Name, _afterlifeMod.BuildInfo.Version, _afterlifeMod.BuildInfo.Author, _afterlifeMod.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]