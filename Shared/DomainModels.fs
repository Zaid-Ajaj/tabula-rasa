module Shared.DomainModels

open System

type UserInfo = { 
    Username: string; 
    Password: string 
}

/// Github-style reactions for blog posts
type SocialReaction = 
    | Like
    | Dislike
    | Heart 
    | Laugh

/// Represents how a comment is stored in the database
type Comment = {
    Id: Guid
    User: string
    Content: string
    DateAdded: DateTime
    Likes: int
    Dislikes: int
}
