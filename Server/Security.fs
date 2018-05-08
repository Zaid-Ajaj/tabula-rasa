/// Proviedes functions for encoding and decoding Json web tokens
module Security

open System
open System.IO
open System.Text
open Newtonsoft.Json
open System.Security.Cryptography

open Shared

type UserInfo = {
    Username: string
    Claims: string[]
}

//  Learn about JWT https://jwt.io/introduction/
//  This module uses the JOSE-JWT library https://github.com/dvsekhvalnov/jose-jwt

let createRandomKey() = 
    let generator = System.Security.Cryptography.RandomNumberGenerator.Create()
    let randomNumber = Array.init 32 byte
    generator.GetBytes(randomNumber)
    randomNumber

let private passPhrase =
    let securityTokenFile = FileInfo(Environment.securityTokenFile)
    if not securityTokenFile.Exists then
        let passPhrase = createRandomKey()
        File.WriteAllBytes(securityTokenFile.FullName, passPhrase)
    File.ReadAllBytes(securityTokenFile.FullName)

let private encodeString (payload:string) =
    Jose.JWT.Encode(payload, passPhrase, Jose.JweAlgorithm.A256KW, Jose.JweEncryption.A256CBC_HS512)

let private decodeString (jwt: string) =
    Jose.JWT.Decode(jwt, passPhrase, Jose.JweAlgorithm.A256KW, Jose.JweEncryption.A256CBC_HS512)

/// Encodes an object as a JSON web token.
let encodeJwt token =
    JsonConvert.SerializeObject token
    |> encodeString

/// Decodes a JSON web token to an object.
let decodeJwt<'a> (jwt: string) : 'a =
    decodeString jwt
    |> JsonConvert.DeserializeObject<'a>

/// Returns true if the JSON Web Token is successfully decoded and the signature is verified.
let validateJwt (jwt:string) : UserInfo option =
    try
        let token = decodeJwt jwt
        Some token
    with
    | _ -> None

let utf8Bytes (input: string) = Encoding.UTF8.GetBytes(input)
let base64 (input: byte[]) = Convert.ToBase64String(input)
let sha256 = SHA256.Create()
let sha256Hash (input: byte[]) : byte[] = sha256.ComputeHash(input)
let verifyPassword password saltBase64 hashBase64 = 
    let salt = Convert.FromBase64String(saltBase64)
    Array.concat [ salt; utf8Bytes password ]
    |> sha256Hash
    |> base64
    |> (=) hashBase64 