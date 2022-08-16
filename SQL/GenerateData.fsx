
#r "nuget: MathNet.Numerics"

open System
open MathNet.Numerics

let metropolisDistribution = Distributions.Normal(12345, 400)
let smallvilleDistribution = Distributions.Normal(1234, 100)

let nValues = 180

let startDate = 
  let lastDate = DateOnly.FromDateTime DateTime.Today
  in lastDate.AddDays (-nValues)

let metropolisValues = [
  for i, value in metropolisDistribution.Samples() |> Seq.take nValues |> Seq.indexed -> 
    let date = startDate.AddDays i
    in {| Date = date; Value = value |}
]

let smallvilleValues = [
  for i, value in smallvilleDistribution.Samples() |> Seq.take nValues |> Seq.indexed -> 
    let date = startDate.AddDays i
    in {| Date = date; Value = value |}
]

for pair in metropolisValues do
  printfn "INSERT INTO [LoadObservation] ([Date], [OpArea], [Value]) VALUES ('%A', @metropolis, %f);" pair.Date pair.Value

for pair in smallvilleValues do
  printfn "INSERT INTO [LoadObservation] ([Date], [OpArea], [Value]) VALUES ('%A', @smallville, %f);" pair.Date pair.Value

