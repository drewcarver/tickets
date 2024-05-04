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

let createTodo description =
    let todo = ctx.Tickets.Ticket.Create()
    todo.Description <- description

    ctx.SubmitUpdates()

let getTickets () =
    query {
        for ticket in ctx.Tickets.Ticket do
            select ticket
    }
    |> Seq.toList
