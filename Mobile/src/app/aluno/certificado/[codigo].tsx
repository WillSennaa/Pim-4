import { Share, StyleSheet, Text, View } from 'react-native';
import * as Clipboard from 'expo-clipboard';
import { useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';

import { API } from '@/api/endpoints';
import { useCarregar } from '@/hooks/useCarregar';
import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { Icone, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Carregando, ErroCarga, Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Certificado — pages/estudante/certificado-detalhe.html
   --------------------------------------------------------------------------
   Buscado pelo CÓDIGO DE AUTENTICAÇÃO no endpoint público de validação
   (GET /api/certificados/{codigo}) — o mesmo que um empregador usaria. Se o
   app consegue exibir, qualquer terceiro consegue validar.

   No celular, "imprimir" vira COMPARTILHAR: o aluno manda o código pelo
   WhatsApp ou e-mail para quem precisa conferir.
   ========================================================================== */

export default function CertificadoDetalhe() {
  const { codigo } = useLocalSearchParams<{ codigo: string }>();
  const { dados: c, erro, carregando, recarregar } = useCarregar(() => API.validarCertificado(codigo), [codigo]);
  const { avisar } = useAvisos();

  if (carregando) return <Tela largura="leitura"><Carregando /></Tela>;
  if (erro || !c) return <Tela largura="leitura"><ErroCarga mensagem={erro ?? 'Certificado não encontrado.'} aoTentar={recarregar} /></Tela>;

  async function copiar() {
    await Clipboard.setStringAsync(c!.codigoAutenticacao);
    avisar('Código de validação copiado.');
  }

  function compartilhar() {
    void Share.share({
      message: `Certificado Tech Quest — ${c!.estudante} concluiu "${c!.curso}" (${c!.cargaHoraria || 0}h) em ${FMT.data(c!.dataEmissao)}.\nCódigo de validação: ${c!.codigoAutenticacao}`
    });
  }

  return (
    <Tela largura="leitura">
      <LinearGradient colors={[cores.ouroFundo1, cores.ouroFundo2]} style={st.moldura}>
        <View style={st.molduraInterna} pointerEvents="none" />
        <View style={st.selo}><Icone nome="award" tamanho={30} cor="#FFFFFF" /></View>
        <Text style={st.sobretitulo}>TECH QUEST · PLATAFORMA DE APRENDIZAGEM</Text>
        <Text style={st.titulo}>Certificado de Conclusão</Text>
        <Text style={st.texto}>Certificamos que</Text>
        <Text style={st.nome}>{c.estudante}</Text>
        <Text style={st.texto}>concluiu com aproveitamento o curso de</Text>
        <Text style={st.curso}>{c.curso}</Text>
        <Text style={st.texto}>com carga horária total de <Text style={{ fontFamily: fonte.semi }}>{c.cargaHoraria || 0} horas</Text>.</Text>
        <View style={st.meta}>
          <Info valor={FMT.data(c.dataEmissao)} rotulo="Data de emissão" />
          <Info valor={c.instrutor || 'tutor não informado'} rotulo="Tutor responsável" />
        </View>
        <Text style={st.codigoRotulo}>Código de validação</Text>
        <Text style={st.codigo} selectable>{c.codigoAutenticacao}</Text>
      </LinearGradient>

      <Faixa tom="sucesso" icone="shield-check"
        texto="Certificado válido. Qualquer pessoa pode conferir este código na plataforma, sem precisar de conta." />

      <View style={{ gap: espaco.sm }}>
        <Botao texto="Compartilhar" icone="send" cheio onPress={compartilhar} />
        <Botao texto="Copiar código" icone="copy" variante="secundario" cheio onPress={copiar} />
      </View>
      <Texto v="xs" cor={cores.textoApagado} centro style={{ marginTop: espaco.md }}>
        Concluído em {FMT.data(c.dataConclusao) || FMT.data(c.dataEmissao)}
      </Texto>
    </Tela>
  );
}

function Info({ valor, rotulo }: { valor: string; rotulo: string }) {
  return (
    <View style={{ alignItems: 'center', flex: 1 }}>
      <Text style={st.metaValor}>{valor}</Text>
      <Text style={st.metaRotulo}>{rotulo}</Text>
    </View>
  );
}

const ouro = cores.ouroBorda;
const st = StyleSheet.create({
  moldura: {
    borderWidth: 4, borderColor: ouro, borderRadius: raio.lg, paddingVertical: espaco.xxl, paddingHorizontal: espaco.xl,
    alignItems: 'center', marginBottom: espaco.lg
  },
  molduraInterna: {
    position: 'absolute', top: 8, left: 8, right: 8, bottom: 8, borderWidth: 1, borderColor: ouro,
    borderRadius: raio.md, opacity: 0.4
  },
  selo: {
    width: 64, height: 64, borderRadius: raio.total, backgroundColor: cores.alerta, borderWidth: 3, borderColor: cores.ouroFundo2,
    alignItems: 'center', justifyContent: 'center', marginBottom: espaco.md
  },
  sobretitulo: { fontFamily: fonte.semi, fontSize: 10, letterSpacing: 2.5, color: ouro, textAlign: 'center', marginBottom: espaco.md },
  titulo: { fontFamily: fonte.negrito, fontSize: 26, color: '#1E293B', textAlign: 'center', marginBottom: espaco.md },
  texto: { fontFamily: fonte.regular, fontSize: 14, color: cores.textoSecundario, textAlign: 'center', lineHeight: 22 },
  nome: {
    fontFamily: 'serif', fontStyle: 'italic', fontWeight: '700', fontSize: 26, color: cores.primariaEscura, textAlign: 'center',
    marginVertical: espaco.md, paddingBottom: espaco.md, borderBottomWidth: 2, borderStyle: 'dashed', borderBottomColor: ouro, alignSelf: 'stretch'
  },
  curso: { fontFamily: fonte.semi, fontSize: 18, color: '#1E293B', textAlign: 'center', marginVertical: espaco.md },
  meta: {
    flexDirection: 'row', alignSelf: 'stretch', gap: espaco.md, marginTop: espaco.xl, paddingTop: espaco.lg,
    borderTopWidth: 1, borderTopColor: 'rgba(180, 83, 9, 0.3)'
  },
  metaValor: { fontFamily: fonte.semi, fontSize: 13, color: '#1E293B', textAlign: 'center' },
  metaRotulo: { fontFamily: fonte.regular, fontSize: 11, color: cores.textoSecundario, textAlign: 'center', marginTop: 2 },
  codigoRotulo: { fontFamily: fonte.regular, fontSize: 11, color: cores.textoSecundario, marginTop: espaco.lg },
  codigo: { fontFamily: fonte.mono, fontSize: 11, color: '#1E293B', textAlign: 'center', marginTop: 2 }
});
