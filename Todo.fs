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
open TodoRepo

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
let toCard (ticket: Ticket) =
    div
        [ _class "card" ]
        [ h3 [] [ str ticket.Title ]
          p [] [ str ticket.Description ]
          p [] [ str "Ready" ] ]

let listTickets: HttpHandler =
    fun _ ctx ->
        task {
            let status = ctx.GetQueryStringValue("status") |> Result.toOption |> Option.map int
            let title = ctx.GetQueryStringValue("title") |> Result.toOption

            let filter: TicketFilter = { title = title; status = status }

            let groupedTickets = getTickets filter |> List.groupBy (fun t -> t.Status)

            let getTicketCardsByStatus status =
                groupedTickets
                |> List.filter (fun t -> fst t = status)
                |> List.tryHead
                |> Option.map snd
                |> Option.map (List.map toCard)
                |> Option.defaultValue []

            let readyCards = getTicketCardsByStatus (int TicketStatus.Ready)
            let inProgressCards = getTicketCardsByStatus (int TicketStatus.InProgress)
            let testingCards = getTicketCardsByStatus (int TicketStatus.Testing)
            let doneCards = getTicketCardsByStatus (int TicketStatus.Done)

            return!
                ctx.WriteHtmlViewAsync(
                    div
                        [ _class "swimlanes" ]
                        [ div [ _class "swimlanes__lane" ] readyCards
                          div [ _class "swimlanes__lane" ] inProgressCards
                          div [ _class "swimlanes__lane" ] testingCards
                          div [ _class "swimlanes__lane" ] doneCards ]
                )
        }

let addTicket: HttpHandler =
    fun _ ctx ->
        task {
            let! ticket = ctx.BindFormAsync<TicketDTO>()

            let! _ = createTicket (ticket)

            return! ctx.WriteHtmlViewAsync(div [] [])
        }
