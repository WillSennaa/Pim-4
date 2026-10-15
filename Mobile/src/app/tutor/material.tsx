import { useState } from 'react';
import { View } from 'react-native';
import { router, Stack, useLocalSearchParams } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import type { Material } from '@/api/tipos';
import { useCarregar } from '@/hooks/useCarregar';
import { cores, espaco } from '@/tema/tokens';
import { Cartao, Texto } from '@/ui/base';
import { Abas } from '@/ui/Abas';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo, Opcoes } from '@/ui/Campo';
import { ConteudoMaterial } from '@/ui/Codigo';
import { Carregando, ErroCarga, Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';

/* ==========================================================================
   Editor de material — adicionarAula() e salvarMaterial() do web
   --------------------------------------------------------------------------
   Serve para criar (sem ?id) e editar (com ?id). Tem uma aba PRÉ-VISUALIZAR
   que renderiza o texto com o MESMO componente da tela do aluno: o tutor vê
   exatamente como o parágrafo e o bloco de código vão aparecer antes de
   salvar. No web isso só era possível abrindo o curso como aluno.

   Tipos: "texto" e "exercicio". PDF e vídeo ficaram de fora pelo motivo
   registrado no script do banco: a plataforma não armazena arquivo nem tem
   player. Um tipo antigo diferente continua aparecendo, para não ser trocado
   sem o tutor perceber.
   ========================================================================== */

export default function EditorMaterial() {
  const params = useLocalSearchParams<{ curso: string; id?: string }>();
  const idCurso = Number(params.curso);
  const idMaterial = params.id ? Number(params.id) : null;
  const { dados, erro, carregando, recarregar } = useCarregar(async () => {
    const [curso, meus] = await Promise.all([API.curso(idCurso), API.tutorCursos()]);
    const material = idMaterial ? curso.materiais.find(m => m.id === idMaterial) ?? null : null;
    const editavel = meus.find(c => c.id === idCurso)?.podeEditar ?? false;
    return { curso, material, editavel };
  }, [idCurso, idMaterial]);

  if (carregando) return <Tela largura="leitura"><Carregando /></Tela>;
  if (erro || !dados) return <Tela largura="leitura"><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;
  if (idMaterial && !dados.material) return <Tela largura="leitura"><Faixa tom="perigo" texto="Material não encontrado neste curso." /></Tela>;

  return <Formulario idCurso={idCurso} idMaterial={idMaterial} nomeCurso={dados.curso.nome}
    material={dados.material} editavel={dados.editavel} />;
}

function Formulario({ idCurso, idMaterial, nomeCurso, material, editavel }: {
  idCurso: number; idMaterial: number | null; nomeCurso: string; material: Material | null; editavel: boolean;
}) {
  const { avisar } = useAvisos();
  const [titulo, setTitulo] = useState(material?.titulo ?? '');
  const [tipo, setTipo] = useState(material?.tipo || 'texto');
  const [conteudo, setConteudo] = useState(material?.conteudo ?? '');
  const [aba, setAba] = useState<'editar' | 'ver'>('editar');
  const [salvando, setSalvando] = useState(false);

  const tipos = [{ valor: 'texto', texto: 'Texto' }, { valor: 'exercicio', texto: 'Lista de exercícios' }];
  if (!tipos.some(t => t.valor === tipo)) tipos.push({ valor: tipo, texto: `${tipo} (tipo antigo)` });

  async function salvar() {
    if (!titulo.trim()) return avisar('O título do material é obrigatório.', 'erro');
    setSalvando(true);
    try {
      if (idMaterial) await API.tutorAtualizarAula(idMaterial, titulo.trim(), tipo, conteudo || null);
      else await API.tutorAdicionarAula(idCurso, titulo.trim(), tipo, conteudo || null);
      avisar(idMaterial ? 'Material salvo.' : 'Material adicionado.');
      router.back();
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível salvar o material.'), 'erro');
    } finally {
      setSalvando(false);
    }
  }

  return (
    <Tela largura="leitura" rodape={editavel
      ? <Botao texto={idMaterial ? 'Salvar material' : 'Adicionar material'} icone="device-floppy" cheio tamanho="lg"
          carregando={salvando} textoCarregando="Salvando..." onPress={salvar} />
      : undefined}>
      <Stack.Screen options={{ title: idMaterial ? 'Editar material' : 'Novo material' }} />
      <Texto v="xs" style={{ marginBottom: espaco.md }}>{nomeCurso}</Texto>
      {!editavel && <Faixa tom="alerta" icone="lock" texto="Este curso não pode ser alterado no estado atual. O material está em modo leitura." />}

      <Abas valor={aba} onChange={setAba} abas={[{ valor: 'editar', texto: 'Editar' }, { valor: 'ver', texto: 'Pré-visualizar' }]} />

      {aba === 'editar' ? (
        <Cartao>
          <Campo rotulo="Título" value={titulo} onChangeText={setTitulo} editable={editavel} maxLength={150}
            placeholder="Ex.: Apostila de herança" />
          <Opcoes rotulo="Tipo" opcoes={tipos} valor={tipo} onChange={setTipo} desabilitado={!editavel} />
          <Campo rotulo="Conteúdo do material" value={conteudo} onChangeText={setConteudo} linhas={14} editable={editavel}
            placeholder="Escreva o texto que o aluno vai ler. Linha em branco separa parágrafos; indente com quatro espaços para criar um bloco de código."
            ajuda={idMaterial ? undefined : 'Pode deixar em branco agora e escrever depois.'}
            autoCapitalize="sentences" />
        </Cartao>
      ) : (
        <View style={{ backgroundColor: cores.superficie, borderRadius: 16, padding: espaco.lg, borderWidth: 1, borderColor: cores.borda }}>
          <Texto v="secao" style={{ marginBottom: espaco.md }}>{titulo || 'Material sem título'}</Texto>
          {conteudo.trim()
            ? <ConteudoMaterial texto={conteudo} />
            : <Texto v="sm" cor={cores.textoApagado}>Sem conteúdo ainda.</Texto>}
        </View>
      )}
    </Tela>
  );
}
