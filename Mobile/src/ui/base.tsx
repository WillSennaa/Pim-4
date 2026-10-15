import type { PropsWithChildren, ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View, type StyleProp, type TextProps, type TextStyle, type ViewStyle } from 'react-native';

import { cores, espaco, fonte, raio, tons, type Tom } from '@/tema/tokens';
import { ICONES, type NomeIcone } from './icones';

/* ==========================================================================
   Componentes básicos — cada um corresponde a uma classe do style.css
   --------------------------------------------------------------------------
   No web a aparência vem de classes (.card, .badge--success...). Em React
   Native não há classe nem cascata, então a classe vira COMPONENTE: <Cartao>
   é o .card, <Selo tom="sucesso"> é o .badge--success. As telas montam a
   interface com esses blocos e quase não escrevem estilo próprio — o mesmo
   efeito que o style.css único tinha sobre as 47 páginas.
   ========================================================================== */

// ---------------- Texto ----------------
type VarianteTexto =
  | 'titulo'       // .page-title
  | 'subtitulo'    // .page-subtitle
  | 'secao'        // .section-title
  | 'tituloCartao' // .card__title
  | 'corpo'
  | 'sm'           // .text-sm
  | 'xs'           // .text-xs
  | 'rotulo';      // rótulos em caixa alta dos KPIs

const estiloTexto: Record<VarianteTexto, TextStyle> = {
  titulo: { fontFamily: fonte.negrito, fontSize: 24, color: cores.texto, letterSpacing: -0.3 },
  subtitulo: { fontFamily: fonte.regular, fontSize: 14, color: cores.textoSecundario, lineHeight: 20 },
  secao: { fontFamily: fonte.semi, fontSize: 17, color: cores.texto },
  tituloCartao: { fontFamily: fonte.semi, fontSize: 14, color: cores.texto, letterSpacing: 0.3 },
  corpo: { fontFamily: fonte.regular, fontSize: 15, color: cores.texto, lineHeight: 22 },
  sm: { fontFamily: fonte.regular, fontSize: 13, color: cores.texto, lineHeight: 19 },
  xs: { fontFamily: fonte.regular, fontSize: 12, color: cores.textoSecundario, lineHeight: 16 },
  rotulo: { fontFamily: fonte.semi, fontSize: 11, color: cores.textoApagado, letterSpacing: 1 }
};

type PesoTexto = 'regular' | 'media' | 'semi' | 'negrito';

interface PropsTexto extends TextProps {
  v?: VarianteTexto;
  cor?: string;
  peso?: PesoTexto;
  centro?: boolean;
}

export function Texto({ v = 'corpo', cor, peso, centro, style, ...resto }: PropsTexto) {
  return (
    <Text
      {...resto}
      style={[
        estiloTexto[v],
        peso && { fontFamily: fonte[peso] },
        cor ? { color: cor } : null,
        centro && { textAlign: 'center' },
        style
      ]}
    />
  );
}

// ---------------- Ícone ----------------
export function Icone({ nome, tamanho = 20, cor = cores.textoSecundario, traco = 2 }:
  { nome: NomeIcone; tamanho?: number; cor?: string; traco?: number }) {
  const Componente = ICONES[nome];
  return <Componente size={tamanho} color={cor} strokeWidth={traco} />;
}

// ---------------- Cartão (.card) ----------------
interface PropsCartao extends PropsWithChildren {
  /** .card--accent-top / --accent-success / --accent-warning / --accent-danger */
  destaqueTopo?: Tom;
  /** Borda lateral colorida usada nas listas de provas e dúvidas. */
  destaqueLateral?: Tom;
  semPadding?: boolean;
  onPress?: () => void;
  style?: StyleProp<ViewStyle>;
  acessibilidade?: string;
}

