module Admin

open System.IO
open Shared.ViewModels
open Security

type AdminInfo = {
    Name: string
    Username: string
    PasswordHash: string
    PasswordSalt: string
    Email: string
    About: string
    ProfileImageUrl: string
}

/// Creates an admin user if admin data does not exist and saves to a json file
let createAdmin (info: CreateAdminReq)  = 
    let adminInfoExists = File.Exists(Environment.adminFile)
    if not adminInfoExists then AdminAlreadyExists
    else 
        let salt = createRandomKey()
        let password = utf8Bytes info.Password
        let saltyPassword = Array.concat [ salt; password ]
        let passwordHash = sha256Hash saltyPassword
        let admin = 
         {  Name = info.Name
            Username = info.Username
            PasswordSalt = base64 salt
            PasswordHash = base64 passwordHash
            Email = info.Email
            About = info.About
            ProfileImageUrl = info.ProfileImageUrl }
        Json.serialize admin
        |> fun json -> 
            try 
                File.WriteAllText(Environment.adminFile, json)
                AdminCreatedSuccesfully
            with _ -> UnknownError

let createGuestAdmin() = 
    let info : CreateAdminReq = 
        { Name = "Cute by default"
          Username = "guest"
          Password = "guest"
          Email = "example@guest.com"
          About = "Here is where you tell a little bit about yourself"
          ProfileImageUrl = "https://raw.githubusercontent.com/Zaid-Ajaj/tabula-rasa/57ea2879d7ec0bb8e62b64e44159a2832eccd7be/Client/public/img/default-cuteness.jpg?token=AMswmPx_1Yk_OXV7-9ird49Rje80J7Jtks5Z6dyFwA%3D%3D" }
    createAdmin info