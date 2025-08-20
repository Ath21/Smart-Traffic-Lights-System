const KEY = 'stls.jwt'


export const tokenService = {
get() { return localStorage.getItem(KEY) },
set(token) { localStorage.setItem(KEY, token) },
clear() { localStorage.removeItem(KEY) },
}


export function parseToken(token) {
// Demo: token is base64url of JSON payload only (not a real JWT!).
// In real JWTs you'd split by '.', decode payload, and JSON.parse it.
try {
const json = atob(token.replace(/-/g,'+').replace(/_/g,'/'))
return JSON.parse(json)
} catch {
return null
}
}