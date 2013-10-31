[<FunScript.JS>] // Placing this attribute on the module allows all functions to be translated to JS. It's just an alias for the ReflectedDefinition F# attribute.
module Page

open System.IO
open System.Reflection
open Microsoft.FSharp.Quotations
open FunScript
open FunScript.TypeScript

type j = Api<"../../Typings/jquery.d.ts"> // Import the TypeScript definition file for jQuery
type jui = Api<"../../Typings/jqueryui.d.ts"> // Import the TypeScript definition file for jQuery UI
type lib = Api<"../../Typings/lib.d.ts"> // Import the TypeScript definition file for JavaScript DOM 

// Define a function that will pop an alert that says Hello World
let hello () = 
    lib.window.alert("Hello world!") 

// This will generate something like: $("#helloWorld").click(function() { window.alert("Hello world!") });
let main() = 
    j.jQuery.Invoke("#helloWorld").click(hello) 

do FunScriptExample.Runtime.Run(components=Interop.Components.all)

