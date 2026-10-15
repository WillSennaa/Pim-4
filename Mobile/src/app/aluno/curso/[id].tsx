import { useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { router, Stack, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import type { Material } from '@/api/tipos';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { Avatar, Cartao, Icone, Progresso, Selo, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Secao, Tela } from '@/ui/Tela';
import { capaDoCurso } from '@/util/decoracao';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Detalhe do curso — pages/estudante/curso.html + renderCursoDetalhe()
   --------------------------------------------------------------------------
   As regras são as do web, porque são as do servidor:
   - "Matricular-me" só aparece para quem NÃO está matriculado (o campo
     matriculado vem no CursoDetalheDto — quem sabe disso é o servidor);
   - o primeiro material não concluído é o "atual", e é para ele que o botão
     principal leva;
   - material concluído continua aberto para revisão;
   - a prova só libera com todos os materiais concluídos.
   ========================================================================== */

export default function CursoAluno() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const idCurso = Number(id);
  const { dados: curso, erro, carregando, atualizando, puxar, recarregar } = useCarregar(() => API.curso(idCurso), [idCurso]);
  const { avisar } = useAvisos();
  const { ehMedio } = useResponsivo();
  const [matriculando, setMatriculando] = useState(false);

  if (carregando) return <Tela><Carregando /></Tela>;
  if (erro || !curso) return <Tela><ErroCarga mensagem={erro ?? 'Curso não encontrado.'} aoTentar={recarregar} /></Tela>;

  const aulas = curso.materiais ?? [];
  const concluidas = aulas.filter(a => a.concluido).length;
  const percentual = aulas.length ? Math.round(concluidas * 100 / aulas.length) : 0;
  const proxima = aulas.find(a => !a.concluido) ?? aulas[aulas.length - 1];
  const liberada = aulas.length > 0 && concluidas === aulas.length;
  const capa = capaDoCurso(curso.nome, curso.categoria);

  const abrirAula = (a: Material) =>
    router.push({ pathname: '/aluno/aula', params: { curso: curso.id, aula: a.id } });

  async function matricular() {
    setMatriculando(true);
    try {
      await API.matricular(curso!.id);
      avisar(`Matrícula confirmada em ${curso!.nome}.`);
      await recarregar();
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível matricular.'), 'erro');
    } finally {
      setMatriculando(false);
    }
  }

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando} largura="leitura">
      <Stack.Screen options={{ title: curso.nome }} />

      {/* Hero do curso (.course-hero) */}
      <LinearGradient colors={capa.gradiente} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={st.hero}>
        <View style={st.heroIcone}><Icone nome={capa.icone} tamanho={56} cor="rgba(255,255,255,0.18)" /></View>
        <View style={{ flexDirection: 'row', gap: espaco.sm, flexWrap: 'wrap' }}>
          <View style={st.heroSelo}><Text style={st.heroSeloTexto}>{(curso.categoria || 'sem categoria').toUpperCase()}</Text></View>
          <View style={st.heroSelo}><Text style={st.heroSeloTexto}>{(curso.nivel || 'sem nível').toUpperCase()}</Text></View>
        </View>
        <Text style={st.heroTitulo}>{curso.nome}</Text>
        {!!curso.descricao && <Text style={st.heroTexto}>{curso.descricao}</Text>}
        <View style={st.instrutor}>
          <Avatar iniciais={curso.instrutorIniciais || FMT.iniciais(curso.instrutor)} tamanho={36}
            fundo="rgba(255,255,255,0.2)" cor="#FFFFFF" />
          <View>
            <Text style={st.instrutorNome}>{curso.instrutor || 'A definir'}</Text>
            <Text style={st.instrutorPapel}>Tutor do curso</Text>
          </View>
        </View>
      </LinearGradient>

      <View style={[st.linhaStats, ehMedio && { gap: espaco.md }]}>
        <Stat icone="clock" rotulo="Duração" valor={`${curso.duracaoHoras || 0} horas`} />
        <Stat icone="chart-bar" rotulo="Nível" valor={curso.nivel || '—'} />
        <Stat icone="file-text" rotulo="Materiais" valor={String(aulas.length)} />
      </View>

      <Cartao>
        <View style={{ flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 }}>
          <Texto v="sm" peso="semi">Seu progresso</Texto>
          <Texto v="sm" peso="semi" cor={cores.primaria}>{percentual}%</Texto>
        </View>
        <Progresso valor={percentual} tom={percentual === 100 ? 'sucesso' : 'primaria'} />
        <Texto v="xs" style={{ marginTop: 6 }}>{concluidas} de {aulas.length} materiais concluídos</Texto>
        {curso.matriculado ? (
          <View style={st.matriculado}>
            <Icone nome="circle-check" tamanho={16} cor={cores.sucessoEscuro} />
            <Texto v="sm" cor={cores.sucessoEscuro}>Você está matriculado neste curso.</Texto>
          </View>
        ) : (
          <Botao texto="Matricular-me neste curso" icone="bookmark" variante="secundario" tamanho="sm" cheio
            style={{ marginTop: espaco.md }} carregando={matriculando} textoCarregando="Matriculando..." onPress={matricular} />
        )}
      </Cartao>

      {aulas.length > 0 && !liberada && proxima && (
        <Botao texto={concluidas === 0 ? 'Começar pelo primeiro material' : 'Continuar de onde parei'}
          iconeDireita="player-play" tamanho="lg" cheio onPress={() => abrirAula(proxima)} style={{ marginBottom: espaco.md }} />
      )}

      <Secao titulo="Trilha do curso" />
      {aulas.length === 0 ? (
        <Vazio texto="Este curso ainda não tem materiais publicados." icone="file-text" />
      ) : aulas.map((a, i) => {
        const atual = !a.concluido && a.id === proxima?.id;
        const estado = a.concluido ? 'feito' : atual ? 'atual' : 'seguinte';
        return (
          <View key={a.id} style={st.modulo}>
            {/* Linha do tempo (.module-item__icon + __line) */}
            <View style={{ alignItems: 'center' }}>
              <View style={[st.moduloIcone, estado === 'feito' && st.iconeFeito, estado === 'atual' && st.iconeAtual]}>
                <Icone nome={a.concluido ? 'check' : atual ? 'player-play' : 'file-text'} tamanho={16}
                  cor={estado === 'seguinte' ? cores.textoApagado : '#FFFFFF'} />
              </View>
              {i < aulas.length - 1 && <View style={[st.moduloLinha, a.concluido && { backgroundColor: cores.sucesso }]} />}
            </View>
            <Cartao onPress={() => abrirAula(a)} style={[{ flex: 1 }, atual && { borderColor: cores.primaria, borderWidth: 1.5 }]}
              acessibilidade={`Material ${i + 1}: ${a.titulo}`}>
              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: espaco.sm }}>
                <Texto v="rotulo" cor={atual ? cores.primaria : cores.textoApagado}>
                  MATERIAL {i + 1}{atual ? ' • ATUAL' : ''}
                </Texto>
                {a.concluido ? <Selo texto="Concluído · rever" tom="sucesso" />
                  : atual ? <Selo texto="Abrir" /> : <Selo texto="A seguir" tom="neutro" />}
              </View>
              <Text style={st.moduloTitulo}>{a.titulo || 'Material sem título'}</Text>
              <Texto v="xs">Tipo: {a.tipo || 'material'}</Texto>
              {a.porcentagemAssistida > 0 && !a.concluido && (
                <View style={{ marginTop: espaco.sm }}><Progresso valor={a.porcentagemAssistida} altura={6} /></View>
              )}
            </Cartao>
          </View>
        );
      })}

      {curso.idProva != null && (
        <View style={{ marginTop: espaco.md }}>
          {liberada ? (
            <Botao texto="Fazer a prova" icone="file-text" tamanho="lg" cheio
              onPress={() => router.push({ pathname: '/aluno/prova/[id]', params: { id: curso.id } })} />
          ) : (
            <Botao texto="Conclua todos os materiais para liberar a prova" variante="secundario" tamanho="lg" cheio desabilitado />
          )}
        </View>
      )}
    </Tela>
  );
}

