import { useState } from 'react';
import { router } from 'expo-router';

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
   Alterar senha — pages/estudante/alterar-senha.html + trocarSenha()
   --------------------------------------------------------------------------
   A senha ATUAL é exigida mesmo com o usuário logado: um celular esquecido
   desbloqueado não pode servir para tomar a conta (TrocarSenhaRequest).
   ========================================================================== */

export default function AlterarSenha() {
  const { avisar } = useAvisos();
  const [atual, setAtual] = useState('');
  const [nova, setNova] = useState('');
  const [confirmar, setConfirmar] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);

  async function salvar() {
    setErro(null);
    if (!atual) return setErro('Informe sua senha atual.');
    if (nova.length < 6) return setErro('A nova senha deve ter ao menos 6 caracteres.');
    if (nova !== confirmar) return setErro('A confirmação não confere com a nova senha.');
    if (nova === atual) return setErro('A nova senha deve ser diferente da atual.');

    setSalvando(true);
    try {
      await API.trocarSenha(atual, nova);
      avisar('Senha alterada. Use a nova senha no próximo acesso.');
      router.back();
    } catch (e) {
      setErro(mensagemDe(e, 'Não foi possível alterar a senha.'));
    } finally {
      setSalvando(false);
    }
  }

  return (
    <Tela largura="formulario">
      <Cartao style={{ padding: espaco.xl }}>
        <Texto v="subtitulo" style={{ marginBottom: espaco.lg }}>Por segurança, confirme sua senha atual antes de definir a nova.</Texto>
        <Campo rotulo="Senha atual" icone="lock" senha value={atual} onChangeText={setAtual} autoCapitalize="none" autoComplete="current-password" />
        <Campo rotulo="Nova senha" icone="key" senha value={nova} onChangeText={setNova} autoCapitalize="none"
          ajuda="Mínimo de 6 caracteres." autoComplete="new-password" />
        <Campo rotulo="Confirmar nova senha" icone="key" senha value={confirmar} onChangeText={setConfirmar}
          autoCapitalize="none" returnKeyType="go" onSubmitEditing={salvar} />
        {!!erro && <Faixa tom="perigo" icone="alert-triangle" texto={erro} />}
        <Botao texto="Salvar nova senha" icone="device-floppy" cheio tamanho="lg" carregando={salvando}
          textoCarregando="Salvando..." onPress={salvar} />
      </Cartao>
    </Tela>
  );
}
