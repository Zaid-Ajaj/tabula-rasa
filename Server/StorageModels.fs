module Storage.Models

open System
open Shared.DomainModels


type AppUser = {
    Id: Guid
    Username: string
    PasswordHash: string
    PasswordSalt: string
    DateAdded: DateTime
    IsAdmin: bool
}


/// Represents how the content of a blog is stored
/// It can either be just the Markdown content of the blog or it can be a markdown page on github
type ContentSource = 
    | Markdown of content : string
    | Github of pageUrl : string

/// A represtation of a blog post as a document in the database
type BlogPost = {
    Id: Guid
    Title: string
    Summary: string
    Content: ContentSource
    IsDraft: bool
    IsFeatured: bool
    Slug: string
    DateAdded: DateTime
    Comments: Comment list
    Reactions: (SocialReaction * int) list
}