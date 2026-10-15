import { useState } from 'react';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { useSessao } from '@/sessao/SessaoContext';
import { cores, espaco } from '@/tema/tokens';
import { Cartao, Texto } from '@/ui/base';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';

/* ==========================================================================
   Cadastro — pages/cadastro.html
   --------------------------------------------------------------------------
   Cria SEMPRE uma conta de Estudante. O papel não é enviado pelo app: o
   RegistrarRequest do servidor nem tem esse campo, de propósito — se o papel
   viesse da requisição, qualquer um se cadastraria como Admin. Tutor é
   criado pelo administrador, no desktop.

   As validações daqui (tamanho da senha, confirmação) são de conforto: a
   regra que vale é a do servidor, e a mensagem dele chega à tela.
   ========================================================================== */

export default function Cadastro() {
  const { entrar } = useSessao();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [confirmar, setConfirmar] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function cadastrar() {
    setErro(null);
    if (!nome.trim()) return setErro('Informe seu nome.');
    if (!/^\S+@\S+\.\S+$/.test(email.trim())) return setErro('Informe um e-mail válido.');
    if (senha.length < 6) return setErro('A senha deve ter ao menos 6 caracteres.');
    if (senha !== confirmar) return setErro('A confirmação não confere com a senha.');

    setEnviando(true);
    try {
      const r = await API.registrar(nome.trim(), email.trim(), senha);
      await entrar(r);
      router.replace('/');
    } catch (e) {
      setErro(mensagemDe(e, 'Não foi possível criar a conta.'));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <Tela largura="formulario">
      <Cartao destaqueTopo="primaria" style={{ padding: espaco.xl }}>
        <Texto v="titulo">Crie sua conta</Texto>
        <Texto v="subtitulo" style={{ marginTop: espaco.xs, marginBottom: espaco.xl }}>
          Comece sua jornada: cursos, provas, XP e certificados.
        </Texto>

        <Campo rotulo="Nome completo" icone="user" value={nome} onChangeText={setNome} autoComplete="name" />
        <Campo rotulo="E-mail" icone="mail" value={email} onChangeText={setEmail} keyboardType="email-address"
          autoCapitalize="none" autoComplete="email" />
        <Campo rotulo="Senha" icone="lock" value={senha} onChangeText={setSenha} senha autoCapitalize="none"
          ajuda="Mínimo de 6 caracteres." autoComplete="new-password" />
        <Campo rotulo="Confirmar senha" icone="lock" value={confirmar} onChangeText={setConfirmar} senha
          autoCapitalize="none" returnKeyType="go" onSubmitEditing={cadastrar} />

        {!!erro && <Faixa texto={erro} tom="perigo" icone="alert-triangle" />}

        <Botao texto="Criar conta" tamanho="lg" cheio carregando={enviando} textoCarregando="Criando conta..." onPress={cadastrar} />
        <Texto v="xs" cor={cores.textoApagado} centro style={{ marginTop: espaco.md }}>
          Contas criadas aqui são de estudante. Tutores são cadastrados pela administração.
        </Texto>
      </Cartao>
    </Tela>
  );
}
