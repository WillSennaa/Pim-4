import { forwardRef, useState } from 'react';
import { Pressable, StyleSheet, Text, TextInput, View, type TextInputProps } from 'react-native';

import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { Icone } from './base';
import type { NomeIcone } from './icones';

/* ==========================================================================
   Campo de formulário — .form-group + .form-label + .form-input
   --------------------------------------------------------------------------
   Campo de senha ganha o "olho" para mostrar o que foi digitado: no teclado
   do celular o erro de digitação é muito mais comum que no computador, e
   esconder a senha sem opção de conferir gera tentativa de login frustrada.
   ========================================================================== */

interface Props extends TextInputProps {
  rotulo?: string;
  icone?: NomeIcone;
  ajuda?: string;
  erro?: string | null;
  senha?: boolean;
  mono?: boolean;
  linhas?: number;
}

export const Campo = forwardRef<TextInput, Props>(function Campo(
  { rotulo, icone, ajuda, erro, senha, mono, linhas, style, editable = true, ...resto }, ref
) {
  const [focado, setFocado] = useState(false);
  const [visivel, setVisivel] = useState(false);
  const multilinha = !!linhas && linhas > 1;

  return (
    <View style={st.grupo}>
      {rotulo && <Text style={st.rotulo}>{rotulo}</Text>}
      <View
        style={[
          st.caixa,
          focado && st.focado,
          !!erro && { borderColor: cores.perigo },
          !editable && { backgroundColor: cores.superficie2 }
        ]}
      >
        {icone && (
          <View style={[st.icone, multilinha && { alignSelf: 'flex-start', marginTop: 12 }]}>
            <Icone nome={icone} tamanho={18} cor={focado ? cores.primaria : cores.textoApagado} />
          </View>
        )}
        <TextInput
          ref={ref}
          {...resto}
          editable={editable}
          secureTextEntry={senha && !visivel}
          multiline={multilinha}
          numberOfLines={linhas}
          placeholderTextColor={cores.textoApagado}
          onFocus={e => { setFocado(true); resto.onFocus?.(e); }}
          onBlur={e => { setFocado(false); resto.onBlur?.(e); }}
          style={[
            st.input,
            mono && { fontFamily: fonte.mono, fontSize: 13 },
            multilinha && { minHeight: Math.max(1, linhas!) * 20 + 24, textAlignVertical: 'top' },
            !editable && { color: cores.textoSecundario },
            style
          ]}
        />
        {senha && (
          <Pressable
            onPress={() => setVisivel(v => !v)}
            hitSlop={10}
            style={st.olho}
            accessibilityRole="button"
            accessibilityLabel={visivel ? 'Ocultar senha' : 'Mostrar senha'}
          >
            <Icone nome={visivel ? 'eye-off' : 'eye'} tamanho={18} cor={cores.textoApagado} />
          </Pressable>
        )}
      </View>
      {!!erro && <Text style={[st.ajuda, { color: cores.perigo }]}>{erro}</Text>}
      {!erro && !!ajuda && <Text style={st.ajuda}>{ajuda}</Text>}
    </View>
  );
});

/** Grupo de opções (substitui o <select> do web, que no toque é ruim de usar). */
export function Opcoes<T extends string | number>({ rotulo, opcoes, valor, onChange, desabilitado }: {
  rotulo?: string;
  opcoes: { valor: T; texto: string }[];
  valor: T | null;
  onChange: (v: T) => void;
  desabilitado?: boolean;
}) {
  return (
    <View style={st.grupo}>
      {rotulo && <Text style={st.rotulo}>{rotulo}</Text>}
      <View style={st.opcoes}>
        {opcoes.map(o => {
          const ativo = o.valor === valor;
          return (
            <Pressable
              key={String(o.valor)}
              onPress={desabilitado ? undefined : () => onChange(o.valor)}
              accessibilityRole="radio"
              accessibilityState={{ selected: ativo, disabled: desabilitado }}
              style={[st.opcao, ativo && st.opcaoAtiva, desabilitado && { opacity: 0.6 }]}
            >
              <Text style={[st.opcaoTexto, ativo && { color: cores.primaria, fontFamily: fonte.semi }]}>{o.texto}</Text>
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

const st = StyleSheet.create({
  grupo: { marginBottom: espaco.lg },
  rotulo: { fontFamily: fonte.media, fontSize: 14, color: cores.texto, marginBottom: 6 },
  caixa: {
    flexDirection: 'row', alignItems: 'center',
    borderWidth: 1, borderColor: cores.borda, borderRadius: raio.md, backgroundColor: cores.superficie
  },
  focado: { borderColor: cores.primaria },
  icone: { paddingLeft: 14 },
  input: {
    flex: 1, minWidth: 0, paddingVertical: 12, paddingHorizontal: 14,
    fontFamily: fonte.regular, fontSize: 15, color: cores.texto, minHeight: 46
  },
  olho: { paddingHorizontal: 14, paddingVertical: 12 },
  ajuda: { fontFamily: fonte.regular, fontSize: 12, color: cores.textoApagado, marginTop: 4 },
  opcoes: { flexDirection: 'row', flexWrap: 'wrap', gap: espaco.sm },
  opcao: {
    paddingVertical: 10, paddingHorizontal: 16, borderRadius: raio.total,
    borderWidth: 1, borderColor: cores.borda, backgroundColor: cores.superficie
  },
  opcaoAtiva: { borderColor: cores.primaria, backgroundColor: cores.primariaMaisClara },
  opcaoTexto: { fontFamily: fonte.media, fontSize: 14, color: cores.textoSecundario }
});