export function Cartao({ children, destaqueTopo, destaqueLateral, semPadding, onPress, style, acessibilidade }: PropsCartao) {
  const estilo: StyleProp<ViewStyle> = [
    s.cartao,
    semPadding && { padding: 0, overflow: 'hidden' },
    destaqueTopo && { borderTopWidth: 4, borderTopColor: tons[destaqueTopo].forte },
    destaqueLateral && { borderLeftWidth: 3, borderLeftColor: tons[destaqueLateral].forte },
    style
  ];
  if (!onPress) return <View style={estilo}>{children}</View>;
  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      accessibilityLabel={acessibilidade}
      android_ripple={{ color: cores.primariaClara }}
      style={({ pressed }) => [estilo, pressed && { opacity: 0.85 }]}
    >
      {children}
    </Pressable>
  );
}

// ---------------- Selo (.badge) ----------------
export function Selo({ texto, tom = 'primaria', icone }: { texto: string; tom?: Tom; icone?: NomeIcone }) {
  const t = tons[tom];
  return (
    <View style={[s.selo, { backgroundColor: t.claro }]}>
      {icone && <Icone nome={icone} tamanho={12} cor={t.escuro} traco={2.5} />}
      <Text style={[s.seloTexto, { color: tom === 'neutro' ? cores.textoSecundario : t.escuro }]}>
        {texto.toUpperCase()}
      </Text>
    </View>
  );
}

// ---------------- Barra de progresso (.progress) ----------------
export function Progresso({ valor, tom = 'primaria', altura = 8 }: { valor: number; tom?: Tom; altura?: number }) {
  const pct = Math.max(0, Math.min(100, Math.round(valor || 0)));
  return (
    <View
      style={[s.progresso, { height: altura }]}
      accessibilityRole="progressbar"
      accessibilityValue={{ min: 0, max: 100, now: pct }}
    >
      <View style={[s.progressoBarra, { width: `${pct}%`, backgroundColor: tons[tom].forte }]} />
    </View>
  );
}

// ---------------- Avatar (.avatar) ----------------
export function Avatar({ iniciais, tamanho = 40, fundo = cores.primariaClara, cor = cores.primariaEscura }:
  { iniciais: string; tamanho?: number; fundo?: string; cor?: string }) {
  return (
    <View style={[s.avatar, { width: tamanho, height: tamanho, backgroundColor: fundo }]}>
      <Text style={{ fontFamily: fonte.semi, color: cor, fontSize: Math.round(tamanho * 0.36) }}>{iniciais}</Text>
    </View>
  );
}

// ---------------- Caixa de ícone (.list-item__icon) ----------------
export function CaixaIcone({ nome, tom = 'primaria', tamanho = 40, children }:
  { nome?: NomeIcone; tom?: Tom; tamanho?: number; children?: ReactNode }) {
  const t = tons[tom];
  return (
    <View style={[s.caixaIcone, { width: tamanho, height: tamanho, backgroundColor: t.claro }]}>
      {nome ? <Icone nome={nome} tamanho={Math.round(tamanho * 0.5)} cor={tom === 'neutro' ? cores.textoSecundario : t.escuro} /> : children}
    </View>
  );
}

// ---------------- Item de lista (.list-item) ----------------
interface PropsItem {
  icone?: NomeIcone;
  tomIcone?: Tom;
  esquerda?: ReactNode;
  titulo: string;
  subtitulo?: string | null;
  detalhe?: string | null;
  direita?: ReactNode;
  onPress?: () => void;
  ultimo?: boolean;
  linhasSubtitulo?: number;
}

export function ItemLista({ icone, tomIcone, esquerda, titulo, subtitulo, detalhe, direita, onPress, ultimo, linhasSubtitulo = 2 }: PropsItem) {
  const conteudo = (
    <>
      {esquerda ?? (icone ? <CaixaIcone nome={icone} tom={tomIcone} /> : null)}
      <View style={{ flex: 1, minWidth: 0 }}>
        <Text style={s.itemTitulo} numberOfLines={2}>{titulo}</Text>
        {!!subtitulo && <Text style={s.itemSub} numberOfLines={linhasSubtitulo}>{subtitulo}</Text>}
        {!!detalhe && <Texto v="xs" cor={cores.textoApagado} style={{ marginTop: 2 }}>{detalhe}</Texto>}
      </View>
      {direita !== undefined ? direita : (onPress ? <Icone nome="chevron-right" tamanho={18} cor={cores.textoApagado} /> : null)}
    </>
  );
  const estilo = [s.item, ultimo && { borderBottomWidth: 0 }];
  return onPress
    ? <Pressable onPress={onPress} accessibilityRole="button" style={({ pressed }) => [estilo, pressed && { opacity: 0.6 }]}>{conteudo}</Pressable>
    : <View style={estilo}>{conteudo}</View>;
}

