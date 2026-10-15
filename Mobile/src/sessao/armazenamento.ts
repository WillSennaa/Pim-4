import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

/* ==========================================================================
   Onde a sessão mora no aparelho
   --------------------------------------------------------------------------
   O web usa sessionStorage (o token morre quando a aba fecha, pensado para o
   computador compartilhado do laboratório). O celular é pessoal, e pedir
   login toda vez que o app é aberto seria atrito sem ganho de segurança —
   então a sessão PERSISTE, mas em local cifrado.

   expo-secure-store grava no Android Keystore (chave guardada em hardware
   quando o aparelho tem) e no Keychain do iOS. O token fica cifrado em
   repouso e não é acessível a outros apps.

   ALTERNATIVA REJEITADA: AsyncStorage. É um arquivo em texto puro na pasta
   do app; em aparelho com root qualquer um lê o JWT. Para um dado que dá
   acesso à conta inteira, o cofre do sistema é o lugar certo.

   No navegador (usado só para conferir o layout durante o desenvolvimento)
   o SecureStore não existe, e a sessão cai para o localStorage.
   ========================================================================== */

const naWeb = Platform.OS === 'web';

export async function ler(chave: string): Promise<string | null> {
  if (naWeb) {
    try { return globalThis.localStorage?.getItem(chave) ?? null; } catch { return null; }
  }
  return SecureStore.getItemAsync(chave);
}

export async function gravar(chave: string, valor: string): Promise<void> {
  if (naWeb) {
    try { globalThis.localStorage?.setItem(chave, valor); } catch { /* armazenamento bloqueado */ }
    return;
  }
  await SecureStore.setItemAsync(chave, valor);
}

export async function apagar(chave: string): Promise<void> {
  if (naWeb) {
    try { globalThis.localStorage?.removeItem(chave); } catch { /* idem */ }
    return;
  }
  await SecureStore.deleteItemAsync(chave);
}
