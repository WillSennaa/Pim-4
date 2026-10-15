import { ScrollView, StyleSheet, Text, View } from 'react-native';

import { cores, espaco, fonte, raio } from '@/tema/tokens';

/* ==========================================================================
   Bloco de código e conteúdo de material
   --------------------------------------------------------------------------
   Porte de escapeCode() e formatarConteudoMaterial() do web. A regra de
   formatação é a MESMA, porque o texto é o mesmo que está no banco:
     - linha em branco separa parágrafos;
     - bloco cujas linhas começam com quatro espaços vira código.
   Não é markdown completo de propósito (mesma justificativa do web: o
   conteúdo é texto, e um interpretador de markdown seria uma dependência
   nova para resolver o que duas regras resolvem).

   O destaque de sintaxe também é o mesmo: strings, comentários, números e a
   lista de palavras-chave de C# e SQL. No web ele gera <span class>; aqui
   gera <Text> aninhado com cor, que é como React Native colore trechos.
   ========================================================================== */

const PALAVRAS = new Set(['List', 'int', 'string', 'foreach', 'in', 'if', 'else', 'new', 'var', 'bool',
  'true', 'false', 'public', 'class', 'static', 'void', 'return', 'Console', 'WriteLine',
  'SELECT', 'FROM', 'WHERE', 'JOIN', 'INSERT', 'UPDATE', 'DELETE']);

// Ordem importa: comentário antes de string, string antes de número.
const TOKEN = /(\/\/[^\n]*)|("[^"\n]*")|(\b\d+\b)|([A-Za-z_][A-Za-z0-9_]*)/g;

function destacar(codigo: string) {
  const partes: { t: string; cor?: string }[] = [];
  let ultimo = 0;
  for (const m of codigo.matchAll(TOKEN)) {
    const i = m.index ?? 0;
    if (i > ultimo) partes.push({ t: codigo.slice(ultimo, i) });
    if (m[1]) partes.push({ t: m[1], cor: cores.codigoComentario });
    else if (m[2]) partes.push({ t: m[2], cor: cores.codigoString });
    else if (m[3]) partes.push({ t: m[3], cor: cores.codigoNumero });
    else if (m[4]) partes.push({ t: m[4], cor: PALAVRAS.has(m[4]) ? cores.codigoPalavra : undefined });
    ultimo = i + m[0].length;
  }
  if (ultimo < codigo.length) partes.push({ t: codigo.slice(ultimo) });
  return partes;
}

export function BlocoCodigo({ codigo, rotulo = 'EXEMPLO' }: { codigo: string; rotulo?: string }) {
  return (
    <View style={st.bloco}>
      <Text style={st.rotulo}>{rotulo}</Text>
      {/* Rolagem horizontal: código não quebra linha, como no overflow-x:auto do web. */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false}>
        <Text style={st.codigo} selectable>
          {destacar(codigo).map((p, i) => (
            <Text key={i} style={p.cor ? { color: p.cor } : undefined}>{p.t}</Text>
          ))}
        </Text>
      </ScrollView>
    </View>
  );
}

export function ConteudoMaterial({ texto }: { texto: string }) {
  const blocos = String(texto).split(/\n\s*\n/);
  return (
    <View>
      {blocos.map((bloco, i) => {
        const linhas = bloco.split('\n');
        const naoVazias = linhas.filter(l => l.trim());
        const ehCodigo = naoVazias.length > 0 && naoVazias.every(l => /^ {4}/.test(l));
        if (ehCodigo) {
          const codigo = linhas.map(l => l.replace(/^ {4}/, '')).join('\n').trim();
          return <BlocoCodigo key={i} codigo={codigo} />;
        }
        return <Text key={i} style={st.paragrafo} selectable>{bloco.trim()}</Text>;
      })}
    </View>
  );
}

const st = StyleSheet.create({
  bloco: {
    backgroundColor: cores.codigoFundo, borderRadius: raio.md, padding: espaco.md, marginVertical: espaco.md
  },
  rotulo: { fontFamily: fonte.semi, fontSize: 11, color: '#94A3B8', letterSpacing: 0.5, marginBottom: espaco.sm },
  codigo: { fontFamily: fonte.mono, fontSize: 13, lineHeight: 21, color: cores.codigoTexto },
  paragrafo: { fontFamily: fonte.regular, fontSize: 15, lineHeight: 26, color: cores.texto, marginBottom: espaco.md }
});
