import { useState } from 'react';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import { useSessao } from '@/sessao/SessaoContext';
import { cores, espaco } from '@/tema/tokens';
import { Cartao, Texto } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';

/* ==========================================================================
   Encerrar conta — pages/estudante/privacidade.html + excluirConta()
   --------------------------------------------------------------------------
   A conta é DESATIVADA, não apagada. Histórico, desempenho e certificados
   apontam para o usuário, e certificados já emitidos continuam validáveis
   por terceiros pelo código público. A tela diz isso ao usuário em vez de
   prometer uma exclusão que o sistema não faz.

   Duas barreiras: a senha (exigida pelo servidor) e uma confirmação na tela.
   ========================================================================== */

export default function EncerrarConta() {
  const { sair, papel } = useSessao();
  const { avisar, confirmar } = useAvisos();
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function encerrar() {
    setErro(null);
    if (!senha) return setErro('Informe sua senha para confirmar.');
    const ok = await confirmar({
      titulo: 'Encerrar sua conta?',
      texto: 'Você perderá o acesso ao Tech Quest no app e no site. Esta ação só pode ser desfeita pela administração.',
      confirmar: 'Encerrar conta', perigo: true
    });
    if (!ok) return;

    setEnviando(true);
    try {
      await API.desativarConta(senha);
      avisar('Conta encerrada. Você foi desconectado.');
      await sair();
    } catch (e) {
      setErro(mensagemDe(e, 'Não foi possível encerrar a conta.'));
      setEnviando(false);
    }
  }

  return (
    <Tela largura="formulario">
      <Cartao destaqueTopo="perigo" style={{ padding: espaco.xl }}>
        <Texto v="secao">O que acontece ao encerrar</Texto>
        <Texto v="sm" cor={cores.textoSecundario} style={{ marginTop: espaco.sm, marginBottom: espaco.lg }}>
          {papel === 'tutor'
            ? 'Seu acesso é desativado. Os cursos que você criou e o histórico dos alunos continuam no sistema.'
            : 'Seu acesso é desativado. Seu histórico e seus certificados são preservados: certificados emitidos continuam válidos para quem conferir o código.'}
        </Texto>
        <Faixa tom="alerta" icone="alert-triangle" texto="Para voltar a usar a conta depois, será preciso falar com a administração." />
        <Campo rotulo="Confirme sua senha" icone="lock" senha value={senha} onChangeText={setSenha} autoCapitalize="none" />
        {!!erro && <Faixa tom="perigo" icone="alert-triangle" texto={erro} />}
        <Botao texto="Encerrar minha conta" icone="user-off" variante="perigo" cheio tamanho="lg"
          carregando={enviando} textoCarregando="Encerrando..." onPress={encerrar} />
      </Cartao>
    </Tela>
  );
}
