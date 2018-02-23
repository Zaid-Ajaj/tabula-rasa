module Shared.ViewModels

open System
open Shared.DomainModels

type BlogInfo = {
    Name: string
    ProfileImageUrl: string
    About: string
}

type LoginInfo = {
    Username: string
    Password: string
}

type Remote<'a> = 
    | Empty
    | Loading
    | LoadError of string
    | Body of 'a

type LoginResult = 
    | Success of token: string
    | UsernameDoesNotExist
    | PasswordIncorrect
    | LoginError of string

type CreateAdminReq = {
    Name: string
    Username: string
    Password: string
    Email: string
    About: string
    BlogTitle: string
    ProfileImageUrl: string
}

type CreateAdminRespose = 
    | AdminCreatedSuccesfully
    | AdminAlreadyExists
    | UnknownError
    
type BlogPostItem = {
    Id : int
    Title : string
    Slug : string
    Content : string
    Featured : bool
    DateAdded : DateTime
}

type SecureRequest<'t> = {
    Token : string
    Body : 't
}

type NewBlogPostReq = {
    Slug : string
    Title : string
    Content : string
    Tags : string list
}