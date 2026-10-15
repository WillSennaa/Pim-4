import { useRef, useState } from 'react';
import { Pressable, StyleSheet, Text, View, type TextInput } from 'react-native';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { papelDe, useSessao } from '@/sessao/SessaoContext';
import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { Cartao, Icone, Selo, Texto } from '@/ui/base';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';

/* ==========================================================================
   Login — pages/login.html
   --------------------------------------------------------------------------
   Mesmo cartão com borda azul no topo, mesmos campos, mesmo "modo
   demonstração" que preenche as credenciais de teste. A validação continua
   inteiramente no servidor: o app só envia e-mail e senha e recebe o token.

   ADMINISTRADOR É RECUSADO AQUI, DEPOIS da autenticação, com a mensagem de
   onde ele deve entrar. A senha dele está certa — dizer "senha inválida"
   seria mentir. O token recebido é descartado sem ser gravado.
   ========================================================================== */

const DEMO = {
  estudante: { email: 'felipe123@gmail.com', senha: 'aluno123@', rotulo: 'Estudante' },
  tutor: { email: 'tutor123@gmail.com', senha: 'tutor123@', rotulo: 'Tutor' }
} as const;

export default function Login() {
  const { entrar, aviso, limparAviso } = useSessao();
  const insets = useSafeAreaInsets();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);
  const [persona, setPersona] = useState<keyof typeof DEMO | null>(null);
  const campoSenha = useRef<TextInput>(null);

  async function fazerLogin() {
    setErro(null);
    limparAviso();
    if (!email.trim() || !senha) { setErro('Informe e-mail e senha.'); return; }

    setEnviando(true);
    try {
      const r = await API.login(email.trim(), senha);
      if (papelDe(r.usuario.papel) === 'admin') {
        setErro('O perfil Administrador usa a versão desktop do Tech Quest. O aplicativo é exclusivo de estudantes e tutores.');
        return;
      }
      await entrar(r);
      router.replace('/');
    } catch (e) {
      setErro(mensagemDe(e, 'Não foi possível entrar.'));
    } finally {
      setEnviando(false);
    }
  }

  function preencher(p: keyof typeof DEMO) {
    setPersona(p);
    setEmail(DEMO[p].email);
    setSenha(DEMO[p].senha);
    setErro(null);
  }

  return (
    <LinearGradient colors={['#EFF6FF', '#DBEAFE']} style={{ flex: 1, paddingTop: insets.top }}>
      <Tela largura="formulario" estiloConteudo={{ paddingTop: espaco.xl }}>
        <Cartao destaqueTopo="primaria" style={{ padding: espaco.xl }}>
          <View style={{ alignItems: 'center', marginBottom: espaco.xl }}>
            <View style={st.logo}><Icone nome="code" tamanho={28} cor="#FFFFFF" /></View>
            <Text style={st.marca}>Tech Quest</Text>
            <Texto v="titulo">Entrar</Texto>
            <Texto v="sm" cor={cores.textoApagado} style={{ marginTop: espaco.sm }}>
              {persona ? `Acessando como ${DEMO[persona].rotulo}` : 'Área do estudante e do tutor'}
            </Texto>
          </View>

          {!!aviso && <Faixa texto={aviso} tom="alerta" icone="clock" />}

          <Campo
            rotulo="E-mail" icone="mail" placeholder="seu@email.com" value={email} onChangeText={setEmail}
            keyboardType="email-address" autoCapitalize="none" autoComplete="email" textContentType="emailAddress"
            returnKeyType="next" onSubmitEditing={() => campoSenha.current?.focus()} submitBehavior="submit"
          />
          <Campo
            ref={campoSenha} rotulo="Senha" icone="lock" placeholder="••••••••" value={senha} onChangeText={setSenha}
            senha autoCapitalize="none" autoComplete="current-password" textContentType="password"
            returnKeyType="go" onSubmitEditing={fazerLogin}
          />

          {!!erro && <Faixa texto={erro} tom="perigo" icone="alert-triangle" />}

          <Botao texto="Entrar" tamanho="lg" cheio carregando={enviando} textoCarregando="Entrando..." onPress={fazerLogin} />
        </Cartao>

        <View style={st.cadastro}>
          <Texto v="sm" cor={cores.textoSecundario}>Não tem conta?</Texto>
          <Pressable onPress={() => router.push('/cadastro')} style={st.linkCadastro} accessibilityRole="link">
            <Text style={{ fontFamily: fonte.semi, color: cores.primaria, fontSize: 14 }}>Cadastre-se</Text>
          </Pressable>
        </View>

        <Cartao style={st.demo}>
          <View style={{ alignItems: 'center', marginBottom: espaco.md }}>
            <Selo texto="Modo demonstração" icone="info-circle" />
            <Texto v="xs" cor={cores.textoApagado} centro style={{ marginTop: espaco.sm }}>
              Preenche as credenciais de teste. A validação e o perfil de acesso são definidos pelo servidor.
            </Texto>
          </View>
          <View style={{ gap: espaco.sm }}>
            <Botao texto="Logar como Estudante" icone="school" tamanho="sm" cheio
              variante={persona === 'estudante' ? 'primario' : 'secundario'} onPress={() => preencher('estudante')} />
            <Botao texto="Logar como Tutor" icone="user-bolt" tamanho="sm" cheio
              variante={persona === 'tutor' ? 'primario' : 'secundario'} onPress={() => preencher('tutor')} />
          </View>
        </Cartao>
      </Tela>
    </LinearGradient>
  );
}

const st = StyleSheet.create({
  logo: {
    width: 56, height: 56, borderRadius: raio.md, backgroundColor: cores.primaria,
    alignItems: 'center', justifyContent: 'center', marginBottom: espaco.md
  },
  marca: { fontFamily: fonte.negrito, fontSize: 20, color: cores.primaria, marginBottom: espaco.xs },
  cadastro: { flexDirection: 'row', justifyContent: 'center', alignItems: 'center', gap: espaco.sm, marginVertical: espaco.lg },
  linkCadastro: {
    borderWidth: 1.5, borderStyle: 'dashed', borderColor: cores.primaria, borderRadius: 4,
    paddingHorizontal: 8, paddingVertical: 2
  },
  demo: { backgroundColor: cores.superficie2, borderWidth: 1.5, borderStyle: 'dashed', borderColor: cores.primaria }
});
