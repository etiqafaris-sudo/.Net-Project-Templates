# One-time URL reservation when you must bind http://+:port/ (SelfHost with wildcard or legacy hosts).
# Run from an elevated PowerShell prompt.

param(
    [int]$Port = 5080
)

$url = "http://+:$Port/"
$user = "$env:USERDOMAIN\$env:USERNAME"

Write-Host "Adding URL ACL: $url for $user"
netsh http add urlacl url=$url user=$user
Write-Host "Done."