function Stat({ icone, rotulo, valor }: { icone: 'clock' | 'chart-bar' | 'file-text'; rotulo: string; valor: string }) {
  return (
    <View style={st.stat}>
      <Icone nome={icone} tamanho={18} cor={cores.primaria} />
      <Texto v="xs" style={{ marginTop: 4 }}>{rotulo}</Texto>
      <Texto v="sm" peso="semi" numberOfLines={1}>{valor}</Texto>
    </View>
  );
}

const st = StyleSheet.create({
  hero: { borderRadius: raio.lg, padding: espaco.xl, marginBottom: espaco.md, overflow: 'hidden' },
  heroIcone: { position: 'absolute', right: espaco.lg, top: espaco.lg },
  heroSelo: { backgroundColor: 'rgba(255,255,255,0.2)', borderRadius: raio.total, paddingHorizontal: 10, paddingVertical: 4 },
  heroSeloTexto: { fontFamily: fonte.semi, fontSize: 11, color: '#FFFFFF', letterSpacing: 0.5 },
  heroTitulo: { fontFamily: fonte.negrito, fontSize: 24, color: '#FFFFFF', marginTop: espaco.md },
  heroTexto: { fontFamily: fonte.regular, fontSize: 14, color: 'rgba(255,255,255,0.9)', marginTop: espaco.sm, lineHeight: 20 },
  instrutor: { flexDirection: 'row', alignItems: 'center', gap: espaco.md, marginTop: espaco.lg },
  instrutorNome: { fontFamily: fonte.semi, fontSize: 14, color: '#FFFFFF' },
  instrutorPapel: { fontFamily: fonte.regular, fontSize: 12, color: 'rgba(255,255,255,0.8)' },
  linhaStats: { flexDirection: 'row', gap: espaco.sm, marginBottom: espaco.md },
  stat: {
    flex: 1, backgroundColor: cores.superficie, borderRadius: raio.md, borderWidth: 1, borderColor: cores.borda,
    padding: espaco.md, alignItems: 'center'
  },
  matriculado: { flexDirection: 'row', alignItems: 'center', gap: 6, marginTop: espaco.md },
  modulo: { flexDirection: 'row', gap: espaco.md },
  moduloIcone: {
    width: 32, height: 32, borderRadius: raio.total, backgroundColor: cores.borda,
    alignItems: 'center', justifyContent: 'center', marginTop: espaco.md
  },
  iconeFeito: { backgroundColor: cores.sucesso },
  iconeAtual: { backgroundColor: cores.primaria },
  moduloLinha: { width: 2, flex: 1, backgroundColor: cores.borda, marginTop: 2 },
  moduloTitulo: { fontFamily: fonte.semi, fontSize: 15, color: cores.texto, marginTop: espaco.sm, marginBottom: 2 }
});
