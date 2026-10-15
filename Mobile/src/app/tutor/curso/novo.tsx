import { useState } from 'react';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import type { DadosCurso } from '@/api/tipos';
import { espaco } from '@/tema/tokens';
import { Cartao, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';
import { FormCurso } from '@/telas/FormCurso';

/* ==========================================================================
   Novo curso — pages/tutor/criar-curso.html + criarCurso()
   --------------------------------------------------------------------------
   O curso nasce como RASCUNHO. Publicar depende de submeter para avaliação
   e de um administrador aprovar: a máquina de estados vive no servidor, e a
   tela só a percorre. Depois de criado, o tutor vai direto para o editor
   adicionar materiais e prova.
   ========================================================================== */

export default function NovoCurso() {
  const { avisar } = useAvisos();
  const [salvando, setSalvando] = useState(false);

  async function criar(d: DadosCurso) {
    setSalvando(true);
    try {
      const curso = await API.tutorCriarCurso(d);
      avisar(`Curso "${curso.nome}" criado como rascunho. Agora adicione os materiais.`);
      router.replace({ pathname: '/tutor/curso/[id]', params: { id: curso.id } });
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível criar o curso.'), 'erro');
    } finally {
      setSalvando(false);
    }
  }

  return (
    <Tela largura="formulario">
      <Texto v="subtitulo" style={{ marginBottom: espaco.lg }}>
        Preencha os dados básicos. Materiais e prova são adicionados no passo seguinte.
      </Texto>
      <Cartao style={{ padding: espaco.xl }}>
        <FormCurso textoBotao="Criar curso" salvando={salvando} aoSalvar={criar} />
      </Cartao>
      <Faixa tom="primaria" icone="info-circle"
        texto="Fluxo de publicação: rascunho → em avaliação → publicado. Enquanto estiver em avaliação ou publicado, o conteúdo fica bloqueado." />
    </Tela>
  );
}
