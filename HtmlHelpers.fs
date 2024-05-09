module HtmlHelpers

open TodoRepo
open Giraffe.ViewEngine

let body content =
    body
        []
        [ nav
              []
              [ header
                    [ _class "main-header" ]
                    [ span [] [ str "Logo" ]
                      div
                          [ _class "main-header__actions" ]
                          [ i [ _class "bx bx-plus bx-md" ] []; i [ _class "bx bxs-grid bx-md" ] [] ] ]
                div
                    [ _class "main-menu" ]
                    [ div
                          [ _class "main-menu__team-profile" ]
                          [ img [ _class "avatar"; _src "/Avatar.png" ]
                            div
                                [ _class "team_profile__description" ]
                                [ p [ _class "team-profile__team-name" ] [ str "Teams in Space" ]
                                  p [ _class "team-profile__team-type" ] [ str "Software Project" ] ] ]
                      ul
                          [ attr "hx-boost" "true" ]
                          [ li
                                []
                                [ a
                                      [ _href "/backlog.html" ]
                                      [ i [ _class "fa fa-table-cells-large" ] []; span [] [ str "Backlog" ] ] ]
                            li
                                []
                                [ a
                                      [ _href "/Board.html" ]
                                      [ i [ _class "fa fa-table-cells" ] []; span [] [ str "Board" ] ] ]
                            li
                                []
                                [ a
                                      [ _href "/Report.html" ]
                                      [ i [ _class "bx bxs-report" ] []; span [] [ str "Report" ] ] ]
                            li
                                []
                                [ a
                                      [ _href "/Releases.html" ]
                                      [ i [ _class "fa fa-ship" ] []; span [] [ str "Releases" ] ] ]
                            li
                                []
                                [ a
                                      [ _href "/Components.html" ]
                                      [ i [ _class "fa fa-box" ] []; span [] [ str "Components" ] ] ]
                            li
                                []
                                [ a [ _href "/Issues.html" ] [ i [ _class "fa fa-bug" ] []; span [] [ str "Issues" ] ] ] ]
                      div
                          [ _class "main-menu__user-profile" ]
                          [ img [ _class "avatar"; _src "/Avatar.png" ]
                            div
                                [ _class "team-profile__description" ]
                                [ p [ _class "team-profile__team-name" ] [ str "Andrew Carver" ]
                                  p [ _class "team-profile__team-type" ] [ str "drew.carver@outlook.com" ] ]
                            i [ _class "fa fa-chevron-right" ] [] ] ] ]
          content ]

let mainPage content =
    html
        [ _lang "en-US" ]
        [ head
              []
              [ title [] [ str "Tickets App" ]
                meta [ _name "viewport"; _content "width=device-width, initial-scale=1.0" ]
                meta [ _name "description"; _content "A ticket management application." ]
                meta [ _charset "UTF-8" ]
                script
                    [ _src "https://unpkg.com/htmx.org@1.9.10"
                      _integrity "sha384-D1Kt99CQMDuVetoL1lrYwg5t+9QdHe7NLX/SoJYkXDFfX37iInKRy5xLSi8nO7UC"
                      _crossorigin "anonymous" ]
                    []
                script [ _src "https://cdn.auth0.com/js/auth0/9.18/auth0.min.js" ] []
                link [ _rel "preconnect"; _href "https://fonts.googleapis.com" ]
                link [ _rel "preconnect"; _href "https://fonts.gstatic.com"; _crossorigin "" ]
                link
                    [ _href
                          "https://fonts.googleapis.com/css2?family=Roboto:ital,wght@0,100;0,300;0,400;0,500;0,700;0,900;1,100;1,300;1,400;1,500;1,700;1,900&display=swap"
                      _rel "stylesheet" ]
                link
                    [ _rel "stylesheet"
                      _href "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.2/css/all.min.css"
                      _integrity
                          "sha512-SnH5WK+bZxgPHs44uWIX+LLJAJ9/2PkPKZ5QiAj6Ta86w+fsb2TkcmfRyVX3pBnMFcV7oQPJkl9QevSCWr3W6A=="
                      _crossorigin "anonymous"
                      attr "referrerpolicy" "no-referrer" ]
                link
                    [ _href "https://unpkg.com/boxicons@2.1.4/css/boxicons.min.css"
                      _rel "stylesheet" ]
                link [ _rel "stylesheet"; _href "/LoadingSpinner.css" ]
                link [ _rel "stylesheet"; _href "/index.css" ] ]
              body
              content ]



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

let toCard (ticket: Ticket) =
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
          button [ _type "submit"; _class "btn save-btn" ] [ str "Save" ] ]

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
          button [ _type "submit"; _class "btn save-btn" ] [ str "Save" ] ]
