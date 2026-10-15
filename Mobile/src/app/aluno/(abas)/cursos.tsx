import { useMemo, useState } from 'react';
import { router } from 'expo-router';
import { View } from 'react-native';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { cores, espaco } from '@/tema/tokens';
import { Progresso, Texto } from '@/ui/base';
import { Abas } from '@/ui/Abas';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { CartaoCurso, Meta } from '@/ui/Cartoes';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Cabecalho, Grade, Tela } from '@/ui/Tela';
import { capaDoCurso } from '@/util/decoracao';
import { normalizar } from '@/util/formatacao';

/* ==========================================================================
   Catálogo — pages/estudante/cursos.html
   --------------------------------------------------------------------------
   Lista os cursos PUBLICADOS (GET /api/cursos) e cruza com o histórico do
   aluno para mostrar o progresso em cada um — uma chamada para a lista e uma
   para o histórico, em vez de uma por curso.

   Busca e filtro por categoria são locais: a lista inteira já está na tela.
   ========================================================================== */

type Filtro = 'todos' | 'meus' | 'novos';

export default function CatalogoCursos() {
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () => {
    const [cursos, historico] = await Promise.all([API.cursos(), API.historico().catch(() => [])]);
    const progresso = new Map(historico.map(h => [h.idCurso, h]));
    return { cursos, progresso };
  });
  const [busca, setBusca] = useState('');
  const [filtro, setFiltro] = useState<Filtro>('todos');

  const lista = useMemo(() => {
    if (!dados) return [];
    const termo = normalizar(busca);
    return dados.cursos.filter(c => {
      const matriculado = dados.progresso.has(c.id);
      if (filtro === 'meus' && !matriculado) return false;
      if (filtro === 'novos' && matriculado) return false;
      return !termo || normalizar(`${c.nome} ${c.categoria} ${c.instrutor}`).includes(termo);
    });
  }, [dados, busca, filtro]);

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Cabecalho titulo="Cursos" subtitulo="Escolha uma trilha e avance no seu ritmo." />
      <Campo icone="search" placeholder="Buscar por curso, categoria ou tutor" value={busca} onChangeText={setBusca}
        returnKeyType="search" />
      <Abas pilulas valor={filtro} onChange={setFiltro}
        abas={[{ valor: 'todos', texto: 'Todos' }, { valor: 'meus', texto: 'Matriculado' }, { valor: 'novos', texto: 'Disponíveis' }]} />

      {carregando ? <Carregando /> : erro ? <ErroCarga mensagem={erro} aoTentar={recarregar} /> : lista.length === 0 ? (
        <Vazio icone="book-2" texto={dados?.cursos.length ? 'Nenhum curso encontrado com esse filtro.' : 'Nenhum curso publicado no momento.'} />
      ) : (
        <Grade>
          {lista.map(c => {
            const capa = capaDoCurso(c.nome, c.categoria);
            const h = dados!.progresso.get(c.id);
            const pct = h?.percentualCurso ?? 0;
            const concluido = h?.status === 'Concluido';
            const abrir = () => router.push({ pathname: '/aluno/curso/[id]', params: { id: c.id } });
            return (
              <CartaoCurso key={c.id} titulo={c.nome} rotulo={c.nivel || c.categoria || 'curso'}
                gradiente={capa.gradiente} icone={capa.icone} onPress={abrir}>
                {!!c.descricao && <Texto v="sm" cor={cores.textoSecundario} numberOfLines={3} style={{ marginBottom: espaco.md }}>{c.descricao}</Texto>}
                <Meta itens={[
                  { icone: 'clock', texto: `${c.duracaoHoras || 0}h` },
                  { icone: 'user', texto: c.instrutor || 'A definir' },
                  ...(c.temProva ? [{ icone: 'file-text' as const, texto: 'com prova' }] : [])
                ]} />
                {h && (
                  <View style={{ marginBottom: espaco.md }}>
                    <View style={{ flexDirection: 'row', justifyContent: 'space-between', marginBottom: 4 }}>
                      <Texto v="xs">{concluido ? 'Concluído' : 'Progresso'}</Texto>
                      <Texto v="xs" peso="semi" cor={cores.texto}>{pct}%</Texto>
                    </View>
                    <Progresso valor={pct} tom={concluido ? 'sucesso' : 'primaria'} />
                  </View>
                )}
                <View style={{ flex: 1 }} />
                <Botao cheio onPress={abrir} iconeDireita="arrow-right"
                  variante={concluido ? 'fantasma' : 'primario'}
                  texto={concluido ? 'Rever curso' : pct > 0 ? 'Continuar' : h ? 'Começar' : 'Ver curso'} />
              </CartaoCurso>
            );
          })}
        </Grade>
      )}
    </Tela>
  );
}
