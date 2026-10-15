import { useState } from 'react';
import { View } from 'react-native';
import { useLocalSearchParams } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { useCarregar } from '@/hooks/useCarregar';
import { cores, espaco } from '@/tema/tokens';
import { Avatar, Cartao, LinhaInfo, Progresso, Selo, Texto } from '@/ui/base';
import { Dialogo, useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Carregando, ErroCarga, Faixa, Vazio } from '@/ui/Estados';
import { Secao, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Detalhe do aluno — pages/tutor/aluno-detalhe.html + enviarMensagemAluno()
   --------------------------------------------------------------------------
   A rota recebe DOIS ids: o do estudante e o do curso. Um aluno pode estar
   em mais de um curso do mesmo tutor, e o web procurava só pelo estudante,
   mostrando sempre a primeira matrícula. Aqui a matrícula aberta é a que foi
   tocada, e as demais aparecem logo abaixo.

   A mensagem é um Chamado do tipo "Mensagem" endereçado ao ID_USUARIO do
   aluno — não ao ID_Estudante, que identifica a matrícula. Por isso o
   AlunoDoTutorDto traz os dois.
   ========================================================================== */

export default function AlunoDetalhe() {
  const params = useLocalSearchParams<{ estudante: string; curso?: string }>();
  const idEstudante = Number(params.estudante);
  const idCurso = params.curso ? Number(params.curso) : null;
  const { dados, erro, carregando, recarregar } = useCarregar(async () =>
    (await API.tutorAlunos()).filter(a => a.idEstudante === idEstudante), [idEstudante]);
  const { avisar } = useAvisos();
  const [aberto, setAberto] = useState(false);
  const [texto, setTexto] = useState('');
  const [enviando, setEnviando] = useState(false);

  if (carregando) return <Tela largura="leitura"><Carregando /></Tela>;
  if (erro) return <Tela largura="leitura"><ErroCarga mensagem={erro} aoTentar={recarregar} /></Tela>;
  if (!dados?.length) return <Tela largura="leitura"><Vazio texto="Aluno não encontrado nos seus cursos." icone="users" /></Tela>;

  const principal = dados.find(a => a.idCurso === idCurso) ?? dados[0];
  const outras = dados.filter(a => a !== principal);

  async function enviar() {
    if (!texto.trim()) return avisar('Escreva a mensagem antes de enviar.', 'erro');
    setEnviando(true);
    try {
      await API.abrirChamado('Mensagem', 'Mensagem do tutor', texto.trim(), principal.idUsuario, null);
      avisar(`Mensagem enviada para ${FMT.primeiroNome(principal.nome)}.`);
      setTexto(''); setAberto(false);
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível enviar a mensagem.'), 'erro');
    } finally {
      setEnviando(false);
    }
  }

  return (
    <Tela largura="leitura">
      <Cartao>
        <View style={{ flexDirection: 'row', alignItems: 'center', gap: espaco.md, marginBottom: espaco.lg }}>
          <Avatar iniciais={FMT.iniciais(principal.nome)} tamanho={56} />
          <View style={{ flex: 1 }}>
            <Texto v="secao">{principal.nome}</Texto>
            <Texto v="sm" cor={cores.textoSecundario}>{principal.email || ''}</Texto>
          </View>
        </View>
        <LinhaInfo rotulo="Curso" valor={principal.curso} />
        <LinhaInfo rotulo="Situação" valor={principal.status || '—'} />
        <LinhaInfo rotulo="Materiais" valor={`${principal.aulasConcluidas} de ${principal.totalAulas}`} />
        <View style={{ paddingVertical: espaco.sm }}>
          <View style={{ flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 }}>
            <Texto v="sm" cor={cores.textoSecundario}>Progresso</Texto>
            <Texto v="sm" peso="semi">{principal.percentualCurso}%</Texto>
          </View>
          <Progresso valor={principal.percentualCurso} tom={principal.percentualCurso === 100 ? 'sucesso' : 'primaria'} />
        </View>
        <LinhaInfo rotulo="Melhor nota" valor={principal.melhorNota != null ? FMT.nota(principal.melhorNota) : 'sem prova'} />
        <LinhaInfo rotulo="Tentativas de prova" valor={String(principal.tentativas || 0)} ultimo />
      </Cartao>

      <Botao texto="Enviar mensagem ao aluno" icone="message-2" cheio tamanho="lg" onPress={() => setAberto(true)} />

      {outras.length > 0 && (
        <>
          <Secao titulo="Outros cursos seus que ele faz" />
          {outras.map(a => (
            <Cartao key={a.idCurso}>
              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: espaco.sm }}>
                <Texto v="sm" peso="semi" style={{ flex: 1 }}>{a.curso}</Texto>
                {a.melhorNota != null && <Selo texto={`Nota ${FMT.nota(a.melhorNota)}`} tom={a.melhorNota >= 7 ? 'sucesso' : 'perigo'} />}
              </View>
              <Progresso valor={a.percentualCurso} />
              <Texto v="xs" style={{ marginTop: 4 }}>{a.aulasConcluidas} de {a.totalAulas} materiais · {a.percentualCurso}%</Texto>
            </Cartao>
          ))}
        </>
      )}

      <Dialogo visivel={aberto} aoFechar={() => setAberto(false)} titulo={`Mensagem para ${FMT.primeiroNome(principal.nome)}`}>
        <Faixa tom="primaria" icone="info-circle" texto="A mensagem aparece nas notificações do aluno, no app e no site." />
        <Campo rotulo="Mensagem" value={texto} onChangeText={setTexto} linhas={5} placeholder="Escreva sua mensagem..." />
        <View style={{ flexDirection: 'row', gap: espaco.sm }}>
          <Botao texto="Cancelar" variante="fantasma" onPress={() => setAberto(false)} style={{ flex: 1 }} />
          <Botao texto="Enviar" icone="send" carregando={enviando} onPress={enviar} style={{ flex: 1 }} />
        </View>
      </Dialogo>
    </Tela>
  );
}
