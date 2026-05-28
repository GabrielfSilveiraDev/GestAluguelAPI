Dim WshShell, scriptDir, exePath, appUrl
Set WshShell = WScript.CreateObject("WScript.Shell")

scriptDir = Left(WScript.ScriptFullName, InStrRev(WScript.ScriptFullName, "\"))
exePath   = scriptDir & "BackEndAluguel.exe"
appUrl    = "http://localhost:5101"

Function PortaAberta()
    Dim ps, r
    ps = "powershell.exe -NonInteractive -WindowStyle Hidden -Command """ & _
         "try { (New-Object Net.Sockets.TcpClient('localhost', 5101)).Close(); exit 1 } catch { exit 0 }" & Chr(34)
    r = WshShell.Run(ps, 0, True)
    PortaAberta = (r = 1)
End Function

' Se ja estiver rodando, apenas abre o navegador
If PortaAberta() Then
    WshShell.Run appUrl
    WScript.Quit 0
End If

' Inicia o servidor sem janela (WinExe: nao cria console)
WshShell.Run Chr(34) & exePath & Chr(34) & " --urls " & appUrl, 0, False

' Aguarda o servidor subir (ate 30 segundos)
Dim i, pronto
pronto = False
For i = 1 To 30
    WScript.Sleep 1000
    If PortaAberta() Then
        pronto = True
        Exit For
    End If
Next

If pronto Then
    WshShell.Run appUrl
Else
    MsgBox "GestAluguel nao conseguiu iniciar." & vbCrLf & _
           "Verifique se a porta 5101 esta disponivel.", 16, "GestAluguel"
End If
