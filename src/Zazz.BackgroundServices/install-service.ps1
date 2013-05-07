param([string] $p)

$fileExists = (Test-Path -Path $p);

if ($fileExists) 
{
    New-Service -Name "Zazz.BackgroundServices" -BinaryPathName $p -DisplayName "Zazz Background Services" -StartupType Automatic -Verbose
}
else 
{
    Write-Host "File not found!" -ForegroundColor Red;
}