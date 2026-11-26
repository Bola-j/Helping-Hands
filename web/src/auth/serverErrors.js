// Reads ASP.NET-style validation errors ({ errors: { Field: ["message"] } })
// from a 400 response and returns { field: "message" } keyed by camelCase name.
export async function readServerFieldErrors(error) {
  const fields = {};
  try {
    const body = await error.response.clone().json();
    const errors = body && body.errors;
    if (errors && typeof errors === 'object' && !Array.isArray(errors)) {
      for (const [key, value] of Object.entries(errors)) {
        const message = Array.isArray(value) ? value[0] : value;
        if (typeof message === 'string') fields[key.charAt(0).toLowerCase() + key.slice(1)] = message;
      }
    }
  } catch {
    // No readable body: the form-level message is enough.
  }
  return fields;
}