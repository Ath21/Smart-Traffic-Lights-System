// Fake API: returns a base64 payload string that *looks* like a token
export async function loginApi({ email, password }) {
// Map an email to a role for demo
const role = email.includes('admin') ? 'admin'
: email.includes('operator') ? 'operator'
: email.includes('user') ? 'user'
: 'viewer'


const payload = { sub: '123', email, role }
const token = btoa(JSON.stringify(payload)) // base64 only (NOT secure)
return { token }
}


export async function registerApi({ email }) {
// Pretend to register, then log in as basic user
const payload = { sub: '124', email, role: 'user' }
const token = btoa(JSON.stringify(payload))
return { token }
}