$ErrorActionPreference = 'Stop'

$runtime = 'C:\Users\NgethSereyboth\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell'
Add-Type -Path (Join-Path $runtime 'Microsoft.CodeAnalysis.dll')
Add-Type -Path (Join-Path $runtime 'Microsoft.CodeAnalysis.CSharp.dll')

$root = Split-Path -Parent $PSScriptRoot
$sourceFiles = @(
    Get-ChildItem (Join-Path $root 'src\KhmerAutoCorrection.Core') -Filter '*.cs'
    Get-Item (Join-Path $root 'tests\KhmerAutoCorrection.Core.Tests\Program.cs')
)

$trees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
foreach ($file in $sourceFiles) {
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        [System.IO.File]::ReadAllText($file.FullName)))
}

$trustedAssemblies = [AppContext]::GetData('TRUSTED_PLATFORM_ASSEMBLIES').Split([System.IO.Path]::PathSeparator)
$references = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($assemblyPath in $trustedAssemblies) {
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($assemblyPath))
}

$options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
    [Microsoft.CodeAnalysis.OutputKind]::ConsoleApplication)
$compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    'KhmerAutoCorrection.Core.Tests', $trees, $references, $options)

$stream = [System.IO.MemoryStream]::new()
$result = $compilation.Emit($stream)
if (-not $result.Success) {
    $result.Diagnostics | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error } |
        ForEach-Object { Write-Error $_.ToString() }
    exit 1
}

$assembly = [System.Reflection.Assembly]::Load($stream.ToArray())
$arguments = New-Object object[] 1
$arguments[0] = [string[]]@()
$assembly.EntryPoint.Invoke($null, $arguments) | Out-Null
