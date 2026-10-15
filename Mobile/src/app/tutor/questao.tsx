import { useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { Cartao, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Tela } from '@/ui/Tela';

/* ==========================================================================
   Nova questão — adicionarQuestao() do web
   --------------------------------------------------------------------------
   Cinco alternativas (A a E); as que ficarem em branco são ignoradas, porque
   nem toda questão usa as cinco. Exatamente uma é marcada como correta — o
   toque na letra marca o gabarito. As mesmas regras são revalidadas no
   servidor (RegrasQuestao, no domínio).
   ========================================================================== */

const LETRAS = ['A', 'B', 'C', 'D', 'E'] as const;

export default function NovaQuestao() {
  const params = useLocalSearchParams<{ prova: string; ordem?: string }>();
  const idProva = Number(params.prova);
  const ordem = Number(params.ordem) || 1;
  const { avisar } = useAvisos();
  const [enunciado, setEnunciado] = useState('');
  const [codigo, setCodigo] = useState('');
  const [textos, setTextos] = useState<Record<string, string>>({});
  const [correta, setCorreta] = useState<string>('A');
  const [salvando, setSalvando] = useState(false);

  async function salvar() {
    if (!enunciado.trim()) return avisar('Informe o enunciado.', 'erro');
    const alternativas = LETRAS
      .map(letra => ({ letra, texto: (textos[letra] || '').trim(), ehCorreta: letra === correta }))
      .filter(a => a.texto);
    if (alternativas.length < 2) return avisar('Preencha ao menos duas alternativas.', 'erro');
    if (!alternativas.some(a => a.ehCorreta)) return avisar('A alternativa marcada como correta está sem texto.', 'erro');

    setSalvando(true);
    try {
      await API.tutorAdicionarQuestao(idProva, { enunciado: enunciado.trim(), codigoExemplo: codigo.trim() || null, ordem, alternativas });
      avisar(`Questão ${ordem} adicionada.`);
      router.back();
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível adicionar a questão.'), 'erro');
    } finally {
      setSalvando(false);
    }
  }

  return (
    <Tela largura="leitura" rodape={
      <Botao texto={`Adicionar questão ${ordem}`} icone="plus" cheio tamanho="lg" carregando={salvando}
        textoCarregando="Adicionando..." onPress={salvar} />
    }>
      <Cartao>
        <Campo rotulo="Enunciado" value={enunciado} onChangeText={setEnunciado} linhas={3} placeholder="O que a questão pergunta" />
        <Campo rotulo="Trecho de código (opcional)" value={codigo} onChangeText={setCodigo} linhas={4} mono
          autoCapitalize="none" autoCorrect={false} placeholder="Código exibido em bloco separado do enunciado" />
      </Cartao>

      <Texto v="secao" style={{ marginTop: espaco.sm }}>Alternativas</Texto>
      <Texto v="sm" cor={cores.textoSecundario} style={{ marginTop: 2, marginBottom: espaco.md }}>
        Toque na letra para marcar a correta. Alternativas em branco são ignoradas.
      </Texto>
      <Cartao>
        {LETRAS.map(letra => {
          const sel = correta === letra;
          return (
            <View key={letra} style={st.linha}>
              <Pressable onPress={() => setCorreta(letra)} accessibilityRole="radio" accessibilityState={{ selected: sel }}
                accessibilityLabel={`Marcar ${letra} como correta`} style={[st.letra, sel && st.letraSel]}>
                <Text style={[st.letraTexto, sel && { color: '#FFFFFF' }]}>{letra}</Text>
              </Pressable>
              <View style={{ flex: 1 }}>
                <Campo value={textos[letra] ?? ''} onChangeText={t => setTextos(x => ({ ...x, [letra]: t }))}
                  placeholder={`Texto da alternativa ${letra}`} maxLength={500} />
              </View>
            </View>
          );
        })}
        <Texto v="xs" cor={cores.sucessoEscuro}>Gabarito: alternativa {correta}</Texto>
      </Cartao>
    </Tela>
  );
}

const st = StyleSheet.create({
  linha: { flexDirection: 'row', gap: espaco.md, alignItems: 'flex-start' },
  letra: {
    width: 44, height: 46, borderRadius: raio.md, borderWidth: 1.5, borderColor: cores.bordaForte,
    alignItems: 'center', justifyContent: 'center'
  },
  letraSel: { backgroundColor: cores.sucesso, borderColor: cores.sucessoEscuro },
  letraTexto: { fontFamily: fonte.negrito, fontSize: 16, color: cores.textoSecundario }
});
