# QueryBuilder - Script de Gerenciamento de Debug
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("check", "free", "docker-down", "docker-up", "status", "help")]
    [string]$Action = "help"
)

function Show-Help {
    Write-Host ""
    Write-Host "QueryBuilder - Debug Manager" -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Uso: .\debug-manager.ps1 [ação]"
    Write-Host ""
    Write-Host "Ações disponíveis:" -ForegroundColor Yellow
    Write-Host "  check        - Verifica se a porta 5249 está em uso"
    Write-Host "  free         - Libera a porta 5249 (para por containers Docker)"
    Write-Host "  docker-down  - Para todos os containers Docker"
    Write-Host "  docker-up    - Inicia containers Docker"
    Write-Host "  status       - Mostra status dos containers e portas"
    Write-Host "  help         - Mostra esta ajuda"
    Write-Host ""
}

function Check-Port {
    Write-Host ""
    Write-Host "Verificando porta 5249..." -ForegroundColor Cyan

    $connections = Get-NetTCPConnection -LocalPort 5249 -ErrorAction SilentlyContinue

    if ($connections) {
        Write-Host "Porta 5249 está EM USO!" -ForegroundColor Yellow
        foreach ($conn in $connections) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "  Processo: $($process.ProcessName)" -ForegroundColor White
                Write-Host "  PID: $($process.Id)" -ForegroundColor White
            }
        }
        Write-Host ""
        Write-Host "Execute: .\debug-manager.ps1 free" -ForegroundColor Green
        return $false
    } else {
        Write-Host "Porta 5249 está LIVRE para debug local!" -ForegroundColor Green
        return $true
    }
}

function Free-Port {
    Write-Host ""
    Write-Host "Liberando porta 5249..." -ForegroundColor Cyan

    $apiContainer = docker ps --filter "name=querybuilder-api" --format "{{.Names}}" 2>$null

    if ($apiContainer) {
        Write-Host "Parando container querybuilder-api..." -ForegroundColor Yellow
        docker stop querybuilder-api | Out-Null
        Write-Host "Container parado!" -ForegroundColor Green
    } else {
        Write-Host "Container querybuilder-api não está rodando" -ForegroundColor Gray
    }

    Start-Sleep -Seconds 2
    Check-Port | Out-Null
}

function Docker-Down {
    Write-Host ""
    Write-Host "Parando todos os containers Docker..." -ForegroundColor Cyan
    docker compose down

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Containers parados com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "Erro ao parar containers" -ForegroundColor Red
    }
}

function Docker-Up {
    Write-Host ""
    Write-Host "Iniciando containers Docker..." -ForegroundColor Cyan
    docker compose up -d

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Containers iniciados com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "Erro ao iniciar containers" -ForegroundColor Red
    }
}

function Show-Status {
    Write-Host ""
    Write-Host "Status do Ambiente QueryBuilder" -ForegroundColor Cyan
    Write-Host "==================================" -ForegroundColor Cyan
    Write-Host ""

    Write-Host "Porta 5249:" -ForegroundColor Yellow
    Check-Port | Out-Null

    Write-Host ""
    Write-Host "Containers Docker:" -ForegroundColor Yellow
    $containers = docker ps --filter "name=querybuilder" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>$null

    if ($containers) {
        Write-Host $containers
    } else {
        Write-Host "  Nenhum container rodando" -ForegroundColor Gray
    }

    Write-Host ""
    Write-Host "Oracle Database:" -ForegroundColor Yellow
    $oracleConn = Get-NetTCPConnection -LocalPort 1522 -ErrorAction SilentlyContinue

    if ($oracleConn) {
        Write-Host "  Oracle rodando na porta 1522" -ForegroundColor Green
    } else {
        Write-Host "  Oracle não está rodando" -ForegroundColor Red
    }

    Write-Host ""
}

# Executar ação
switch ($Action) {
    "check" { Check-Port | Out-Null }
    "free" { Free-Port }
    "docker-down" { Docker-Down }
    "docker-up" { Docker-Up }
    "status" { Show-Status }
    "help" { Show-Help }
    default { Show-Help }
}
