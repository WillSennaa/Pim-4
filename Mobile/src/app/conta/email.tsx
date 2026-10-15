import { useState } from 'react';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { useSessao } from '@/sessao/SessaoContext';
import { espaco } from '@/tema/tokens';
import { Cartao, LinhaInfo, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';

/* ==========================================================================
   Alterar e-mail — pages/estudante/alterar-email.html + trocarEmail()
   --------------------------------------------------------------------------
   O token continua válido depois da troca: ele carrega o ID do usuário, não
   o e-mail. O que precisa ser atualizado é a cópia do usuário guardada no
   aparelho, senão o app mostraria o e-mail antigo até o próximo login.
   ========================================================================== */

export default function AlterarEmail() {
  const { usuario, atualizarUsuario } = useSessao();
  const { avisar } = useAvisos();
  const [senha, setSenha] = useState('');
  const [novo, setNovo] = useState('');
  const [confirmar, setConfirmar] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);

  async function salvar() {
    setErro(null);
    if (!senha) return setErro('Informe sua senha.');
    if (!/^\S+@\S+\.\S+$/.test(novo.trim())) return setErro('Informe um e-mail válido.');
    if (novo.trim().toLowerCase() !== confirmar.trim().toLowerCase()) return setErro('Os dois e-mails não conferem.');

    setSalvando(true);
    try {
      const r = await API.trocarEmail(senha, novo.trim());
      await atualizarUsuario({ email: r.email });
      avisar(`E-mail alterado para ${r.email}.`);
      router.back();
    } catch (e) {
      setErro(mensagemDe(e, 'Não foi possível alterar o e-mail.'));
    } finally {
      setSalvando(false);
    }
  }

  return (
    <Tela largura="formulario">
      <Cartao style={{ padding: espaco.xl }}>
        <LinhaInfo rotulo="E-mail atual" valor={usuario?.email || '-'} ultimo />
        <Texto v="subtitulo" style={{ marginVertical: espaco.lg }}>
          O e-mail é o seu login. Por isso a troca exige a sua senha.
        </Texto>
        <Campo rotulo="Novo e-mail" icone="mail" value={novo} onChangeText={setNovo} keyboardType="email-address" autoCapitalize="none" />
        <Campo rotulo="Confirmar novo e-mail" icone="mail" value={confirmar} onChangeText={setConfirmar}
          keyboardType="email-address" autoCapitalize="none" />
        <Campo rotulo="Senha" icone="lock" senha value={senha} onChangeText={setSenha} autoCapitalize="none"
          returnKeyType="go" onSubmitEditing={salvar} />
        {!!erro && <Faixa tom="perigo" icone="alert-triangle" texto={erro} />}
        <Botao texto="Alterar e-mail" icone="device-floppy" cheio tamanho="lg" carregando={salvando}
          textoCarregando="Salvando..." onPress={salvar} />
      </Cartao>
    </Tela>
  );
}
