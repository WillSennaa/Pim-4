import { ActivityIndicator, StyleSheet, View } from 'react-native';

import { cores, espaco } from '@/tema/tokens';
import { Cartao, Icone, Texto } from './base';
import { Botao } from './Botao';
import type { NomeIcone } from './icones';

/* ==========================================================================
   Estados de carregamento, erro e vazio
   --------------------------------------------------------------------------
   No web cada função escrevia seu próprio "<div class=card>Carregando...".
   Aqui são três componentes, e TODA tela que busca dado da API passa por
   eles. Erro sempre oferece "Tentar novamente": no celular a rede oscila, e
   obrigar o usuário a sair e voltar da tela para repetir é atrito à toa.
   ========================================================================== */

export function Carregando({ texto = 'Carregando...' }: { texto?: string }) {
  return (
    <View style={st.centro}>
      <ActivityIndicator size="large" color={cores.primaria} />
      <Texto v="sm" cor={cores.textoSecundario} style={{ marginTop: espaco.md }}>{texto}</Texto>
    </View>
  );
}

export function ErroCarga({ mensagem, aoTentar }: { mensagem: string; aoTentar?: () => void }) {
  return (
    <Cartao style={{ alignItems: 'center', paddingVertical: espaco.xl }}>
      <Icone nome="cloud-off" tamanho={36} cor={cores.perigo} />
      <Texto v="sm" centro style={{ marginTop: espaco.md, marginBottom: aoTentar ? espaco.lg : 0 }}>{mensagem}</Texto>
      {aoTentar && <Botao texto="Tentar novamente" icone="refresh" variante="secundario" tamanho="sm" onPress={aoTentar} />}
    </Cartao>
  );
}

export function Vazio({ texto, icone = 'info-circle', acao }:
  { texto: string; icone?: NomeIcone; acao?: { texto: string; onPress: () => void } }) {
  return (
    <Cartao style={{ alignItems: 'center', paddingVertical: espaco.xl }}>
      <Icone nome={icone} tamanho={32} cor={cores.textoApagado} />
      <Texto v="sm" cor={cores.textoSecundario} centro style={{ marginTop: espaco.md }}>{texto}</Texto>
      {acao && <Botao texto={acao.texto} variante="fantasma" tamanho="sm" onPress={acao.onPress} style={{ marginTop: espaco.md }} />}
    </Cartao>
  );
}

/** Faixa de aviso amarela/vermelha/azul dentro do conteúdo (.card com fundo de estado). */
export function Faixa({ texto, tom = 'alerta', icone = 'info-circle' }:
  { texto: string; tom?: 'alerta' | 'perigo' | 'primaria' | 'sucesso'; icone?: NomeIcone }) {
  const mapa = {
    alerta: [cores.alertaClaro, cores.alerta, cores.alertaEscuro],
    perigo: [cores.perigoClaro, cores.perigo, cores.perigoEscuro],
    primaria: [cores.primariaMaisClara, cores.primariaClara, cores.primariaEscura],
    sucesso: [cores.sucessoClaro, cores.sucesso, cores.sucessoEscuro]
  } as const;
  const [fundo, borda, cor] = mapa[tom];
  return (
    <View style={[st.faixa, { backgroundColor: fundo, borderColor: borda }]}>
      <Icone nome={icone} tamanho={18} cor={cor} />
      <Texto v="sm" cor={cor} style={{ flex: 1 }}>{texto}</Texto>
    </View>
  );
}

const st = StyleSheet.create({
  centro: { paddingVertical: espaco.xxxl, alignItems: 'center', justifyContent: 'center' },
  faixa: {
    flexDirection: 'row', gap: espaco.sm, alignItems: 'flex-start',
    padding: espaco.md, borderWidth: 1, borderRadius: 16, marginBottom: espaco.md
  }
});
