module TodoRepo

open FSharp.Data.Sql

[<Literal>]
let resolutionPath = "/home/drew/Downloads/mysql-connector/net8.0"

[<Literal>]
let connectionString =
    "Data Source=localhost;Initial Catalog=TICKETS;User ID=ticketapp;Password=ticket123"

type sql =
    SqlDataProvider<ConnectionString=connectionString, DatabaseVendor=Common.DatabaseProviderTypes.MYSQL, ResolutionPath=resolutionPath>

let ctx = sql.GetDataContext()

[<CLIMutable>]
type Ticket =
    { Title: string
      Description: string
      Status: string }

type TicketStatus =
    | Ready = 1
    | InProgress = 2
    | Testing = 3
    | Done = 4


let createTicket (ticket: Ticket) =
    let createdTicket = ctx.Tickets.Ticket.Create()
    createdTicket.Title <- ticket.Title
    createdTicket.Description <- ticket.Description
    createdTicket.Status <- int TicketStatus.Ready

    ctx.SubmitUpdates()

let getTickets () =
    query {
        for ticket in ctx.Tickets.Ticket do
            select ticket
    }
    |> Seq.toList
