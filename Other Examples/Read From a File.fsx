open System.IO

let fileName = @"C:\Users\Daniel Mohl\Dropbox\Dan\Presentations\FSharp Lab\Lab Examples\FruitTestFile.txt"

File.ReadAllLines(fileName)
|> Seq.iter (fun s -> printfn "%s" s)