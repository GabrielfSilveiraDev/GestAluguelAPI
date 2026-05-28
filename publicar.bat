@echo off
setlocal

set FRONTEND_DIR=C:\Users\gabri\WebstormProjects\frontendaluguel
set BACKEND_DIR=%~dp0
set WWWROOT_DIR=%BACKEND_DIR%BackEndAluguel\wwwroot
set OUTPUT_DIR=%BACKEND_DIR%dist-local

echo ============================================
echo  GestAluguel - Gerando versao local
echo ============================================

:: 1. Limpar pasta de saida
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

:: 2. Build do frontend
echo.
echo [1/3] Compilando frontend...
cd /d "%FRONTEND_DIR%"
call npm install --silent
if errorlevel 1 ( echo ERRO: npm install falhou & pause & exit /b 1 )
call npm run build
if errorlevel 1 ( echo ERRO: npm run build falhou & pause & exit /b 1 )

:: 3. Copiar frontend para wwwroot
echo.
echo [2/3] Copiando frontend para o backend...
if exist "%WWWROOT_DIR%\assets" rmdir /s /q "%WWWROOT_DIR%\assets"
xcopy /e /y /q "%FRONTEND_DIR%\dist\*" "%WWWROOT_DIR%\"
if errorlevel 1 ( echo ERRO: copia do frontend falhou & pause & exit /b 1 )

:: 4. Publicar backend self-contained
echo.
echo [3/3] Compilando backend (modo self-contained)...
cd /d "%BACKEND_DIR%"
dotnet publish BackEndAluguel/BackEndAluguel.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -o "%OUTPUT_DIR%"
if errorlevel 1 ( echo ERRO: dotnet publish falhou & pause & exit /b 1 )

:: 5. Copiar iniciar.vbs para a pasta de saida
copy /y "%BACKEND_DIR%iniciar.vbs" "%OUTPUT_DIR%\iniciar.vbs"

echo.
echo ============================================
echo  PRONTO! Pasta gerada: dist-local\
echo  Distribua a pasta dist-local\ para o usuario
echo  O usuario deve executar: dist-local\iniciar.vbs
echo ============================================
pause
