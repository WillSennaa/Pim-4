import { useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { Icone, Texto } from '@/ui/base';
import { Abas } from '@/ui/Abas';
import { Kpi } from '@/ui/Cartoes';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Cabecalho, Grade, Tela } from '@/ui/Tela';
import { iconeDaMedalha } from '@/util/decoracao';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Conquistas — pages/estudante/conquistas.html + carregarConquistas()
   --------------------------------------------------------------------------
   Contagens e grade saem da MESMA lista (GET /api/perfil/conquistas, que
   devolve todas as medalhas marcando as conquistadas). Foi a separação entre
   número e lista que fez o web mostrar "5 conquistas" com uma só.
   ========================================================================== */

type Filtro = 'todas' | 'conquistadas' | 'bloqueadas';

export default function Conquistas() {
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () => {
    const [medalhas, perfil] = await Promise.all([API.conquistas(), API.perfil().catch(() => null)]);
    return { medalhas, provas: perfil?.provasAprovadas ?? 0 };
  });
  const [filtro, setFiltro] = useState<Filtro>('todas');
  const { ehMedio, ehGrande } = useResponsivo();

  const medalhas = dados?.medalhas ?? [];
  const conquistadas = medalhas.filter(m => m.conquistada);
  const visiveis = filtro === 'conquistadas' ? conquistadas
    : filtro === 'bloqueadas' ? medalhas.filter(m => !m.conquistada) : medalhas;

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Cabecalho titulo="Conquistas" subtitulo="Medalhas desbloqueadas ao concluir materiais, provas e cursos." />
      {carregando ? <Carregando /> : erro ? <ErroCarga mensagem={erro} aoTentar={recarregar} /> : (
        <>
          <Grade colunas={2}>
            <Kpi rotulo="Provas aprovadas" valor={dados!.provas} tom="primaria" />
            <Kpi rotulo="Medalhas" valor={`${conquistadas.length}/${medalhas.length}`} tom="alerta" />
          </Grade>
          <View style={{ height: espaco.lg }} />
          <Abas valor={filtro} onChange={setFiltro} abas={[
            { valor: 'todas', texto: 'Todas' },
            { valor: 'conquistadas', texto: `Obtidas (${conquistadas.length})` },
            { valor: 'bloqueadas', texto: `Bloqueadas (${medalhas.length - conquistadas.length})` }
          ]} />

          {visiveis.length === 0 ? (
            <Vazio icone="trophy" texto={filtro === 'conquistadas'
              ? 'Você ainda não desbloqueou nenhuma medalha. Conclua materiais e provas para começar.'
              : 'Nenhuma medalha nesta situação.'} />
          ) : (
            <Grade colunas={ehGrande ? 4 : ehMedio ? 3 : 2}>
              {visiveis.map(m => {
                const aberta = m.conquistada;
                return (
                  <View key={m.id} style={[st.cartao, aberta ? st.aberta : st.fechada]}>
                    <View style={[st.icone, aberta ? { backgroundColor: cores.alertaClaro } : { backgroundColor: cores.borda }]}>
                      <Icone nome={aberta ? iconeDaMedalha(m.nome) : 'lock'} tamanho={28}
                        cor={aberta ? cores.alertaEscuro : cores.textoApagado} />
                    </View>
                    <Text style={[st.titulo, !aberta && { color: cores.textoSecundario }]} numberOfLines={2}>{m.nome}</Text>
                    {aberta ? (
                      <>
                        <Texto v="rotulo" cor={cores.sucessoEscuro} centro>CONQUISTADA</Texto>
                        <Texto v="xs" cor={cores.textoApagado} centro>{FMT.relativo(m.dataConquista)}</Texto>
                      </>
                    ) : (
                      <Texto v="xs" cor={cores.textoApagado} centro numberOfLines={3}>{m.descricao || ''}</Texto>
                    )}
                    {!!m.raridade && <Texto v="rotulo" centro style={{ marginTop: 4 }}>{m.raridade.toUpperCase()}</Texto>}
                  </View>
                );
              })}
            </Grade>
          )}
        </>
      )}
    </Tela>
  );
}

const st = StyleSheet.create({
  cartao: {
    flex: 1, borderRadius: raio.lg, borderWidth: 1, padding: espaco.lg, alignItems: 'center', gap: 4, minHeight: 170
  },
  aberta: { backgroundColor: cores.superficie, borderColor: '#FCD34D' },
  fechada: { backgroundColor: cores.superficie2, borderColor: cores.borda, borderStyle: 'dashed' },
  icone: { width: 56, height: 56, borderRadius: raio.total, alignItems: 'center', justifyContent: 'center', marginBottom: espaco.sm },
  titulo: { fontFamily: fonte.semi, fontSize: 14, color: cores.texto, textAlign: 'center' }
});
