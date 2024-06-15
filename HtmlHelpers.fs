module HtmlHelpers

open TodoRepo
open Giraffe.ViewEngine

let ticketStatusMap =
    [ "Ready", int TicketStatus.Ready
      "In Progress", int TicketStatus.InProgress
      "Testing", int TicketStatus.Testing
      "Done", int TicketStatus.Done ]

let statusKeyValuePairs =
    ticketStatusMap |> List.map (fun (name, value) -> (name, value.ToString()))

let updateDialogDiv =
    div
        [ _id "dialog-anchor"
          attr "hx-trigger" "load"
          attr "hx-get" "/tickets"
          attr "hx-target" ".swimlanes"
          attr "hx-swap" "outerHTML" ]
        []

let createToast title message = 
    div [ attr "hx-swap-oob" "afterbegin:#toast-anchor" ] [
      div [
        attr "remove-me" "3s" 
        _class "toast" 
      ] [ 
        div [ _class "toast__body" ] [
          h3 [] [ str title ]
          p [] [ str message ] 
        ]
        div [ _class "toast__progress-bar" ] []
    ]
  ]

let toDialog title innerContent =
    dialog
        [ _id "ticket-dialog"
          _class "modal"
          attr "hx-on::load" "document.getElementById('ticket-dialog').showModal()" ]
        [ header
              [ _class "modal__header" ]
              [ h1 [] [ str title ]
                button
                    [ _class "close-btn"
                      attr "aria-label" "Close"
                      _onclick "document.getElementById('ticket-dialog').close()" ]
                    [ i [ _class "fa fa-times fa-2x" ] [] ] ]
          innerContent
          div
              [ _id "save-ticket-spinner"; _class "htmx-indicator" ]
              [ div [ _class "lds-ring" ] [ div [] []; div [] []; div [] []; div [] [] ]
                p [] [ str "Saving changes..." ] ] ]

let toCard (ticket: DbRecords.Ticket.Ticket) =
    div
        [ _class "card"
          _id $"ticket_{ticket.TicketId}"
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