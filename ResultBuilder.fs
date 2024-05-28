module ResultBuilder

type ResultBuilder() =
   member this.Bind(x, f) = 
      match x with        
      | Ok(x) -> f(x)        
      | Error(x) -> Error(x)
   member this.Delay(f) = f()   
 
   member this.Return(x) = Ok x

let result = ResultBuilder() 