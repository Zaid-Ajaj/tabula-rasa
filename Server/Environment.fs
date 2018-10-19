module Environment

open System.IO

let (</>) x y = Path.Combine(x, y)

/// The path of the directory that holds the data of the application such as the database file, the config files and files concerning security keys.
let dataFolder =
    let appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
    let folder = appDataFolder </> "tabula-rasa"
    let directoryInfo = DirectoryInfo(folder)
    if not directoryInfo.Exists then Directory.CreateDirectory folder |> ignore
    printfn "Using data folder: %s" folder
    folder

/// The path of database file
let databaseFilePath = dataFolder </> "TabulaRasa.db"

/// The path of the file containing the security pass phrase
let securityTokenFile = dataFolder </> "token.txt"

let rec findRoot dir =
    let paketDeps = dir </> "paket.dependencies"
    if File.Exists paketDeps then dir
    else 
        let parent = Directory.GetParent(dir)
        if isNull parent then failwith "Couldn't find root directory"
        findRoot parent.FullName

let solutionRoot =
    let cwd = System.Reflection.Assembly.GetEntryAssembly().Location
    let root = findRoot cwd
    root
