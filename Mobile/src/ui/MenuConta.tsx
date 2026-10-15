import Constants from 'expo-constants';
import { router } from 'expo-router';

import { useSessao } from '@/sessao/SessaoContext';
import { cores, espaco } from '@/tema/tokens';
import { Cartao, ItemLista, Texto } from './base';
import { useAvisos } from './Avisos';
import { Secao } from './Tela';

/* ==========================================================================
   Bloco "Conta" — comum a estudante e tutor
   --------------------------------------------------------------------------
   As três operações sensíveis (senha, e-mail, encerrar) vivem em /conta e
   servem aos dois papéis, porque /api/perfil responde para qualquer papel.
   É a mesma decisão do web com pages/conta/perfil.html: duplicar a tela por
   papel significaria corrigir cada ajuste duas vezes.
   ========================================================================== */

export function MenuConta() {
  const { sair } = useSessao();
  const { confirmar } = useAvisos();

  async function sairDaConta() {
    const ok = await confirmar({
      titulo: 'Sair da conta?', texto: 'Você precisará entrar novamente com e-mail e senha.',
      confirmar: 'Sair', icone: 'logout'
    });
    if (ok) await sair();
  }

  return (
    <>
      <Secao titulo="Conta e segurança" />
      <Cartao style={{ paddingVertical: 4 }}>
        <ItemLista icone="key" titulo="Alterar senha" subtitulo="Exige a senha atual" onPress={() => router.push('/conta/senha')} />
        <ItemLista icone="at" titulo="Alterar e-mail" subtitulo="O e-mail é o seu login" onPress={() => router.push('/conta/email')} />
        <ItemLista icone="user-off" tomIcone="perigo" titulo="Encerrar conta" subtitulo="Desativa o acesso e preserva certificados"
          onPress={() => router.push('/conta/encerrar')} />
        <ItemLista icone="logout" tomIcone="neutro" titulo="Sair" ultimo onPress={sairDaConta} direita={null} />
      </Cartao>
      <Texto v="xs" cor={cores.textoApagado} centro style={{ marginTop: espaco.sm }}>
        Tech Quest Mobile {Constants.expoConfig?.version ?? ''} · PIM IV · UNIP
      </Texto>
    </>
  );
}
