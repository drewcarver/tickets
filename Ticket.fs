module Ticket

open Giraffe
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
                        [ div [ _class "swimlanes__lane" ] [h2 [] [ str "Ready" ]; div [ _class "swimlanes__cards" ] readyCards]
                          div [ _class "swimlanes__lane" ] [h2 [] [ str "In Progress" ]; div [ _class "swimlanes__cards" ] inProgressCards]
                          div [ _class "swimlanes__lane" ] [h2 [] [ str "Testing" ]; div [ _class "swimlanes__cards" ] testingCards]
                          div [ _class "swimlanes__lane" ] [h2 [] [ str "Done" ]; div [ _class "swimlanes__cards" ] doneCards] ]
                )
        }

type TicketFormProps = 
    | Add of ErrorMessage: string option * Ticket: TicketDTO
    | Edit of ErrorMessage: string option * Ticket: TicketDTO * TicketId: int
    member x.ErrorMessage = x |> function
      | Add (ErrorMessage=message) 
      | Edit (ErrorMessage=message) -> message
    member x.Ticket = x |> function
      | Add (Ticket=ticket) 
      | Edit (Ticket=ticket) -> ticket

let empty : XmlNode = Text ""

let ticketForm (props: TicketFormProps) =
    let saveAttr = 
      match props with 
        | Add _ -> attr "hx-post" "/tickets" 
        | Edit (TicketId=ticketId) -> attr "hx-put" (sprintf "/tickets/%i" ticketId)

    let defaultEmpty = Option.defaultValue ""

    form
        [ saveAttr 
          _class "ticket-form"
          _formmethod "dialog"
          attr "hx-indicator" "#save-ticket-spinner"
          attr "hx-target" "#dialog-anchor"
          attr "hx-swap" "innerHTML" ]
        [ 
          match props.ErrorMessage with 
            | Some message -> div [] [str message]
            | None -> empty
          input [ _required; _name "title"; _placeholder "Title"; _value (defaultEmpty props.Ticket.title) ]
          input [  _name "description"; _placeholder "Description"; _value (defaultEmpty props.Ticket.description) ]
          toSelect [ _name "status" ] statusKeyValuePairs (string props.Ticket.status |> Some)
          button [ _type "submit"; _class "btn save-btn" ] [ str "Save" ] ]

let showEditTicketDialog ticketId : HttpHandler =
    let ticket = getTicket ticketId

    let ticketDTO: TicketDTO = {
      title = Some ticket.Title
      description = Some ticket.Description
      status = Some ticket.Status
    }
    let ticketForm = ticketForm (Edit(None, ticketDTO, ticketId))

    htmlView (ticketForm |> toDialog "Edit Ticket")

let showAddTicketDialog: HttpHandler =
    let ticketForm = ticketForm (Add(None, TicketDTO.Default))
    htmlView (ticketForm |> toDialog "Add Ticket")

let addTicket: HttpHandler =
    fun _ ctx ->
        let saveChanges validTicket = 
          task {
            do! createTicket validTicket 
            return! ctx.WriteHtmlViewAsync (html [] [ createToast "Ticket Saved" "Your ticket was successfully saved."; updateDialogDiv ])
          }

        task {
            let! ticket = ctx.BindFormAsync<TicketDTO>() 
            let result = createValidTicket ticket 


            let ticketForm message = ticketForm (Add(Some message, ticket))

            return! match result with
                      | Ok result -> saveChanges result
                      | Error message -> 
                          ctx.WriteHtmlViewAsync (ticketForm message |> toDialog "Add Ticket")
        }

let editTicket ticketId: HttpHandler =
    fun _ ctx ->
        let saveChanges validTicket = 
          task {
            do! updateTicket validTicket ticketId 
            return! ctx.WriteHtmlViewAsync (html [] [ createToast "Ticket Updated" "Your ticket was successfully updated."; updateDialogDiv ])
          }

        task {
            let! ticket = ctx.BindFormAsync<TicketDTO>() 
            let result = createValidTicket ticket 

            let ticketForm message = ticketForm (Edit(Some message, ticket, ticketId))

            return! match result with
                      | Ok result -> saveChanges result
                      | Error message -> 
                          ctx.WriteHtmlViewAsync (ticketForm message |> toDialog "Edit Ticket")
        }