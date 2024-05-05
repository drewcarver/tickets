module Todo

open System
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open Giraffe.EndpointRouting
open Giraffe.ViewEngine

(*
let todosList = ul [] [ li [ _class "todo__list-item" ] [ str "Clean the house" ] ]
let emptyList = ul [] [ li [] [ str "No todos found" ] ]

let searchTodosHandler: HttpHandler =
    fun _ ctx ->
        let search = ctx.GetQueryStringValue "search"

        match search with
        | Ok "todo" -> ctx.WriteHtmlViewAsync todosList
        | _ -> ctx.WriteHtmlViewAsync emptyList

let todoItem todo =
    li [ _class "todo__list-item" ] [ str todo ]

let listTodos () =
    ul [] (TodoRepo.getTodos () |> List.map (fun x -> x.Description) |> List.map todoItem)

let getTodosHandler: HttpHandler = listTodos () |> htmlView

let addTodo: HttpHandler =
    fun _ ctx ->
        let todo = ctx.GetFormValue("todo")

        match todo with
        | Some todo ->
            TodoRepo.createTodo todo
            ctx.WriteHtmlViewAsync(listTodos ())
        | None -> ctx.WriteHtmlViewAsync(listTodos ())
*)
let addTicket: HttpHandler =
    fun _ ctx ->
        task {
            let! ticket = ctx.BindFormAsync<TodoRepo.Ticket>()
            Console.WriteLine(ticket.Title)

            return! ctx.WriteHtmlViewAsync(div [] [])
        }
