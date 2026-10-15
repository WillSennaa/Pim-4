import { useState } from 'react';
import { View } from 'react-native';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import type { CursoTutor } from '@/api/tipos';
import { useAcao, useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { espaco } from '@/tema/tokens';
import { Selo } from '@/ui/base';
import { Abas } from '@/ui/Abas';
import { useAvisos } from '@/ui/Avisos';
import { Botao, BotaoIcone } from '@/ui/Botao';
import { CartaoCurso, Kpi, Meta } from '@/ui/Cartoes';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Cabecalho, Grade, Tela } from '@/ui/Tela';
import { capaDoCurso } from '@/util/decoracao';
import { estadoDoCurso } from '@/util/estadosCurso';

/* ==========================================================================
   Meus cursos — pages/tutor/cursos.html
   --------------------------------------------------------------------------
   Os estados vêm de FluxoPublicacaoCurso no domínio:
     Rascunho -> (submeter) -> Pendente -> (admin) -> Publicado | Rejeitado
   Os botões de cada cartão dependem do estado, porque o servidor recusa
   transição inválida. A tela só não oferece o que seria recusado; quem
   garante a regra é o servidor (podeEditar vem no CursoTutorDto).
   ========================================================================== */

type Filtro = 'todos' | 'publicado' | 'pendente' | 'rascunho' | 'rejeitado';

export default function CursosTutor() {
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(() => API.tutorCursos());
  const { avisar, confirmar } = useAvisos();
  const { ocupado, rodar } = useAcao();
  const { colunasKpi } = useResponsivo();
  const [filtro, setFiltro] = useState<Filtro>('todos');

  const todos = dados ?? [];
  const conta = (s: string) => todos.filter(c => c.status.toLowerCase() === s).length;
  const lista = filtro === 'todos' ? todos : todos.filter(c => c.status.toLowerCase() === filtro);

  async function submeter(c: CursoTutor) {
    const ok = await confirmar({
      titulo: 'Enviar para avaliação?',
      texto: `"${c.nome}" vai para o administrador. Enquanto estiver em avaliação, o conteúdo fica bloqueado para alteração.`,
      confirmar: 'Submeter', icone: 'upload'
    });
    if (!ok) return;
    await rodar('s' + c.id, async () => {
      try {
        await API.tutorSubmeterCurso(c.id);
        avisar('Curso enviado para avaliação.');
        await recarregar();
      } catch (e) {
        // O servidor recusa curso sem material e curso já publicado; a
        // mensagem dele é mais precisa que qualquer texto fixo aqui.
        avisar(mensagemDe(e, 'Não foi possível submeter o curso.'), 'erro');
      }
    });
  }

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Cabecalho titulo="Meus cursos" subtitulo="Crie, edite e envie seus cursos para avaliação."
        direita={<Botao texto="Novo" icone="plus" tamanho="sm" onPress={() => router.push('/tutor/curso/novo')} />} />

      {carregando ? <Carregando /> : erro ? <ErroCarga mensagem={erro} aoTentar={recarregar} /> : (
        <>
          <Grade colunas={colunasKpi}>
            <Kpi rotulo="Publicados" valor={conta('publicado')} tom="sucesso" />
            <Kpi rotulo="Rascunhos" valor={conta('rascunho')} tom="neutro" />
            <Kpi rotulo="Em avaliação" valor={conta('pendente')} tom="alerta" />
            <Kpi rotulo="Alunos" valor={todos.reduce((t, c) => t + (c.totalMatriculados || 0), 0)} detalhe="matrículas" tom="primaria" />
          </Grade>
          <View style={{ height: espaco.lg }} />
          <Abas pilulas valor={filtro} onChange={setFiltro} abas={[
            { valor: 'todos', texto: 'Todos' }, { valor: 'publicado', texto: 'Publicados' },
            { valor: 'pendente', texto: 'Em avaliação' }, { valor: 'rascunho', texto: 'Rascunhos' },
            { valor: 'rejeitado', texto: 'Rejeitados' }
          ]} />

          {lista.length === 0 ? (
            <Vazio icone="book" texto={todos.length ? 'Nenhum curso nesta situação.' : 'Você ainda não criou nenhum curso.'}
              acao={!todos.length ? { texto: 'Criar meu primeiro curso', onPress: () => router.push('/tutor/curso/novo') } : undefined} />
          ) : (
            <Grade>
              {lista.map(c => {
                const e = estadoDoCurso(c.status);
                const capa = capaDoCurso(c.nome, c.categoria);
                const abrir = () => router.push({ pathname: '/tutor/curso/[id]', params: { id: c.id } });
                return (
                  <CartaoCurso key={c.id} titulo={c.nome} rotulo={e.rotulo} gradiente={capa.gradiente} icone={capa.icone} onPress={abrir}>
                    <Meta itens={[
                      { icone: 'users', texto: `${c.totalMatriculados || 0} alunos` },
                      { icone: 'list', texto: `${c.totalAulas || 0} materiais` },
                      { icone: 'help-circle', texto: `${c.totalQuestoes || 0} questões` }
                    ]} />
                    <Selo texto={e.rotulo} tom={e.tom} />
                    <View style={{ flex: 1, minHeight: espaco.md }} />
                    <View style={{ flexDirection: 'row', gap: espaco.sm, alignItems: 'center' }}>
                      {c.podeEditar ? (
                        <>
                          <Botao texto="Editar" icone="edit" variante="secundario" tamanho="sm" onPress={abrir} style={{ flex: 1 }} />
                          <BotaoIcone icone="upload" rotulo="Submeter para avaliação" carregando={ocupado === 's' + c.id}
                            onPress={() => submeter(c)} />
                        </>
                      ) : (
                        <Botao texto="Ver detalhes" icone="eye" variante="fantasma" tamanho="sm" onPress={abrir} style={{ flex: 1 }} />
                      )}
                    </View>
                  </CartaoCurso>
                );
              })}
            </Grade>
          )}
        </>
      )}
    </Tela>
  );
}
