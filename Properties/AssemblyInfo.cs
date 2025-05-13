using System.Reflection;
using MelonLoader;
using static _afterlifeScModMenu._globalVariables;
using static _afterlifeScModMenu.BuildInfo;

[assembly: AssemblyTitle(Description)]
[assembly: AssemblyDescription(Description)]
[assembly: AssemblyCompany(Company)]
[assembly: AssemblyProduct(Name)]
[assembly: AssemblyCopyright("Created by " + Author)]
[assembly: AssemblyTrademark(Company)]
[assembly: AssemblyVersion(Version)]
[assembly: AssemblyFileVersion(Version)]
[assembly: MelonInfo(typeof(_afterlifeMod._afterlifeMod), Name, Version, Author, DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]