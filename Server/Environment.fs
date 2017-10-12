module Environment

open System.IO

/// The path of the directory that holds the data of the application such as the database file, the config files and files concerning security keys.
let dataFolder = 
    let appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
    let folder = Path.Combine(appDataFolder, "tabula-rasa")
    let directoryInfo = DirectoryInfo(folder)
    if not directoryInfo.Exists then 
        Directory.CreateDirectory folder |> ignore
    folder

/// The path of database file
let databaseFilePath = Path.Combine(dataFolder, "TabulaRasa.db")

/// The path of the file containing the security pass phrase
let securityTokenFile = Path.Combine(dataFolder, "token.txt")

let adminFile = Path.Combine(dataFolder, "admin.json")

