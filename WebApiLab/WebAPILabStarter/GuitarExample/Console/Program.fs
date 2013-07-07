open System
open System.Net
open System.Net.Http

// Rather than using an if/then/else branch structure, let's leverage F# Active Patterns.
let (|OK|BadRequest|NotFound|Unknown|) (response: HttpResponseMessage) =
    match response.StatusCode with
    | HttpStatusCode.OK -> OK(response.Headers, response.Content)
    | HttpStatusCode.BadRequest -> BadRequest(response.Headers, response.Content)
    | HttpStatusCode.NotFound -> NotFound(response.Headers, response.Content)
    | _ -> Unknown(response.Headers, response.Content)

[<EntryPoint>]
let main argv = 
    let client = new HttpClient()
    let request = new HttpRequestMessage()

    // Set up your request
    request.RequestUri <- Uri("http://localhost:16489/guitars")

    async {
        use! response = Async.AwaitTask <| client.SendAsync(request, Async.DefaultCancellationToken)
        match response with
        | OK(_, content) -> // content removed for clarity
            let! result = content.AsyncReadAsString()
            Console.WriteLine("OK with " + result)
        | BadRequest(_, content) ->
            let! result = content.AsyncReadAsString()
            Console.WriteLine("Bad Request with " + result)
        | NotFound(_, content) ->
            let! result = content.AsyncReadAsString()
            Console.WriteLine("Not Found with " + result)
        | Unknown(_,_) -> Console.WriteLine("Unexpected result")
    } |> Async.RunSynchronously

    Console.ReadLine() |> ignore
    client.Dispose()

    0 // return an integer exit code
