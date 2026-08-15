// Parses ASP.NET Core's ValidationProblemDetails shape (automatic 400s from
// [ApiController] model validation, camelCase keys matching our request
// field names) into a flat { fieldName: firstMessage } map for inline
// per-field form errors.
export async function parseFieldErrors(response: Response): Promise<Record<string, string>> {
  try {
    const body = (await response.json()) as { errors?: Record<string, string[]> };
    if (!body.errors) {
      return {};
    }

    return Object.fromEntries(
      Object.entries(body.errors)
        .filter(([, messages]) => messages.length > 0)
        .map(([field, messages]) => [field.charAt(0).toLowerCase() + field.slice(1), messages[0]]),
    );
  } catch {
    return {};
  }
}
