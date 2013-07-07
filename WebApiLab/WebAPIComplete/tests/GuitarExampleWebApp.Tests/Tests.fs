namespace GuitarExampleWebApp.Tests

open System
open System.Net
open System.Net.Http
open System.Web.Http
open NUnit.Framework

module ClientTests =

    // Rather than using an if/then/else branch structure, let's leverage F# Active Patterns.
    let (|JSON|_|) (response: HttpResponseMessage) =
        if response.StatusCode = HttpStatusCode.OK &&
           response.Content.Headers.ContentType.MediaType = "application/json" then
            let content = response.Content.AsyncReadAs<Newtonsoft.Json.Linq.JToken>()
            Some(response.Headers, content)
        else None

    let (|OK|_|) (response: HttpResponseMessage) =
        if response.StatusCode = HttpStatusCode.OK then
            Some(response.Headers, response.Content)
        else None
      
    let (|BadRequest|_|) (response: HttpResponseMessage) =
        if response.StatusCode = HttpStatusCode.BadRequest then
            Some(response.Headers, response.Content)
        else None
     
    let (|NotFound|_|) (response: HttpResponseMessage) =
        if response.StatusCode = HttpStatusCode.NotFound then
            Some(response.Headers, response.Content)
        else None


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
            | JSON(_, content) ->
                let! json = content
                Assert.That(response.StatusCode = HttpStatusCode.OK)
                // In the case above, we will retrieve a JSON array.
                Assert.IsAssignableFrom<Newtonsoft.Json.Linq.JArray>(json)
            | OK(_, content) -> // content removed for clarity
                let! result = content.AsyncReadAsString()
                Assert.That(response.StatusCode = HttpStatusCode.OK)
            | BadRequest(_, content) ->
                let! result = content.AsyncReadAsString()
                Assert.That(response.StatusCode = HttpStatusCode.BadRequest)
            | NotFound(_, content) ->
                let! result = content.AsyncReadAsString()
                Assert.That(response.StatusCode = HttpStatusCode.NotFound)
            | _ -> Assert.Fail("Received an unexpected response")
        } |> Async.RunSynchronously

        Console.ReadLine() |> ignore
        client.Dispose()
