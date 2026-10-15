import { useState } from 'react';

import type { DadosCurso } from '@/api/tipos';
import { Botao } from '@/ui/Botao';
import { Campo, Opcoes } from '@/ui/Campo';

/* ==========================================================================
   Formulário de dados do curso (CriarCursoRequest)
   --------------------------------------------------------------------------
   Usado na criação e no editor. Nível vira escolha entre três opções em vez
   de texto livre: no web era um campo de texto, e o banco acabou com
   "Iniciante", "iniciante" e "Básico" para a mesma coisa. Um valor antigo
   diferente continua aparecendo como opção, para não ser apagado sem querer.

   O estado inicial vem das props uma vez só. Quando o curso é recarregado,
   o editor troca a "key" do formulário e o React o recria com os dados
   novos — em vez de um efeito copiando props para estado a cada mudança.
   ========================================================================== */

const NIVEIS = ['Iniciante', 'Intermediário', 'Avançado'];

export function FormCurso({ inicial, editavel = true, textoBotao, salvando, aoSalvar }: {
  inicial?: Partial<DadosCurso>;
  editavel?: boolean;
  textoBotao: string;
  salvando?: boolean;
  aoSalvar: (d: DadosCurso) => void;
}) {
  const [nome, setNome] = useState(inicial?.nome ?? '');
  const [descricao, setDescricao] = useState(inicial?.descricao ?? '');
  const [categoria, setCategoria] = useState(inicial?.categoria ?? '');
  const [nivel, setNivel] = useState<string | null>(inicial?.nivel ?? null);
  const [horas, setHoras] = useState(inicial?.duracaoHoras ? String(inicial.duracaoHoras) : '');
  const [erroNome, setErroNome] = useState<string | null>(null);


  const opcoesNivel = [...NIVEIS, ...(nivel && !NIVEIS.includes(nivel) ? [nivel] : [])].map(n => ({ valor: n, texto: n }));

  function enviar() {
    if (!nome.trim()) { setErroNome('O título do curso é obrigatório.'); return; }
    setErroNome(null);
    const h = parseInt(horas, 10);
    aoSalvar({
      nome: nome.trim(),
      descricao: descricao.trim() || null,
      categoria: categoria.trim() || null,
      nivel: nivel || null,
      duracaoHoras: isNaN(h) ? null : h
    });
  }

  return (
    <>
      <Campo rotulo="Título" value={nome} onChangeText={setNome} editable={editavel} erro={erroNome}
        placeholder="Ex.: Fundamentos de C#" maxLength={150} />
      <Campo rotulo="Descrição" value={descricao} onChangeText={setDescricao} linhas={3} editable={editavel}
        placeholder="O que o aluno vai aprender" />
      <Campo rotulo="Categoria" value={categoria} onChangeText={setCategoria} editable={editavel}
        placeholder="Ex.: Programação" maxLength={50} />
      <Opcoes rotulo="Nível" opcoes={opcoesNivel} valor={nivel} onChange={setNivel} desabilitado={!editavel} />
      <Campo rotulo="Carga horária (horas)" value={horas} onChangeText={t => setHoras(t.replace(/\D/g, ''))}
        keyboardType="number-pad" editable={editavel} placeholder="Ex.: 12" />
      {editavel && <Botao texto={textoBotao} icone="device-floppy" cheio carregando={salvando} textoCarregando="Salvando..." onPress={enviar} />}
    </>
  );
}
