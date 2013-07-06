namespace FsWeb.Controllers

open System
open System.Web.Mvc
open FsWeb.Models

[<HandleError>]
type GuitarsController() =    
    inherit Controller()    

    let guitars = match System.IO.File.Exists @"c:\temp\Guitars.txt" with
                  | true -> 
                      System.IO.File.ReadAllText(@"c:\temp\Guitars.txt").Split(',') 
                      |> Array.map (fun x -> Guitar(Name = x))
                  | _ -> [||]

    member this.Index () = guitars |> this.View 

    [<HttpGet>] 
    member this.Create () = this.View() 
    
    [<HttpPost>] 
    member this.Create (guitar : Guitar) : ActionResult =
        let isNameValid = not (String.IsNullOrEmpty(guitar.Name))

        match base.ModelState.IsValid, isNameValid with
        | false, false | true, false | false, true -> 
            upcast this.View guitar
        | _ -> 
            let result = guitars |> Array.fold(fun acc x -> acc + x.Name + ",") ""
            System.IO.File.WriteAllText(@"c:\temp\Guitars.txt", result + guitar.Name)
            upcast base.RedirectToAction("Index") 
