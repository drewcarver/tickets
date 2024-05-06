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


let updateDialogDiv =
    div
        [ _id "dialog-anchor"
          attr "hx-trigger" "load"
          attr "hx-get" "/tickets"
          attr "hx-target" ".swimlanes"
          attr "hx-swap" "outerHTML" ]
        []

let ticketStatusMap =
    [ "Ready", int TicketStatus.Ready
      "In Progress", int TicketStatus.InProgress
      "Testing", int TicketStatus.Testing
      "Done", int TicketStatus.Done ]

let toDialog title innerContent =
    dialog
        [ _id "ticket-dialog"
          _class "modal"
          attr "hx-on::load" "document.getElementById('ticket-dialog').showModal()" ]
        [ header
              [ _class "modal__header" ]
              [ h1 [] [ str title ]
                button
                    [ _class "btn"
                      attr "aria-label" "Close"
                      _onclick "document.getElementById('ticket-dialog').close()" ]
                    [ i [ _class "fa fa-times fa-2x" ] [] ] ]
          innerContent
          div
              [ _id "save-ticket-spinner"; _class "htmx-indicator" ]
              [ div [ _class "lds-ring" ] [ div [] []; div [] []; div [] []; div [] [] ]
                p [] [ str "Saving changes..." ] ] ]


let toCard (ticket: Ticket) =
    div
        [ _class "card"
          attr "hx-get" $"/show-edit-dialog/{ticket.TicketId}"
          attr "hx-trigger" "click"
          attr "hx-target" "#dialog-anchor" ]
        [ h3 [] [ str ticket.Title ]
          p [] [ str ticket.Description ]
          p
              []
              [ str (
                    ticketStatusMap
                    |> List.filter (fun s -> snd s = ticket.Status)
                    |> List.head
                    |> fst
                ) ] ]

let toSelect attrs values (selectedValue: string option) =
    let baseAttrs value = [ _value value ]
    let selectedAttrs value = _selected :: baseAttrs value

    let options =
        values
        |> List.map (fun ((key, value)) ->
            option
                (if selectedValue.IsSome && selectedValue.Value = value then
                     selectedAttrs value
                 else
                     baseAttrs value)
                [ str key ])

    select attrs options

let statusKeyValuePairs =
    ticketStatusMap |> List.map (fun (name, value) -> (name, value.ToString()))

let addTicketForm =
    form
        [ attr "hx-post" "/tickets"
          _class "ticket-form"
          _formmethod "dialog"
          attr "hx-indicator" "#save-ticket-spinner"
          attr "hx-target" "#dialog-anchor"
          attr "hx-swap" "outerHTML" ]
        [ input [ _required; _name "title"; _placeholder "Title" ]
          input [ _required; _name "description"; _placeholder "Description" ]
          toSelect [ _name "status" ] statusKeyValuePairs None
          button [ _type "submit" ] [ str "Save" ] ]

let showAddTicketDialog: HttpHandler =
    htmlView (addTicketForm |> toDialog "Add Ticket")

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

let editTicketForm (ticket: Ticket) =
    form
        [ attr "hx-put" "/tickets"
          _class "ticket-form"
          _formmethod "dialog"
          attr "hx-indicator" "#save-ticket-spinner"
          attr "hx-target" "#dialog-anchor"
          attr "hx-swap" "outerHTML" ]
        [ input [ _hidden; _name "ticketId"; _value (string ticket.TicketId) ]
          input [ _required; _name "title"; _placeholder "Title"; _value ticket.Title ]
          input
              [ _required
                _name "description"
                _placeholder "Description"
                _value ticket.Description ]
          toSelect [ _name "status" ] statusKeyValuePairs (string ticket.Status |> Some)
          button [ _type "submit" ] [ str "Save" ] ]

let showEditTicketDialog ticketId : HttpHandler =
    let ticket = getTicket ticketId

    htmlView (editTicketForm ticket |> toDialog "Edit Ticket")
