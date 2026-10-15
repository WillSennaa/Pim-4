import { Stack } from 'expo-router';

import { opcoesCabecalho } from '@/ui/Navegacao';

/* ==========================================================================
   Área do tutor — equivalente a pages/tutor/
   --------------------------------------------------------------------------
   As abas são as do .bottom-nav do tutor no web (Painel, Alunos, Dúvidas,
   Cursos), mais a aba Conta. As telas de edição empilham por cima.

   O editor de curso do web era UMA página longa com todos os formulários
   (dados, cada material, prova, cada questão). No celular isso vira rolagem
   sem fim e teclado cobrindo campo. Aqui o editor mostra o RESUMO e cada
   item abre a própria tela: material e questão têm editor dedicado.
   ========================================================================== */

export default function LayoutTutor() {
  return (
    <Stack screenOptions={opcoesCabecalho}>
      <Stack.Screen name="(abas)" options={{ headerShown: false }} />
      <Stack.Screen name="aluno" options={{ title: 'Aluno' }} />
      <Stack.Screen name="curso/novo" options={{ title: 'Novo curso' }} />
      <Stack.Screen name="curso/[id]" options={{ title: 'Editor de curso' }} />
      <Stack.Screen name="material" options={{ title: 'Material' }} />
      <Stack.Screen name="prova" options={{ title: 'Prova do curso' }} />
      <Stack.Screen name="questao" options={{ title: 'Nova questão' }} />
      <Stack.Screen name="notificacoes" options={{ title: 'Notificações' }} />
    </Stack>
  );
}
