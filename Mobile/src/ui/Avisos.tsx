import { createContext, use, useCallback, useEffect, useMemo, useRef, useState, type PropsWithChildren, type ReactNode } from 'react';
import { Animated, KeyboardAvoidingView, Modal, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { cores, espaco, fonte, raio, sombra, tons, type Tom } from '@/tema/tokens';
import { CaixaIcone, Texto } from './base';
import { Botao } from './Botao';
import type { NomeIcone } from './icones';

/* ==========================================================================
   Avisos e confirmações — avisar() e .modal do web
   --------------------------------------------------------------------------
   avisar(msg, tipo) mostra uma faixa no topo que some sozinha, como a do
   web. confirmar() abre o mesmo cartão de modal (.modal com ícone, título,
   texto e dois botões) e devolve uma Promise<boolean>, para a tela escrever
   "if (!(await confirmar(...))) return;" em uma linha.

   ALTERNATIVA REJEITADA: Alert.alert, o diálogo nativo do Android. Funciona,
   mas tem a cara do sistema e não a do Tech Quest, não aceita ícone nem
   botão vermelho de ação destrutiva com o estilo da plataforma.
   ========================================================================== */

type TipoAviso = 'sucesso' | 'erro' | 'info';

interface Confirmacao {
  titulo: string;
  texto?: string;
  confirmar?: string;
  cancelar?: string;
  perigo?: boolean;
  icone?: NomeIcone;
}

interface ValorAvisos {
  avisar: (mensagem: string, tipo?: TipoAviso) => void;
  confirmar: (c: Confirmacao) => Promise<boolean>;
}

const Contexto = createContext<ValorAvisos | null>(null);

export function useAvisos(): ValorAvisos {
  const v = use(Contexto);
  if (!v) throw new Error('useAvisos precisa estar dentro de <AvisosProvider>.');
  return v;
}

const TOM_AVISO: Record<TipoAviso, { tom: Tom; icone: NomeIcone }> = {
  sucesso: { tom: 'sucesso', icone: 'circle-check' },
  erro: { tom: 'perigo', icone: 'alert-triangle' },
  info: { tom: 'primaria', icone: 'info-circle' }
};

export function AvisosProvider({ children }: PropsWithChildren) {
  const insets = useSafeAreaInsets();
  const [aviso, setAviso] = useState<{ msg: string; tipo: TipoAviso; chave: number } | null>(null);
  const [opacidade] = useState(() => new Animated.Value(0));
  const timer = useRef<ReturnType<typeof setTimeout> | null>(null);

  const [pedido, setPedido] = useState<(Confirmacao & { resolver: (v: boolean) => void }) | null>(null);

  const avisar = useCallback((msg: string, tipo: TipoAviso = 'sucesso') => {
    if (timer.current) clearTimeout(timer.current);
    setAviso({ msg, tipo, chave: Date.now() });
    // Erro fica mais tempo: precisa ser lido, não só notado.
    timer.current = setTimeout(() => setAviso(null), tipo === 'erro' ? 5500 : 3500);
  }, []);

  useEffect(() => {
    Animated.timing(opacidade, { toValue: aviso ? 1 : 0, duration: 200, useNativeDriver: Platform.OS !== 'web' }).start();
  }, [aviso, opacidade]);

  const confirmar = useCallback((c: Confirmacao) =>
    new Promise<boolean>(resolver => setPedido({ ...c, resolver })), []);

  const responder = (v: boolean) => { pedido?.resolver(v); setPedido(null); };

  const valor = useMemo(() => ({ avisar, confirmar }), [avisar, confirmar]);
  const t = aviso ? tons[TOM_AVISO[aviso.tipo].tom] : null;

  return (
    <Contexto value={valor}>
      {children}

      {aviso && t && (
        <Animated.View pointerEvents="box-none" style={[st.avisoArea, { top: insets.top + espaco.sm, opacity: opacidade }]}>
          <Pressable onPress={() => setAviso(null)} style={[st.aviso, { borderLeftColor: t.forte }]} accessibilityLiveRegion="polite">
            <CaixaIcone nome={TOM_AVISO[aviso.tipo].icone} tom={TOM_AVISO[aviso.tipo].tom} tamanho={32} />
            <Text style={st.avisoTexto}>{aviso.msg}</Text>
          </Pressable>
        </Animated.View>
      )}

      <Dialogo visivel={!!pedido} aoFechar={() => responder(false)}>
        {pedido && (
          <View style={{ alignItems: 'center' }}>
            <CaixaIcone nome={pedido.icone ?? (pedido.perigo ? 'alert-triangle' : 'help-circle')}
              tom={pedido.perigo ? 'perigo' : 'primaria'} tamanho={56} />
            <Texto v="secao" centro style={{ marginTop: espaco.md }}>{pedido.titulo}</Texto>
            {!!pedido.texto && <Texto v="sm" cor={cores.textoSecundario} centro style={{ marginTop: espaco.sm }}>{pedido.texto}</Texto>}
            <View style={st.acoes}>
              <Botao texto={pedido.cancelar ?? 'Cancelar'} variante="fantasma" onPress={() => responder(false)} style={{ flex: 1 }} />
              <Botao texto={pedido.confirmar ?? 'Confirmar'} variante={pedido.perigo ? 'perigo' : 'primario'}
                onPress={() => responder(true)} style={{ flex: 1 }} />
            </View>
          </View>
        )}
      </Dialogo>
    </Contexto>
  );
}

/**
 * Cartão modal genérico (.modal-overlay + .modal). Usado também pelos
 * formulários curtos: responder dúvida, mensagem ao aluno.
 */
export function Dialogo({ visivel, aoFechar, titulo, children }:
  { visivel: boolean; aoFechar: () => void; titulo?: string; children?: ReactNode }) {
  return (
    <Modal visible={visivel} transparent animationType="fade" onRequestClose={aoFechar} statusBarTranslucent>
      <KeyboardAvoidingView style={st.fundo} behavior={Platform.OS === 'ios' ? 'padding' : 'height'}>
        <Pressable style={StyleSheet.absoluteFill} onPress={aoFechar} accessibilityLabel="Fechar" />
        <View style={st.modal}>
          <ScrollView keyboardShouldPersistTaps="handled" contentContainerStyle={{ padding: espaco.xl }}>
            {titulo && <Texto v="secao" style={{ marginBottom: espaco.md }}>{titulo}</Texto>}
            {children}
          </ScrollView>
        </View>
      </KeyboardAvoidingView>
    </Modal>
  );
}

const st = StyleSheet.create({
  avisoArea: { position: 'absolute', left: espaco.lg, right: espaco.lg, alignItems: 'center', zIndex: 100 },
  aviso: {
    width: '100%', maxWidth: 560, flexDirection: 'row', alignItems: 'center', gap: espaco.md,
    backgroundColor: cores.superficie, borderRadius: raio.md, padding: espaco.md,
    borderLeftWidth: 4, borderWidth: 1, borderColor: cores.borda, ...sombra.md
  },
  avisoTexto: { flex: 1, fontFamily: fonte.media, fontSize: 14, color: cores.texto, lineHeight: 20 },
  fundo: { flex: 1, backgroundColor: 'rgba(15, 23, 42, 0.5)', justifyContent: 'center', alignItems: 'center', padding: espaco.lg },
  modal: {
    width: '100%', maxWidth: 480, maxHeight: '90%',
    backgroundColor: cores.superficie, borderRadius: raio.lg, ...sombra.md
  },
  acoes: { flexDirection: 'row', gap: espaco.sm, marginTop: espaco.xl, alignSelf: 'stretch' }
});
