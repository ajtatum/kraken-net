// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#r @"tools\FAKE\tools\FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile
open Fake.Testing 

// --------------------------------------------------------------------------------------
// Information about the project to be used at NuGet and in AssemblyInfo files
// --------------------------------------------------------------------------------------

let project = "kraken-net"
let authors = ["Kraken.io, Kevin Bronsdijk"]
let summary = "The official Kraken.io .Net client"
let version = "0.1.1.6"
let description = "The official kraken-net client interacts with the Kraken.io REST API allowing you to utilize Krakens features using a .NET interface."
let notes = "Added None as a strategy. For more information and documentation, please visit the project site on GitHub."
let nugetVersion = "1.1.6"
let tags = "kraken.io C# API image optimization official"
let gitHome = "https://github.com/kraken-io"
let gitName = "kraken-net"

// --------------------------------------------------------------------------------------
// Build script 
// --------------------------------------------------------------------------------------

let buildDir = "./output/"
let packagingOutputPath = "./nuGet/"
let packagingWorkingDir = "./inputNuget/"
let nugetDependencies = getDependencies "./src/kraken-net/packages.config"

// --------------------------------------------------------------------------------------

Target "Clean" (fun _ ->
 CleanDir buildDir
)

// --------------------------------------------------------------------------------------

Target "AssemblyInfo" (fun _ ->
    let attributes =
        [ 
            Attribute.Title project
            Attribute.Product project
            Attribute.Description summary
            Attribute.Version version
            Attribute.FileVersion version
        ]

    CreateCSharpAssemblyInfo "src/kraken-net/Properties/AssemblyInfo.cs" attributes
)

// --------------------------------------------------------------------------------------

Target "RestorePackages" (fun _ -> 
     "src/Tests/packages.config"
     |> RestorePackage (fun p ->
         { p with
             OutputPath = "src/packages"
             Retries = 4 })
 )

// --------------------------------------------------------------------------------------

Target "Build" (fun _ ->
 !! "src/*.sln"
 |> MSBuildRelease buildDir "Build"
 |> Log "AppBuild-Output: "
)

// --------------------------------------------------------------------------------------

let nunitRunnerPath = "src/packages/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe"

Target "TestNunit" (fun _ ->
    !! (buildDir + @"\*Tests.dll") 
    |> NUnit3 (fun p ->
        {p with 
             ToolPath = nunitRunnerPath
        }))

// --------------------------------------------------------------------------------------

Target "CreatePackage" (fun _ ->

    CreateDir packagingWorkingDir
    CleanDir packagingWorkingDir
    CopyFile packagingWorkingDir "./output/kraken.dll"
    CreateDir packagingOutputPath

    NuGet (fun p -> 
        {p with
            Authors = authors
            Dependencies = nugetDependencies      
            Files = [ (@"kraken.dll", Some @"lib/net452", None);
                        (@"kraken.dll", Some @"lib/net45", None) ] 
            Project = project
            Description = description
            OutputPath = packagingOutputPath
            Summary = summary
            WorkingDir = packagingWorkingDir
            Version = nugetVersion
            ReleaseNotes = notes
            Publish = false }) 
            "kraken.nuspec"
            
    DeleteDir packagingWorkingDir
)

// --------------------------------------------------------------------------------------

Target "All" DoNothing

"Clean"
  ==> "AssemblyInfo"
  ==> "RestorePackages"
  ==> "Build"
  ==> "TestNunit"
  ==> "CreatePackage"
  ==> "All"

RunTargetOrDefault "All"