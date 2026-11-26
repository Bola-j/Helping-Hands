export async function applicationError(error, fallback) {
  try {
    const problem = await error.response?.clone().json();
    if (problem?.detail) return problem.detail;
    if (problem?.title) return problem.title;
  } catch { /* Use the safe fallback below. */ }
  return fallback;
}
