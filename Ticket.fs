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
open HtmlHelpers


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

            return! ctx.WriteHtmlViewAsync updateDialogDiv
        }

let editTicket: HttpHandler =
    fun _ ctx ->
        task {
            let! ticket = ctx.BindFormAsync<TicketDTO>()

            let! _ = updateTicket (ticket) ticket.ticketId

            return! ctx.WriteHtmlViewAsync updateDialogDiv
        }

let showEditTicketDialog ticketId : HttpHandler =
    let ticket = getTicket ticketId

    htmlView (editTicketForm ticket |> toDialog "Edit Ticket")

let showAddTicketDialog: HttpHandler =
    htmlView (addTicketForm |> toDialog "Add Ticket")
