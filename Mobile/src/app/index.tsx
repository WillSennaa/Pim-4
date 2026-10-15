import { Redirect } from 'expo-router';

import { useSessao } from '@/sessao/SessaoContext';

/**
 * Rota inicial: não tem tela, só decide para onde ir. É também a "rota
 * âncora" para a qual o Expo Router devolve o usuário quando um guarda muda
 * (login, logout, sessão expirada) — então a decisão fica em um lugar só.
 */
export default function Inicio() {
  const { papel } = useSessao();
  if (papel === 'estudante') return <Redirect href="/aluno" />;
  if (papel === 'tutor') return <Redirect href="/tutor" />;
  return <Redirect href="/login" />;
}
