module NonEmptyString

type NonEmptyString = NonEmptyString of string

let value nonEmptyString = 
  match nonEmptyString with
    | NonEmptyString s -> s

let createNonEmptyString fieldName (s:string option) = 
  match s with
    | Some "" | None -> Error (sprintf "Field \"%s\" is required." fieldName)
    | Some s -> Ok (NonEmptyString s)