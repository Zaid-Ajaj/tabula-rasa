module WebApp

open Shared.ViewModels
open Shared.DomainModels
open ClientServer
open Environment
open Admin
open Security

module Async = 
    let lift (x: 'a) =
        async { return x }

type FilePath = string
type FileReader = FilePath -> string option

let adminLogin (readFile: FileReader) (loginInfo: LoginInfo)  = 
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
            let passwordDidNotMatch = Security.verifyPassword password salt hash |> not
            if passwordDidNotMatch then 
                PasswordIncorrect
            else
            let userInfo = { Username = username; Claims = [| "admin" |] }
            let token = Security.encodeJwt userInfo
            Success token


let createUsing store = 
    let database = Storage.createDatabaseUsing store
    let readFile file = Storage.readFile file database
    let writeFile filename content = Storage.saveFile filename content database

    // create initial admin guest admin if one does not exists
    Admin.writeAdminIfDoesNotExists Admin.guestAdmin writeFile readFile

    let login = adminLogin readFile
    let serverProtocol =
        { login = adminLogin readFile >> Async.lift }
    serverProtocol