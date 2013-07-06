// This is a slightly modified version of a script written by Don Syme and published in the 13th chapter of the book Expert F#
open System.Net
open System.IO
open Microsoft.FSharp.Control

let museums = ["MOMA", "http://moma.org/"
               "British Museum", "http://www.thebritishmuseum.ac.uk/"
               "Museum of Fine Arts", "http://www.mfs.org"]

let printWithThread format = 
    printf "[.NET Thread %d]" 
        System.Threading.Thread.CurrentThread.ManagedThreadId
    printfn format

let fetchAsync(name, url:string) =
    async { do printWithThread "Creating request for %s..." name
            let req = WebRequest.Create(url)
            let! resp = req.AsyncGetResponse()
            do printWithThread "Getting response stream for %s..." name
            let stream = resp.GetResponseStream()
            do printWithThread "Reading response for %s..." name
            let reader = new StreamReader(stream)
            let html = reader.ReadToEnd()
            do printWithThread "Read %d characters for %s..." 
                html.Length name }

for name, url in museums do
    Async.Start (fetchAsync(name,url))
