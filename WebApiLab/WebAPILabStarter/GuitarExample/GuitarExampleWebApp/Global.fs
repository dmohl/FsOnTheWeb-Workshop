namespace FsWeb

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
(* REPLACE with code from Step 1 of the "Add a Get Method" instructions *)
module Api =
    ()
(* END OF REPLACE with code from Step 1 of the "Add a Get Method" instructions *)

type WebApiConfig() =
    static member Register(config: HttpConfiguration) =
        config
        |> HttpResource.register [ (* REPLACE with code from Step 3 of the "Add a Get Method" instructions *) ]
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
