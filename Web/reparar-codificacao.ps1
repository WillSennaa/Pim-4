<#
  Repara paginas HTML gravadas com codificacao dupla (UTF-8 lido como CP1252
  e regravado como UTF-8). Sintoma: "OlA!" com acento estranho no lugar de
  "Ola" acentuado corretamente.

  COMO DECIDE SE O ARQUIVO ESTA CORROMPIDO (teste de ida e volta):
    1. le o arquivo como UTF-8  -> texto
    2. converte o texto para bytes CP1252
    3. tenta decodificar esses bytes como UTF-8 ESTRITO
  Se o passo 3 funciona e o resultado e DIFERENTE do texto do passo 1, o
  arquivo estava codificado duas vezes, e o resultado do passo 3 e o conteudo
  original. Se o passo 3 falha, o arquivo esta correto e nao e tocado.

  Este arquivo nao contem nenhum caractere fora do ASCII, de proposito: assim
  ele nao pode ser corrompido pelo mesmo problema que veio corrigir.

  Uso, na raiz do front-end:
      powershell -ExecutionPolicy Bypass -File .\reparar-codificacao.ps1
#>

$utf8SemBom   = New-Object System.Text.UTF8Encoding($false)
$utf8Estrito  = New-Object System.Text.UTF8Encoding($false, $true)
$cp1252       = [System.Text.Encoding]::GetEncoding(1252)

$backup = ".\_backup_html_" + (Get-Date -Format "yyyyMMdd_HHmmss")
New-Item -ItemType Directory -Path $backup | Out-Null

$reparados = 0
$intactos  = 0

Get-ChildItem -Path . -Filter *.html -Recurse |
    Where-Object { $_.FullName -notlike "*\_backup_html_*" } |
    ForEach-Object {

    $arquivo = $_.FullName
    $texto   = [System.IO.File]::ReadAllText($arquivo, $utf8SemBom)

    $bytesCandidatos = $cp1252.GetBytes($texto)

    $original = $null
    try {
        $original = $utf8Estrito.GetString($bytesCandidatos)
    } catch {
        # Nao e UTF-8 valido: arquivo nao sofreu codificacao dupla.
        $intactos++
        return
    }

    if ($original -eq $texto) {
        # Conteudo so ASCII, ou ja correto. Nada a fazer.
        $intactos++
        return
    }

    $relativo = $arquivo.Substring($PWD.Path.Length + 1)
    $destino  = Join-Path $backup $relativo
    New-Item -ItemType Directory -Path (Split-Path $destino) -Force | Out-Null
    Copy-Item $arquivo $destino

    [System.IO.File]::WriteAllBytes($arquivo, $bytesCandidatos)

    Write-Host ("  reparado: " + $relativo) -ForegroundColor Green
    $reparados++
}

Write-Host ""
Write-Host ("Reparados: " + $reparados)
Write-Host ("Ja estavam corretos: " + $intactos)
Write-Host ("Backup em: " + $backup)
