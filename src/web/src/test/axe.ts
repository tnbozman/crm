import axe from 'axe-core'

/** Runs axe against a rendered component and returns a readable list of violations (empty when accessible). */
export async function axeViolations(container: Element): Promise<string[]> {
  const results = await axe.run(container, { resultTypes: ['violations'] })
  return results.violations.map(
    (v) => `${v.id}: ${v.help} (${v.nodes.map((n) => n.target.join(' ')).join(', ')})`,
  )
}
