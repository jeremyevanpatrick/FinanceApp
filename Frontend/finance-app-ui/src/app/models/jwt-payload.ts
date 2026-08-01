export interface JwtPayload {
    sub: string; // user.Id
    jti: string; // Guid
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": string; // Email
    exp: number; // Expiration timestamp
    iss: string; // Issuer
    aud: string; // Audience
    iat: number; // Issued At (usually added automatically)
}