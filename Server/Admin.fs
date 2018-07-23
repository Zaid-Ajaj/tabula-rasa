module Admin

open System
open Shared
open StorageTypes
open Security
open Serilog
open LiteDB
open LiteDB.FSharp.Extensions

/// Creates an admin user data
let create (info: CreateAdminReq)  = 
    let salt = createRandomKey()
    let password = utf8Bytes info.Password
    let saltyPassword = Array.concat [ salt; password ]
    let passwordHash = sha256Hash saltyPassword
    {   Id = 0
        Name = info.Name
        BlogTitle = info.BlogTitle
        Username = info.Username
        PasswordSalt = base64 salt
        PasswordHash = base64 passwordHash
        Email = info.Email
        About = info.About
        Bio = info.Bio
        ProfileImageUrl = info.ProfileImageUrl }

/// Default admin if there isn't one
let guestAdmin : AdminInfo = 
    let info : CreateAdminReq = 
        { Name = "Guest Guest"
          BlogTitle = "Blog title"
          Username = "guest"
          Password = "guest"
          Email = "example@guest.com"
          About = "# About"
          Bio = "Here is where you tell a little bit about yourself, adjust it from the settings"
          ProfileImageUrl = "https://user-images.githubusercontent.com/13316248/31862023-6bb4bb10-b737-11e7-9de3-58ca0b1644c3.jpg" }
    create info

/// If no admin exists in the database, then 
let writeAdminIfDoesNotExists (db: LiteDatabase) (adminInfo: AdminInfo)  =     
    let admins = db.GetCollection<AdminInfo> "admins"
    if admins.Count() = 0 then ignore (admins.Insert adminInfo)

/// Reads admin data from the database. There must be atleast one admn    
let readAdminData (db: LiteDatabase)  =
    let admins = db.GetCollection<AdminInfo> "admins"
    admins.FindAll() 
    |> Seq.tryHead
    |> function 
        | Some admin -> admin
        | None -> failwith "Expected at least one admin to be present in the database"

let login (logger: ILogger) (db: LiteDatabase) (loginInfo: LoginInfo)  = 
    let admins = db.GetCollection<AdminInfo> "admins"
    let username = loginInfo.Username
    let password = loginInfo.Password
    logger.Information("Logging in user {Username}", username)
    match admins.tryFindOne <@ fun admin -> admin.Username = username @> with 
    | None -> UsernameDoesNotExist
    | Some user -> 
        let salt = user.PasswordSalt
        let hash = user.PasswordHash
        let passwordDidNotMatch = Security.verifyPassword password salt hash |> not
        if passwordDidNotMatch then  PasswordIncorrect
        else
          let userInfo = { Username = username; Claims = [| "admin" |] }
          let token = encodeJwt userInfo
          Success token

let updatePassword (logger: ILogger) (db: LiteDatabase) (updateInfo: SecureRequest<UpdatePasswordInfo>) = 
    match Security.validateJwt updateInfo.Token with 
    | None -> 
        logger.Information("Invalid security token when updating password")
        Error "User unauthorized: invalid JSON web token."
    | Some user when not (Array.contains "admin" user.Claims) -> 
        logger.Information("Non-Admin User {Username} is updating the password", user.Username)
        Error "User unauthorized: must be an admin"
    | Some userInfo -> 
        let admins = db.GetCollection<AdminInfo> "admins"
        match admins.tryFindOne <@ fun admin -> admin.Username = updateInfo.Body.Username @> with
        | None -> 
            logger.Information("Could not find admin {Username} in the database", userInfo.Username)
            Error (sprintf "User '%s' was not found" updateInfo.Body.Username)
        | Some admin -> 
            let salt = admin.PasswordSalt
            let hash = admin.PasswordHash
            let passwordDidMatch = Security.verifyPassword updateInfo.Body.CurrentPassword salt hash 
            if not passwordDidMatch 
            then Error "The input for your current password is incorrect"
            else let salt = createRandomKey()
                 let password = utf8Bytes updateInfo.Body.NewPassword
                 let saltyPassword = Array.concat [ salt; password ]
                 let passwordHash = sha256Hash saltyPassword
                 let modifiedAdmin = 
                    { admin with 
                        PasswordSalt = base64 salt
                        PasswordHash = base64 passwordHash  }
                 if admins.Update(modifiedAdmin) then Ok "Password succesfully updated"
                 else 
                   logger.Error("Database error while trying to update admin password {Data}", updateInfo)  
                   Error "Internal error while updating the admin password"
             
let validateBlogInfo (blogInfo: BlogInfo) = 
    if String.IsNullOrWhiteSpace blogInfo.BlogTitle 
    then Some "Blog Title cannot be empty"
    elif String.IsNullOrWhiteSpace blogInfo.Name 
    then Some "The name of the blog cannot be empty"
    else None 
    
let updateBlogInfo (db: LiteDatabase) (req: SecureRequest<BlogInfo>) = 
    match Security.validateJwt req.Token with 
    | None -> Error (ErrorMsg "User is unauthorized")
    | Some user when not (Array.contains "admin" user.Claims) -> Error (ErrorMsg "User must be an admin")
    | Some user ->
        let admins = db.GetCollection<AdminInfo> "admins"
        match admins.tryFindOne <@ fun admin -> admin.Username = user.Username  @> with 
        | None -> Error (ErrorMsg "Admin was not found")
        | Some foundAdmin -> 
            let blogInfo = req.Body
            match validateBlogInfo blogInfo with 
            | Some errorMsg -> Error (ErrorMsg errorMsg)
            | None ->
                let modifiedAdminInfo = 
                    { foundAdmin with 
                        Name = blogInfo.Name
                        Bio = blogInfo.Bio
                        About = blogInfo.About
                        ProfileImageUrl = blogInfo.ProfileImageUrl
                        BlogTitle = blogInfo.BlogTitle  } 
    
                if admins.Update(modifiedAdminInfo) 
                then Ok (SuccessMsg "Updated succesfully")
                else Error (ErrorMsg "Database error while updaing admin blog info") 

let blogInfoFromAdmin (admin: AdminInfo) : BlogInfo = 
    { Name = admin.Name; 
      Bio = admin.Bio;
      About = admin.About
      ProfileImageUrl = admin.ProfileImageUrl
      BlogTitle = admin.BlogTitle }

let blogInfo (db: LiteDatabase) : Result<BlogInfo, string> = 
    db.GetCollection<AdminInfo> "admins"
    |> fun admins -> admins.FindAll()
    |> Seq.tryHead
    |> Option.map blogInfoFromAdmin
    |> function 
        | Some blogInfo -> Ok blogInfo 
        | None -> Error "Fatal error: could not find blog info from an admin. Database must be corrupt"