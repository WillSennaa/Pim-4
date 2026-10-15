// Cada peso importado pelo próprio arquivo: importar do índice do pacote
// colocaria no APK todos os 18 pesos de cada família, e o app usa 5.
import { Inter_400Regular } from '@expo-google-fonts/inter/400Regular';
import { Inter_500Medium } from '@expo-google-fonts/inter/500Medium';
import { Inter_600SemiBold } from '@expo-google-fonts/inter/600SemiBold';
import { Inter_700Bold } from '@expo-google-fonts/inter/700Bold';
import { JetBrainsMono_400Regular } from '@expo-google-fonts/jetbrains-mono/400Regular';
import { useFonts } from 'expo-font';
import { SplashScreen, Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { SafeAreaProvider } from 'react-native-safe-area-context';

import { SessaoProvider, useSessao } from '@/sessao/SessaoContext';
import { cores, fonte } from '@/tema/tokens';
import { AvisosProvider } from '@/ui/Avisos';

/* ==========================================================================
   Layout raiz — quem pode ver o quê
   --------------------------------------------------------------------------
   As rotas do app são pastas (Expo Router, roteamento por arquivo, o mesmo
   conceito das pastas pages/estudante e pages/tutor do web). O que decide
   quais pastas EXISTEM para o usuário é o papel que veio no login:

     sem sessão  -> login, cadastro
     estudante   -> /aluno/...
     tutor       -> /tutor/...
     qualquer um logado -> /conta/... (senha, e-mail, encerrar conta)

   Administrador não tem pasta: o perfil Admin é atendido pela versão
   desktop, conforme a divisão de acessos do projeto (Web geral, Desktop
   admin, Mobile aluno e tutor). A tela de login recusa a entrada dele com
   essa explicação, em vez de abrir um app vazio.

   Stack.Protected (Expo Router) remove do histórico as telas de um grupo
   quando o guarda vira falso. Ao sair da conta, nenhuma tela do aluno fica
   acessível pelo botão voltar do Android.
   ========================================================================== */

void SplashScreen.preventAutoHideAsync();

export default function Raiz() {
  const [fontesProntas] = useFonts({
    Inter_400Regular, Inter_500Medium, Inter_600SemiBold, Inter_700Bold, JetBrainsMono_400Regular
  });

  return (
    <SafeAreaProvider>
      <SessaoProvider>
        <AvisosProvider>
          <StatusBar style="dark" />
          <Navegacao fontesProntas={fontesProntas} />
        </AvisosProvider>
      </SessaoProvider>
    </SafeAreaProvider>
  );
}

function Navegacao({ fontesProntas }: { fontesProntas: boolean }) {
  const { carregando, usuario, papel } = useSessao();
  const pronto = !carregando && fontesProntas;

  // A splash continua na tela até a sessão gravada ser lida e as fontes
  // carregarem: evita piscar a tela de login para quem já está logado.
  useEffect(() => { if (pronto) SplashScreen.hide(); }, [pronto]);
  if (!pronto) return null;

  return (
    <Stack
      screenOptions={{
        headerStyle: { backgroundColor: cores.superficie },
        headerTintColor: cores.primaria,
        headerTitleStyle: { fontFamily: fonte.semi, fontSize: 16, color: cores.texto },
        headerTitleAlign: 'center',
        contentStyle: { backgroundColor: cores.fundo },
        headerBackButtonDisplayMode: 'minimal'
      }}
    >
      <Stack.Screen name="index" options={{ headerShown: false }} />

      <Stack.Protected guard={!usuario}>
        <Stack.Screen name="login" options={{ headerShown: false }} />
        <Stack.Screen name="cadastro" options={{ title: 'Criar conta' }} />
      </Stack.Protected>

      <Stack.Protected guard={papel === 'estudante'}>
        <Stack.Screen name="aluno" options={{ headerShown: false }} />
      </Stack.Protected>

      <Stack.Protected guard={papel === 'tutor'}>
        <Stack.Screen name="tutor" options={{ headerShown: false }} />
      </Stack.Protected>

      <Stack.Protected guard={!!usuario}>
        <Stack.Screen name="conta/senha" options={{ title: 'Alterar senha' }} />
        <Stack.Screen name="conta/email" options={{ title: 'Alterar e-mail' }} />
        <Stack.Screen name="conta/encerrar" options={{ title: 'Encerrar conta' }} />
      </Stack.Protected>
    </Stack>
  );
}
