module TodoRepo

open FSharp.Data.Sql

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
    { ticketId: int
      title: string
      description: string
      status: int }

type Ticket = sql.dataContext.``Ticket.TicketEntity``

let createTicket (ticket: TicketDTO) =
    let createdTicket = ctx.Ticket.Ticket.Create()
    createdTicket.Title <- ticket.title
    createdTicket.Description <- ticket.description
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

let updateTicket (ticket: TicketDTO) ticketId =
    let existingTicket = getTicket ticketId
    existingTicket.Title <- ticket.title
    existingTicket.Description <- ticket.description
    existingTicket.Status <- ticket.status

    ctx.SubmitUpdatesAsync()
