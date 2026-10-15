import { Text, View } from 'react-native';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { useSessao } from '@/sessao/SessaoContext';
import { cores, espaco, fonte } from '@/tema/tokens';
import { Avatar, CabecalhoCartao, Cartao, ItemLista, LinkTexto, Selo, Texto } from '@/ui/base';
import { Kpi } from '@/ui/Cartoes';
import { Carregando, ErroCarga } from '@/ui/Estados';
import { Grade, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Painel do tutor — pages/tutor/home.html + TQ.carregarTutor()
   --------------------------------------------------------------------------
   Três chamadas em paralelo — cursos do tutor, alunos e dúvidas recebidas —
   e todos os indicadores derivados delas. Nenhum número é escrito na tela.

   "Alunos ativos" conta ESTUDANTES DISTINTOS: o mesmo aluno matriculado em
   dois cursos do tutor aparece duas vezes na lista (uma linha por
   matrícula), mas é uma pessoa só.
   ========================================================================== */

export default function PainelTutor() {
  const { usuario } = useSessao();
  const { colunasKpi, ehTablet } = useResponsivo();
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () => {
    const [cursos, alunos, duvidas] = await Promise.all([API.tutorCursos(), API.tutorAlunos(), API.chamadosRecebidos()]);
    return { cursos, alunos, duvidas };
  });

  if (carregando) return <Tela><Carregando texto="Carregando seu painel..." /></Tela>;
  if (erro || !dados) return <Tela><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;

  const { cursos, alunos, duvidas } = dados;
  const porStatus = (s: string) => cursos.filter(c => c.status.toLowerCase() === s).length;
  const pendentes = duvidas
    .filter(d => d.status !== 'Resolvido')
    .sort((a, b) => new Date(b.dataAbertura).getTime() - new Date(a.dataAbertura).getTime());
  const alunosDistintos = new Set(alunos.map(a => a.idEstudante)).size;
  const materiais = cursos.reduce((t, c) => t + (c.totalAulas || 0), 0);
  const recentes = [...alunos].sort((a, b) => b.percentualCurso - a.percentualCurso).slice(0, 5);

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Text style={{ fontFamily: fonte.negrito, fontSize: 24, color: cores.texto }}>
        Olá, {FMT.primeiroNome(usuario?.nome)}! 👋
      </Text>
      <Texto v="subtitulo" style={{ marginTop: 4, marginBottom: espaco.lg }}>
        Veja o panorama dos seus alunos e responda dúvidas pendentes.
      </Texto>

      <Grade colunas={colunasKpi}>
        <Kpi rotulo="Alunos ativos" valor={alunosDistintos} detalhe="matriculados nos seus cursos" tom="primaria"
          onPress={() => router.navigate('/tutor/alunos')} />
        <Kpi rotulo="Dúvidas pendentes" valor={pendentes.length} detalhe="aguardando resposta" tom="perigo"
          onPress={() => router.navigate('/tutor/duvidas')} />
        <Kpi rotulo="Cursos publicados" valor={porStatus('publicado')}
          detalhe={`${porStatus('rascunho')} rascunho · ${porStatus('pendente')} em avaliação`} tom="sucesso"
          onPress={() => router.navigate('/tutor/cursos')} />
        <Kpi rotulo="Materiais" valor={materiais} detalhe="em todos os seus cursos" tom="alerta" />
      </Grade>

      <View style={{ height: espaco.md }} />
      <Grade colunas={ehTablet ? 2 : 1}>
        <Cartao style={{ marginBottom: 0, flex: 1 }}>
          <CabecalhoCartao titulo="Alunos" direita={<LinkTexto texto="Ver todos" onPress={() => router.navigate('/tutor/alunos')} />} />
          {recentes.length === 0 ? (
            <Texto v="sm" cor={cores.textoApagado}>Nenhum aluno matriculado nos seus cursos ainda.</Texto>
          ) : recentes.map((a, i) => (
            <ItemLista key={`${a.idEstudante}-${a.idCurso}`} esquerda={<Avatar iniciais={FMT.iniciais(a.nome)} />}
              titulo={a.nome} subtitulo={`${a.curso} · ${a.percentualCurso}%`} ultimo={i === recentes.length - 1}
              onPress={() => router.push({ pathname: '/tutor/aluno', params: { estudante: a.idEstudante, curso: a.idCurso } })} />
          ))}
        </Cartao>

        <Cartao style={{ marginBottom: 0, flex: 1 }}>
          <CabecalhoCartao titulo="Dúvidas pendentes" direita={<LinkTexto texto="Ver todas" onPress={() => router.navigate('/tutor/duvidas')} />} />
          {pendentes.length === 0 ? (
            <Texto v="sm" cor={cores.textoApagado}>Nenhuma dúvida aguardando resposta. 🎉</Texto>
          ) : pendentes.slice(0, 5).map((d, i) => (
            <ItemLista key={d.id} esquerda={<Avatar iniciais={FMT.iniciais(d.remetente)} />}
              titulo={d.assunto || 'Dúvida'} subtitulo={`${d.remetente} · ${d.curso || 'sem curso'} · ${FMT.relativo(d.dataAbertura)}`}
              ultimo={i === Math.min(pendentes.length, 5) - 1}
              direita={<Selo texto={d.status} tom={d.status === 'Aberto' ? 'alerta' : 'primaria'} />}
              onPress={() => router.navigate('/tutor/duvidas')} />
          ))}
        </Cartao>
      </Grade>
    </Tela>
  );
}
