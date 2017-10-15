open System

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

open System.IO
open System.Reflection
open System.Text

open Shared.ViewModels
open Security

let (</>) x y = Path.Combine(x, y) 

let rec findRoot dir =
    if File.Exists(IO.Path.Combine(dir, "TabulaRasa.sln"))
    then dir
    else
        let parent = Directory.GetParent(dir)
        if isNull parent then
            failwith "Couldn't find package.json directory"
        findRoot parent.FullName

let login (ctx: HttpContext) =  
    let loginInfo =
        ctx.request.rawForm
        |> Encoding.UTF8.GetString
        |> Json.tryDeserialize<LoginInfo>

    let loginResult =
      match loginInfo with
      | None -> JsonFormatIncorrect
      | Some user ->
         if user.Username <> "guest" then 
            UsernameDoesNotExist
         elif user.Password <> "guest" then
            PasswordIncorrect
         else 
           let userInfo = { Username = "guest"; Claims = [| "admin" |] }
           let token = Security.encodeJwt userInfo
           Success token
    let loginResult = Json.serialize loginResult
    OK loginResult ctx


[<EntryPoint>]
let main argv =
    let cwd = Assembly.GetEntryAssembly().Location
    let root = findRoot cwd
    let client = root </> "Client" </> "src" </> "public"
    let app = 
        choose 
          [ POST >=> choose 
               [ path "/api/login" >=> login ] ]

    startWebServer defaultConfig app
    printfn "Hello %s from F#!" client
    0 // return an integer exit code
