module Shared.ViewModels

open System
open Shared.DomainModels

type BlogPostItem = {
    Id: int
    Title: string
    Summary: string
    IsFeatured: bool
    Slug: string
    DateAdded: DateTime
    CommentCount: int
    Reactions: (SocialReaction * int) list
}

type BlogPost = {
    Id: int
    Title: string
    Content: string
    IsFeatured: bool
    Slug: string
    DateAdded: DateTime
    Comments: Comment list
    Reactions: (SocialReaction * int) list
}

type LoginInfo = {
    Username: string
    Password: string
}

type CreateAdminReq = {
    Name: string
    Username: string
    Password: string
    Email: string
    About: string
    ProfileImageUrl: string
}

type CreateAdminRespose = 
    | AdminCreatedSuccesfully
    | AdminAlreadyExists
    | UnknownError