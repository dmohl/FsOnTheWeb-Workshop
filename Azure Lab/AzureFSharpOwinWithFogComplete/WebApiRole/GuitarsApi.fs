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
    open Fog.Storage.Blob

    let containerName = "guitars"
    let blobName = "guitarsblob"
    let blob = GetBlobReference containerName blobName
    try
        blob.FetchAttributes()
    with _ -> UploadBlob containerName blobName "" |> ignore

    let getGuitars() =
        let guitars = DownloadBlob<string> containerName blobName
        if not (String.IsNullOrEmpty guitars) then
            guitars.Split(',') |> Array.map (fun x -> Guitar(Name = x))
        else [||]

    let getGuitar name =
        getGuitars() |> Array.tryFind(fun g -> (g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))

    let addGuitar (guitar: Guitar) =
        if not (String.IsNullOrEmpty(guitar.Name)) then
            let result = getGuitars() |> Array.fold(fun acc x -> acc + "," + x.Name) guitar.Name
            UploadBlob containerName blobName result |> ignore
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
            String.Join(",", data) |> UploadBlob containerName blobName |> ignore
            Some()
        | None -> None


(**
 * Expose APIs
 *)
module Api =
    open System
    open System.Net
    open System.Web.Http.HttpResource

    type ApiGuitar() =
        let mutable link = Unchecked.defaultof<Uri>
        let mutable name = ""
        member x.Link
            with get() = link
            and set(v) = link <- v
        member x.Name
            with get() = name
            and set(v) = name <- v

    let guitarsPath = "guitars"
    let guitarPath = "guitar"

    let getGuitarUri (request: HttpRequestMessage) (guitar: Guitar) =
        Uri(request.RequestUri, guitarPath + "/" + guitar.Name)

    let toApiGuitar request (guitar: Guitar) =
        ApiGuitar(Name = guitar.Name, Link = getGuitarUri request guitar)


    (* Guitars API *)
    let getGuitars (request: HttpRequestMessage) = async {
        let guitars =
            Guitars.getGuitars()
            |> Array.map (toApiGuitar request)
        return request.CreateResponse(HttpStatusCode.OK, guitars)
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
                response.Headers.Location <- getGuitarUri request guitar
                return response
            | None ->
                return request.CreateResponse(HttpStatusCode.InternalServerError)
        | MissingGuitar -> return request.CreateResponse(HttpStatusCode.BadRequest)
    }

    let guitarsResource = route guitarsPath (get getGuitars <|> post postGuitar)


    (* Guitar API *)
    let (|Guitar|NotFound|) request =
        let result =
            getParam<string> request "name"
            |> Option.bind (fun name ->
                let name = System.Uri.UnescapeDataString name
                Guitars.getGuitar name
                |> Option.map (fun guitar ->
                    (name, guitar)))
        match result with
        | Some value -> Guitar value
        | None -> NotFound

    let getGuitar (request: HttpRequestMessage) = async {
        match request with
        | Guitar(_, guitar) ->
            return request.CreateResponse(HttpStatusCode.OK, guitar |> toApiGuitar request)
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

    let guitarResource = routeResource (guitarPath+"/{name}") [ get getGuitar; delete deleteGuitar ]


type WebApiConfig() =
    static member Register(config: HttpConfiguration) =
        config
        |> HttpResource.register [ Api.guitarsResource; Api.guitarResource ]
        |> ignore

        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <-
            Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
