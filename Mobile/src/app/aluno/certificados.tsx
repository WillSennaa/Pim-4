import { useState } from 'react';
import { View } from 'react-native';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { cores, espaco, fonte } from '@/tema/tokens';
import { Progresso, Texto } from '@/ui/base';
import { Abas } from '@/ui/Abas';
import { Botao } from '@/ui/Botao';
import { CartaoCurso, Kpi, Meta } from '@/ui/Cartoes';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Grade, Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Certificados — pages/estudante/certificados.html + carregarCertificados()
   --------------------------------------------------------------------------
   Dois estados, os que o modelo sustenta: EMITIDO (curso concluído) e EM
   ANDAMENTO (matriculado, certificado a emitir). "Bloqueado" e "nota final"
   ficaram de fora pelas razões registradas no web: o modelo não tem
   pré-requisito entre cursos nem média de curso.
   ========================================================================== */

type Filtro = 'todos' | 'emitido' | 'andamento';

export default function Certificados() {
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(async () => {
    const [certs, historico] = await Promise.all([API.certificados(), API.historico()]);
    return { certs, emAndamento: historico.filter(h => h.status !== 'Concluido') };
  });
  const [filtro, setFiltro] = useState<Filtro>('todos');

  if (carregando) return <Tela><Carregando /></Tela>;
  if (erro || !dados) return <Tela><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;

  const { certs, emAndamento } = dados;
  const horas = certs.reduce((t, c) => t + (c.cargaHoraria || 0), 0);

  const emitidos = filtro === 'andamento' ? [] : certs;
  const andamento = filtro === 'emitido' ? [] : emAndamento;

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Grade colunas={3}>
        <Kpi rotulo="Emitidos" valor={certs.length} tom="sucesso" />
        <Kpi rotulo="Em andamento" valor={emAndamento.length} tom="alerta" />
        <Kpi rotulo="Horas" valor={`${horas}h`} detalhe="certificadas" tom="primaria" />
      </Grade>
      <View style={{ height: espaco.lg }} />
      <Abas valor={filtro} onChange={setFiltro} abas={[
        { valor: 'todos', texto: 'Todos' }, { valor: 'emitido', texto: 'Emitidos' }, { valor: 'andamento', texto: 'Em andamento' }
      ]} />

      {emitidos.length + andamento.length === 0 ? (
        <Vazio icone="certificate"
          texto={certs.length || emAndamento.length ? 'Nenhum certificado nesta situação.' : 'Você ainda não está matriculado em nenhum curso.'}
          acao={!certs.length && !emAndamento.length ? { texto: 'Ver catálogo', onPress: () => router.navigate('/aluno/cursos') } : undefined} />
      ) : (
        <Grade>
          {emitidos.map(c => (
            <CartaoCurso key={'c' + c.id} titulo={c.curso} rotulo="Emitido" iconeRotulo="check"
              gradiente={['#15803D', '#166534']} icone="certificate">
              <Texto v="xs" cor={cores.textoApagado}>Emitido em <Texto v="xs" peso="semi">{FMT.data(c.dataEmissao)}</Texto></Texto>
              <Texto v="xs" cor={cores.textoApagado} style={{ marginTop: 4, marginBottom: espaco.md }}>
                Código: <Texto v="xs" style={{ fontFamily: fonte.mono }}>{c.codigoAutenticacao}</Texto>
              </Texto>
              <Meta itens={[{ icone: 'clock', texto: `${c.cargaHoraria || 0}h` }, { icone: 'user', texto: c.instrutor || 'tutor não informado' }]} />
              <View style={{ flex: 1 }} />
              <Botao texto="Ver certificado" icone="eye" variante="sucesso" cheio
                onPress={() => router.push({ pathname: '/aluno/certificado/[codigo]', params: { codigo: c.codigoAutenticacao } })} />
            </CartaoCurso>
          ))}
          {andamento.map(h => (
            <CartaoCurso key={'h' + h.idCurso} titulo={h.curso} rotulo="Em andamento" iconeRotulo="clock"
              gradiente={['#F59E0B', '#B45309']} icone="certificate">
              <Texto v="sm" cor={cores.textoSecundario} style={{ marginBottom: espaco.md }}>
                O certificado é emitido automaticamente quando você concluir todos os materiais e for aprovado na prova do curso.
              </Texto>
              <View style={{ flexDirection: 'row', justifyContent: 'space-between', marginBottom: 4 }}>
                <Texto v="xs">{h.aulasConcluidas} de {h.totalAulas} materiais</Texto>
                <Texto v="xs" peso="semi" cor={cores.texto}>{h.percentualCurso}%</Texto>
              </View>
              <Progresso valor={h.percentualCurso} tom="alerta" />
              <View style={{ flex: 1, minHeight: espaco.md }} />
              <Botao texto="Continuar o curso" icone="player-play" variante="fantasma" cheio
                onPress={() => router.push({ pathname: '/aluno/curso/[id]', params: { id: h.idCurso } })} />
            </CartaoCurso>
          ))}
        </Grade>
      )}
    </Tela>
  );
}
