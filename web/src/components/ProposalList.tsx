import { api, errorMessage, statusLabel, type Proposal, type ProposalStatus, type Workflow } from '../api.ts'

// Button text for moving *to* each status.
const ACTION_LABELS: Record<ProposalStatus, string> = {
  Submitted: 'Reopen',
  UnderReview: 'Start review',
  Approved: 'Approve',
  Rejected: 'Reject',
  Completed: 'Mark complete',
}

const DAY_MS = 24 * 60 * 60 * 1000

interface Props {
  proposals: Proposal[]
  workflow: Workflow | null
  onChanged: () => void
  onError: (message: string) => void
}

export function ProposalList({ proposals, workflow, onChanged, onError }: Props) {
  async function changeStatus(id: number, status: ProposalStatus) {
    try {
      await api.changeStatus(id, status)
      onChanged()
    } catch (e) {
      onError(errorMessage(e)) // e.g. the 409 message if the rules reject it
    }
  }

  if (proposals.length === 0) {
    return <p className="muted">No proposals.</p>
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Proposal</th>
          <th>Area</th>
          <th>Status</th>
          <th>Submitted</th>
          <th>Lead time</th>
          <th aria-label="Actions" />
        </tr>
      </thead>
      <tbody>
        {proposals.map((p) => (
          <tr key={p.id}>
            <td>
              <strong>{p.title}</strong>
              <div className="muted">{p.description}</div>
            </td>
            <td>{p.area}</td>
            <td>
              <span className={`badge badge-${p.status}`}>{statusLabel(p.status)}</span>
            </td>
            <td>{new Date(p.submittedAt).toLocaleDateString()}</td>
            <td>{p.completedAt ? `${daysBetween(p.submittedAt, p.completedAt)} days` : '—'}</td>
            <td className="actions">
              {/* Only the transitions the API allows, from GET api/proposals/workflow. */}
              {workflow?.[p.status].map((next) => (
                <button key={next} type="button" onClick={() => changeStatus(p.id, next)}>
                  {ACTION_LABELS[next]}
                </button>
              ))}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

function daysBetween(from: string, to: string): number {
  return Math.round((new Date(to).getTime() - new Date(from).getTime()) / DAY_MS)
}
