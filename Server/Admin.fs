module Admin

open System.IO
open Shared.ViewModels
open Security

type AdminInfo = {
    BlogTitle: string
    Name: string
    Username: string
    PasswordHash: string
    PasswordSalt: string
    Email: string
    About: string
    ProfileImageUrl: string
    Theme: string
}

/// Creates an admin user if admin data does not exist and saves to a json file
let create (info: CreateAdminReq)  = 
    let salt = createRandomKey()
    let password = utf8Bytes info.Password
    let saltyPassword = Array.concat [ salt; password ]
    let passwordHash = sha256Hash saltyPassword
    {   Name = info.Name
        BlogTitle = info.BlogTitle
        Theme = info.Theme
        Username = info.Username
        PasswordSalt = base64 salt
        PasswordHash = base64 passwordHash
        Email = info.Email
        About = info.About
        ProfileImageUrl = info.ProfileImageUrl }

let guestAdmin = 
    let info : CreateAdminReq = 
        { Name = "Cute by default"
          BlogTitle = "Blog title"
          Theme = "default"
          Username = "guest"
          Password = "guest"
          Email = "example@guest.com"
          About = "Here is where you tell a little bit about yourself, adjust it from the settings"
          ProfileImageUrl = "https://raw.githubusercontent.com/Zaid-Ajaj/tabula-rasa/57ea2879d7ec0bb8e62b64e44159a2832eccd7be/Client/public/img/default-cuteness.jpg?token=AMswmPx_1Yk_OXV7-9ird49Rje80J7Jtks5Z6dyFwA%3D%3D" }
    create info


let writeAdminIfDoesNotExists (adminInfo: AdminInfo) (writeFile: string -> string -> unit) (readFile: string -> string option) = 
    let adminPath = Environment.adminFile
    match readFile adminPath with
    | None ->
        // no file found, write data
        let data = Json.serialize adminInfo
        writeFile adminPath data 
    | Some _ ->
        // there already exists some data, don't do anything
        ()