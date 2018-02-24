module Admin

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
    Bio : string
    ProfileImageUrl: string
}

/// Creates an admin user data
let create (info: CreateAdminReq)  = 
    let salt = createRandomKey()
    let password = utf8Bytes info.Password
    let saltyPassword = Array.concat [ salt; password ]
    let passwordHash = sha256Hash saltyPassword
    {   Name = info.Name
        BlogTitle = info.BlogTitle
        Username = info.Username
        PasswordSalt = base64 salt
        PasswordHash = base64 passwordHash
        Email = info.Email
        About = info.About
        Bio = info.Bio
        ProfileImageUrl = info.ProfileImageUrl }

let guestAdmin = 
    let info : CreateAdminReq = 
        { Name = "Guest Guest"
          BlogTitle = "Blog title"
          Username = "guest"
          Password = "guest"
          Email = "example@guest.com"
          About = "#About"
          Bio = "Here is where you tell a little bit about yourself, adjust it from the settings"
          ProfileImageUrl = "https://user-images.githubusercontent.com/13316248/31862023-6bb4bb10-b737-11e7-9de3-58ca0b1644c3.jpg" }
    create info


let writeAdminIfDoesNotExists (adminInfo: AdminInfo) 
                              (writeFile: string -> string -> unit) 
                              (readFile: string -> string option) = 
    let adminPath = Environment.adminFile
    match readFile adminPath with
    | None ->
        // no file found, write data
        let data = Json.serialize adminInfo
        writeFile adminPath data 
    | Some _ ->
        // there already exists some data, don't do anything
        ()

let readAdminData (readFile: string -> string option)  =
    let adminPath = Environment.adminFile
    readFile adminPath
    |> Option.map Json.deserialize<AdminInfo>
    |> function 
        | Some admin -> admin
        | None -> failwith "Could not read admin data for initial render"

let login (readFile: string -> string option) (loginInfo: LoginInfo)  = 
    let username = loginInfo.Username
    let password = loginInfo.Password
    match readFile Environment.adminFile with
    | None ->
        LoginError "Could not read admin information from data store"
    | Some adminInfoContent -> 
        let adminInfo = Json.tryDeserialize<AdminInfo> adminInfoContent
        match adminInfo with
        | None ->
            LoginError "Admin data is corrupt and is not readable as Json-formatted text"
        | Some adminInfo -> 
            if adminInfo.Username <> username then
                UsernameDoesNotExist
            else
            let salt = adminInfo.PasswordSalt
            let hash = adminInfo.PasswordHash
            let passwordDidNotMatch = verifyPassword password salt hash |> not
            if passwordDidNotMatch then 
                PasswordIncorrect
            else
            let userInfo : UserInfo = { Username = username; Claims = [| "admin" |] }
            let token = encodeJwt userInfo
            Success token

let blogInfoFromAdmin (admin: AdminInfo) : BlogInfo = 
    { Name = admin.Name; 
      Bio = admin.Bio;
      About = admin.About
      ProfileImageUrl = admin.ProfileImageUrl }