module Pages.Board

open Giraffe.ViewEngine
open MainPage

let ticketBoard= 
    main [] [
      h1 [] [str "Board"]
      div [] [
        input [
          _type "search" 
          _name "title" 
          _placeholder "Search" 
          _class "board-search"
          attr "hx-get" "tickets"
          attr "hx-trigger" "search, keyup delay:200ms changed"
          attr "hx-target" ".swimlanes"
          attr "hx-swap" "outerHTML"
        ]
        button [
          _class "btn"
          attr "aria-label" "Add New Ticket"
          attr "hx-get" "/show-add-dialog"
          attr "hx-target" "#dialog-anchor"
        ] [
          i [_class "fa fa-plus"] []
        ]
        button [
          _class "btn"
          attr "hx-post" "/show-toast"
          attr "hx-target" "#toast-anchor"
          attr "hx-swap" "afterbegin"
        ] [
          str "Show Toast"
        ]
      ]
      div [_class "swimlane-titles"] [
        h2 [] [str "Ready"]
        h2 [] [str "In Progress"]
        h2 [] [str "Testing"]
        h2 [] [str "Done"]
      ]
      div [
        _id "swimlanes"
        _class "swimlanes"
        attr "hx-get" "/tickets"
        attr "hx-trigger" "load"
        attr "hx-indicator" "#load-swim-spinner"
        attr "hx-swap" "outerHTML"
      ] [
        div [_class "swimlanes__lane"] []
        div [_class "swimlanes__lane"] []
        div [_class "swimlanes__lane"] []
        div [_class "swimlanes__lane"] []
      ]
      div [
        _id "load-swim-spinner" 
        _class "htmx-indicator"
      ] [
        div [_class "lds-ring"] [
          div [] []
          div [] []
          div [] []
          div [] []
        ]
      ]
    ] |> mainPage
