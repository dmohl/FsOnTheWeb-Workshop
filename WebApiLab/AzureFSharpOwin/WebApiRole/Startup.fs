namespace WebApiRole

open Owin
open System
open System.IO
open System.Net
open System.Net.Http
open System.Web.Http

type H = StartupExtensions.OwinHandlerAsync
type RouteOptions = { id: RouteParameter }

type Startup() =
    member x.Configuration(app: IAppBuilder) =
        // Host Web API
        let config = new HttpConfiguration()
        WebApiConfig.Register config
        app.UseWebApi(config) |> ignore

        let fileServerOptions = Microsoft.Owin.StaticFiles.FileServerOptions() in
        fileServerOptions
            .WithDefaultFileNames("index.html")
            .WithPhysicalPath(AppDomain.CurrentDomain.BaseDirectory)
        |> app.UseFileServer
        |> ignore
