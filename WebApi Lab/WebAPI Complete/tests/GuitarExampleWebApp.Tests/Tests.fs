namespace GuitarExampleWebApp.Tests

open System
open System.Net
open System.Net.Http
open System.Web.Http
open NUnit.Framework

module ClientTests =

    // Rather than using an if/then/else branch structure, let's leverage F# Active Patterns.
    let (|OK|BadRequest|NotFound|Unknown|) (response: HttpResponseMessage) =
        match response.StatusCode with
        | HttpStatusCode.OK -> OK(response.Headers, response.Content)
        | HttpStatusCode.BadRequest -> BadRequest(response.Headers, response.Content)
        | HttpStatusCode.NotFound -> NotFound(response.Headers, response.Content)
        | _ -> Unknown(response.Headers, response.Content)

    [<Test>]
    let ``Test client can use active patterns``() = 
        let config = new HttpConfiguration()
        let server = new HttpServer(config |> HttpResource.register [FsWeb.Api.guitarsResource])
        let client = new HttpClient(server)

        // Set up your request
        let request = new HttpRequestMessage()
        request.RequestUri <- Uri("http://localhost:16489/guitars")

        async {
            use! response = Async.AwaitTask <| client.SendAsync(request, Async.DefaultCancellationToken)
            match response with
            | OK(_, content) -> // content removed for clarity
                let! result = content.AsyncReadAsString()
                Assert.That(response.StatusCode = HttpStatusCode.OK)
            | BadRequest(_, content) ->
                let! result = content.AsyncReadAsString()
                Assert.That(response.StatusCode = HttpStatusCode.BadRequest)
            | NotFound(_, content) ->
                let! result = content.AsyncReadAsString()
                Assert.That(response.StatusCode = HttpStatusCode.NotFound)
            | Unknown(_,_) -> Assert.Fail("Received an unexpected response")
        } |> Async.RunSynchronously

        Console.ReadLine() |> ignore
        client.Dispose()
