import { useMemo, useState } from 'react';
import { View } from 'react-native';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { cores, espaco } from '@/tema/tokens';
import { Avatar, Cartao, Progresso, Selo, Texto } from '@/ui/base';
import { Abas } from '@/ui/Abas';
import { Campo } from '@/ui/Campo';
import { Carregando, ErroCarga, Vazio } from '@/ui/Estados';
import { Cabecalho, Grade, Tela } from '@/ui/Tela';
import { FMT, normalizar } from '@/util/formatacao';

/* ==========================================================================
   Alunos — pages/tutor/alunos.html
   --------------------------------------------------------------------------
   Uma linha por MATRÍCULA (aluno + curso), como devolve GET /api/tutor/alunos.
   O servidor só entrega alunos dos cursos DESTE tutor: o filtro está na
   consulta, não na tela.

   No web a lista é uma tabela; no celular, tabela de 6 colunas não cabe, e
   cada linha vira um cartão com as mesmas informações.
   ========================================================================== */

export default function AlunosTutor() {
  const { dados, erro, carregando, atualizando, puxar, recarregar } = useCarregar(() => API.tutorAlunos());
  const [busca, setBusca] = useState('');
  const [curso, setCurso] = useState('todos');

  const cursos = useMemo(() => {
    const m = new Map<number, string>();
    (dados ?? []).forEach(a => m.set(a.idCurso, a.curso));
    return [...m.entries()];
  }, [dados]);

  const lista = useMemo(() => {
    const termo = normalizar(busca);
    return (dados ?? []).filter(a =>
      (curso === 'todos' || String(a.idCurso) === curso) &&
      (!termo || normalizar(`${a.nome} ${a.email}`).includes(termo)));
  }, [dados, busca, curso]);

  return (
    <Tela aoAtualizar={puxar} atualizando={atualizando}>
      <Cabecalho titulo="Alunos" subtitulo="Progresso e notas de quem está matriculado nos seus cursos." />
      <Campo icone="search" placeholder="Buscar por nome ou e-mail" value={busca} onChangeText={setBusca} autoCapitalize="none" />
      {cursos.length > 1 && (
        <Abas pilulas valor={curso} onChange={setCurso}
          abas={[{ valor: 'todos', texto: 'Todos os cursos' }, ...cursos.map(([id, nome]) => ({ valor: String(id), texto: nome }))]} />
      )}

      {carregando ? <Carregando /> : erro ? <ErroCarga mensagem={erro} aoTentar={recarregar} /> : lista.length === 0 ? (
        <Vazio icone="users" texto={dados?.length ? 'Nenhum aluno encontrado com esse filtro.' : 'Nenhum aluno matriculado nos seus cursos ainda.'} />
      ) : (
        <>
          <Texto v="xs" style={{ marginBottom: espaco.sm }}>{FMT.plural(lista.length, 'matrícula', 'matrículas')}</Texto>
          <Grade>
            {lista.map(a => {
              const concluido = a.percentualCurso === 100;
              return (
                <Cartao key={`${a.idEstudante}-${a.idCurso}`} style={{ marginBottom: 0, flex: 1 }}
                  onPress={() => router.push({ pathname: '/tutor/aluno', params: { estudante: a.idEstudante, curso: a.idCurso } })}>
                  <View style={{ flexDirection: 'row', alignItems: 'center', gap: espaco.md, marginBottom: espaco.md }}>
                    <Avatar iniciais={FMT.iniciais(a.nome)} />
                    <View style={{ flex: 1 }}>
                      <Texto v="corpo" peso="semi" numberOfLines={1}>{a.nome}</Texto>
                      <Texto v="xs" numberOfLines={1}>{a.email || ''}</Texto>
                    </View>
                    {a.melhorNota != null && (
                      <Selo texto={`Nota ${FMT.nota(a.melhorNota)}`} tom={a.melhorNota >= 7 ? 'sucesso' : 'perigo'} />
                    )}
                  </View>
                  <Texto v="xs" cor={cores.textoSecundario} numberOfLines={1}>{a.curso}</Texto>
                  <View style={{ flexDirection: 'row', justifyContent: 'space-between', marginTop: 4, marginBottom: 4 }}>
                    <Texto v="xs">{a.aulasConcluidas} de {a.totalAulas} materiais</Texto>
                    <Texto v="xs" peso="semi" cor={cores.texto}>{a.percentualCurso}%</Texto>
                  </View>
                  <Progresso valor={a.percentualCurso} tom={concluido ? 'sucesso' : 'primaria'} />
                </Cartao>
              );
            })}
          </Grade>
        </>
      )}
    </Tela>
  );
}
