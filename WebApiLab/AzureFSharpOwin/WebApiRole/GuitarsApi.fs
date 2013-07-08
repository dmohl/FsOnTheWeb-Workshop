namespace WebApiRole

open System.Net.Http
open System.Web.Http

type Guitar() = 
    let mutable name = ""
    member x.Name
        with get() = name
        and set(v) = name <- v

(**
 * Define Guitar catalog operations
 *)
module Guitars =
    open System
    open System.IO

    let getGuitars() =
        if File.Exists @"c:\temp\Guitars.txt" then
            File.ReadAllText(@"c:\temp\Guitars.txt").Split(',') 
            |> Array.map (fun x -> Guitar(Name = x))
        else [||]

    let getGuitar name =
        getGuitars() |> Array.tryFind(fun g -> g.Name = name)

    let addGuitar (guitar: Guitar) =
        if not (String.IsNullOrEmpty(guitar.Name)) then
            let result = getGuitars() |> Array.fold(fun acc x -> acc + x.Name + ",") ""
            File.WriteAllText(@"c:\temp\Guitars.txt", result + guitar.Name)
            Some()
        else None

    let removeGuitar name =
        let guitars = getGuitars()
        match guitars |> Array.tryFindIndex(fun g -> g.Name = name) with
        | Some(_) ->
            let result = String.Join(",", guitars |> Array.filter (fun g -> g.Name <> name))
            File.WriteAllText(@"c:\temp\Guitars.txt", result)
            Some()
        | None -> None


(**
 * Expose APIs
 *)
module Api =
    open System
    open System.Net
    open System.Web.Http.HttpResource

    (* Guitars API *)
    let getGuitars (request: HttpRequestMessage) = async {
        return request.CreateResponse(HttpStatusCode.OK, Guitars.getGuitars())
    }

    let (|ContainsGuitar|MissingGuitar|) (content: HttpContent) =
        if content.Headers.ContentLength.HasValue && content.Headers.ContentLength.Value > 0L then
            let guitar = content.AsyncReadAs<Guitar>()
            ContainsGuitar(guitar)
        else MissingGuitar
 
    let postGuitar (request: HttpRequestMessage) = async {
        match request.Content with
        | ContainsGuitar(content) ->
            let! guitar = content
            match Guitars.addGuitar guitar with
            | Some() ->
                return request.CreateResponse(HttpStatusCode.Created, guitar)
            | None ->
                return request.CreateResponse(HttpStatusCode.InternalServerError)
        | MissingGuitar -> return request.CreateResponse(HttpStatusCode.BadRequest)
    }

    let guitarsResource = route "guitars" (get getGuitars <|> post postGuitar)


type WebApiConfig() =
    static member Register(config: HttpConfiguration) =
        config
        |> HttpResource.register [ Api.guitarsResource ]
        |> ignore

        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <-
            Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
