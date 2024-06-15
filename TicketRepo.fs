module TodoRepo

open FSharp.Data.Sql
open NonEmptyString
open ResultBuilder
open System.Data.SQLite
open Dapper.FSharp.MySQL
open Dapper.FSharp.SQLite
open DbRecords.Ticket
open System.Data


[<Literal>]
let resolutionPath = "/home/drew/Downloads/mysql-connector/net8.0"

[<Literal>]
let connectionString =
    "Data Source=localhost;Initial Catalog=Ticket;User ID=ticketapp;Password=ticket123"

type sql =
    SqlDataProvider<ConnectionString=connectionString, DatabaseVendor=Common.DatabaseProviderTypes.MYSQL, ResolutionPath=resolutionPath>

let ctx = sql.GetDataContext()

let ticketTable = table<Ticket>

let mysqlConnection: IDbConnection = new MySql.Data.MySqlClient.MySqlConnection("Data Source=localhost;Initial Catalog=Ticket;User ID=ticketapp;Password=ticket123")
let sqliteConnection: IDbConnection = new SQLite.SQLiteConnection("Data Source=./ticket.db;Version=3;New=True;")

let isTest = Config.appConfig.IsTest

let connection isUnitTest = 
  if isTest
    then sqliteConnection 
    else mysqlConnection

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

let getTicketsQuery (filter: TicketFilter) =
    select {
      for ticket in ticketTable do
      andWhereIf filter.status.IsSome (ticket.Status = filter.status.Value)
      andWhereIf filter.title.IsSome (ticket.Title.Contains(filter.title.Value))
    } 

let getTickets (filter: TicketFilter) =
    getTicketsQuery(filter) 
    |> connection(false).SelectAsync<DbRecords.Ticket.Ticket> 

let updateTicket (ticket: ValidTicket) ticketId =
    let existingTicket = getTicket ticketId
    existingTicket.Title <- value ticket.title
    existingTicket.Description <- value ticket.description
    existingTicket.Status <- ticket.status

    ctx.SubmitUpdatesAsync()
