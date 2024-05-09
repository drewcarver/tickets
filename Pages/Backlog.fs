module Pages.Backlog

open Giraffe.ViewEngine

let backlog = 
  h1 [] [str "Backlog"] |> MainPage.mainPage