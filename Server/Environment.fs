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

let adminFile = "admin.json"

let (</>) x y = Path.Combine(x, y) 

let rec findRoot dir =
    if File.Exists(System.IO.Path.Combine(dir, "paket.dependencies"))
    then dir
    else
        let parent = Directory.GetParent(dir)
        if isNull parent then
            failwith "Couldn't find root directory"
        findRoot parent.FullName

let solutionRoot = 
    let cwd = System.Reflection.Assembly.GetEntryAssembly().Location
    let root = findRoot cwd
    root
