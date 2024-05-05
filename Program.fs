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

let ticketModal ticketId = dialog [] [ h1 [] [ str "Test" ] ]

let addTicketDialog =
    htmlView (
        dialog
            [ _id "add-ticket-dialog"
              _class "modal"
              attr "hx-on::load" "document.getElementById('add-ticket-dialog').showModal()" ]
            [ header
                  [ _class "modal__header" ]
                  [ h1 [] [ str "Add Ticket" ]
                    button
                        [ _class "btn"
                          attr "aria-label" "Close"
                          _onclick "document.getElementById('add-ticket-dialog').close()" ]
                        [ i [ _class "fa fa-times fa-2x" ] [] ] ]
              form
                  [ attr "hx-post" "/tickets"
                    _class "ticket-form"
                    _formmethod "dialog"
                    attr "hx-indicator" "#save-ticket-spinner"
                    attr "hx-target" "#dialog-anchor"
                    attr "hx-swap" "outerHTML" ]
                  [ input [ _required; _name "title"; _placeholder "Title" ]
                    input [ _required; _name "description"; _placeholder "Description" ]
                    input [ _required; _name "status"; _placeholder "Status" ]
                    button [ _type "submit" ] [ str "Save" ] ]
              div
                  [ _id "save-ticket-spinner"; _class "htmx-indicator" ]
                  [ div [ _class "lds-ring" ] [ div [] []; div [] []; div [] []; div [] [] ]
                    p [] [ str "Saving changes..." ] ] ]

    )


let endpoints =
    [ GET [ routef "/tickets/%s" (fun ticketId -> htmlView (ticketModal ticketId)) ]
      POST [ route "/tickets" Todo.addTicket ]
      GET [ route "/tickets" Todo.listTickets ]
      GET [ route "/show-add-dialog" addTicketDialog ]
      GET [ routef "/show-edit-dialog/%s" (fun ticketId -> Todo.showEditTicketDialog (int ticketId)) ] ]

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
