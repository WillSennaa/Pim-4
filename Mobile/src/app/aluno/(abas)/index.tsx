import { StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';

import { API } from '@/api/endpoints';
import type { CursoDetalhe, ItemHistorico, Medalha, Perfil } from '@/api/tipos';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { CabecalhoCartao, CaixaIcone, Cartao, Icone, ItemLista, Progresso, Selo, Texto } from '@/ui/base';
import { Botao } from '@/ui/Botao';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Grade, Secao, Tela } from '@/ui/Tela';
import { iconeDaMedalha, XP_POR_MEDALHA } from '@/util/decoracao';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Home do estudante — pages/estudante/home.html + renderEstudanteHome()
   --------------------------------------------------------------------------
   Mesmos blocos, na mesma ordem: saudação com a data, cartão de nível,
   conquistas recentes, "Continue de onde parou", prova liberada e atalho de
   certificados. Todos os números vêm da API (perfil, conquistas, histórico,
   certificados), buscados EM PARALELO: a tela espera a mais lenta das
   quatro, e não a soma delas.
   ========================================================================== */

interface DadosHome {
  perfil: Perfil;
  medalhas: Medalha[];
  historico: ItemHistorico[];
  certificados: number | null;
  provaLiberada: CursoDetalhe | null;
}

async function carregar(): Promise<DadosHome> {
  const [perfil, medalhas, historico, certificados] = await Promise.all([
    API.perfil(),
    API.conquistas().catch(() => [] as Medalha[]),
    API.historico(),
    API.certificados().then(c => c.length).catch(() => null)
  ]);

  // Prova liberada: curso em andamento com 100% dos materiais e prova
  // cadastrada — a mesma regra que o servidor usa para liberar a prova.
  let provaLiberada: CursoDetalhe | null = null;
  for (const h of historico.filter(x => x.status !== 'Concluido' && x.totalAulas > 0 && x.percentualCurso === 100)) {
    try {
      const c = await API.curso(h.idCurso);
      if (c?.idProva) { provaLiberada = c; break; }
    } catch { /* segue para o próximo */ }
  }
  return { perfil, medalhas, historico, certificados, provaLiberada };
}

