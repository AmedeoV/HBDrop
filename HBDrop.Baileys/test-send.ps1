$body = @{
    phone = "+353899548661"
    message = "Hello from Baileys! Testing HBDrop 🎉🚀"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:3000/send" -Method Post -Body $body -ContentType "application/json"

Write-Host "`n✅ Response:" -ForegroundColor Green
$response | ConvertTo-Json
