module WebApp

open Shared.ViewModels
open Shared.DomainModels
open ClientServer
open Security
open Fable.Remoting.Suave


open Suave
open Suave.Successful
open Suave.Operators
open Suave.Writers
open Suave.Filters

module Async = 
    let lift (x: 'a) =
        async { return x }


/// Composition root of the application
let createUsing store = 
    let database = Storage.createDatabaseUsing store
    let readFile file = Storage.readFile file database
    let writeFile filename content = Storage.saveFile filename content database

    // create initial admin guest admin if one does not exists
    Admin.writeAdminIfDoesNotExists Admin.guestAdmin writeFile readFile
    let adminData = Admin.readAdminData readFile
    
    let login = Admin.login readFile
    let getBlogInfo() = 
        Admin.blogInfoFromAdmin adminData
        |> Async.lift

    let serverProtocol =
        {  getBlogInfo = getBlogInfo 
           login = login >> Async.lift }
    
    let clientServerProtocol = FableSuaveAdapter.webPartWithBuilderFor serverProtocol routeBuilder
   
    let webApp = 
        choose 
          [ GET >=> Files.browseHome
            clientServerProtocol ]
    webApp