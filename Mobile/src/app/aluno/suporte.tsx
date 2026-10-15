import { useState } from 'react';
import { View } from 'react-native';
import { useLocalSearchParams } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { useCarregar } from '@/hooks/useCarregar';
import { useResponsivo } from '@/hooks/useResponsivo';
import { useSessao } from '@/sessao/SessaoContext';
import { cores, espaco } from '@/tema/tokens';
import { Cartao, ItemLista, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo, Opcoes } from '@/ui/Campo';
import { Carregando, ErroCarga, Faixa } from '@/ui/Estados';
import { Grade, Secao, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Suporte — pages/estudante/suporte.html + enviarChamado()
   --------------------------------------------------------------------------
   O TIPO decide o caminho, e quem escolhe o destinatário é o SERVIDOR:
     Dúvida  -> tutor do curso escolhido
     Técnico -> fila dos administradores (atendida no desktop)
   O app nunca envia "para quem": se enviasse, qualquer um poderia endereçar
   a qualquer usuário, e o app precisaria saber quem é o tutor de cada curso.
   ========================================================================== */

type Tipo = 'Duvida' | 'Tecnico';

export default function Suporte() {
  const params = useLocalSearchParams<{ tipo?: string }>();
  const { usuario } = useSessao();
  const { avisar } = useAvisos();
  const { ehMedio } = useResponsivo();

  const [tipo, setTipo] = useState<Tipo>(params.tipo === 'Tecnico' ? 'Tecnico' : 'Duvida');
  const [idCurso, setIdCurso] = useState<number | null>(null);
  const [assunto, setAssunto] = useState('');
  const [descricao, setDescricao] = useState('');
  const [enviando, setEnviando] = useState(false);

  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () => {
    const [historico, chamados] = await Promise.all([API.historico().catch(() => []), API.chamados()]);
    const meus = chamados
      .filter(c => c.idRemetente === usuario?.id)
      .sort((a, b) => new Date(b.dataAbertura).getTime() - new Date(a.dataAbertura).getTime());
    return { historico, meus };
  }, [usuario?.id]);

  async function enviar() {
    if (!assunto.trim()) return avisar('Informe o assunto.', 'erro');
    if (!descricao.trim()) return avisar('Descreva o que aconteceu.', 'erro');
    if (tipo === 'Duvida' && idCurso == null) return avisar('Escolha o curso sobre o qual é a dúvida.', 'erro');

    setEnviando(true);
    try {
      const c = await API.abrirChamado(tipo, assunto.trim(), descricao.trim(), null, tipo === 'Duvida' ? idCurso : null);
      avisar(`${tipo === 'Duvida' ? 'Sua dúvida foi enviada ao tutor do curso' : 'Seu chamado foi enviado ao suporte'}. Protocolo #${c.id}.`);
      setAssunto(''); setDescricao('');
      await recarregar();
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível abrir o chamado.'), 'erro');
    } finally {
      setEnviando(false);
    }
  }

  if (carregando) return <Tela><Carregando /></Tela>;
  if (erro || !dados) return <Tela><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;

  const formulario = (
    <Cartao style={{ padding: espaco.xl }}>
      <Opcoes<Tipo> rotulo="Tipo de chamado" valor={tipo} onChange={setTipo}
        opcoes={[{ valor: 'Duvida', texto: 'Dúvida sobre o conteúdo' }, { valor: 'Tecnico', texto: 'Problema técnico' }]} />
      <Faixa tom="primaria" icone={tipo === 'Duvida' ? 'user-bolt' : 'tool'}
        texto={tipo === 'Duvida'
          ? 'Esta dúvida vai para o tutor do curso escolhido.'
          : 'Este chamado vai para a equipe de suporte (administradores).'} />

      {tipo === 'Duvida' && (dados.historico.length ? (
        <Opcoes<number> rotulo="Curso" valor={idCurso} onChange={setIdCurso}
          opcoes={dados.historico.map(h => ({ valor: h.idCurso, texto: h.curso }))} />
      ) : (
        <Faixa tom="alerta" texto="Você ainda não está matriculado em nenhum curso. Para dúvida de conteúdo, matricule-se primeiro." />
      ))}

      <Campo rotulo="Assunto" value={assunto} onChangeText={setAssunto} placeholder="Resumo em poucas palavras" maxLength={100} />
      <Campo rotulo="Descrição" value={descricao} onChangeText={setDescricao} linhas={5}
        placeholder={tipo === 'Duvida' ? 'Qual parte do material gerou a dúvida?' : 'O que aconteceu? Em qual tela?'} />
      <Botao texto="Enviar" icone="send" cheio tamanho="lg" carregando={enviando} textoCarregando="Enviando..." onPress={enviar}
        desabilitado={tipo === 'Duvida' && !dados.historico.length} />
    </Cartao>
  );

  const lista = (
    <View>
      <Secao titulo="Meus chamados" />
      {dados.meus.length === 0 ? (
        <Texto v="sm" cor={cores.textoApagado}>Nenhum chamado registrado.</Texto>
      ) : (
        <Cartao style={{ paddingVertical: 4 }}>
          {dados.meus.slice(0, 15).map((c, i) => {
            const resolvido = c.status === 'Resolvido';
            const andamento = c.status === 'Em andamento';
            return (
              <ItemLista key={c.id}
                icone={resolvido ? 'circle-check' : 'clock'} tomIcone={resolvido ? 'sucesso' : andamento ? 'primaria' : 'alerta'}
                titulo={c.assunto || c.tipo || 'Chamado'}
                subtitulo={`Protocolo #${c.id} · ${resolvido ? 'resolvido' : andamento ? 'em análise' : 'aberto'} · ${FMT.relativo(c.dataAbertura)}`}
                detalhe={c.curso ? c.curso : c.tipo === 'Tecnico' ? 'Suporte técnico' : null}
                ultimo={i === Math.min(dados.meus.length, 15) - 1} />
            );
          })}
        </Cartao>
      )}
    </View>
  );

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      {ehMedio ? <Grade colunas={2}>{formulario}{lista}</Grade> : <>{formulario}{lista}</>}
    </Tela>
  );
}
