import { StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco, fonte } from '@/tema/tokens';
import { Avatar, CabecalhoCartao, Cartao, ItemLista, LinhaInfo, LinkTexto, Progresso, Texto } from '@/ui/base';
import { Kpi } from '@/ui/Cartoes';
import { Carregando, ErroCarga } from '@/ui/Estados';
import { MenuConta } from '@/ui/MenuConta';
import { Grade, Secao, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Perfil do estudante — pages/estudante/perfil.html + carregarPerfilEstudante()
   --------------------------------------------------------------------------
   A barra de nível usa ProgressoNivel calculado pelo domínio — o mesmo valor
   da home. (No web, home e perfil chegaram a discordar: 21% e 75% para o
   mesmo aluno, porque o perfil tinha o número escrito no HTML.)
   ========================================================================== */

export default function PerfilAluno() {
  const { dados: p, erro, carregando, atualizando, puxar, recarregar } = useCarregar(() => API.perfil());
  const { ehMedio, colunasKpi } = useResponsivo();

  if (carregando) return <Tela><Carregando /></Tela>;
  if (erro || !p) return <Tela><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Cartao style={{ alignItems: 'center', paddingVertical: espaco.xl }}>
        <Avatar iniciais={p.iniciais} tamanho={80} fundo={cores.primaria} cor="#FFFFFF" />
        <Text style={st.nome}>{p.nome}</Text>
        <Texto v="sm" cor={cores.textoSecundario}>{p.email}</Texto>
        <View style={{ alignSelf: 'stretch', marginTop: espaco.lg }}>
          <View style={{ flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 }}>
            <Texto v="sm" peso="semi">Nível {p.nivel} · {FMT.milhar(p.xp)} XP</Texto>
            <Texto v="sm" peso="semi" cor={cores.primaria}>{p.progressoNivel}%</Texto>
          </View>
          <Progresso valor={p.progressoNivel} />
        </View>
      </Cartao>

      <Grade colunas={colunasKpi}>
        <Kpi rotulo="Provas" valor={p.provasAprovadas} detalhe="aprovadas" tom="primaria"
          onPress={() => router.push('/aluno/minhas-provas')} />
        <Kpi rotulo="Medalhas" valor={p.medalhas} detalhe="conquistadas" tom="alerta"
          onPress={() => router.navigate('/aluno/conquistas')} />
        <Kpi rotulo="Certificados" valor={p.cursosConcluidos} detalhe="cursos concluídos" tom="sucesso"
          onPress={() => router.push('/aluno/certificados')} />
        <Kpi rotulo="Materiais" valor={p.aulasConcluidas} detalhe="concluídos" tom="neutro" />
      </Grade>

      <Grade colunas={ehMedio ? 2 : 1}>
        <View>
          <Secao titulo="Dados pessoais" />
          <Cartao>
            <CabecalhoCartao titulo="INFORMAÇÕES" direita={<LinkTexto texto="Editar" onPress={() => router.push('/aluno/editar-perfil')} />} />
            <LinhaInfo rotulo="Nome" valor={p.nome} />
            <LinhaInfo rotulo="E-mail" valor={p.email || '-'} />
            <LinhaInfo rotulo="Telefone" valor={p.telefone || 'não informado'} />
            <LinhaInfo rotulo="Nascimento" valor={p.dataNascimento ? FMT.data(p.dataNascimento) : 'não informada'} />
            <LinhaInfo rotulo="Cidade" valor={p.cidade || 'não informada'} />
            <LinhaInfo rotulo="Membro desde" valor={FMT.data(p.dataCadastro)} ultimo />
          </Cartao>
        </View>
        <View>
          <Secao titulo="Minha jornada" />
          <Cartao style={{ paddingVertical: 4 }}>
            <ItemLista icone="certificate" tomIcone="alerta" titulo="Meus certificados" onPress={() => router.push('/aluno/certificados')} />
            <ItemLista icone="clipboard-list" titulo="Minhas provas" subtitulo="Histórico de notas e tentativas" onPress={() => router.push('/aluno/minhas-provas')} />
            <ItemLista icone="bell" tomIcone="sucesso" titulo="Notificações" onPress={() => router.push('/aluno/notificacoes')} />
            <ItemLista icone="headset" tomIcone="neutro" titulo="Suporte e dúvidas" ultimo onPress={() => router.push('/aluno/suporte')} />
          </Cartao>
        </View>
      </Grade>

      <MenuConta />
    </Tela>
  );
}

const st = StyleSheet.create({
  nome: { fontFamily: fonte.negrito, fontSize: 20, color: cores.texto, marginTop: espaco.md }
});
