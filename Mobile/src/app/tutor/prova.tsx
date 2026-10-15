import { useState } from 'react';
import { router, useLocalSearchParams } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { espaco } from '@/tema/tokens';
import { Cartao, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';

/* ==========================================================================
   Criar prova — criarProvaDoCurso() do web
   --------------------------------------------------------------------------
   Uma prova por curso (o servidor recusa a segunda). Nota mínima de 0 a 10,
   validada aqui por conforto e no servidor por regra.
   ========================================================================== */

export default function CriarProva() {
  const params = useLocalSearchParams<{ curso: string; nome?: string }>();
  const idCurso = Number(params.curso);
  const { avisar } = useAvisos();
  const [titulo, setTitulo] = useState(params.nome ? `Prova final de ${params.nome}`.slice(0, 100) : '');
  const [nota, setNota] = useState('7');
  const [tempo, setTempo] = useState('30');
  const [salvando, setSalvando] = useState(false);

  async function criar() {
    if (!titulo.trim()) return avisar('Informe o título da prova.', 'erro');
    const n = parseFloat(nota.replace(',', '.'));
    const t = parseInt(tempo, 10);
    if (isNaN(n) || n < 0 || n > 10) return avisar('A nota mínima deve estar entre 0 e 10.', 'erro');
    setSalvando(true);
    try {
      await API.tutorCriarProva(idCurso, { titulo: titulo.trim(), notaMinima: n, tempoMinutos: isNaN(t) ? 30 : t });
      avisar('Prova criada. Agora adicione as questões.');
      router.back();
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível criar a prova.'), 'erro');
    } finally {
      setSalvando(false);
    }
  }

  return (
    <Tela largura="formulario">
      <Cartao style={{ padding: espaco.xl }}>
        <Campo rotulo="Título da prova" value={titulo} onChangeText={setTitulo} maxLength={100} />
        <Campo rotulo="Nota mínima (0 a 10)" value={nota} onChangeText={setNota} keyboardType="decimal-pad"
          ajuda="Use vírgula ou ponto: 7 ou 7,5." />
        <Campo rotulo="Tempo (minutos)" value={tempo} onChangeText={t => setTempo(t.replace(/\D/g, ''))} keyboardType="number-pad" />
        <Faixa tom="primaria" icone="info-circle" texto="Depois de criar, adicione as questões. Prova sem questão é rejeitada na avaliação." />
        <Botao texto="Criar prova" icone="plus" cheio tamanho="lg" carregando={salvando} textoCarregando="Criando..." onPress={criar} />
      </Cartao>
      <Texto v="xs" centro style={{ marginTop: espaco.md }}>A prova só é liberada ao aluno depois que ele conclui todos os materiais.</Texto>
    </Tela>
  );
}
