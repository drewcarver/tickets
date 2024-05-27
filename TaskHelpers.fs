module TaskHelpers
open System.Threading.Tasks

type MaybeBuilder() =
   member this.Bind(x, f) = 
      match x with        
      | Some(x) -> f(x)        
      | _ -> None
   member this.Delay(f) = f()   
 
   member this.Return(x) = Some x

let maybe = MaybeBuilder() 

type ResultBuilder() =
   member this.Bind(x, f) = 
      match x with        
      | Ok(x) -> f(x)        
      | Error(x) -> Error(x)
   member this.Delay(f) = f()   
 
   member this.Return(x) = Ok x

let result = ResultBuilder() 

let bindTaskResult (binder : 'T -> Task<Result<'U, 'TError>>) (taskResult : Task<Result<'T, 'TError>>): Task<Result<'U, 'TError>> = 
        taskResult.ContinueWith(fun (tr: Task<Result<'T, 'TError>>) ->
            match tr.Result with
            | Ok value -> binder value 
            | Error e -> Task.FromResult(Error e)
        ) |> TaskExtensions.Unwrap 

type TaskResultBuilder() =
    member _.Return(value) : Task<Result<'T, 'TError>> =
        Task.FromResult(Ok value)

    member _.ReturnFrom(taskResult : Task<Result<'T, 'TError>>) : Task<Result<'T, 'TError>> =
        taskResult

    member _.Bind(taskResult : Task<Result<'T, 'TError>>, binder : 'T -> Task<Result<'U, 'TError>>) : Task<Result<'U, 'TError>> =
        bindTaskResult binder taskResult

    member _.Zero() : Task<Result<unit, 'TError>> =
        Task.FromResult(Ok ())

let taskResult= TaskResultBuilder() 

let toTaskResult (task: Task) =
    task.ContinueWith(fun _ -> Task.FromResult (Ok ())).Unwrap()

let mapTaskResultError (taskResult: Task<Result<'a, 'b>>) (f : 'b -> 'c): Task<Result<'a, 'c>> = 
    let continuation (tr: Task<Result<'a, 'b>>) : Task<Result<'a, 'c>> = 
      match tr.Result with
      | Ok a -> a
      | Error b -> f b

    taskResult.ContinueWith(continuation).Unwrap()


let bindTask (f: 'a -> Task<'b>) (t: Task<'a>) : Task<'b> =
    task {
        let! result = t
        return! f result
    }

let mapTask (f: 'a -> 'b) (t: Task<'a>) : Task<'b> =
    task {
        let! result = t
        return f result
    }

let always value = 
    fun _ -> value
