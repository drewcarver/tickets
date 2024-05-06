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

//let notLoggedIn =
//    RequestErrors.UNAUTHORIZED "Token" "Auth0" "You must be logged in."

//let mustBeLoggedIn = requiresAuthentication notLoggedIn

let notFoundHandler: HttpHandler = "Not Found" |> text |> RequestErrors.notFound

let clientId = "kRYFKmwQiKoGQegrMTTmwRpgHoRZaByz"
let domain = "dev-fu83rki86r8dd5bd.us.auth0.com"

let endpoints =
    [ POST [ route "/tickets" Todo.addTicket ]
      PUT [ route "/tickets" Todo.editTicket ]
      GET [ route "/tickets" Todo.listTickets ]
      GET [ route "/show-add-dialog" Todo.showAddTicketDialog ]
      GET [ routef "/show-edit-dialog/%i" (fun ticketId -> Todo.showEditTicketDialog ticketId) ] ]

let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseFileServer()
        .UseGiraffe(endpoints)
        .UseGiraffe(notFoundHandler)

let configureServices (services: IServiceCollection) =
    services.AddRouting().AddGiraffe()
    (*.AddAuthorization()
        .AddAuthentication(fun o ->
            o.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            o.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(fun o ->
            o.Authority <- sprintf "https://%s/" domain
            o.Audience <- "Todo API")
        *)
    |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services

    let app = builder.Build()

    //app.UseAuthentication().UseAuthorization() |> ignore


    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore

    configureApp app
    app.Run()

    0
