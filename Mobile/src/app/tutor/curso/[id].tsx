import { useState } from 'react';
import { View } from 'react-native';
import { router, Stack, useLocalSearchParams } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import type { DadosCurso, Material, QuestaoCompletaTutor } from '@/api/tipos';
import { useAcao, useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { useSessao } from '@/sessao/SessaoContext';
import { cores, espaco } from '@/tema/tokens';
import { CaixaIcone, Cartao, ItemLista, Selo, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao, BotaoIcone } from '@/ui/Botao';
import { Carregando, ErroCarga, Faixa } from '@/ui/Estados';
import { Grade, Secao, Tela } from '@/ui/Tela';
import { FormCurso } from '@/telas/FormCurso';
import { estadoDoCurso } from '@/util/estadosCurso';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Editor de curso — pages/tutor/editar-curso.html + carregarEditorCurso()
   --------------------------------------------------------------------------
   O que a tela permite depende do ESTADO do curso, porque o servidor recusa
   alteração em curso publicado ou em avaliação. Oferecer o campo e deixar a
   API negar seria frustração gratuita. A permissão vem do próprio servidor
   (podeEditar, no CursoTutorDto), não é deduzida aqui.

   Curso REJEITADO mostra o motivo: o parecer do administrador chega como
   chamado do tipo "Avaliacao" endereçado ao tutor (a tabela Curso não tem
   campo de parecer). O web não exibia esse motivo no editor.

   A prova é opcional e única por curso: no modelo, a chave estrangeira da
   prova fica no curso (1 curso : 1 prova).
   ========================================================================== */

export default function EditorCurso() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const idCurso = Number(id);
  const { usuario } = useSessao();
  const { avisar, confirmar } = useAvisos();
  const { ocupado, rodar } = useAcao();
  const { ehTablet } = useResponsivo();
  const [salvandoDados, setSalvandoDados] = useState(false);

  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () => {
    const [curso, meus] = await Promise.all([API.curso(idCurso), API.tutorCursos()]);
    const resumo = meus.find(c => c.id === idCurso) ?? null;
    const prova = curso.idProva ? await API.tutorProvaCompleta(curso.idProva).catch(() => null) : null;

    let parecer: string | null = null;
    if ((curso.status || '').toLowerCase() === 'rejeitado') {
      const caixa = await API.chamados().catch(() => []);
      const ultimo = caixa
        .filter(c => c.tipo === 'Avaliacao' && c.idDestinatario === usuario?.id && (c.assunto || '').includes(curso.nome))
        .sort((a, b) => new Date(b.dataAbertura).getTime() - new Date(a.dataAbertura).getTime())[0];
      parecer = ultimo?.descricao ?? null;
    }
    return { curso, resumo, prova, parecer };
  }, [idCurso, usuario?.id]);

  if (carregando) return <Tela><Carregando texto="Abrindo o curso..." /></Tela>;
  if (erro || !dados) return <Tela><ErroCarga mensagem={erro ?? 'Curso não encontrado.'} aoTentar={recarregar} /></Tela>;

  const { curso, resumo, prova, parecer } = dados;
  const estado = estadoDoCurso(curso.status);
  const editavel = resumo?.podeEditar ?? ['rascunho', 'rejeitado'].includes((curso.status || '').toLowerCase());
  const aulas = curso.materiais ?? [];
  const questoes = [...(prova?.questoes ?? [])].sort((a, b) => a.ordem - b.ordem);

  async function salvarDados(d: DadosCurso) {
    setSalvandoDados(true);
    try { await API.tutorAtualizarCurso(curso.id, d); avisar('Dados do curso salvos.'); await recarregar(); }
    catch (e) { avisar(mensagemDe(e, 'Não foi possível salvar.'), 'erro'); }
    finally { setSalvandoDados(false); }
  }

  async function removerAula(a: Material) {
    const ok = await confirmar({ titulo: 'Remover material?', texto: `"${a.titulo}" será excluído do curso.`, confirmar: 'Remover', perigo: true });
    if (!ok) return;
    await rodar('m' + a.id, async () => {
      // O servidor recusa remover material que algum aluno já estudou.
      try { await API.tutorRemoverAula(a.id); avisar('Material removido.'); await recarregar(); }
      catch (e) { avisar(mensagemDe(e, 'Não foi possível remover o material.'), 'erro'); }
    });
  }

  async function removerQuestao(q: QuestaoCompletaTutor) {
    const ok = await confirmar({ titulo: `Remover a questão ${q.ordem}?`, confirmar: 'Remover', perigo: true });
    if (!ok) return;
    await rodar('q' + q.id, async () => {
      // O servidor recusa alterar prova que algum aluno já respondeu.
      try { await API.tutorRemoverQuestao(q.id); avisar('Questão removida.'); await recarregar(); }
      catch (e) { avisar(mensagemDe(e, 'Não foi possível remover a questão.'), 'erro'); }
    });
  }

  async function submeter() {
    const ok = await confirmar({
      titulo: 'Enviar para avaliação?',
      texto: 'O administrador avalia e publica. Enquanto estiver em avaliação, o conteúdo fica bloqueado para alteração.',
      confirmar: 'Submeter', icone: 'upload'
    });
    if (!ok) return;
    await rodar('sub', async () => {
      try { await API.tutorSubmeterCurso(curso.id); avisar('Curso enviado para avaliação.'); await recarregar(); }
      catch (e) { avisar(mensagemDe(e, 'Não foi possível submeter o curso.'), 'erro'); }
    });
  }

  const blocoDados = (
    <View>
      <Secao titulo="Dados do curso" />
      <Cartao>
        <FormCurso key={`${curso.nome}|${curso.descricao}|${curso.categoria}|${curso.nivel}|${curso.duracaoHoras}`} inicial={curso} editavel={editavel} textoBotao="Salvar dados do curso" salvando={salvandoDados} aoSalvar={salvarDados} />
      </Cartao>
    </View>
  );

  const blocoMateriais = (
    <View>
      <Secao titulo={`Materiais (${aulas.length})`} direita={editavel
        ? <Botao texto="Adicionar" icone="plus" tamanho="sm" variante="secundario"
            onPress={() => router.push({ pathname: '/tutor/material', params: { curso: curso.id } })} />
        : undefined} />
      <Cartao style={{ paddingVertical: aulas.length ? 4 : espaco.lg }}>
        {aulas.length === 0 ? (
          <Texto v="sm" cor={cores.textoApagado}>Nenhum material cadastrado. Um curso sem material não pode ser submetido.</Texto>
        ) : aulas.map((a, i) => (
          <ItemLista key={a.id}
            esquerda={<CaixaIcone tom={a.conteudo ? 'sucesso' : 'alerta'}><Texto v="sm" peso="negrito" cor={a.conteudo ? cores.sucessoEscuro : cores.alertaEscuro}>{i + 1}</Texto></CaixaIcone>}
            titulo={a.titulo || 'Material sem título'}
            subtitulo={`${a.tipo || 'texto'} · ${a.conteudo ? 'com conteúdo' : 'sem conteúdo'}`}
            ultimo={i === aulas.length - 1}
            onPress={() => router.push({ pathname: '/tutor/material', params: { curso: curso.id, id: a.id } })}
            direita={editavel
              ? <BotaoIcone icone="trash" cor={cores.perigo} rotulo={`Remover ${a.titulo}`} carregando={ocupado === 'm' + a.id} onPress={() => removerAula(a)} />
              : undefined}
          />
        ))}
      </Cartao>
    </View>
  );

  const blocoProva = (
    <View>
      <Secao titulo="Prova" />
      {!curso.idProva ? (
        <Cartao>
          <Texto v="sm" cor={cores.textoApagado} style={{ marginBottom: editavel ? espaco.md : 0 }}>Este curso ainda não tem prova.</Texto>
          {editavel && <Botao texto="Criar prova" icone="plus" variante="secundario" cheio
            onPress={() => router.push({ pathname: '/tutor/prova', params: { curso: curso.id, nome: curso.nome } })} />}
        </Cartao>
      ) : (
        <Cartao>
          <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: espaco.sm }}>
            <Texto v="corpo" peso="semi" style={{ flex: 1 }}>{prova?.titulo || 'Prova'}</Texto>
            <Selo texto={FMT.plural(questoes.length, 'questão', 'questões')} tom="neutro" />
          </View>
          {prova && <Texto v="xs" style={{ marginTop: 2 }}>Nota mínima {FMT.nota(prova.notaMinima)} · {prova.tempoMinutos} min</Texto>}
          {questoes.length === 0 && (
            <View style={{ marginTop: espaco.md }}>
              <Faixa tom="perigo" icone="alert-triangle" texto="Prova sem questões: o administrador vai rejeitar." />
            </View>
          )}
          {questoes.map((q, i) => (
            <ItemLista key={q.id}
              esquerda={<CaixaIcone><Texto v="sm" peso="negrito" cor={cores.primariaEscura}>{q.ordem}</Texto></CaixaIcone>}
              titulo={q.enunciado}
              subtitulo={`${q.alternativas.length} alternativas · gabarito ${q.alternativas.find(a => a.ehCorreta)?.letra ?? '?'}`}
              ultimo={i === questoes.length - 1}
              direita={editavel
                ? <BotaoIcone icone="trash" cor={cores.perigo} rotulo={`Remover questão ${q.ordem}`} carregando={ocupado === 'q' + q.id} onPress={() => removerQuestao(q)} />
                : null}
            />
          ))}
          {editavel && (
            <Botao texto="Adicionar questão" icone="plus" variante="secundario" cheio style={{ marginTop: espaco.md }}
              onPress={() => router.push({
                pathname: '/tutor/questao',
                params: { prova: curso.idProva!, ordem: (questoes.reduce((m, q) => Math.max(m, q.ordem), 0) + 1) }
              })} />
          )}
        </Cartao>
      )}
    </View>
  );

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Stack.Screen options={{ title: curso.nome }} />

      <View style={{ flexDirection: 'row', alignItems: 'center', gap: espaco.sm, marginBottom: espaco.md, flexWrap: 'wrap' }}>
        <Selo texto={estado.rotulo} tom={estado.tom} />
        {resumo && <Texto v="xs">{FMT.plural(resumo.totalMatriculados, 'aluno matriculado', 'alunos matriculados')}</Texto>}
      </View>

      {!editavel && (
        <Faixa tom="alerta" icone="lock" texto={`Curso ${estado.rotulo.toLowerCase()}: o conteúdo não pode ser alterado. Alterar materiais ou questões agora mudaria o curso debaixo de alunos já matriculados e de provas já respondidas.`} />
      )}
      {parecer && (
        <Faixa tom="perigo" icone="message-circle" texto={`Parecer da administração: ${parecer}`} />
      )}

      {ehTablet ? (
        <Grade colunas={2}>
          {blocoDados}
          <View>{blocoMateriais}{blocoProva}</View>
        </Grade>
      ) : (
        <>{blocoDados}{blocoMateriais}{blocoProva}</>
      )}

      {editavel && (
        <Cartao style={{ marginTop: espaco.lg, borderColor: cores.primaria }}>
          <Texto v="secao">Enviar para avaliação</Texto>
          <Texto v="sm" cor={cores.textoSecundario} style={{ marginTop: 4, marginBottom: espaco.md }}>
            O administrador avalia e publica. Enquanto estiver em avaliação, o conteúdo fica bloqueado para alteração.
          </Texto>
          <Botao texto="Submeter curso" icone="upload" tamanho="lg" cheio carregando={ocupado === 'sub'} onPress={submeter} />
        </Cartao>
      )}
    </Tela>
  );
}
