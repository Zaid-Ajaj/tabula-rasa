module Json

open Newtonsoft.Json

let private fableConverter = Fable.JsonConverter() :> JsonConverter

let serialize value =
    JsonConvert.SerializeObject(value, [| fableConverter |])

let deserialize<'a> (json:string) : 'a =
    JsonConvert.DeserializeObject<'a>(json, [| fableConverter |])

let tryDeserialize<'a> (json: string) = 
    try deserialize<'a> json |> Some
    with | _ -> None