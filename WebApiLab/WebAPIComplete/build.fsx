#if BOOT
open Fake
module FB = Fake.Boot
FB.Prepare {
    FB.Config.Default __SOURCE_DIRECTORY__ with
        NuGetDependencies =
            let (!!) x = FB.NuGetDependency.Create x
            [
                !!"FAKE"
                !!"NuGet.Build"
                !!"NuGet.Core"
                !!"NUnit.Runners"
            ]
}
#endif

#load ".build/boot.fsx"

open System.IO
open Fake 
open Fake.AssemblyInfoFile
open Fake.MSBuild

// directories
let buildDir = __SOURCE_DIRECTORY__ @@ "build"
let deployDir = __SOURCE_DIRECTORY__ @@ "deploy"
let packagesDir = __SOURCE_DIRECTORY__ @@ "packages"
let testDir = __SOURCE_DIRECTORY__ @@ "test"

// tools
let nugetPath = "./.nuget/nuget.exe"
let nunitPath = "./packages/NUnit.Runners.2.6.2/tools"

// files
let appReferences =
    !+ "GuitarExample/**/*.fsproj"
        |> Scan

let testReferences =
    !+ "tests/**/*.fsproj"
        |> Scan

// targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir
               testDir
               deployDir]
)

Target "BuildApp" (fun _ ->
    if not isLocalBuild then
        [ Attribute.Version(buildVersion)
          Attribute.Title("GuitarExampleWeb")
          Attribute.Description("GuitarExampleWeb web application")
          Attribute.Guid("A48042E1-F7DE-4E68-9D79-4914510B9194")
        ]
        |> CreateFSharpAssemblyInfo "GuitarExample/GuitarExampleWeb/AssemblyInfo.fs"

        [ Attribute.Version(buildVersion)
          Attribute.Title("GuitarExampleWebApp")
          Attribute.Description("Web APIs for GuitarExampleWeb")
          Attribute.Guid("a126575f-c66e-42bf-a70f-655d4c3f0069")
        ]
        |> CreateFSharpAssemblyInfo "GuitarExample/GuitarExampleWebApp/AssemblyInfo.fs"

    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "BuildTest" (fun _ ->
    MSBuildDebug testDir "Build" testReferences
        |> Log "TestBuild-Output: "
)

Target "Test" (fun _ ->
    let nunitOutput = testDir @@ "TestResults.xml"
    !+ (testDir @@ "*.Tests.dll")
        |> Scan
        |> NUnit (fun p ->
                    {p with
                        ToolPath = nunitPath
                        DisableShadowCopy = true
                        OutputFile = nunitOutput })
)

FinalTarget "CloseTestRunner" (fun _ ->
    ProcessHelper.killProcess "nunit-agent.exe"
)

Target "Deploy" DoNothing
Target "Default" DoNothing

// Build order
"Clean"
  ==> "BuildApp" <=> "BuildTest"
  ==> "Test"
  ==> "Deploy"

"Default" <== ["Deploy"]

// Start build
RunTargetOrDefault "Default"

