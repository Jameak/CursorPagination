[CmdletBinding()]
param (
	[ValidateSet('local', 'ci')]
	[string]$Env = 'local'
)
Set-StrictMode -Version 1

function Exec([scriptblock] $cmd) {
    $expandedCmdForLog = $ExecutionContext.InvokeCommand.ExpandString($cmd)
    Write-Host "Executing: $expandedCmdForLog"
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function CleanFolder($folderPath){
    if(Test-Path -Path $folderPath){
        Remove-Item $folderPath -Recurse
    }
}

function RunNugetTests {
    Param
    (
         [Parameter(Mandatory=$true, Position=0)]
         [string] $Project,
         [Parameter(Mandatory=$true, Position=1)]
         [string] $PackageDir,
         [Parameter(Mandatory=$true, Position=2)]
         [string] $Configuration,
         [Parameter(Mandatory=$true, Position=3)]
         [AllowEmptyString()]
         [string] $AdditionalConfiguration
    )
    
    Exec { & dotnet restore $Project --packages $PackageDir --configfile "nuget.local-packages.config" $AdditionalConfiguration }
    Exec { & dotnet build $Project --packages $PackageDir  --no-restore -c $Configuration $AdditionalConfiguration '/p:ExtraNoWarn1="JAMCP0007"' '/p:ExtraNoWarn2="JAMCP0018"' }
    Exec { & dotnet test $Project --no-build --no-restore --logger trx -c $Configuration $AdditionalConfiguration }
}

function PrepareAndRunNugetTests{
    Param
    (
         [Parameter(Mandatory=$true, Position=0)]
         [string] $Configuration,
         [Parameter(Mandatory=$true, Position=1)]
         [AllowEmptyString()]
         [string] $AdditionalConfiguration
    )

    $artifactsDir = "./artifacts";
    $packageDir = "./packages"
    CleanFolder $artifactsDir
    CleanFolder $packageDir

    Exec { & dotnet pack -c $Configuration -o $artifactsDir ./src/Jameak.CursorPagination/Jameak.CursorPagination.csproj }

    RunNugetTests -Project "./test/Jameak.CursorPagination.SourceGenerator.Nuget.IntegrationTests" -PackageDir $packageDir -Configuration $Configuration -AdditionalConfiguration $AdditionalConfiguration
    RunNugetTests -Project "./test/Jameak.CursorPagination.Nuget.Tests" -PackageDir $packageDir -Configuration $Configuration -AdditionalConfiguration $AdditionalConfiguration
}

function RunLinterAndStandardTests{
    Param
    (
         [Parameter(Mandatory=$true, Position=0)]
         [string] $Configuration,
         [Parameter(Mandatory=$true, Position=1)]
         [AllowEmptyString()]
         [string] $AdditionalConfiguration
    )
    
    if ($Env -eq 'ci') {
        Exec { & dotnet format style --verify-no-changes }
        Exec { & dotnet format analyzers --verify-no-changes }
    } else {
        Exec { & dotnet format --verify-no-changes }
    }

    Exec { & dotnet build -c $Configuration $AdditionalConfiguration  '/p:ExtraNoWarn1="JAMCP0007"' '/p:ExtraNoWarn2="JAMCP0018"' }
    Exec { & dotnet test -c $Configuration $AdditionalConfiguration --logger trx --no-build --no-restore }
}

$configValue = 'Debug'
$msbuildConstant = ''

if ($Env -eq 'ci') {
    $configValue = 'Release'
    $msbuildConstant = '/p:ExtraDefineConstants="IS_CI_TEST_BUILD"'
}

RunLinterAndStandardTests -Configuration $configValue -AdditionalConfiguration $msbuildConstant
PrepareAndRunNugetTests -Configuration $configValue -AdditionalConfiguration $msbuildConstant
