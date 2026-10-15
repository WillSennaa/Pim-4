import { useState } from 'react';
import { LayoutAnimation, Pressable, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';

import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { CaixaIcone, Cartao, Icone, Texto } from '@/ui/base';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import type { NomeIcone } from '@/ui/icones';
import { Cabecalho, Grade, Secao, Tela } from '@/ui/Tela';
import type { Tom } from '@/tema/tokens';
import { normalizar } from '@/util/formatacao';

/* ==========================================================================
   Central de ajuda — pages/estudante/ajuda.html
   --------------------------------------------------------------------------
   Os atalhos são os do web: dúvida de conteúdo vai ao TUTOR do curso,
   problema técnico vai à fila dos ADMINISTRADORES. Quem decide o
   destinatário é o servidor, a partir do tipo.

   AS RESPOSTAS DO FAQ FORAM REESCRITAS a partir das regras reais do sistema.
   O FAQ do mock.json citava botão "Emitir" certificado, aba "Anotações" e
   download de dados pela LGPD — nada disso existe. Os números de XP abaixo
   são as constantes de RegrasGamificacao no domínio.
   ========================================================================== */

const FAQ = [
  {
    p: 'Como funciona o sistema de XP e níveis?',
    r: 'Cada material concluído vale 50 XP, cada prova aprovada vale 100 XP (mais 50 de bônus com nota máxima) e cada medalha vale 75 XP. A cada 350 XP você sobe um nível.'
  },
  {
    p: 'Posso refazer uma prova em que fui reprovado?',
    r: 'Sim. Cada tentativa fica registrada com o número dela, e você pode revisar o conteúdo do curso e tentar de novo. O histórico completo está em Perfil > Minhas provas.'
  },
  {
    p: 'Como recebo meu certificado?',
    r: 'O certificado é emitido automaticamente quando você conclui todos os materiais do curso e é aprovado na prova. Ele aparece em Meus certificados, sem precisar pedir.'
  },
  {
    p: 'Como alguém confere se meu certificado é verdadeiro?',
    r: 'Cada certificado tem um código de validação único. Qualquer pessoa pode consultar esse código na plataforma, sem precisar de conta, e ver nome, curso e datas.'
  },
  {
    p: 'Como falo com o tutor do curso?',
    r: 'Abra um chamado do tipo "Dúvida sobre o conteúdo" e escolha o curso: ele vai direto para o tutor responsável. A resposta aparece em Notificações.'
  },
  {
    p: 'Posso usar o app e o site com a mesma conta?',
    r: 'Sim. App e site usam a mesma conta e os mesmos dados: o que você concluir em um aparece no outro.'
  }
];

export default function Ajuda() {
  const [aberta, setAberta] = useState<number | null>(null);
  const [busca, setBusca] = useState('');
  const termo = normalizar(busca);
  const perguntas = FAQ.map((f, i) => ({ ...f, i })).filter(f => !termo || normalizar(f.p + ' ' + f.r).includes(termo));

  const alternar = (i: number) => {
    LayoutAnimation.configureNext(LayoutAnimation.Presets.easeInEaseOut);
    setAberta(a => (a === i ? null : i));
  };

  return (
    <Tela>
      <Cabecalho titulo="Central de Ajuda" subtitulo="Encontre respostas para as dúvidas mais comuns ou fale com o suporte." />
      <Campo icone="search" placeholder="Digite sua dúvida..." value={busca} onChangeText={setBusca} />

      <Grade colunas={2}>
        <Atalho icone="user-bolt" tom="alerta" titulo="Dúvida sobre o conteúdo" texto="Vai direto para o tutor do curso"
          onPress={() => router.push({ pathname: '/aluno/suporte', params: { tipo: 'Duvida' } })} />
        <Atalho icone="tool" tom="primaria" titulo="Problema na plataforma" texto="Vai para a equipe de suporte"
          onPress={() => router.push({ pathname: '/aluno/suporte', params: { tipo: 'Tecnico' } })} />
        <Atalho icone="bell" tom="sucesso" titulo="Notificações" texto="Respostas do tutor e do suporte"
          onPress={() => router.push('/aluno/notificacoes')} />
        <Atalho icone="history" tom="perigo" titulo="Meus chamados" texto="Acompanhe o que você já abriu"
          onPress={() => router.push('/aluno/suporte')} />
      </Grade>

      <Secao titulo="Perguntas frequentes" />
      {perguntas.length === 0 && <Texto v="sm" cor={cores.textoApagado}>Nenhuma pergunta encontrada para essa busca.</Texto>}
      {perguntas.map(f => (
        <View key={f.i} style={st.faq}>
          <Pressable onPress={() => alternar(f.i)} style={st.pergunta} accessibilityRole="button"
            accessibilityState={{ expanded: aberta === f.i }}>
            <Text style={st.perguntaTexto}>{f.p}</Text>
            <View style={{ transform: [{ rotate: aberta === f.i ? '180deg' : '0deg' }] }}>
              <Icone nome="chevron-down" tamanho={18} />
            </View>
          </Pressable>
          {aberta === f.i && <Texto v="sm" cor={cores.textoSecundario} style={st.resposta}>{f.r}</Texto>}
        </View>
      ))}

      <LinearGradient colors={[cores.primaria, cores.primariaEscura]} style={st.cta}>
        <Icone nome="headset" tamanho={36} cor="#FFFFFF" />
        <Text style={st.ctaTitulo}>Não encontrou o que procurava?</Text>
        <Text style={st.ctaTexto}>A dúvida vai direto ao tutor do curso; o problema técnico, para a fila dos administradores.</Text>
        <Botao texto="Abrir chamado" icone="message-2" variante="secundario" onPress={() => router.push('/aluno/suporte')} />
      </LinearGradient>
    </Tela>
  );
}

function Atalho({ icone, tom, titulo, texto, onPress }:
  { icone: NomeIcone; tom: Tom; titulo: string; texto: string; onPress: () => void }) {
  return (
    <Cartao onPress={onPress} style={{ marginBottom: 0, flex: 1 }} acessibilidade={titulo}>
      <CaixaIcone nome={icone} tom={tom} />
      <Text style={st.atalhoTitulo}>{titulo}</Text>
      <Texto v="xs">{texto}</Texto>
    </Cartao>
  );
}

const st = StyleSheet.create({
  atalhoTitulo: { fontFamily: fonte.semi, fontSize: 14, color: cores.texto, marginTop: espaco.md, marginBottom: 4 },
  faq: {
    backgroundColor: cores.superficie, borderWidth: 1, borderColor: cores.borda, borderRadius: raio.md,
    marginBottom: espaco.sm, overflow: 'hidden'
  },
  pergunta: { flexDirection: 'row', alignItems: 'center', gap: espaco.md, paddingVertical: espaco.md, paddingHorizontal: espaco.lg },
  perguntaTexto: { flex: 1, fontFamily: fonte.media, fontSize: 14, color: cores.texto },
  resposta: { paddingHorizontal: espaco.lg, paddingBottom: espaco.md, lineHeight: 21 },
  cta: { borderRadius: raio.lg, padding: espaco.xl, alignItems: 'center', marginTop: espaco.xl, gap: espaco.sm },
  ctaTitulo: { fontFamily: fonte.negrito, fontSize: 18, color: '#FFFFFF', textAlign: 'center' },
  ctaTexto: { fontFamily: fonte.regular, fontSize: 13, color: 'rgba(255,255,255,0.9)', textAlign: 'center', marginBottom: espaco.sm }
});
