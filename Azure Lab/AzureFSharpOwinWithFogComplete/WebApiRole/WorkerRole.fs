namespace WebApiRole

open System
open System.Diagnostics
open System.Net
open System.Threading
open Microsoft.Owin.Hosting
open Microsoft.WindowsAzure.ServiceRuntime

type WorkerRole() =
    inherit RoleEntryPoint() 

    // This is a sample worker implementation. Replace with your logic.
    let mutable webApp = Unchecked.defaultof<IDisposable>
    let log message (kind : string) = Trace.TraceInformation(message, kind)

    override wr.Run() =
        log "WebApiRole entry point called" "Information"
        while(true) do 
            Thread.Sleep(10000)
            log "Working" "Information"

    override wr.OnStart() = 
        // Set the maximum number of concurrent connections 
        ServicePointManager.DefaultConnectionLimit <- 12

        let endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints.["Endpoint1"]
        let baseUri = sprintf "%s://%A" endpoint.Protocol endpoint.IPEndpoint

        log ("Starting OWIN at " + baseUri) "Information"

        let options = StartOptions()
        options.Urls.Add(baseUri)
        webApp <- WebApp.Start<Startup>(options)

        base.OnStart()

    override wr.OnStop() =
        if webApp <> Unchecked.defaultof<IDisposable> then
            webApp.Dispose()
        base.OnStop()
