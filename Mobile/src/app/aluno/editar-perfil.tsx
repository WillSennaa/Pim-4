import { useState } from 'react';
import { router } from 'expo-router';

import { API } from '@/api/endpoints';
import { mensagemDe } from '@/api/cliente';
import type { Perfil } from '@/api/tipos';
import { useCarregar } from '@/hooks/useCarregar';
import { espaco } from '@/tema/tokens';
import { Cartao } from '@/ui/base';
import { useAvisos } from '@/ui/Avisos';
import { Botao } from '@/ui/Botao';
import { Campo } from '@/ui/Campo';
import { Carregando, ErroCarga, Faixa } from '@/ui/Estados';
import { Tela } from '@/ui/Tela';
import { FMT } from '@/util/formatacao';

/* ==========================================================================
   Editar perfil — pages/estudante/editar-perfil.html + salvarPerfilEstudante()
   --------------------------------------------------------------------------
   Nome e e-mail aparecem travados: o PUT /api/perfil só aceita telefone,
   nascimento e cidade (AtualizarPerfilRequest). E-mail tem tela própria
   porque exige a senha. A data é digitada no formato brasileiro e convertida
   para o ISO que o DateOnly do C# aceita.
   ========================================================================== */

/** Máscara simples dd/mm/aaaa enquanto digita. */
function mascaraData(t: string) {
  const d = t.replace(/\D/g, '').slice(0, 8);
  if (d.length <= 2) return d;
  if (d.length <= 4) return `${d.slice(0, 2)}/${d.slice(2)}`;
  return `${d.slice(0, 2)}/${d.slice(2, 4)}/${d.slice(4)}`;
}

export default function EditarPerfil() {
  const { dados: p, erro, carregando, recarregar } = useCarregar(() => API.perfil());

  if (carregando) return <Tela largura="formulario"><Carregando /></Tela>;
  if (erro || !p) return <Tela largura="formulario"><ErroCarga mensagem={erro ?? 'Erro.'} aoTentar={recarregar} /></Tela>;
  // O formulário só monta com o perfil em mãos: o estado inicial vem dele.
  return <FormularioPerfil perfil={p} />;
}

function FormularioPerfil({ perfil: p }: { perfil: Perfil }) {
  const { avisar } = useAvisos();
  const [telefone, setTelefone] = useState(p.telefone || '');
  const [nascimento, setNascimento] = useState(p.dataNascimento ? FMT.data(p.dataNascimento) : '');
  const [cidade, setCidade] = useState(p.cidade || '');
  const [erroData, setErroData] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);

  async function salvar() {
    const iso = FMT.dataBrParaIso(nascimento);
    if (iso === 'invalida') { setErroData('Data inválida. Use dd/mm/aaaa.'); return; }
    setErroData(null);
    setSalvando(true);
    try {
      await API.salvarPerfil({ telefone: telefone.trim() || null, dataNascimento: iso, cidade: cidade.trim() || null });
      avisar('Perfil atualizado.');
      router.back();
    } catch (e) {
      avisar(mensagemDe(e, 'Não foi possível salvar o perfil.'), 'erro');
    } finally {
      setSalvando(false);
    }
  }

  return (
    <Tela largura="formulario">
      <Cartao style={{ padding: espaco.xl }}>
        <Campo rotulo="Nome" icone="user" value={p.nome} editable={false} />
        <Campo rotulo="E-mail" icone="mail" value={p.email ?? ''} editable={false}
          ajuda="Para trocar o e-mail, use Perfil > Alterar e-mail." />
        <Campo rotulo="Telefone" icone="phone" value={telefone} onChangeText={setTelefone} keyboardType="phone-pad"
          placeholder="(11) 90000-0000" autoComplete="tel" />
        <Campo rotulo="Data de nascimento" icone="calendar" value={nascimento}
          onChangeText={t => setNascimento(mascaraData(t))} keyboardType="number-pad" placeholder="dd/mm/aaaa" erro={erroData} />
        <Campo rotulo="Cidade" icone="map-pin" value={cidade} onChangeText={setCidade} placeholder="Ex.: São Paulo" />
        <Faixa tom="primaria" texto="Telefone, nascimento e cidade são opcionais." />
        <Botao texto="Salvar alterações" icone="device-floppy" cheio tamanho="lg" carregando={salvando}
          textoCarregando="Salvando..." onPress={salvar} />
      </Cartao>
    </Tela>
  );
}
