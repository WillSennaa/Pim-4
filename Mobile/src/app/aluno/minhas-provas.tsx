import { useState } from 'react';
import { View } from 'react-native';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco } from '@/tema/tokens';
import { Cartao, Selo, Texto } from '@/ui/base';
import { Abas } from '@/ui/Abas';
import { Kpi } from '@/ui/Cartoes';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Grade, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Minhas provas — pages/estudante/minhas-provas.html + carregarMinhasProvas()
   --------------------------------------------------------------------------
   Os dados vêm da STORED PROCEDURE SP_RelatorioDesempenhoEstudante, exposta
   em GET /api/perfil/desempenho. É o ponto do app em que a procedure do
   banco (requisito do PB02) aparece para o usuário.

   Os quatro indicadores saem das MESMAS linhas listadas abaixo, então a soma
   nunca discorda da lista. A procedure não devolve a nota mínima de cada
   prova; 7,0 é o padrão do modelo, a mesma convenção do web.
   ========================================================================== */

const NOTA_PADRAO = 7;
type Filtro = 'todas' | 'aprovado' | 'reprovado';

export default function MinhasProvas() {
  const { dados: linhas, erro, carregando, atualizando, puxar, recarregar } = useCarregar(() => API.desempenho());
  const [filtro, setFiltro] = useState<Filtro>('todas');
  const { colunasKpi, ehMedio } = useResponsivo();

  if (carregando) return <Tela><Carregando /></Tela>;
  if (erro || !linhas) return <Tela><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;

  const aprovou = (n: number | null) => (n ?? 0) >= NOTA_PADRAO;
  const aprovadas = linhas.filter(l => aprovou(l.nota)).length;
  const media = linhas.length ? linhas.reduce((t, l) => t + (l.nota ?? 0), 0) / linhas.length : null;
  const visiveis = linhas
    .filter(l => filtro === 'todas' || (filtro === 'aprovado') === aprovou(l.nota))
    .sort((a, b) => new Date(b.dataRealizacao).getTime() - new Date(a.dataRealizacao).getTime());

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Grade colunas={colunasKpi}>
        <Kpi rotulo="Realizadas" valor={linhas.length} tom="primaria" />
        <Kpi rotulo="Aprovadas" valor={aprovadas} tom="sucesso" />
        <Kpi rotulo="Reprovadas" valor={linhas.length - aprovadas} tom="perigo" />
        <Kpi rotulo="Média" valor={FMT.nota(media)} tom="alerta" />
      </Grade>
      <View style={{ height: espaco.lg }} />
      <Abas valor={filtro} onChange={setFiltro} abas={[
        { valor: 'todas', texto: 'Todas' }, { valor: 'aprovado', texto: 'Aprovadas' }, { valor: 'reprovado', texto: 'Reprovadas' }
      ]} />

      {visiveis.length === 0 ? (
        <Vazio icone="clipboard-list" texto={linhas.length ? 'Nenhuma prova nesta situação.' : 'Você ainda não realizou nenhuma prova.'} />
      ) : (
        <Grade colunas={ehMedio ? 2 : 1}>
          {visiveis.map((l, i) => {
            const ok = aprovou(l.nota);
            return (
              <Cartao key={i} destaqueLateral={ok ? 'sucesso' : 'perigo'} style={{ marginBottom: 0, flex: 1 }}>
                <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: espaco.sm, marginBottom: espaco.sm }}>
                  <Texto v="corpo" peso="semi" style={{ flex: 1 }} numberOfLines={2}>{l.prova || 'Prova'}</Texto>
                  <Selo texto={ok ? 'Aprovado' : 'Reprovado'} tom={ok ? 'sucesso' : 'perigo'} />
                </View>
                <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
                  <Texto v="sm" cor={cores.textoSecundario}>
                    Nota <Texto v="sm" peso="negrito" cor={ok ? cores.sucessoEscuro : cores.perigoEscuro}>{FMT.nota(l.nota)}</Texto>
                  </Texto>
                  <Texto v="sm" cor={cores.textoSecundario}>Tentativa {l.tentativas}</Texto>
                  <Texto v="sm" cor={cores.textoSecundario}>{FMT.data(l.dataRealizacao)}</Texto>
                </View>
              </Cartao>
            );
          })}
        </Grade>
      )}
    </Tela>
  );
}
