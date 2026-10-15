import { StyleSheet, Text } from 'react-native';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco, fonte } from '@/tema/tokens';
import { Avatar, Cartao, LinhaInfo, Selo, Texto } from '@/ui/base';
import { Kpi } from '@/ui/Cartoes';
import { Carregando, ErroCarga } from '@/ui/Estados';
import { MenuConta } from '@/ui/MenuConta';
import { Grade, Secao, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Conta do tutor — pages/conta/perfil.html + renderIndicadoresDaConta()
   --------------------------------------------------------------------------
   No web, a conta do tutor mostrava só nome, e-mail e data de cadastro, e
   ganhou depois o bloco "Minha atuação". Aqui ele já nasce com esse bloco:
   cursos, materiais, alunos e dúvidas — os números de quem ENSINA, enquanto
   o perfil do estudante mostra XP e nível, que só fazem sentido para quem
   estuda.
   ========================================================================== */

export default function ContaTutor() {
  const { colunasKpi } = useResponsivo();
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () => {
    const [perfil, cursos, alunos, duvidas] = await Promise.all([
      API.perfil(), API.tutorCursos(), API.tutorAlunos(), API.chamadosRecebidos()
    ]);
    return { perfil, cursos, alunos, duvidas };
  });

  if (carregando) return <Tela><Carregando /></Tela>;
  if (erro || !dados) return <Tela><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;

  const { perfil: p, cursos, alunos, duvidas } = dados;
  const publicados = cursos.filter(c => c.status.toLowerCase() === 'publicado').length;
  const pendentes = cursos.filter(c => c.status.toLowerCase() === 'pendente').length;

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando} largura="leitura">
      <Cartao style={{ alignItems: 'center', paddingVertical: espaco.xl }}>
        <Avatar iniciais={p.iniciais} tamanho={80} fundo={cores.primaria} cor="#FFFFFF" />
        <Text style={st.nome}>{p.nome}</Text>
        <Texto v="sm" cor={cores.textoSecundario} style={{ marginBottom: espaco.sm }}>{p.email}</Texto>
        <Selo texto="Tutor" icone="user-bolt" />
      </Cartao>

      <Secao titulo="Minha atuação" />
      <Grade colunas={colunasKpi}>
        <Kpi rotulo="Cursos" valor={cursos.length} detalhe={`${publicados} publicados, ${pendentes} em avaliação`} tom="primaria" />
        <Kpi rotulo="Materiais" valor={cursos.reduce((t, c) => t + (c.totalAulas || 0), 0)} detalhe="nos seus cursos" tom="alerta" />
        <Kpi rotulo="Alunos" valor={new Set(alunos.map(a => a.idEstudante)).size} detalhe="matriculados" tom="sucesso" />
        <Kpi rotulo="Dúvidas" valor={duvidas.filter(d => d.status !== 'Resolvido').length} detalhe="aguardando resposta" tom="perigo" />
      </Grade>

      <Secao titulo="Dados da conta" />
      <Cartao>
        <LinhaInfo rotulo="Telefone" valor={p.telefone || 'não informado'} />
        <LinhaInfo rotulo="Cidade" valor={p.cidade || 'não informada'} />
        <LinhaInfo rotulo="Membro desde" valor={FMT.data(p.dataCadastro)} ultimo />
      </Cartao>

      <MenuConta />
    </Tela>
  );
}

const st = StyleSheet.create({
  nome: { fontFamily: fonte.negrito, fontSize: 20, color: cores.texto, marginTop: espaco.md }
});
