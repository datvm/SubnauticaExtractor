This repository is for a tool I created that extracts and show-cases information from the game Subnautica. A web version is hosted here: [https://subnautica.lukevo.com/](https://subnautica.lukevo.com/).

# Data Dump

I think it is helpful for wiki editor to get the data you need. Currently I am only extracting data from `CraftData` class, but I will add more in the future. You can download it in [DataDump folder](https://github.com/datvm/SubnauticaExtractor/tree/master/DataDump). You can either use the JSON files and the SQLite database, whichever is better for you.

# Extraction Script

This is the script I used to extract the data. You will need .NET Framework 4.7.2.

**IMPORTANT:** The script itself does not prompt any input upon running, so you need to configure it with the `options.json` file before running:

```
{
    "GameFolder": "F:\\Game\\SteamLibrary\\steamapps\\common\\Subnautica",
    "OutputFolder": "D:\\Temp\\Subnautica\\RawData",
    "ExportToDatabase": true,
    "ExportToFiles": true,
    "AddTildeForUnusedNames": true,
    "ExportUnusedData": true
}
```

*GameFolder*: the folder of the game. The script will look for `Subnautica_Data\Managed\Assembly-CSharp.dll` file in that folder.

*OutputFolder*: the folder to output the data files.

*ExportToDatabase*: export to a SQLite database.

*ExportToFiles*: export to JSON files.

*ExportUnusedData*: export **all** data, regardless of my script using it or not.

*AddTildeForUnusedNames*: if you set `ExportUnusedData` to true, when exporting such data, the script will add a tilde (~) at the beginning of the file/table name so when you sort, important names are together.

# Material Calculator and Information Website

I also have an ASP.NET Core 3.0 website that uses the JSON files as data to showcase what the scripts do. It has a Material Calculator and a "wiki-styled" page (although I do not think it's very useful with current information). Feel free to use it however you see fit.
