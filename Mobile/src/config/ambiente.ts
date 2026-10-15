/* ==========================================================================
   Endereço da API
   --------------------------------------------------------------------------
   O app fala com a MESMA API publicada no Azure App Service que o site e o
   desktop usam. Não existe back-end próprio do mobile: os três clientes são
   apresentações diferentes do mesmo contrato REST, e a regra de negócio vive
   em um lugar só.

   O endereço é fixo e embutido no APK. O app sempre se conecta ao Azure.
   ========================================================================== */

export const API_BASE = 'https://tqbackendapi-cpb2edg0gxcsfggz.brazilsouth-01.azurewebsites.net';

/**
 * Tempo máximo de uma requisição. O Azure SQL serverless faz auto-pause, e a
 * primeira consulta depois da pausa demora (a API tem EnableRetryOnFailure
 * por isso). 45 s cobre esse "acordar" sem deixar o usuário olhando para um
 * indicador girando para sempre se a rede cair.
 */
export const TEMPO_LIMITE_MS = 45_000;
