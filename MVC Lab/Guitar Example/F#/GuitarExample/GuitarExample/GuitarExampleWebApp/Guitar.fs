namespace FsWeb.Models

open System.ComponentModel.DataAnnotations

type Guitar() = 
    [<Required>] member val Name = "" with get, set