// ---------------- Cabeçalho de cartão (.card__header) ----------------
export function CabecalhoCartao({ titulo, direita }: { titulo: string; direita?: ReactNode }) {
  return (
    <View style={s.cabecalhoCartao}>
      <Texto v="tituloCartao" style={{ flex: 1 }}>{titulo}</Texto>
      {direita}
    </View>
  );
}

/** Link de texto à direita de um cabeçalho ("Ver todos"). */
export function LinkTexto({ texto, onPress, cor = cores.primaria }: { texto: string; onPress: () => void; cor?: string }) {
  return (
    <Pressable onPress={onPress} hitSlop={10} accessibilityRole="link">
      <Text style={{ fontFamily: fonte.semi, fontSize: 13, color: cor }}>{texto}</Text>
    </Pressable>
  );
}

/** Linha "rótulo .......... valor" das telas de detalhe. */
export function LinhaInfo({ rotulo, valor, ultimo }: { rotulo: string; valor: string; ultimo?: boolean }) {
  return (
    <View style={[s.linhaInfo, ultimo && { borderBottomWidth: 0 }]}>
      <Texto v="sm" cor={cores.textoSecundario}>{rotulo}</Texto>
      <Texto v="sm" peso="media" style={{ flexShrink: 1, textAlign: 'right' }}>{valor}</Texto>
    </View>
  );
}

/** Espaço vertical explícito, para não espalhar marginTop pelas telas. */
export function Espaco({ h = espaco.md }: { h?: number }) {
  return <View style={{ height: h }} />;
}

export function Linha({ children, gap = espaco.sm, style, wrap }:
  PropsWithChildren<{ gap?: number; style?: StyleProp<ViewStyle>; wrap?: boolean }>) {
  return <View style={[{ flexDirection: 'row', alignItems: 'center', gap }, wrap && { flexWrap: 'wrap' }, style]}>{children}</View>;
}

export const s = StyleSheet.create({
  cartao: {
    backgroundColor: cores.superficie,
    borderRadius: raio.lg,
    padding: espaco.lg,
    borderWidth: 1,
    borderColor: cores.borda,
    marginBottom: espaco.md
  },
  selo: {
    flexDirection: 'row', alignItems: 'center', alignSelf: 'flex-start', gap: espaco.xs,
    paddingVertical: 4, paddingHorizontal: 10, borderRadius: raio.total
  },
  seloTexto: { fontFamily: fonte.semi, fontSize: 11, letterSpacing: 0.5 },
  progresso: { width: '100%', backgroundColor: cores.borda, borderRadius: raio.total, overflow: 'hidden' },
  progressoBarra: { height: '100%', borderRadius: raio.total },
  avatar: { borderRadius: raio.total, alignItems: 'center', justifyContent: 'center' },
  caixaIcone: { borderRadius: raio.md, alignItems: 'center', justifyContent: 'center' },
  item: {
    flexDirection: 'row', alignItems: 'center', gap: espaco.md,
    paddingVertical: espaco.md, borderBottomWidth: 1, borderBottomColor: cores.borda
  },
  itemTitulo: { fontFamily: fonte.media, fontSize: 14, color: cores.texto },
  itemSub: { fontFamily: fonte.regular, fontSize: 12, color: cores.textoSecundario, marginTop: 2, lineHeight: 16 },
  cabecalhoCartao: { flexDirection: 'row', alignItems: 'center', marginBottom: espaco.md, gap: espaco.sm },
  linhaInfo: {
    flexDirection: 'row', justifyContent: 'space-between', gap: espaco.md,
    paddingVertical: espaco.sm, borderBottomWidth: 1, borderBottomColor: cores.borda
  }
});
