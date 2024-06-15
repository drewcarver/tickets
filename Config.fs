module Config
open System
open Microsoft.Extensions.Configuration

// Define a record type for your settings
type AppConfig = {
  IsTest: bool
}

let configuration =
    ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
        .Build()

let getSection<'T> (sectionName: string) =
    let section = configuration.GetSection(sectionName)
    section.Get<'T>()

let appConfig = getSection<AppConfig>("AppConfig")
