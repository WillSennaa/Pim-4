import { useCallback, useEffect, useRef, useState } from 'react';
import { useFocusEffect } from 'expo-router';

import { mensagemDe } from '@/api/cliente';

/* ==========================================================================
   useCarregar — busca de dados de uma tela
   --------------------------------------------------------------------------
   Concentra o que toda tela repetia: estado de carregando, erro, recarregar
   e "puxar para atualizar". Também RECARREGA QUANDO A TELA VOLTA AO FOCO.

   Por que o foco importa no app e não importava no web: no web, voltar de
   aula.html para curso.html recarrega a página inteira. Na pilha de telas do
   app, a tela do curso continua montada embaixo da aula; sem recarregar no
   foco, o aluno concluiria a aula e voltaria para um curso mostrando o
   progresso antigo.

   O recarregamento no foco é silencioso (sem indicador de tela cheia): o
   conteúdo antigo fica visível até o novo chegar.

   "deps" funciona como no useEffect: quando um parâmetro da rota muda (outro
   curso, outra aula), a busca roda de novo.
   ========================================================================== */

type Modo = 'inicial' | 'recarregar' | 'puxar' | 'silencioso';

interface Estado<T> { dados: T | null; erro: string | null; carregando: boolean; atualizando: boolean; }

export function useCarregar<T>(buscar: () => Promise<T>, deps: unknown[] = []) {
  const [estado, setEstado] = useState<Estado<T>>({ dados: null, erro: null, carregando: true, atualizando: false });

  // A função de busca muda a cada render (é uma arrow function da tela). A
  // referência guarda sempre a mais recente, sem obrigar a tela a memorizá-la.
  const buscarRef = useRef(buscar);
  useEffect(() => { buscarRef.current = buscar; });

  const montado = useRef(true);
  useEffect(() => { montado.current = true; return () => { montado.current = false; }; }, []);

  // Contador de requisições: se o usuário trocar de tela ou puxar duas vezes,
  // só a resposta da ÚLTIMA busca é aplicada.
  const sequencia = useRef(0);

  const executar = useCallback(async (modo: Modo) => {
    const minha = ++sequencia.current;
    if (modo === 'recarregar') setEstado(e => ({ ...e, carregando: true, erro: null }));
    if (modo === 'puxar') setEstado(e => ({ ...e, atualizando: true }));
    try {
      const dados = await buscarRef.current();
      if (!montado.current || minha !== sequencia.current) return;
      setEstado({ dados, erro: null, carregando: false, atualizando: false });
    } catch (e) {
      if (!montado.current || minha !== sequencia.current) return;
      // Falha no recarregamento silencioso não apaga o que já estava na tela.
      setEstado(atual => modo === 'silencioso'
        ? { ...atual, carregando: false, atualizando: false }
        : { ...atual, erro: mensagemDe(e, 'Não foi possível carregar.'), carregando: false, atualizando: false });
    }
  }, []);

  const chave = JSON.stringify(deps);
  useEffect(() => {
    // Busca inicial (e nova busca quando os parâmetros mudam). O estado de
    // "carregando" já nasce verdadeiro, então nada é alterado antes da espera.
    void executar('inicial');
  }, [executar, chave]);

  const primeiraVez = useRef(true);
  useFocusEffect(useCallback(() => {
    if (primeiraVez.current) { primeiraVez.current = false; return; }
    void executar('silencioso');
  }, [executar]));

  return {
    dados: estado.dados,
    erro: estado.erro,
    carregando: estado.carregando && estado.dados === null,
    atualizando: estado.atualizando,
    recarregar: () => executar('recarregar'),
    puxar: () => executar('puxar')
  };
}

/** Executa uma ação de botão marcando "ocupado" — o comBotaoOcupado() do web. */
export function useAcao() {
  const [ocupado, setOcupado] = useState<string | null>(null);
  const rodar = useCallback(async (chave: string, acao: () => Promise<void>) => {
    if (ocupado) return;          // evita toque duplo enquanto a requisição não volta
    setOcupado(chave);
    try { await acao(); } finally { setOcupado(null); }
  }, [ocupado]);
  return { ocupado, rodar };
}
