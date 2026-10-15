import { StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';

import { API } from '@/api/endpoints';
import { resultadoProva } from '@/estado/resultadoProva';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { useSessao } from '@/sessao/SessaoContext';
import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { CaixaIcone, Cartao, Icone, Progresso, Selo, Texto } from '@/ui/base';
import { Botao } from '@/ui/Botao';
import { BlocoCodigo } from '@/ui/Codigo';
import { Kpi } from '@/ui/Cartoes';
import { Vazio } from '@/ui/Estados';
import { Grade, Secao, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Resultado — resultado-aprovado.html + resultado-reprovado.html
   --------------------------------------------------------------------------
   As duas telas do web viraram uma, com a cor e as ações decididas por
   "aprovado". A revisão questão a questão usa a correção REAL do servidor
   (letra marcada x letra correta), e não a simulação do PIM III.

   "Análise por tópico" continua fora, pelo mesmo motivo registrado no web: a
   tabela Questao não tem coluna de assunto, então não há de onde agrupar.
   ========================================================================== */

export default function Resultado() {
  const guardado = resultadoProva.ler();
  const { usuario } = useSessao();
  const { colunasKpi } = useResponsivo();
  // Nível e XP DEPOIS da prova: o XP ganho já foi somado pelo servidor.
  const { dados: perfil } = useCarregar(() => API.perfil());

  if (!guardado) {
    return (
      <Tela>
        <Vazio texto="Nenhum resultado nesta sessão." icone="file-text"
          acao={{ texto: 'Voltar aos cursos', onPress: () => router.replace('/aluno/cursos') }} />
      </Tela>
    );
  }

  const { resultado: r, questoes, tituloProva, idCurso } = guardado;
  const porId = new Map(questoes.map(q => [q.id, q]));
  const ok = r.aprovado;

  return (
    <Tela largura="leitura">
      <LinearGradient colors={ok ? ['#22C55E', '#15803D'] : ['#EF4444', '#B91C1C']} style={st.hero}>
        <View style={st.heroIcone}><Icone nome={ok ? 'trophy' : 'circle-x'} tamanho={40} cor="#FFFFFF" /></View>
        <Text style={st.heroTitulo}>
          {ok ? `Parabéns, ${FMT.primeiroNome(usuario?.nome) || 'estudante'}!` : 'Não foi desta vez'}
        </Text>
        <Text style={st.heroTexto}>
          {ok ? `Você foi aprovado em ${tituloProva}.` : `Você não atingiu a nota mínima em ${tituloProva}. Revise o conteúdo e tente de novo.`}
        </Text>
        <Text style={st.nota}>{FMT.nota(r.nota)} <Text style={st.notaDe}>/ 10</Text></Text>
      </LinearGradient>

      <Grade colunas={colunasKpi}>
        <Kpi rotulo="Acertos" valor={`${r.acertos}/${r.totalQuestoes}`} tom={ok ? 'sucesso' : 'perigo'} />
        <Kpi rotulo="Nota mínima" valor={FMT.nota(r.notaMinima)} tom="primaria" />
        <Kpi rotulo="Tentativa" valor={`#${r.numeroTentativa}`} tom="neutro" />
        <Kpi rotulo="XP ganho" valor={`+${r.xpGanho}`} tom="alerta" />
      </Grade>

      {r.medalhasNovas?.length > 0 && (
        <Cartao destaqueTopo="alerta" style={{ marginTop: espaco.md }}>
          <View style={st.linha}>
            <CaixaIcone nome="medal" tom="alerta" tamanho={48} />
            <View style={{ flex: 1 }}>
              <Texto v="sm" peso="semi" cor={cores.alertaEscuro}>Nova conquista desbloqueada</Texto>
              <Text style={st.medalha}>{r.medalhasNovas.join(', ')}</Text>
            </View>
          </View>
        </Cartao>
      )}

      {perfil && (
        <Cartao style={{ marginTop: espaco.md }}>
          <View style={[st.linha, { justifyContent: 'space-between', marginBottom: 6 }]}>
            <Texto v="sm" peso="semi">Progresso do nível {perfil.nivel}</Texto>
            <Texto v="sm" peso="semi" cor={cores.primaria}>{FMT.milhar(perfil.xp)} XP</Texto>
          </View>
          <Progresso valor={perfil.progressoNivel} />
          <Texto v="xs" style={{ marginTop: 6 }}>{FMT.milhar(perfil.xpProximoNivel)} XP para o nível {perfil.nivel + 1}</Texto>
        </Cartao>
      )}

      <View style={{ gap: espaco.sm, marginTop: espaco.md }}>
        {ok ? (
          <>
            <Botao texto="Ver meus certificados" icone="certificate" variante="sucesso" cheio tamanho="lg"
              onPress={() => router.replace('/aluno/certificados')} />
            <Botao texto="Voltar ao curso" variante="fantasma" cheio
              onPress={() => router.replace({ pathname: '/aluno/curso/[id]', params: { id: idCurso } })} />
          </>
        ) : (
          <>
            <Botao texto="Revisar o conteúdo" icone="book-2" cheio tamanho="lg"
              onPress={() => router.replace({ pathname: '/aluno/curso/[id]', params: { id: idCurso } })} />
            <Botao texto="Tentar novamente" icone="refresh" variante="secundario" cheio
              onPress={() => router.replace({ pathname: '/aluno/prova/[id]', params: { id: idCurso } })} />
          </>
        )}
      </View>

      <Secao titulo="Revisão da prova" />
      {r.correcao.length === 0 ? (
        <Vazio texto="O servidor não devolveu a correção desta tentativa." />
      ) : r.correcao.map((c, i) => {
        const q = porId.get(c.idQuestao);
        const tom = c.acertou ? 'sucesso' : 'perigo';
        return (
          <Cartao key={c.idQuestao} destaqueLateral={tom} style={{ padding: espaco.md }}>
            <View style={[st.linha, { justifyContent: 'space-between', marginBottom: espaco.sm }]}>
              <Texto v="sm" peso="semi">Questão {String(c.ordem || i + 1).padStart(2, '0')}</Texto>
              <Selo texto={c.acertou ? 'Correta' : 'Incorreta'} tom={tom} />
            </View>
            <Texto v="sm" style={{ marginBottom: espaco.sm }}>{q?.enunciado ?? '(enunciado não disponível)'}</Texto>
            {!!q?.codigo && <BlocoCodigo codigo={q.codigo} />}
            {c.acertou ? (
              <Resposta letra={c.letraCorreta} texto={q?.alternativas[c.letraCorreta ?? '']} rotulo="sua resposta, correta" certa />
            ) : (
              <>
                <Resposta letra={c.letraMarcada} texto={q?.alternativas[c.letraMarcada ?? '']} rotulo="sua resposta" />
                <Resposta letra={c.letraCorreta} texto={q?.alternativas[c.letraCorreta ?? '']} rotulo="resposta correta" certa />
              </>
            )}
          </Cartao>
        );
      })}
    </Tela>
  );
}

function Resposta({ letra, texto, rotulo, certa }: { letra: string | null; texto?: string; rotulo: string; certa?: boolean }) {
  if (!letra) return <Texto v="xs" cor={cores.textoApagado}>Questão não respondida.</Texto>;
  return (
    <View style={[st.resposta, { backgroundColor: certa ? cores.sucessoClaro : cores.perigoClaro }]}>
      <Text style={[st.respostaTexto, { color: certa ? cores.sucessoEscuro : cores.perigoEscuro }]}>
        <Text style={{ fontFamily: fonte.negrito }}>{certa ? '✓' : '✗'} {letra})</Text> {texto || '(alternativa não disponível)'} — {rotulo}
      </Text>
    </View>
  );
}

const st = StyleSheet.create({
  hero: { borderRadius: raio.lg, padding: espaco.xl, alignItems: 'center', marginBottom: espaco.md },
  heroIcone: {
    width: 72, height: 72, borderRadius: raio.total, backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center', justifyContent: 'center', marginBottom: espaco.md
  },
  heroTitulo: { fontFamily: fonte.negrito, fontSize: 22, color: '#FFFFFF', textAlign: 'center' },
  heroTexto: { fontFamily: fonte.regular, fontSize: 14, color: 'rgba(255,255,255,0.92)', textAlign: 'center', marginTop: espaco.sm },
  nota: { fontFamily: fonte.negrito, fontSize: 48, color: '#FFFFFF', marginTop: espaco.md },
  notaDe: { fontSize: 20, fontFamily: fonte.media, color: 'rgba(255,255,255,0.8)' },
  linha: { flexDirection: 'row', alignItems: 'center', gap: espaco.md },
  medalha: { fontFamily: fonte.negrito, fontSize: 16, color: cores.texto },
  resposta: { borderRadius: raio.sm, paddingVertical: 6, paddingHorizontal: 10, marginTop: 4 },
  respostaTexto: { fontFamily: fonte.regular, fontSize: 12, lineHeight: 17 }
});
