import type { Dashboard, SalesResponse } from './types'

// Единая обёртка над fetch проверяет HTTP-статус и превращает ProblemDetails в текст ошибки для UI.
const get = async <T,>(url: string, signal: AbortSignal): Promise<T> => {
  const response = await fetch(url, { signal })
  if (!response.ok) throw new Error((await response.json().catch(() => null))?.title ?? 'Не удалось загрузить данные')
  return response.json() as Promise<T>
}

export const api = {
  // AbortSignal приходит из TanStack Query и отменяет устаревший запрос при смене фильтра.
  dashboard: (from: string, to: string, rankingBy: string, signal: AbortSignal) => get<Dashboard>(`/api/dashboard?from=${from}&to=${to}&rankingBy=${rankingBy}`, signal),
  sales: (from: string, to: string, signal: AbortSignal) => get<SalesResponse>(`/api/sales?from=${from}&to=${to}&limit=20`, signal),
} as const
