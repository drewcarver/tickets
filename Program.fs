open System
open System.Net.Http
open System.Security.Claims
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open Giraffe
open Giraffe.EndpointRouting
open Giraffe.ViewEngine

let notLoggedIn =
    div [] [ 
      script [] [ str "login()"] 
    ] |> MainPage.mainPage |> htmlView

let mustBeLoggedIn = requiresAuthentication notLoggedIn

let notFoundHandler: HttpHandler = "Not Found" |> text |> RequestErrors.notFound

let clientId = "kRYFKmwQiKoGQegrMTTmwRpgHoRZaByz"
let domain = "dev-fu83rki86r8dd5bd.us.auth0.com"

let endpoints =
    [ 
      GET [ route "/" (htmlView Pages.Board.ticketBoard) ]
      GET [ route "/index.html" (htmlView Pages.Board.ticketBoard) ]
      GET [ route "/backlog.html" (mustBeLoggedIn >=> htmlView Pages.Backlog.backlog) ]
      POST [ route "/tickets" Ticket.addTicket ]
      PUT [ route "/tickets" Ticket.editTicket ]
      GET [ route "/tickets" Ticket.listTickets ]
      GET [ route "/show-add-dialog" Ticket.showAddTicketDialog ]
      GET [ routef "/show-edit-dialog/%i" (fun ticketId -> Ticket.showEditTicketDialog ticketId) ] ]

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseFileServer()
        .UseGiraffe(endpoints)
        .UseGiraffe(notFoundHandler)

let configureServices (services: IServiceCollection) =
    services
      .AddRouting()
      .AddGiraffe()
      .AddAuthorization()
        .AddAuthentication(fun o ->
            o.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            o.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(fun o ->
            o.Authority <- sprintf "https://%s/" domain
            o.Audience <- "Todo API")
    |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services

    let app = builder.Build()

    app.UseAuthentication().UseAuthorization() |> ignore

    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore

    configureApp app
    app.Run()

    0
