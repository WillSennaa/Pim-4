import { createContext, use, useCallback, useEffect, useMemo, useState, type PropsWithChildren } from 'react';

import { aoSessaoExpirar, definirToken } from '@/api/cliente';
import type { LoginResponse, Papel, PapelApi, UsuarioLogado } from '@/api/tipos';
import { apagar, gravar, ler } from './armazenamento';

/* ==========================================================================
   Sessão do usuário
   --------------------------------------------------------------------------
   Um contexto React guarda quem está logado e qual o papel. O layout raiz lê
   o papel e decide quais grupos de telas existem (Stack.Protected): o aluno
   simplesmente não tem a rota /tutor montada, e vice-versa.

   RESPOSTA PRONTA PARA A BANCA: isso é ORGANIZAÇÃO DE INTERFACE, não
   segurança. A segurança está na API — todo endpoint exige JWT e os de tutor
   têm [Authorize(Roles = "Tutor")], além de o serviço conferir se o curso é
   daquele tutor. Um aluno que adulterasse o app para abrir a tela do tutor
   receberia 403 em todas as chamadas.
   ========================================================================== */

const CHAVE_TOKEN = 'tq_token';
const CHAVE_USUARIO = 'tq_usuario';
const CHAVE_EXPIRA = 'tq_expira';

/** Converte o enum numérico do C# para o vocabulário que o front-end já usa. */
export function papelDe(p: PapelApi | undefined | null): Papel | null {
  const mapa: Record<string, Papel> = {
    '1': 'estudante', '2': 'tutor', '3': 'admin',
    Estudante: 'estudante', Tutor: 'tutor', Admin: 'admin'
  };
  return p == null ? null : (mapa[String(p)] ?? null);
}

interface ValorSessao {
  carregando: boolean;
  usuario: UsuarioLogado | null;
  papel: Papel | null;
  /** Motivo do último encerramento (ex.: token expirado), exibido no login. */
  aviso: string | null;
  entrar: (resposta: LoginResponse) => Promise<void>;
  sair: (aviso?: string | null) => Promise<void>;
  atualizarUsuario: (parcial: Partial<UsuarioLogado>) => Promise<void>;
  limparAviso: () => void;
}

const Contexto = createContext<ValorSessao | null>(null);

export function useSessao(): ValorSessao {
  const valor = use(Contexto);
  if (!valor) throw new Error('useSessao precisa estar dentro de <SessaoProvider>.');
  return valor;
}

export function SessaoProvider({ children }: PropsWithChildren) {
  const [carregando, setCarregando] = useState(true);
  const [usuario, setUsuario] = useState<UsuarioLogado | null>(null);
  const [aviso, setAviso] = useState<string | null>(null);

  const sair = useCallback(async (motivo: string | null = null) => {
    definirToken(null);
    setUsuario(null);
    setAviso(motivo);
    await Promise.all([apagar(CHAVE_TOKEN), apagar(CHAVE_USUARIO), apagar(CHAVE_EXPIRA)]);
  }, []);

  // Restaura a sessão gravada ao abrir o app. Token vencido é descartado aqui
  // mesmo, sem gastar uma requisição só para descobrir que vai dar 401.
  useEffect(() => {
    (async () => {
      try {
        const [token, bruto, expira] = await Promise.all([
          ler(CHAVE_TOKEN), ler(CHAVE_USUARIO), ler(CHAVE_EXPIRA)
        ]);
        const vencido = expira ? new Date(expira).getTime() <= Date.now() : false;
        if (token && bruto && !vencido) {
          definirToken(token);
          setUsuario(JSON.parse(bruto));
        } else if (token) {
          await sair(vencido ? 'Sua sessão expirou. Entre novamente.' : null);
        }
      } catch {
        await sair();
      } finally {
        setCarregando(false);
      }
    })();
  }, [sair]);

  // O cliente HTTP avisa quando recebe 401; a sessão encerra e os guardas de
  // rota mandam para o login.
  useEffect(() => {
    aoSessaoExpirar(() => { void sair('Sua sessão expirou. Entre novamente.'); });
    return () => aoSessaoExpirar(null);
  }, [sair]);

  const entrar = useCallback(async (r: LoginResponse) => {
    definirToken(r.token);
    await Promise.all([
      gravar(CHAVE_TOKEN, r.token),
      gravar(CHAVE_USUARIO, JSON.stringify(r.usuario)),
      gravar(CHAVE_EXPIRA, r.expiraEm)
    ]);
    setAviso(null);
    setUsuario(r.usuario);
  }, []);

  const atualizarUsuario = useCallback(async (parcial: Partial<UsuarioLogado>) => {
    setUsuario(atual => {
      if (!atual) return atual;
      const novo = { ...atual, ...parcial };
      void gravar(CHAVE_USUARIO, JSON.stringify(novo));
      return novo;
    });
  }, []);

  const valor = useMemo<ValorSessao>(() => ({
    carregando,
    usuario,
    papel: papelDe(usuario?.papel),
    aviso,
    entrar,
    sair,
    atualizarUsuario,
    limparAviso: () => setAviso(null)
  }), [carregando, usuario, aviso, entrar, sair, atualizarUsuario]);

  return <Contexto value={valor}>{children}</Contexto>;
}
