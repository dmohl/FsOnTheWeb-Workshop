﻿namespace FsWeb

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

    let dataSource = @"c:\temp\Guitars.txt"

    let getGuitars() =
        if File.Exists dataSource then
            File.ReadAllText(dataSource).Split(',') 
            |> Array.map (fun x -> Guitar(Name = x))
        else [||]

    let getGuitar name =
        getGuitars() |> Array.tryFind(fun g -> (g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))

    let addGuitar (guitar: Guitar) =
        if not (String.IsNullOrEmpty(guitar.Name)) then
            let result = getGuitars() |> Array.fold(fun acc x -> acc + x.Name + ",") ""
            File.WriteAllText(dataSource, result + guitar.Name)
            Some()
        else None

    let removeGuitar name =
        let guitars = getGuitars()
        match guitars |> Array.tryFindIndex(fun g -> g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) with
        | Some(_) ->
            let data =
                guitars
                |> Array.filter (fun g -> not <| g.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                |> Array.map (fun g -> g.Name)
            let result = String.Join(",", data)
            File.WriteAllText(dataSource, result)
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
                let response = request.CreateResponse(HttpStatusCode.Created, guitar)
                response.Headers.Location <- Uri(request.RequestUri, "guitar/" + guitar.Name)
                return response
            | None ->
                return request.CreateResponse(HttpStatusCode.InternalServerError)
        | MissingGuitar -> return request.CreateResponse(HttpStatusCode.BadRequest)
    }

    let guitarsResource = route "guitars" (get getGuitars <|> post postGuitar)


    (* Guitar API *)
    let (|Guitar|NotFound|) request =
        let result =
            getParam<string> request "name"
            |> Option.bind (fun name ->
                Guitars.getGuitar name
                |> Option.map (fun guitar ->
                    (name, guitar)))
        match result with
        | Some value -> Guitar value
        | None -> NotFound

    let getGuitar (request: HttpRequestMessage) = async {
        match request with
        | Guitar(_, guitar) ->
            return request.CreateResponse(HttpStatusCode.OK, guitar)
        | NotFound ->
            return request.CreateResponse(HttpStatusCode.NotFound)
    }

    let deleteGuitar (request: HttpRequestMessage) = async {
        match request with
        | Guitar(name, _) ->
            match Guitars.removeGuitar name with
            | Some() -> return request.CreateResponse(HttpStatusCode.NoContent)
            | None -> return request.CreateResponse(HttpStatusCode.InternalServerError)
        | NotFound -> return request.CreateResponse(HttpStatusCode.NotFound)
    }

    let guitarResource = routeResource "guitar/{name}" [ get getGuitar; delete deleteGuitar ]


type WebApiConfig() =
    static member Register(config: HttpConfiguration) =
        config
        |> HttpResource.register [ Api.guitarsResource; Api.guitarResource ]
        |> ignore

        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <-
            Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

(**
 * Run the app in ASP.NET
 *)
type Global() =
    inherit System.Web.HttpApplication() 
    member this.Start() =
        WebApiConfig.Register GlobalConfiguration.Configuration
