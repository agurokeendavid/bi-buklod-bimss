// Decodes a JWT's payload for display purposes only (e.g. showing the
// signed-in user's name in the header) — never for authorization
// decisions, which stay server-side per this app's design. The claim read
// here is Bimss.Infrastructure.Identity.JwtTokenService's ClaimTypes.Name.
export function decodeJwtDisplayName(accessToken: string): string | null {
  try {
    const payload = accessToken.split(".")[1];
    if (!payload) {
      return null;
    }

    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
      atob(base64)
        .split("")
        .map((char) => `%${char.charCodeAt(0).toString(16).padStart(2, "0")}`)
        .join(""),
    );

    const claims = JSON.parse(json) as Record<string, unknown>;
    // System.IdentityModel.Tokens.Jwt's JwtSecurityTokenHandler applies its
    // DefaultOutboundClaimTypeMap when serializing, which shortens
    // ClaimTypes.Name to "unique_name" — checking a couple of fallbacks
    // keeps this resilient to that mapping detail changing.
    const name = claims["unique_name"] ?? claims["name"] ?? claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
    return typeof name === "string" ? name : null;
  } catch {
    return null;
  }
}
