module TodoRepo

open FSharp.Data.Sql

type Todo = { Description: string }

[<Literal>]
let resolutionPath = "/home/drew/Downloads/mysql-connector/net8.0"

[<Literal>]
let connectionString =
    "Data Source=localhost;Initial Catalog=Todo;User ID=todo;Password=todo123"

type sql =
    SqlDataProvider<ConnectionString=connectionString, DatabaseVendor=Common.DatabaseProviderTypes.MYSQL, ResolutionPath=resolutionPath>

let ctx = sql.GetDataContext()

let createTodo description =
    let todo = ctx.Todo.Todo.Create()
    todo.Description <- description

    ctx.SubmitUpdates()

let getTodos () =
    query {
        for todo in ctx.Todo.Todo do
            select todo
    }
    |> Seq.toList
