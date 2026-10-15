import { API_BASE, TEMPO_LIMITE_MS } from '@/config/ambiente';

/* ==========================================================================
   Cliente HTTP — equivalente mobile de Web/js/api.js > API.request
   --------------------------------------------------------------------------
   Camada única de comunicação. Nenhuma tela chama fetch diretamente: a URL,
   o envio do token, o tempo limite e o formato de erro ficam definidos aqui.

   DIFERENÇAS EM RELAÇÃO AO WEB, e por quê:
   - O token não fica em sessionStorage (não existe no React Native). Ele é
     guardado pelo módulo de sessão no SecureStore e entregue a este cliente
     por definirToken(). Assim o cliente não sabe ONDE o token mora, só o usa.
   - No 401 o web redireciona com window.location. Aqui não há URL de página:
     o cliente avisa quem se inscreveu (a sessão), e a sessão limpa o estado;
     os guardas de rota do Expo Router levam o usuário ao login sozinhos.
   - CORS não se aplica: CORS é uma regra do NAVEGADOR. O app nativo não tem
     "origem", então a API do Azure não precisou de nenhuma alteração.
   ========================================================================== */

/** Erro que as telas exibem. Mesmo formato { status, mensagem } do web. */
export class ErroApi extends Error {
  constructor(public status: number, mensagem: string) {
    super(mensagem);
    this.name = 'ErroApi';
  }
}

let tokenAtual: string | null = null;
let aoExpirar: (() => void) | null = null;

export function definirToken(token: string | null) { tokenAtual = token; }
export function aoSessaoExpirar(callback: (() => void) | null) { aoExpirar = callback; }

type Metodo = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

async function request<T>(metodo: Metodo, rota: string, corpo?: unknown): Promise<T> {
  const cabecalhos: Record<string, string> = { Accept: 'application/json' };
  if (corpo !== undefined) cabecalhos['Content-Type'] = 'application/json';
  if (tokenAtual) cabecalhos.Authorization = 'Bearer ' + tokenAtual;

  // fetch do React Native não tem timeout próprio: sem isto, uma rede que cai
  // no meio da requisição deixa a tela carregando indefinidamente.
  const controle = new AbortController();
  const timer = setTimeout(() => controle.abort(), TEMPO_LIMITE_MS);

  let resposta: Response;
  try {
    resposta = await fetch(API_BASE + rota, {
      method: metodo,
      headers: cabecalhos,
      body: corpo !== undefined ? JSON.stringify(corpo) : undefined,
      signal: controle.signal
    });
  } catch (e) {
    const abortado = (e as Error)?.name === 'AbortError';
    throw new ErroApi(0, abortado
      ? 'O servidor demorou para responder. Tente de novo em instantes.'
      : 'Não foi possível falar com o servidor. Verifique sua conexão.');
  } finally {
    clearTimeout(timer);
  }

  // 401 = token ausente, inválido ou expirado. Tratado uma vez aqui, e não
  // em cada tela. Rotas de /api/auth ficam de fora: lá o 401 significa
  // "senha errada", e a mensagem do servidor deve chegar à tela de login.
  if (resposta.status === 401 && !rota.startsWith('/api/auth/')) {
    aoExpirar?.();
    throw new ErroApi(401, 'Sua sessão expirou. Entre novamente.');
  }

  if (resposta.status === 403) {
    const texto403 = await resposta.text();
    let msg = 'Você não tem permissão para esta ação.';
    try { msg = JSON.parse(texto403).mensagem || msg; } catch { /* corpo vazio */ }
    throw new ErroApi(403, msg);
  }

  if (resposta.status === 204) return null as T;

  const texto = await resposta.text();
  let dados: unknown = null;
  if (texto) { try { dados = JSON.parse(texto); } catch { dados = texto; } }

  if (!resposta.ok) {
    const mensagem = (dados as { mensagem?: string } | null)?.mensagem
      || (resposta.status >= 500
        ? 'O servidor encontrou um erro. Tente novamente.'
        : 'Erro ' + resposta.status + '.');
    throw new ErroApi(resposta.status, mensagem);
  }

  return dados as T;
}

export const http = {
  get: <T>(rota: string) => request<T>('GET', rota),
  post: <T>(rota: string, corpo?: unknown) => request<T>('POST', rota, corpo ?? undefined),
  put: <T>(rota: string, corpo?: unknown) => request<T>('PUT', rota, corpo),
  patch: <T>(rota: string, corpo?: unknown) => request<T>('PATCH', rota, corpo),
  del: <T>(rota: string) => request<T>('DELETE', rota)
};

/** Mensagem amigável para qualquer erro capturado em uma tela. */
export function mensagemDe(e: unknown, padrao = 'Algo deu errado.'): string {
  if (e instanceof ErroApi) return e.message;
  if (e instanceof Error && e.message) return e.message;
  return padrao;
}