export default function HomeAluno() {
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(carregar);
  const { ehMedio } = useResponsivo();

  if (carregando) return <Tela><Carregando texto="Carregando seu painel..." /></Tela>;
  if (erro || !dados) return <Tela><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;

  const { perfil: p, medalhas, historico, certificados, provaLiberada } = dados;
  const recentes = medalhas
    .filter(m => m.conquistada)
    .sort((a, b) => new Date(b.dataConquista ?? 0).getTime() - new Date(a.dataConquista ?? 0).getTime())
    .slice(0, 3);
  const emAndamento = historico.filter(h => h.status !== 'Concluido');
  const faltam = Math.max(0, p.xpProximoNivel - p.xp);

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <View style={{ marginBottom: espaco.lg }}>
        <Text style={st.ola}>Olá, {FMT.primeiroNome(p.nome)}! 👋</Text>
        <Texto v="sm" cor={cores.textoSecundario}>{FMT.dataLonga()}</Texto>
      </View>

      <Grade colunas={ehMedio ? 2 : 1}>
        {/* Cartão de nível (.level-card) */}
        <Cartao destaqueTopo="alerta" style={{ marginBottom: 0, flex: 1 }}>
          <View style={{ alignItems: 'center' }}>
            <View>
              <LinearGradient colors={['#475569', '#64748B']} style={st.avatarNivel}>
                <Text style={st.avatarTexto}>{p.iniciais}</Text>
              </LinearGradient>
              <View style={st.seloNivel}><Text style={st.seloNivelTexto}>{p.nivel}</Text></View>
            </View>
            <View style={st.linhaTitulo}>
              <Icone nome="medal" tamanho={18} cor={cores.texto} />
              <Text style={st.nivelTitulo}>Nível {p.nivel}</Text>
            </View>
          </View>
          <View style={st.linhaXp}>
            <Texto v="xs" style={{ flex: 1 }}>
              {FMT.milhar(p.xp)} / {FMT.milhar(p.xpProximoNivel)} XP para o Nível {p.nivel + 1}
            </Texto>
            <Texto v="xs" peso="semi" cor={cores.primaria}>{p.progressoNivel}%</Texto>
          </View>
          <Progresso valor={p.progressoNivel} />
          <Texto v="sm" cor={cores.textoSecundario} centro style={{ marginTop: espaco.md, fontStyle: 'italic' }}>
            Faltam {FMT.milhar(faltam)} XP para você chegar ao nível {p.nivel + 1}!
          </Texto>
        </Cartao>

        {/* Conquistas recentes */}
        <Cartao style={{ marginBottom: 0, flex: 1 }} onPress={() => router.navigate('/aluno/conquistas')}>
          <CabecalhoCartao titulo="CONQUISTAS RECENTES" direita={<Icone nome="star-filled" tamanho={20} cor={cores.alerta} />} />
          {recentes.length ? recentes.map((m, i) => (
            <ItemLista key={m.id} icone={iconeDaMedalha(m.nome)} titulo={m.nome}
              subtitulo={FMT.relativo(m.dataConquista)} ultimo={i === recentes.length - 1}
              direita={<Texto v="sm" peso="semi" cor={cores.alerta}>+{XP_POR_MEDALHA} XP</Texto>} />
          )) : (
            <Texto v="sm" cor={cores.textoApagado}>Conclua sua primeira aula para desbloquear conquistas.</Texto>
          )}
        </Cartao>
      </Grade>

      <Secao titulo="Continue de onde parou" />
      {emAndamento.length === 0 ? (
        <Vazio
          icone="book-2"
          texto={historico.length
            ? 'Você concluiu todos os cursos em que está matriculado.'
            : 'Você ainda não está matriculado em nenhum curso.'}
          acao={{ texto: 'Ver catálogo', onPress: () => router.navigate('/aluno/cursos') }}
        />
      ) : (
        <Grade>
          {emAndamento.map(h => (
            <Cartao key={h.idCurso} destaqueTopo="primaria" style={{ marginBottom: 0, flex: 1 }}>
              <Selo texto="Em andamento" />
              <Text style={st.cursoTitulo} numberOfLines={2}>{h.curso}</Text>
              <View style={st.linhaXp}>
                <Texto v="sm" cor={cores.textoSecundario} style={{ flex: 1 }}>
                  {h.aulasConcluidas} de {h.totalAulas} materiais
                </Texto>
                <Texto v="sm" peso="semi">{h.percentualCurso}%</Texto>
              </View>
              <Progresso valor={h.percentualCurso} />
              <Botao texto="Continuar" iconeDireita="player-play" cheio style={{ marginTop: espaco.md }}
                onPress={() => router.push({ pathname: '/aluno/curso/[id]', params: { id: h.idCurso } })} />
            </Cartao>
          ))}
        </Grade>
      )}

      {provaLiberada && (
        <Cartao destaqueTopo="perigo" style={{ marginTop: espaco.md }}>
          <Selo texto="Prova liberada" tom="perigo" />
          <Text style={st.cursoTitulo}>{provaLiberada.nome}</Text>
          <View style={st.linhaIcone}>
            <Icone nome="checks" tamanho={16} cor={cores.primaria} />
            <Texto v="sm">{provaLiberada.materiais.length} de {provaLiberada.materiais.length} materiais concluídos</Texto>
          </View>
          <View style={st.linhaIcone}>
            <Icone nome="chart-bar" tamanho={16} cor={cores.primaria} />
            <Texto v="sm">Nível: {provaLiberada.nivel || 'não informado'}</Texto>
          </View>
          <Botao texto="Fazer a prova" iconeDireita="arrow-right" variante="secundario" cheio style={{ marginTop: espaco.md }}
            onPress={() => router.push({ pathname: '/aluno/prova/[id]', params: { id: provaLiberada.id } })} />
        </Cartao>
      )}

      {/* Atalho de certificados */}
      <Cartao onPress={() => router.push('/aluno/certificados')} style={[st.atalhoCert, { marginTop: espaco.md }]}
        acessibilidade="Meus certificados">
        <LinearGradient colors={[cores.ouroFundo1, cores.ouroFundo2]} style={StyleSheet.absoluteFill} />
        <View style={st.linhaIcone}>
          <CaixaIcone nome="certificate" tom="alerta" tamanho={48} />
          <View style={{ flex: 1 }}>
            <Text style={{ fontFamily: fonte.negrito, fontSize: 15, color: cores.texto }}>Meus Certificados</Text>
            <Texto v="xs">
              {certificados == null ? 'não foi possível consultar'
                : certificados === 0 ? 'nenhum certificado emitido ainda'
                  : FMT.plural(certificados, 'certificado emitido', 'certificados emitidos')}
            </Texto>
          </View>
          <Icone nome="chevron-right" cor={cores.textoApagado} />
        </View>
      </Cartao>

      <Cartao onPress={() => router.push('/aluno/minhas-provas')}>
        <View style={st.linhaIcone}>
          <CaixaIcone nome="clipboard-list" tamanho={48} />
          <View style={{ flex: 1 }}>
            <Text style={{ fontFamily: fonte.negrito, fontSize: 15, color: cores.texto }}>Minhas provas</Text>
            <Texto v="xs">{FMT.plural(p.provasAprovadas, 'prova aprovada', 'provas aprovadas')} · histórico completo</Texto>
          </View>
          <Icone nome="chevron-right" cor={cores.textoApagado} />
        </View>
      </Cartao>
    </Tela>
  );
}

const st = StyleSheet.create({
  ola: { fontFamily: fonte.negrito, fontSize: 24, color: cores.texto, marginBottom: 2 },
  avatarNivel: { width: 88, height: 88, borderRadius: raio.total, alignItems: 'center', justifyContent: 'center' },
  avatarTexto: { fontFamily: fonte.negrito, fontSize: 32, color: '#FFFFFF' },
  seloNivel: {
    position: 'absolute', right: -4, bottom: -4, width: 34, height: 34, borderRadius: raio.total,
    backgroundColor: cores.alerta, borderWidth: 3, borderColor: cores.superficie, alignItems: 'center', justifyContent: 'center'
  },
  seloNivelTexto: { fontFamily: fonte.negrito, fontSize: 13, color: '#FFFFFF' },
  linhaTitulo: { flexDirection: 'row', alignItems: 'center', gap: 6, marginTop: espaco.md, marginBottom: espaco.sm },
  nivelTitulo: { fontFamily: fonte.negrito, fontSize: 18, color: cores.texto },
  linhaXp: { flexDirection: 'row', alignItems: 'center', gap: espaco.sm, marginBottom: 6, marginTop: espaco.sm },
  cursoTitulo: { fontFamily: fonte.semi, fontSize: 17, color: cores.texto, marginTop: espaco.sm },
  linhaIcone: { flexDirection: 'row', alignItems: 'center', gap: espaco.md, marginTop: espaco.xs },
  atalhoCert: { borderColor: '#FCD34D', overflow: 'hidden' }
});
