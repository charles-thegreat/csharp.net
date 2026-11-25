// Types mirror the C# models. Enums arrive as strings because of JsonStringEnumConverter.
export type ProposalStatus = 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Completed'

export const STATUSES: ProposalStatus[] = ['Submitted', 'UnderReview', 'Approved', 'Rejected', 'Completed']

export interface Proposal {
  id: number
  title: string
  description: string
  area: string
  status: ProposalStatus
  submittedAt: string // ISO 8601 with offset, from DateTimeOffset
  completedAt: string | null
}

export interface NewProposal {
  title: string
  description: string
  area: string
}

export type Workflow = Record<ProposalStatus, ProposalStatus[]>

export interface AreaLeadTime {
  area: string
  completedCount: number
  averageLeadTimeDays: number
}

export interface LeadTimeReport {
  completedCount: number
  averageLeadTimeDays: number | null
  byArea: AreaLeadTime[]
}

// ASP.NET Core returns errors as RFC 9457 ProblemDetails; validation errors add an `errors` map.
interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })

  if (!response.ok) {
    let message = `${response.status} ${response.statusText}`
    try {
      const problem = (await response.json()) as ProblemDetails
      message = problem.errors
        ? Object.values(problem.errors).flat().join(' ')
        : (problem.detail ?? problem.title ?? message)
    } catch {
      // Body wasn't JSON; keep the status line.
    }
    throw new Error(message)
  }

  return (await response.json()) as T
}

export const api = {
  listProposals: (status?: ProposalStatus) =>
    request<Proposal[]>(status ? `/api/proposals?status=${status}` : '/api/proposals'),

  createProposal: (proposal: NewProposal) =>
    request<Proposal>('/api/proposals', { method: 'POST', body: JSON.stringify(proposal) }),

  changeStatus: (id: number, status: ProposalStatus) =>
    request<Proposal>(`/api/proposals/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status }) }),

  getWorkflow: () => request<Workflow>('/api/proposals/workflow'),

  getLeadTime: () => request<LeadTimeReport>('/api/metrics/lead-time'),
}

export function statusLabel(status: ProposalStatus): string {
  return status === 'UnderReview' ? 'Under review' : status
}

export function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : String(error)
}
