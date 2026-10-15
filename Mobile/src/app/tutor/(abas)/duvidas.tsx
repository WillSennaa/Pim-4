import { useState } from 'react';
import { View } from 'react-native';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import type { Chamado } from '@/api/tipos';
import { useAcao, useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco, type Tom } from '@/tema/tokens';
import { Avatar, Cartao, Icone, Selo, Texto } from '@/ui/base';
import { Abas } from '@/ui/Abas';
import { Dialogo, useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Cabecalho, Grade, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Dúvidas — pages/tutor/duvidas.html + responderDuvida()
   --------------------------------------------------------------------------
   Chegam aqui as dúvidas ENDEREÇADAS a este tutor (GET /api/chamados/
   recebidos). O roteamento é do servidor: o aluno escolhe o curso, e a
   dúvida vai para quem criou o curso.

   Responder é UM endpoint: o servidor cria a resposta endereçada a quem
   perguntou e marca a original como resolvida na MESMA transação. (O web
   fazia isso em duas chamadas, e a segunda podia falhar deixando a dúvida
   aberta com a resposta já enviada.)
   ========================================================================== */

type Filtro = 'pendentes' | 'respondidas' | 'todas';
const COR: Record<string, Tom> = { Aberto: 'alerta', 'Em andamento': 'primaria', Resolvido: 'sucesso' };

export default function DuvidasTutor() {
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () =>
    (await API.chamadosRecebidos()).sort((a, b) => new Date(b.dataAbertura).getTime() - new Date(a.dataAbertura).getTime()));
  const { avisar, confirmar } = useAvisos();
  const { ocupado, rodar } = useAcao();
  const { ehTablet } = useResponsivo();
  const [filtro, setFiltro] = useState<Filtro>('pendentes');
  const [respondendo, setRespondendo] = useState<Chamado | null>(null);
  const [resposta, setResposta] = useState('');

  const todas = dados ?? [];
  const pendentes = todas.filter(d => d.status !== 'Resolvido');
  const visiveis = filtro === 'pendentes' ? pendentes
    : filtro === 'respondidas' ? todas.filter(d => d.status === 'Resolvido') : todas;

  const analisar = (d: Chamado) => rodar('a' + d.id, async () => {
    try { await API.statusChamado(d.id, 'Em andamento'); avisar('Dúvida marcada como em análise.'); await recarregar(); }
    catch (e) { avisar(mensagemDe(e, 'Não foi possível alterar a situação.'), 'erro'); }
  });

  const encerrar = async (d: Chamado) => {
    const ok = await confirmar({
      titulo: 'Encerrar sem responder?', texto: 'A dúvida será marcada como resolvida e o aluno não recebe mensagem.',
      confirmar: 'Encerrar', icone: 'circle-check'
    });
    if (!ok) return;
    await rodar('e' + d.id, async () => {
      try { await API.statusChamado(d.id, 'Resolvido'); avisar('Dúvida encerrada.'); await recarregar(); }
      catch (e) { avisar(mensagemDe(e, 'Não foi possível encerrar a dúvida.'), 'erro'); }
    });
  };

  const enviarResposta = () => rodar('r', async () => {
    if (!respondendo) return;
    if (!resposta.trim()) { avisar('Escreva a resposta antes de enviar.', 'erro'); return; }
    try {
      await API.responderChamado(respondendo.id, resposta.trim());
      avisar('Resposta enviada. A dúvida foi marcada como resolvida.');
      setRespondendo(null); setResposta('');
      await recarregar();
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível enviar a resposta.'), 'erro');
    }
  });

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Cabecalho titulo="Dúvidas" subtitulo="Perguntas dos alunos sobre os seus cursos." />
      <Abas valor={filtro} onChange={setFiltro} abas={[
        { valor: 'pendentes', texto: `Pendentes (${pendentes.length})` },
        { valor: 'respondidas', texto: 'Respondidas' },
        { valor: 'todas', texto: 'Todas' }
      ]} />

      {carregando ? <Carregando /> : erro ? <ErroCarga mensagem={erro} aoTentar={recarregar} /> : visiveis.length === 0 ? (
        <Vazio icone="message-circle" texto={todas.length
          ? 'Nenhuma dúvida nesta situação.'
          : 'Nenhuma dúvida recebida. Quando um aluno abrir uma dúvida sobre um dos seus cursos, ela aparece aqui.'} />
      ) : (
        <Grade colunas={ehTablet ? 2 : 1}>
          {visiveis.map(d => {
            const tom = COR[d.status] ?? 'neutro';
            const pendente = d.status !== 'Resolvido';
            return (
              <Cartao key={d.id} destaqueLateral={tom} style={{ marginBottom: 0, flex: 1 }}>
                <View style={{ flexDirection: 'row', gap: espaco.md, marginBottom: espaco.md }}>
                  <Avatar iniciais={FMT.iniciais(d.remetente)} />
                  <View style={{ flex: 1 }}>
                    <View style={{ flexDirection: 'row', alignItems: 'center', gap: espaco.sm, justifyContent: 'space-between' }}>
                      <Texto v="corpo" peso="semi" style={{ flex: 1 }} numberOfLines={1}>{d.remetente}</Texto>
                      <Selo texto={d.status} tom={tom} />
                    </View>
                    <View style={{ flexDirection: 'row', alignItems: 'center', gap: 4, marginTop: 2 }}>
                      <Icone nome="book-2" tamanho={13} cor={cores.textoApagado} />
                      <Texto v="xs" cor={cores.textoApagado} numberOfLines={1} style={{ flex: 1 }}>
                        {d.curso || 'sem curso informado'} · {FMT.relativo(d.dataAbertura)}
                      </Texto>
                    </View>
                  </View>
                </View>
                {!!d.assunto && <Texto v="sm" peso="semi">{d.assunto}</Texto>}
                <Texto v="sm" cor={cores.textoSecundario} style={{ marginTop: 2 }}>{d.descricao}</Texto>

                {pendente && (
                  <View style={{ flexDirection: 'row', gap: espaco.sm, marginTop: espaco.md, flexWrap: 'wrap' }}>
                    <Botao texto="Responder" icone="message-reply" tamanho="sm" style={{ flexGrow: 1 }}
                      onPress={() => { setRespondendo(d); setResposta(''); }} />
                    {d.status === 'Aberto' && (
                      <Botao texto="Estou analisando" variante="fantasma" tamanho="sm" carregando={ocupado === 'a' + d.id}
                        onPress={() => analisar(d)} />
                    )}
                    <Botao texto="Encerrar" variante="fantasma" tamanho="sm" carregando={ocupado === 'e' + d.id}
                      onPress={() => encerrar(d)} />
                  </View>
                )}
              </Cartao>
            );
          })}
        </Grade>
      )}

      <Dialogo visivel={!!respondendo} aoFechar={() => setRespondendo(null)} titulo="Responder dúvida">
        {respondendo && (
          <>
            <Cartao style={{ backgroundColor: cores.superficie2 }}>
              <Texto v="sm" peso="semi">{respondendo.remetente}</Texto>
              <Texto v="xs" cor={cores.textoApagado} style={{ marginBottom: espaco.sm }}>{respondendo.curso}</Texto>
              <Texto v="sm">{respondendo.descricao}</Texto>
            </Cartao>
            <Campo rotulo="Sua resposta" value={resposta} onChangeText={setResposta} linhas={6}
              placeholder="Explique com calma; o aluno lê nas notificações." />
            <View style={{ flexDirection: 'row', gap: espaco.sm }}>
              <Botao texto="Cancelar" variante="fantasma" onPress={() => setRespondendo(null)} style={{ flex: 1 }} />
              <Botao texto="Enviar" icone="send" carregando={ocupado === 'r'} onPress={enviarResposta} style={{ flex: 1 }} />
            </View>
          </>
        )}
      </Dialogo>
    </Tela>
  );
}
