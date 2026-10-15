import { useEffect, useRef, useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { router, Stack, useLocalSearchParams, useNavigation } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import type { CursoDetalhe, Prova } from '@/api/tipos';
import { resultadoProva } from '@/estado/resultadoProva';
import { useCarregar } from '@/hooks/useCarregar';
import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { Icone, Progresso, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { BlocoCodigo } from '@/ui/Codigo';
import { Carregando, ErroCarga, Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Prova — pages/estudante/prova.html + objeto QUIZ do app.js
   --------------------------------------------------------------------------
   O parâmetro da rota é o id do CURSO (como no web: prova.html?id=N). A
   prova é resolvida pelo idProva do curso. Tratar o número como id de prova
   carregaria a prova errada silenciosamente, porque os dois são inteiros —
   a mesma armadilha documentada no QUIZ.resolverIdProva do web.

   SEGURANÇA DA PROVA (resposta pronta para a banca): o app recebe a prova
   SEM gabarito. Ele envia só as letras marcadas; quem corrige, calcula a
   nota, grava em Desempenho e concede XP é o servidor. Não há nada no APK
   que permita descobrir a resposta certa antes de entregar.

   Sair no meio da prova pede confirmação (botão voltar do Android ou gesto):
   as respostas ficam só na memória até a entrega.
   ========================================================================== */

interface DadosProva { curso: CursoDetalhe; prova: Prova; }

export default function ProvaAluno() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const idCurso = Number(id);
  const navigation = useNavigation();
  const { avisar, confirmar } = useAvisos();

  const { dados, erro, carregando, recarregar } = useCarregar<DadosProva>(async () => {
    const curso = await API.curso(idCurso);
    if (!curso?.idProva) throw new Error('Este curso não tem prova cadastrada.');
    const prova = await API.prova(curso.idProva);
    if (!prova.questoes?.length) throw new Error('A prova deste curso ainda não tem questões.');
    prova.questoes.sort((a, b) => a.ordem - b.ordem);
    return { curso, prova };
  }, [idCurso]);

  const [indice, setIndice] = useState(0);
  const [respostas, setRespostas] = useState<Record<number, string>>({});
  const [enviando, setEnviando] = useState(false);
  const entregue = useRef(false);
  const temResposta = Object.keys(respostas).length > 0;

  // Confirmação ao sair com respostas marcadas e prova não entregue.
  useEffect(() => {
    const cancelar = navigation.addListener('beforeRemove', (e: { preventDefault: () => void; data: { action: unknown } }) => {
      if (entregue.current || !temResposta) return;
      e.preventDefault();
      void confirmar({
        titulo: 'Sair da prova?',
        texto: 'Suas respostas ainda não foram enviadas e serão perdidas.',
        confirmar: 'Sair', cancelar: 'Continuar a prova', perigo: true
      }).then(sair => {
        if (sair) { entregue.current = true; navigation.dispatch(e.data.action as never); }
      });
    });
    return cancelar;
  }, [navigation, temResposta, confirmar]);

  if (carregando) return <Tela><Carregando texto="Preparando a prova..." /></Tela>;
  if (erro || !dados) return <Tela><ErroCarga mensagem={erro ?? 'Prova não encontrada.'} aoTentar={recarregar} /></Tela>;

  const { prova, curso } = dados;
  const total = prova.questoes.length;
  const q = prova.questoes[indice];
  const marcada = respostas[q.id];
  const ultima = indice === total - 1;
  const pct = Math.round(((indice + 1) / total) * 100);
  const letras = [...q.alternativas].filter(a => a.letra).sort((a, b) => String(a.letra).localeCompare(String(b.letra)));

  async function finalizar() {
    const respondidas = Object.keys(respostas).length;
    const ok = await confirmar({
      titulo: 'Entregar a prova?',
      texto: `Você respondeu ${respondidas} de ${total} questões. Depois de entregar não é possível alterar as respostas.`,
      confirmar: 'Entregar', icone: 'send'
    });
    if (!ok) return;

    setEnviando(true);
    try {
      const lista = prova.questoes
        .filter(x => respostas[x.id])
        .map(x => ({ idQuestao: x.id, letra: respostas[x.id] }));
      const r = await API.submeterProva(prova.id, lista);

      resultadoProva.guardar({
        resultado: r,
        idCurso: curso.id,
        tituloProva: prova.titulo || 'Prova',
        questoes: prova.questoes.map(x => ({
          id: x.id,
          enunciado: x.enunciado,
          codigo: x.codigoExemplo,
          alternativas: Object.fromEntries(x.alternativas.map(a => [a.letra ?? '', a.texto ?? '']))
        }))
      });
      entregue.current = true;
      router.replace('/aluno/resultado');
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível enviar a prova. Tente novamente.'), 'erro');
    } finally {
      setEnviando(false);
    }
  }

  const rodape = (
    <View style={{ flexDirection: 'row', gap: espaco.sm }}>
      <Botao texto="Anterior" icone="arrow-left" variante="fantasma" desabilitado={indice === 0 || enviando}
        onPress={() => setIndice(i => i - 1)} style={{ flex: 1 }} />
      <Botao
        texto={ultima ? 'Finalizar prova' : 'Próxima'}
        iconeDireita={ultima ? 'check' : 'arrow-right'}
        variante={ultima ? 'sucesso' : 'primario'}
        desabilitado={!marcada}
        carregando={enviando}
        textoCarregando="Corrigindo..."
        onPress={ultima ? finalizar : () => setIndice(i => i + 1)}
        style={{ flex: 1.4 }}
      />
    </View>
  );

  return (
    <Tela largura="leitura" rodape={rodape}>
      <Stack.Screen options={{ title: prova.titulo || 'Prova' }} />

      <Texto v="secao">{prova.titulo || 'Prova'}</Texto>
      <Texto v="xs" style={{ marginTop: 2, marginBottom: espaco.md }}>
        {curso.nome} · {FMT.plural(total, 'questão', 'questões')} · nota mínima {FMT.nota(prova.notaMinima)}
        {prova.tempoMinutos ? ` · ${prova.tempoMinutos} min` : ''}
      </Texto>

      <View style={st.progressoTopo}>
        <Texto v="sm">Questão <Text style={{ fontFamily: fonte.semi }}>{indice + 1} de {total}</Text></Texto>
        <Texto v="sm" cor={cores.primaria} peso="semi">{pct}% concluído</Texto>
      </View>
      <Progresso valor={pct} />

      {/* Navegação direta entre questões: no celular, voltar uma a uma até a 2 é tedioso. */}
      <View style={st.pontos}>
        {prova.questoes.map((x, i) => (
          <Pressable key={x.id} onPress={() => setIndice(i)} accessibilityLabel={`Ir para a questão ${i + 1}`}
            style={[st.ponto, respostas[x.id] && st.pontoRespondido, i === indice && st.pontoAtual]}>
            <Text style={[st.pontoTexto, (respostas[x.id] || i === indice) && { color: i === indice ? cores.primaria : '#FFFFFF' }]}>{i + 1}</Text>
          </Pressable>
        ))}
      </View>

      <Text style={st.enunciado}>{q.enunciado}</Text>
      {!!q.codigoExemplo && <BlocoCodigo codigo={q.codigoExemplo} rotulo="EXEMPLO" />}

      <View style={{ gap: espaco.sm, marginTop: espaco.md }}>
        {letras.map(a => {
          const sel = marcada === a.letra;
          return (
            <Pressable key={a.id} onPress={() => setRespostas(r => ({ ...r, [q.id]: a.letra! }))}
              accessibilityRole="radio" accessibilityState={{ selected: sel }}
              style={({ pressed }) => [st.opcao, sel && st.opcaoSel, pressed && { opacity: 0.85 }]}>
              <View style={[st.letra, sel && { backgroundColor: cores.primaria, borderColor: cores.primaria }]}>
                <Text style={[st.letraTexto, sel && { color: '#FFFFFF' }]}>{a.letra}</Text>
              </View>
              <Text style={st.opcaoTexto}>{a.texto}</Text>
              {sel && <Icone nome="check" tamanho={18} cor={cores.primaria} />}
            </Pressable>
          );
        })}
      </View>

      {!marcada && <Faixa tom="primaria" icone="info-circle" texto="Marque uma alternativa para avançar." />}
    </Tela>
  );
}

const st = StyleSheet.create({
  progressoTopo: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 },
  pontos: { flexDirection: 'row', flexWrap: 'wrap', gap: 6, marginTop: espaco.md, marginBottom: espaco.lg },
  ponto: {
    width: 32, height: 32, borderRadius: raio.total, borderWidth: 1.5, borderColor: cores.bordaForte,
    alignItems: 'center', justifyContent: 'center', backgroundColor: cores.superficie
  },
  pontoRespondido: { backgroundColor: cores.primaria, borderColor: cores.primaria },
  pontoAtual: { borderColor: cores.primaria, borderWidth: 2.5, backgroundColor: cores.primariaMaisClara },
  pontoTexto: { fontFamily: fonte.semi, fontSize: 12, color: cores.textoSecundario },
  enunciado: { fontFamily: fonte.semi, fontSize: 17, lineHeight: 25, color: cores.texto },
  opcao: {
    flexDirection: 'row', alignItems: 'center', gap: espaco.md, padding: espaco.md,
    borderWidth: 1.5, borderColor: cores.borda, borderRadius: raio.md, backgroundColor: cores.superficie
  },
  opcaoSel: { borderColor: cores.primaria, backgroundColor: cores.primariaMaisClara },
  letra: {
    width: 32, height: 32, borderRadius: raio.sm, borderWidth: 1.5, borderColor: cores.bordaForte,
    alignItems: 'center', justifyContent: 'center'
  },
  letraTexto: { fontFamily: fonte.negrito, fontSize: 14, color: cores.textoSecundario },
  opcaoTexto: { flex: 1, fontFamily: fonte.regular, fontSize: 15, color: cores.texto, lineHeight: 21 }
});
