module TodoRepo

open FSharp.Data.Sql
open NonEmptyString
open ResultBuilder


[<Literal>]
let resolutionPath = "/home/drew/Downloads/mysql-connector/net8.0"

[<Literal>]
let connectionString =
    "Data Source=localhost;Initial Catalog=Ticket;User ID=ticketapp;Password=ticket123"

type sql =
    SqlDataProvider<ConnectionString=connectionString, DatabaseVendor=Common.DatabaseProviderTypes.MYSQL, ResolutionPath=resolutionPath>

let ctx = sql.GetDataContext()

type TicketStatus =
    | Ready = 1
    | InProgress = 2
    | Testing = 3
    | Done = 4

type TicketFilter =
    { status: int option
      title: string option }

[<CLIMutable>]
type TicketDTO =
    {
      title: string option
      description: string option
      status: int option }
    static member Default = 
      { 
        title = Some ""
        description = Some ""
        status = None
      }

type ValidTicket = 
    { 
      title: NonEmptyString
      description: NonEmptyString
      status: int }

let createValidTicket (ticket: TicketDTO): Result<ValidTicket, string> = 
    result {
      let! title = ticket.title |> createNonEmptyString "Title" 
      let! description = ticket.description|> createNonEmptyString "Description"

      return {
        title = title
        description = description
        status = Option.defaultValue 1 ticket.status
      }
    }

     
type Ticket = sql.dataContext.``ticket.ticketEntity`` 

let createTicket (ticket: ValidTicket) =
    let createdTicket = ctx.Ticket.Ticket.Create()
    createdTicket.Title <- value ticket.title
    createdTicket.Description <- value ticket.description
    createdTicket.Status <- ticket.status

    ctx.SubmitUpdatesAsync()

let getTicket ticketId =
    query {
        for ticket in ctx.Ticket.Ticket do
            where (ticket.TicketId = ticketId)

            select ticket
    }
    |> Seq.head

let getTickets (filter: TicketFilter) : List<Ticket> =
    let doesNotHaveStatus = filter.status.IsNone
    let doesNotHaveTitle = filter.title.IsNone

    query {
        for ticket in ctx.Ticket.Ticket do
            where (doesNotHaveStatus || ticket.Status = filter.status.Value)
            where (doesNotHaveTitle || ticket.Title.Contains(filter.title.Value))

            select ticket
    }
    |> Seq.toList

let updateTicket (ticket: ValidTicket) ticketId =
    let existingTicket = getTicket ticketId
    existingTicket.Title <- value ticket.title
    existingTicket.Description <- value ticket.description
    existingTicket.Status <- ticket.status

    ctx.SubmitUpdatesAsync()
