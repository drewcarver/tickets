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
