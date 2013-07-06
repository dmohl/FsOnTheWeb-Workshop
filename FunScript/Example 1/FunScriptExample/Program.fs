[<FunScript.JS>] // Placing this attribute on the module allows all functions to be translated to JS. It's just an alias for the ReflectedDefinition F# attribute.
module Page

open System.IO
open System.Reflection
open Microsoft.FSharp.Quotations
open FunScript
open FunScript.TypeScript

type j = Api<"../../Typings/jquery.d.ts"> // Import the TypeScript definition file for jQuery
type lib = Api<"../../Typings/lib.d.ts"> // Import the TypeScript definition file for JavaScript DOM 

// Define a function that will pop an alert that says Hello World
let hello () = 
    lib.window.alert("Hello world!") 

// This will generate something like: $("#helloWorld").click(function() { window.alert("Hello world!") });
let main() = 
    j.jQuery.Invoke("#helloWorld").click(hello) 

// Compile to JavaScript
let additionalComponents = FunScript.Interop.Components.all
let source = Compiler.Compiler.Compile(<@@ main() @@>, 
                 components=additionalComponents, noReturn=true)

let sourceWrapped = sprintf "$(function () {\n%s\n});" source // Wrap the generated JS in jQuery.Ready.
let filename = "tutorial.js" // Specify the desired name of the JS file that is generated
System.IO.File.Delete filename // Remove the file if it exists.
System.IO.File.WriteAllText(filename, sourceWrapped) // Write the file to disk
sourceWrapped |> printfn "%A" // Print the resulting JS
System.Console.ReadLine() |> ignore

