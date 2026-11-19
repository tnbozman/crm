import type { paths } from './schema'

/** Paths in the generated OpenAPI types that support GET. */
export type GetPath = {
  [P in keyof paths]: paths[P] extends { get: object } ? P : never
}[keyof paths]

/** The JSON body of a 200 response for a GET path, taken from the generated types. */
export type GetResponse<P extends GetPath> = paths[P] extends {
  get: { responses: { 200: { content: { 'application/json': infer Body } } } }
}
  ? Body
  : never

/** Thrown when the API answers with a non-success status. */
export class ApiError extends Error {
  constructor(readonly status: number) {
    super(`API request failed with status ${status}`)
    this.name = 'ApiError'
  }
}

/** Typed GET against the CRM API. Paths and response bodies come from the generated OpenAPI types. */
export async function apiFetch<P extends GetPath>(path: P): Promise<GetResponse<P>> {
  const response = await fetch(new URL(path, window.location.origin), {
    headers: { Accept: 'application/json' },
  })
  if (!response.ok) {
    throw new ApiError(response.status)
  }
  return (await response.json()) as GetResponse<P>
}
