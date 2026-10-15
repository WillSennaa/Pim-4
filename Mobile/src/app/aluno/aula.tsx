import { useEffect, useRef, useState } from 'react';
import { ScrollView, StyleSheet, Text, View, type NativeScrollEvent, type NativeSyntheticEvent } from 'react-native';
import { router, Stack, useLocalSearchParams } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco, fonte } from '@/tema/tokens';
import { Avatar, Selo, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { ConteudoMaterial } from '@/ui/Codigo';
import { Carregando, ErroCarga, Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Material (aula) — pages/estudante/aula.html + renderAula() + concluirAula()
   --------------------------------------------------------------------------
   O botão final muda conforme a posição na trilha (próximo material, prova
   ou volta ao curso) — a mesma correção feita no web, onde o PIM III dizia
   sempre "ir para a Prova".

   UMA DIFERENÇA DELIBERADA: o app registra a LEITURA PARCIAL. Ao sair da
   tela sem concluir, ele envia até onde o aluno rolou o texto (campo
   PorcentagemAssistida, que já existia na tabela Progresso e no endpoint,
   mas nenhuma tela usava). O servidor nunca deixa o progresso retroceder,
   então enviar de novo é seguro (PUT idempotente). No curso, o material
   aparece com a barrinha de quanto já foi lido.
   ========================================================================== */

export default function Aula() {
  const params = useLocalSearchParams<{ curso: string; aula?: string }>();
  const idCurso = Number(params.curso);
  const idAula = params.aula ? Number(params.aula) : null;
  const { dados: curso, erro, carregando, recarregar } = useCarregar(() => API.curso(idCurso), [idCurso]);
  const { avisar } = useAvisos();
  const { margem } = useResponsivo();
  const insets = useSafeAreaInsets();
  const [concluindo, setConcluindo] = useState(false);

  // Até onde o aluno rolou (0 a 100). Ref, e não state: muda a cada quadro
  // de rolagem e não deve redesenhar a tela.
  const lido = useRef(0);
  const finalizado = useRef(false);

  const materiais = curso?.materiais ?? [];
  const atual = (idAula ? materiais.find(m => m.id === idAula) : undefined)
    ?? materiais.find(m => !m.concluido) ?? materiais[0];

  // Ao sair sem concluir, grava a leitura parcial (sem esperar resposta).
  useEffect(() => {
    if (!atual || atual.concluido) return;
    return () => {
      const pct = Math.round(lido.current);
      if (!finalizado.current && pct >= 10 && pct > atual.porcentagemAssistida) {
        API.progressoAula(atual.id, false, Math.min(pct, 99)).catch(() => { /* leitura parcial é melhor esforço */ });
      }
    };
  }, [atual]);

  if (carregando) return <Tela><Carregando texto="Abrindo material..." /></Tela>;
  if (erro || !curso) return <Tela><ErroCarga mensagem={erro ?? 'Curso não encontrado.'} aoTentar={recarregar} /></Tela>;
  if (!atual) return <Tela><Faixa texto="Este curso ainda não tem materiais publicados." /></Tela>;

  const indice = materiais.findIndex(m => m.id === atual.id);
  const proximo = materiais[indice + 1] ?? null;
  const todosFeitos = materiais.every(m => m.concluido || m.id === atual.id);
  const vaiParaProva = !proximo && curso.idProva != null && todosFeitos;

  function seguir() {
    if (proximo) router.replace({ pathname: '/aluno/aula', params: { curso: curso!.id, aula: proximo.id } });
    else if (vaiParaProva) router.replace({ pathname: '/aluno/prova/[id]', params: { id: curso!.id } });
    else router.back();
  }

  async function concluir() {
    setConcluindo(true);
    try {
      // Matrícula é idempotente no servidor. Garante que o curso entre no
      // histórico do aluno, que é de onde saem "Continue de onde parou" e o
      // certificado, mesmo que ele tenha aberto o material sem se matricular.
      if (!curso!.matriculado) await API.matricular(curso!.id);

      const r = await API.progressoAula(atual!.id, true, 100);
      finalizado.current = true;

      let msg = `Material concluído: ${r.aulasConcluidas} de ${r.totalAulas} (${r.percentualCurso}%). Nível ${r.nivel}, ${FMT.milhar(r.xpAtual)} XP.`;
      if (r.medalhasNovas?.length) msg += ` Nova conquista: ${r.medalhasNovas.join(', ')}.`;
      if (r.cursoConcluido) msg += ' Curso concluído — seu certificado já foi emitido.';
      avisar(msg, 'sucesso');
      seguir();
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível registrar o material.'), 'erro');
    } finally {
      setConcluindo(false);
    }
  }

  function aoRolar(e: NativeSyntheticEvent<NativeScrollEvent>) {
    const { contentOffset, contentSize, layoutMeasurement } = e.nativeEvent;
    const total = contentSize.height - layoutMeasurement.height;
    const pct = total <= 0 ? 100 : (contentOffset.y / total) * 100;
    if (pct > lido.current) lido.current = Math.min(100, pct);
  }

  const rotuloBotao = proximo
    ? `Concluir e ir para "${proximo.titulo || 'próximo material'}"`
    : vaiParaProva ? 'Concluir e ir para a prova' : 'Concluir e voltar ao curso';

  return (
    <View style={{ flex: 1, backgroundColor: cores.superficie }}>
      <Stack.Screen options={{ title: `Material ${indice + 1} de ${materiais.length}` }} />
      <ScrollView onScroll={aoRolar} scrollEventThrottle={200}
        contentContainerStyle={{ paddingHorizontal: margem, paddingTop: espaco.lg, paddingBottom: espaco.xl }}>
        <View style={st.leitura}>
          <Texto v="xs" cor={cores.textoApagado}>{curso.nome} › material {indice + 1} de {materiais.length}</Texto>
          <View style={{ flexDirection: 'row', gap: espaco.sm, marginTop: espaco.sm, flexWrap: 'wrap' }}>
            <Selo texto={(atual.tipo || 'material')} />
            <Selo texto={curso.nivel || 'sem nível'} tom="neutro" />
            {atual.concluido && <Selo texto="Concluído" tom="sucesso" icone="circle-check" />}
          </View>
          <Text style={st.titulo}>{atual.titulo || 'Material sem título'}</Text>
          <View style={st.autor}>
            <Avatar iniciais={curso.instrutorIniciais || FMT.iniciais(curso.instrutor)} tamanho={32} />
            <Texto v="sm" cor={cores.textoSecundario}>{curso.instrutor || 'A definir'}</Texto>
          </View>

          {atual.conteudo
            ? <ConteudoMaterial texto={atual.conteudo} />
            : <Faixa tom="alerta" texto="Este material ainda não tem conteúdo cadastrado. O tutor do curso escreve o texto pelo editor de curso." />}
        </View>
      </ScrollView>

      <View style={[st.rodape, { paddingHorizontal: margem, paddingBottom: espaco.md + insets.bottom }]}>
        <View style={st.leitura}>
          {atual.concluido ? (
            <>
              <Botao cheio tamanho="lg" iconeDireita="arrow-right" onPress={seguir}
                texto={proximo ? 'Próximo material' : vaiParaProva ? 'Ir para a prova' : 'Voltar ao curso'} />
              <Texto v="xs" cor={cores.textoApagado} centro style={{ marginTop: espaco.sm }}>
                Material já concluído. Você está revendo o conteúdo.
              </Texto>
            </>
          ) : (
            <Botao cheio tamanho="lg" iconeDireita="arrow-right" texto={rotuloBotao}
              carregando={concluindo} textoCarregando="Registrando..." onPress={concluir} />
          )}
        </View>
      </View>
    </View>
  );
}

const st = StyleSheet.create({
  leitura: { width: '100%', maxWidth: 720, alignSelf: 'center' },
  titulo: { fontFamily: fonte.negrito, fontSize: 24, color: cores.texto, marginTop: espaco.md, lineHeight: 30 },
  autor: {
    flexDirection: 'row', alignItems: 'center', gap: espaco.sm, marginTop: espaco.md, marginBottom: espaco.lg,
    paddingBottom: espaco.lg, borderBottomWidth: 1, borderBottomColor: cores.borda
  },
  rodape: { borderTopWidth: 1, borderTopColor: cores.borda, backgroundColor: cores.superficie, paddingTop: espaco.md }
});
