<#
  Insere <script src=".../js/api.js"></script> antes do app.js em todas as
  paginas HTML, respeitando a profundidade de cada pasta.

  Como usar (PowerShell, na pasta raiz do front-end):
      .\injetar-api-js.ps1

  Seguro de rodar duas vezes: paginas que ja tem api.js sao ignoradas.
#>

$alterados = 0
$ignorados = 0

Get-ChildItem -Path . -Filter *.html -Recurse | ForEach-Object {
    $conteudo = Get-Content $_.FullName -Raw

    if ($conteudo -match 'js/api\.js') {
        $ignorados++
        return
    }

    # $2 captura o prefixo relativo ("", "../", "../../") do caminho do app.js
    $substituicao = '<script src="$2js/api.js"></script>' + "`r`n" + '$1'
    $novo = $conteudo -replace '(<script src="([^"]*?)js/app\.js"></script>)', $substituicao

    if ($novo -ne $conteudo) {
        Set-Content -Path $_.FullName -Value $novo -Encoding UTF8
        Write-Host "  alterado: $($_.FullName.Replace($PWD.Path + '\',''))" -ForegroundColor Green
        $alterados++
    }
}

Write-Host ""
Write-Host "Paginas alteradas: $alterados"
Write-Host "Paginas que ja tinham api.js: $ignorados"
