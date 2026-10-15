import { useSessao } from '@/sessao/SessaoContext';
import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { Cartao, ItemLista } from '@/ui/base';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Cabecalho, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Notificações — pages/estudante/notificacoes.html + carregarNotificacoes()
   (usada pelo aluno e pelo tutor: para o tutor, chegam aqui os pareceres do
   administrador sobre os cursos submetidos, chamados do tipo Avaliacao)
   --------------------------------------------------------------------------
   NÃO EXISTE TABELA DE NOTIFICAÇÃO no modelo, e criar uma duplicaria o que
   Chamado já guarda. As mensagens que chegam ao aluno são as RESPOSTAS do
   tutor e do suporte e as mensagens diretas do tutor — chamados endereçados
   a ele. É essa caixa de entrada, a que o modelo tem, que a tela mostra.

   Fica registrado o mesmo limite do web: aviso de medalha nova e de
   certificado emitido aparece no momento da ação e não fica guardado.
   ========================================================================== */

export function TelaNotificacoes({ subtitulo }: { subtitulo: string }) {
  const { usuario } = useSessao();
  const eu = usuario?.id;
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () => {
    const chamados = await API.chamados();
    return chamados
      .filter(c => c.idDestinatario === eu && c.idRemetente !== eu)
      .sort((a, b) => new Date(b.dataAbertura).getTime() - new Date(a.dataAbertura).getTime());
  }, [eu]);

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando} largura="leitura">
      <Cabecalho titulo="Notificações" subtitulo={subtitulo} />
      {carregando ? <Carregando /> : erro ? <ErroCarga mensagem={erro} aoTentar={recarregar} /> : !dados?.length ? (
        <Vazio icone="bell" texto="Tudo em dia por aqui. Quando chegar uma mensagem para você, ela aparece nesta tela." />
      ) : (
        <Cartao style={{ paddingVertical: 4 }}>
          {dados.map((c, i) => {
            const resposta = c.tipo === 'Resposta' || c.tipo === 'Avaliacao';
            return (
              <ItemLista key={c.id}
                icone={resposta ? 'message-circle-check' : 'message-circle'} tomIcone={resposta ? 'sucesso' : 'alerta'}
                titulo={c.assunto || c.tipo || 'Mensagem'}
                subtitulo={c.descricao}
                linhasSubtitulo={6}
                detalhe={`${c.remetente || 'Tech Quest'}${c.curso ? ' · ' + c.curso : ''} · ${FMT.relativo(c.dataAbertura)}`}
                ultimo={i === dados.length - 1}
              />
            );
          })}
        </Cartao>
      )}
    </Tela>
  );
}

