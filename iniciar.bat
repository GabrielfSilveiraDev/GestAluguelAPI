@echo off
setlocal

set APP_EXE=%~dp0BackEndAluguel.exe
set APP_URL=http://localhost:5101

echo ============================================
echo  GestAluguel - Iniciando...
echo ============================================
echo.

:: Verificar se o executavel existe
if not exist "%APP_EXE%" (
    echo ERRO: BackEndAluguel.exe nao encontrado.
    echo Certifique-se de executar este arquivo dentro da pasta dist-local\
    pause
    exit /b 1
)

:: Verificar se a porta ja esta em uso
netstat -an | find ":%APP_URL:~-4%.*LISTEN" >nul 2>&1
if not errorlevel 1 (
    echo Porta 5101 ja em uso. Abrindo o navegador...
    start "" "%APP_URL%"
    exit /b 0
)

:: Iniciar o backend em background
echo Iniciando servidor (aguarde alguns segundos)...
start "" /b "%APP_EXE%" --urls "%APP_URL%"

:: Aguardar o servidor subir
:wait_loop
timeout /t 1 /nobreak >nul
powershell -Command "try { (Invoke-WebRequest -Uri '%APP_URL%/swagger/v1/swagger.json' -UseBasicParsing -TimeoutSec 1).StatusCode } catch { 0 }" 2>nul | find "200" >nul
if errorlevel 1 goto wait_loop

echo Servidor iniciado com sucesso!
echo Abrindo navegador em %APP_URL%
start "" "%APP_URL%"

echo.
echo Pressione Ctrl+C neste terminal para encerrar o GestAluguel.
echo ============================================
pause
